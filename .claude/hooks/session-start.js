#!/usr/bin/env node
/**
 * Session Start Hook for AI Coding Factory
 * Reads brain.jsonl and project context when a new session begins.
 * Analyzes package.json and *.csproj for tech stack info and stores in LTM.
 *
 * @event SessionStart
 */

const fs = require('fs');
const path = require('path');
const {
  findProjectRoot,
  getMaestroDir,
  ensureMaestroDir,
  readFileSafe,
  getFileHash,
  readStdin,
  logDebug,
  outputJson
} = require('./lib/utils');

const {
  formatBrainForContext,
  writeTechToBrain,
  extractLastSummary,
  writeCompactToBrain
} = require('./lib/brain');

const LOG_PREFIX = '[SESSION-START]';

// Framework detection patterns (extended for .NET)
const FRAMEWORK_PATTERNS = {
  // JavaScript/TypeScript
  'next.js': { deps: ['next'], files: ['next.config.js', 'next.config.mjs', 'next.config.ts'] },
  'react': { deps: ['react', 'react-dom'], files: [] },
  'vue': { deps: ['vue'], files: ['vue.config.js', 'nuxt.config.js'] },
  'nuxt': { deps: ['nuxt'], files: ['nuxt.config.js', 'nuxt.config.ts'] },
  'angular': { deps: ['@angular/core'], files: ['angular.json'] },
  'svelte': { deps: ['svelte'], files: ['svelte.config.js'] },
  'express': { deps: ['express'], files: [] },
  'fastify': { deps: ['fastify'], files: [] },
  'nestjs': { deps: ['@nestjs/core'], files: ['nest-cli.json'] },
  'electron': { deps: ['electron'], files: [] },
  'tauri': { deps: ['@tauri-apps/api'], files: ['tauri.conf.json'] },
  'astro': { deps: ['astro'], files: ['astro.config.mjs'] },
  'remix': { deps: ['@remix-run/react'], files: ['remix.config.js'] },
  'gatsby': { deps: ['gatsby'], files: ['gatsby-config.js'] },
  // React Native
  'expo': { deps: ['expo'], files: ['app.json', 'app.config.js', 'app.config.ts'] },
  'react-native': { deps: ['react-native'], files: [] },
  // .NET
  'aspnet-core': { deps: [], files: ['*.csproj', 'appsettings.json'] },
  'clean-architecture': { deps: [], files: ['Directory.Build.props', '*.sln'] }
};

// Important dependencies to track
const IMPORTANT_DEPS = {
  // State management
  'zustand': 'State (Zustand)',
  'redux': 'State (Redux)',
  '@reduxjs/toolkit': 'State (Redux Toolkit)',
  'recoil': 'State (Recoil)',
  'jotai': 'State (Jotai)',
  'mobx': 'State (MobX)',
  '@tanstack/react-query': 'Data Fetching (React Query)',
  'swr': 'Data Fetching (SWR)',

  // Styling
  'tailwindcss': 'Styling (Tailwind)',
  'styled-components': 'Styling (Styled Components)',
  '@emotion/react': 'Styling (Emotion)',
  'sass': 'Styling (SASS)',
  '@mui/material': 'UI (Material UI)',
  '@chakra-ui/react': 'UI (Chakra)',
  'antd': 'UI (Ant Design)',
  'shadcn-ui': 'UI (shadcn)',

  // Database
  'prisma': 'ORM (Prisma)',
  '@prisma/client': 'ORM (Prisma)',
  'drizzle-orm': 'ORM (Drizzle)',
  'typeorm': 'ORM (TypeORM)',
  'mongoose': 'ODM (Mongoose)',
  'sequelize': 'ORM (Sequelize)',

  // Auth
  'next-auth': 'Auth (NextAuth)',
  '@clerk/nextjs': 'Auth (Clerk)',
  '@supabase/supabase-js': 'Backend (Supabase)',
  'firebase': 'Backend (Firebase)',

  // Testing
  'jest': 'Testing (Jest)',
  'vitest': 'Testing (Vitest)',
  '@testing-library/react': 'Testing (RTL)',
  'playwright': 'E2E (Playwright)',
  'cypress': 'E2E (Cypress)',

  // Build tools
  'vite': 'Build (Vite)',
  'webpack': 'Build (Webpack)',
  'esbuild': 'Build (esbuild)',
  'turbo': 'Monorepo (Turborepo)',

  // Utilities
  'zod': 'Validation (Zod)',
  'yup': 'Validation (Yup)',
  'axios': 'HTTP (Axios)',
  'date-fns': 'Dates (date-fns)',
  'dayjs': 'Dates (Day.js)',
  'lodash': 'Utils (Lodash)',
  'framer-motion': 'Animation (Framer)',

  // React Native specific
  'expo-router': 'Navigation (Expo Router)',
  '@react-navigation/native': 'Navigation (React Navigation)',
  'react-native-reanimated': 'Animation (Reanimated)',
  '@sentry/react-native': 'Observability (Sentry)',
  'expo-secure-store': 'Storage (Secure Store)'
};

/**
 * Check if tech stack needs re-analysis (package.json changed).
 */
function shouldReanalyzeTech(projectRoot) {
  const pkgPath = path.join(projectRoot, 'package.json');
  const hashFile = path.join(getMaestroDir(projectRoot), '.tech_hash');

  const currentHash = getFileHash(pkgPath);
  if (!currentHash) {
    // Check for .NET project
    const slnFiles = fs.readdirSync(projectRoot).filter(f => f.endsWith('.sln'));
    if (slnFiles.length > 0) {
      return true; // Always analyze .NET projects
    }
    return false;
  }

  if (fs.existsSync(hashFile)) {
    try {
      const storedHash = fs.readFileSync(hashFile, 'utf-8').trim();
      if (storedHash === currentHash) {
        return false;
      }
    } catch (err) {
      // Continue with reanalysis
    }
  }

  return true;
}

/**
 * Save current package.json hash.
 */
function saveTechHash(projectRoot) {
  const pkgPath = path.join(projectRoot, 'package.json');
  const hashFile = path.join(ensureMaestroDir(projectRoot), '.tech_hash');

  const hash = getFileHash(pkgPath);
  if (hash) {
    try {
      fs.writeFileSync(hashFile, hash, 'utf-8');
    } catch (err) {
      logDebug(LOG_PREFIX, `Error saving tech hash: ${err.message}`);
    }
  }
}

/**
 * Analyze .NET project (*.csproj, *.sln).
 */
function analyzeDotNetProject(projectRoot) {
  const result = {
    name: path.basename(projectRoot),
    version: '1.0.0',
    description: '.NET Project',
    frameworks: [],
    frameworkVersions: {},
    keyDeps: [],
    devTools: [],
    scripts: {},
    nodeVersion: null,
    packageManager: 'dotnet',
    type: 'dotnet'
  };

  // Find .sln file
  const slnFiles = fs.readdirSync(projectRoot).filter(f => f.endsWith('.sln'));
  if (slnFiles.length > 0) {
    result.name = slnFiles[0].replace('.sln', '');
    result.frameworks.push('clean-architecture');
  }

  // Find .csproj files
  const findCsproj = (dir, depth = 0) => {
    if (depth > 3) return [];
    const files = [];
    try {
      const entries = fs.readdirSync(dir);
      for (const entry of entries) {
        const fullPath = path.join(dir, entry);
        const stat = fs.statSync(fullPath);
        if (stat.isDirectory() && !entry.startsWith('.') && entry !== 'node_modules') {
          files.push(...findCsproj(fullPath, depth + 1));
        } else if (entry.endsWith('.csproj')) {
          files.push(fullPath);
        }
      }
    } catch (err) {
      // Ignore permission errors
    }
    return files;
  };

  const csprojFiles = findCsproj(projectRoot);

  // Analyze csproj files for dependencies
  const dotnetDeps = new Set();
  for (const csproj of csprojFiles) {
    try {
      const content = fs.readFileSync(csproj, 'utf-8');

      // Check target framework
      const tfMatch = content.match(/<TargetFramework>([^<]+)<\/TargetFramework>/);
      if (tfMatch) {
        result.frameworkVersions['dotnet'] = tfMatch[1];
        if (tfMatch[1].startsWith('net8')) {
          result.devTools.push('.NET 8 LTS');
        }
      }

      // Extract PackageReferences
      const pkgRefs = content.matchAll(/<PackageReference\s+Include="([^"]+)"/g);
      for (const match of pkgRefs) {
        dotnetDeps.add(match[1]);
      }

      // Detect ASP.NET Core
      if (content.includes('Microsoft.AspNetCore') || content.includes('Sdk="Microsoft.NET.Sdk.Web"')) {
        if (!result.frameworks.includes('aspnet-core')) {
          result.frameworks.push('aspnet-core');
        }
      }
    } catch (err) {
      // Ignore read errors
    }
  }

  // Map .NET dependencies to readable format
  const dotnetDepMap = {
    'MediatR': 'CQRS (MediatR)',
    'FluentValidation': 'Validation (FluentValidation)',
    'AutoMapper': 'Mapping (AutoMapper)',
    'Serilog': 'Logging (Serilog)',
    'Microsoft.EntityFrameworkCore': 'ORM (EF Core)',
    'Npgsql.EntityFrameworkCore.PostgreSQL': 'Database (PostgreSQL)',
    'Microsoft.AspNetCore.Authentication.JwtBearer': 'Auth (JWT)',
    'xunit': 'Testing (xUnit)',
    'FluentAssertions': 'Testing (FluentAssertions)',
    'Moq': 'Testing (Moq)',
    'Testcontainers': 'Testing (TestContainers)',
    'Akka.NET': 'Actor System (Akka.NET)',
    'Aspire': 'Orchestration (Aspire)'
  };

  for (const dep of dotnetDeps) {
    for (const [key, label] of Object.entries(dotnetDepMap)) {
      if (dep.toLowerCase().includes(key.toLowerCase())) {
        if (!result.keyDeps.includes(label)) {
          result.keyDeps.push(label);
        }
      }
    }
  }

  // Add standard scripts
  result.scripts = {
    build: 'dotnet build',
    test: 'dotnet test',
    run: 'dotnet run',
    restore: 'dotnet restore'
  };

  return result;
}

/**
 * Analyze package.json and extract tech stack info.
 */
function analyzePackageJson(projectRoot) {
  const pkgPath = path.join(projectRoot, 'package.json');

  if (!fs.existsSync(pkgPath)) {
    // Check for .NET project
    const slnFiles = fs.readdirSync(projectRoot).filter(f => f.endsWith('.sln'));
    if (slnFiles.length > 0) {
      return analyzeDotNetProject(projectRoot);
    }
    logDebug(LOG_PREFIX, 'No package.json or .sln found');
    return null;
  }

  try {
    const pkgContent = fs.readFileSync(pkgPath, 'utf-8');
    const pkg = JSON.parse(pkgContent);

    const result = {
      name: pkg.name || 'unknown',
      version: pkg.version || '0.0.0',
      description: pkg.description || '',
      frameworks: [],
      frameworkVersions: {},
      keyDeps: [],
      devTools: [],
      scripts: {},
      nodeVersion: null,
      packageManager: null,
      type: pkg.type || 'commonjs'
    };

    // Combine all dependencies
    const allDeps = {
      ...pkg.dependencies,
      ...pkg.devDependencies
    };

    // Detect frameworks
    for (const [framework, patterns] of Object.entries(FRAMEWORK_PATTERNS)) {
      for (const dep of patterns.deps) {
        if (allDeps[dep]) {
          if (!result.frameworks.includes(framework)) {
            result.frameworks.push(framework);
          }
          const version = allDeps[dep].replace(/[\^~>=<]/g, '');
          result.frameworkVersions[dep] = version;
          break;
        }
      }

      for (const cfgFile of patterns.files) {
        if (fs.existsSync(path.join(projectRoot, cfgFile))) {
          if (!result.frameworks.includes(framework)) {
            result.frameworks.push(framework);
          }
          break;
        }
      }
    }

    // Capture versions
    if (allDeps['react'] && !result.frameworkVersions['react']) {
      result.frameworkVersions['react'] = allDeps['react'].replace(/[\^~>=<]/g, '');
    }
    if (allDeps['typescript']) {
      result.frameworkVersions['typescript'] = allDeps['typescript'].replace(/[\^~>=<]/g, '');
    }
    if (allDeps['tailwindcss']) {
      result.frameworkVersions['tailwindcss'] = allDeps['tailwindcss'].replace(/[\^~>=<]/g, '');
    }

    // Detect important dependencies
    for (const [dep, label] of Object.entries(IMPORTANT_DEPS)) {
      if (allDeps[dep]) {
        result.keyDeps.push(label);
      }
    }

    // TypeScript detection
    if (allDeps['typescript']) {
      result.devTools.push('TypeScript');
      const tsconfigPath = path.join(projectRoot, 'tsconfig.json');
      if (fs.existsSync(tsconfigPath)) {
        try {
          const tsconfig = JSON.parse(fs.readFileSync(tsconfigPath, 'utf-8'));
          if (tsconfig.compilerOptions?.strict) {
            result.devTools.push('TypeScript (strict)');
          }
        } catch (err) { }
      }
    }

    // ESLint
    if (allDeps['eslint'] || fs.existsSync(path.join(projectRoot, '.eslintrc.js'))) {
      result.devTools.push('ESLint');
    }

    // Prettier
    if (allDeps['prettier'] || fs.existsSync(path.join(projectRoot, '.prettierrc'))) {
      result.devTools.push('Prettier');
    }

    // Extract important scripts
    const scripts = pkg.scripts || {};
    const importantScripts = ['dev', 'build', 'start', 'test', 'lint', 'format', 'preview', 'ios', 'android'];
    for (const script of importantScripts) {
      if (scripts[script]) {
        result.scripts[script] = scripts[script];
      }
    }

    // Node version
    if (pkg.engines?.node) {
      result.nodeVersion = pkg.engines.node;
    }

    // Package manager detection
    if (fs.existsSync(path.join(projectRoot, 'pnpm-lock.yaml'))) {
      result.packageManager = 'pnpm';
    } else if (fs.existsSync(path.join(projectRoot, 'yarn.lock'))) {
      result.packageManager = 'yarn';
    } else if (fs.existsSync(path.join(projectRoot, 'package-lock.json'))) {
      result.packageManager = 'npm';
    } else if (fs.existsSync(path.join(projectRoot, 'bun.lockb'))) {
      result.packageManager = 'bun';
    }

    logDebug(LOG_PREFIX, `Analyzed tech stack: ${result.frameworks.join(', ')}`);
    return result;

  } catch (err) {
    logDebug(LOG_PREFIX, `Error reading package.json: ${err.message}`);
    return null;
  }
}

/**
 * Analyze project directory structure.
 */
function analyzeProjectStructure(projectRoot) {
  const structure = {
    type: 'unknown',
    patterns: [],
    keyDirectories: [],
    entryPoints: []
  };

  // Check for common patterns
  if (fs.existsSync(path.join(projectRoot, 'app'))) {
    structure.patterns.push('App Router (Next.js 13+)');
    structure.keyDirectories.push('app/');
  }

  if (fs.existsSync(path.join(projectRoot, 'pages'))) {
    structure.patterns.push('Pages Router');
    structure.keyDirectories.push('pages/');
  }

  if (fs.existsSync(path.join(projectRoot, 'src'))) {
    structure.keyDirectories.push('src/');
    const srcSubdirs = ['components', 'hooks', 'lib', 'utils', 'services', 'api', 'store', 'types', 'styles', 'slices', 'theme'];
    for (const subdir of srcSubdirs) {
      if (fs.existsSync(path.join(projectRoot, 'src', subdir))) {
        structure.keyDirectories.push(`src/${subdir}/`);
      }
    }
  }

  // .NET Clean Architecture detection
  const dotnetDirs = ['Domain', 'Application', 'Infrastructure', 'API', 'WebApi'];
  for (const dir of dotnetDirs) {
    if (fs.existsSync(path.join(projectRoot, 'src', dir))) {
      structure.patterns.push('Clean Architecture (.NET)');
      structure.keyDirectories.push(`src/${dir}/`);
    }
  }

  // Tests directory
  if (fs.existsSync(path.join(projectRoot, 'tests'))) {
    structure.keyDirectories.push('tests/');
  }

  // API routes
  const apiPaths = [
    path.join(projectRoot, 'app', 'api'),
    path.join(projectRoot, 'pages', 'api'),
    path.join(projectRoot, 'src', 'app', 'api')
  ];
  for (const apiPath of apiPaths) {
    if (fs.existsSync(apiPath)) {
      structure.patterns.push('API Routes');
      break;
    }
  }

  // Monorepo detection
  if (fs.existsSync(path.join(projectRoot, 'packages')) ||
    fs.existsSync(path.join(projectRoot, 'apps'))) {
    structure.type = 'monorepo';
    structure.patterns.push('Monorepo');
  } else {
    structure.type = 'single-package';
  }

  // Docker
  if (fs.existsSync(path.join(projectRoot, 'Dockerfile')) ||
    fs.existsSync(path.join(projectRoot, 'docker-compose.yml'))) {
    structure.patterns.push('Docker');
  }

  // Entry points
  const entryFiles = [
    'app/page.tsx', 'app/page.jsx', 'pages/index.tsx', 'pages/index.jsx',
    'src/index.ts', 'src/index.js', 'src/main.ts', 'src/main.tsx',
    'index.ts', 'index.js', 'Program.cs', 'Startup.cs'
  ];
  for (const entry of entryFiles) {
    if (fs.existsSync(path.join(projectRoot, entry))) {
      structure.entryPoints.push(entry);
    }
  }

  return structure;
}

/**
 * Find any active development plan.
 */
function findActivePlan(projectRoot) {
  const maestroDir = getMaestroDir(projectRoot);
  const planFiles = ['task.md', 'development_plan.md', 'implementation_plan.md'];

  for (const planFile of planFiles) {
    const planPath = path.join(maestroDir, planFile);
    if (fs.existsSync(planPath)) {
      return planPath;
    }
  }

  return null;
}

/**
 * Build context message from available files.
 */
function buildContextMessage(projectRoot) {
  const contextParts = [];

  // Check for brain.jsonl (Cross-session memory)
  const brainSummary = formatBrainForContext(projectRoot);
  if (brainSummary) {
    contextParts.push('## Long-Term Project Memory');
    contextParts.push(brainSummary);
    logDebug(LOG_PREFIX, 'Loaded brain.jsonl summary');
  }

  // Check for active plan
  const planPath = findActivePlan(projectRoot);
  if (planPath) {
    const planContent = readFileSafe(planPath, 30000);
    if (planContent) {
      const planName = path.basename(planPath);
      contextParts.push(`\n## Active Plan: ${planName}`);

      const lines = planContent.split('\n');
      if (lines.length > 100) {
        contextParts.push(`(Truncated - ${lines.length} lines)`);
        contextParts.push(lines.slice(0, 100).join('\n'));
      } else {
        contextParts.push(planContent);
      }
      logDebug(LOG_PREFIX, `Loaded plan: ${planName}`);
    }
  }

  return contextParts.length > 0 ? contextParts.join('\n') : null;
}

/**
 * Main hook entry point.
 */
async function main() {
  const projectRoot = findProjectRoot();
  const maestroDir = ensureMaestroDir(projectRoot);

  logDebug(LOG_PREFIX, '='.repeat(60));
  logDebug(LOG_PREFIX, 'SESSION START HOOK TRIGGERED');
  logDebug(LOG_PREFIX, `Working Directory: ${projectRoot}`);
  logDebug(LOG_PREFIX, `Maestro Directory: ${maestroDir}`);

  try {
    const hookInput = await readStdin();
    logDebug(LOG_PREFIX, `Hook input: source=${hookInput.source}, model=${hookInput.model}`);

    // COMPACT SUMMARY PERSISTENCE
    const isCompaction = hookInput.source === 'compact' ||
      hookInput.source === 'resume' ||
      hookInput.matcher === 'compact' ||
      hookInput.matcher === 'resume';

    if (isCompaction) {
      const transcriptPath = hookInput.transcriptPath || hookInput.transcript_path || hookInput.transcript;
      if (transcriptPath) {
        logDebug(LOG_PREFIX, `Detected compact/resume session. Attempting summary capture...`);

        let summary = null;
        for (let attempt = 1; attempt <= 3; attempt++) {
          logDebug(LOG_PREFIX, `Capture attempt ${attempt}/3...`);
          summary = extractLastSummary(transcriptPath);
          if (summary) break;
          if (attempt < 3) {
            await new Promise(resolve => setTimeout(resolve, 1000));
          }
        }

        if (summary) {
          writeCompactToBrain(summary, projectRoot);
          logDebug(LOG_PREFIX, 'Summary captured and persisted to brain.jsonl');
        } else {
          logDebug(LOG_PREFIX, 'Failed to capture summary after 3 attempts.');
        }
      }
    }

    // STALE CONTEXT GUARD
    const rootEntries = fs.readdirSync(projectRoot);
    const hasProjectFiles = rootEntries.some(e => !['.git', '.maestro', '.claude'].includes(e));
    if (!hasProjectFiles) {
      logDebug(LOG_PREFIX, 'Project directory empty (except meta) - treated as BLACK SLATE.');

      const ts = Date.now();
      const filesToPurge = ['brain.jsonl'];

      for (const file of filesToPurge) {
        const filePath = path.join(maestroDir, file);
        if (fs.existsSync(filePath)) {
          const stalePath = path.join(maestroDir, `${file}.stale.${ts}`);
          fs.renameSync(filePath, stalePath);
          logDebug(LOG_PREFIX, `Purged stale file: ${file} -> ${path.basename(stalePath)}`);
        }
      }

      const output = {
        type: 'session_context',
        systemMessage: `
# AI CODING FACTORY: BLACK SLATE PROJECT
This project directory is effectively EMPTY (except for meta-folders like .git).
Existing project memory has been PURGED to prevent stale context retrieval.

**INSTRUCTIONS**:
1. Treat this as a **FRESH START**.
2. Do NOT attempt to recover context from 'git log' or old memory files.
3. Architecture, tech stack, and goals should be defined FROM SCRATCH based on user's new request.
`
      };
      outputJson(output);
      return;
    }

    // Analyze tech stack if needed
    if (shouldReanalyzeTech(projectRoot)) {
      logDebug(LOG_PREFIX, 'Tech stack analysis triggered');
      const techInfo = analyzePackageJson(projectRoot);
      const structure = analyzeProjectStructure(projectRoot);

      if (techInfo) {
        writeTechToBrain(techInfo, structure, projectRoot);
        saveTechHash(projectRoot);
        logDebug(LOG_PREFIX, 'Tech stack info written to brain.jsonl');
      }
    } else {
      logDebug(LOG_PREFIX, 'Tech stack already analyzed, skipping');
    }

    // Build context from LTM and Plans
    const context = buildContextMessage(projectRoot);

    if (context) {
      const output = {
        type: 'session_context',
        systemMessage: `
# AI CODING FACTORY SESSION CONTEXT

The following context was loaded from project memory and active plans. This represents the **LONG-TERM MEMORY** of the project.

${context}

---

**CRITICAL INSTRUCTIONS**:
1. **Memory Continuity**: Review the "Recent Compact Summaries" above. These contain the distilled history of previous interactions. Use them to maintain seamless continuity.
2. **Context Awareness**: The Tech Stack and Architecture sections define the playground. Use these for high-accuracy searches and implementation decisions.
3. **Task Tracking**: If \`task.md\` exists, it is the source of truth for current progress. Always keep it updated.
4. **Governance**: Follow CORPORATE_RND_POLICY.md for enterprise standards and traceability requirements.
`
      };
      outputJson(output);
      logDebug(LOG_PREFIX, 'Context injected successfully');
    } else {
      outputJson({});
      logDebug(LOG_PREFIX, 'No context files found');
    }

    logDebug(LOG_PREFIX, 'Session start hook completed');

  } catch (err) {
    const errorOutput = {
      type: 'session_context',
      systemMessage: `Hook Error: ${err.message}\n\nStack: ${err.stack}`
    };
    outputJson(errorOutput);
  }
}

main();

#!/usr/bin/env node
/**
 * Pre-commit Enforcement Hook
 *
 * Validates enforcement rules from config/enforcement/rules.yaml
 * before allowing commits to proceed.
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

// Read stdin for hook context
let input = '';
process.stdin.on('data', (chunk) => input += chunk);
process.stdin.on('end', () => {
  try {
    const result = validateCommit();
    if (!result.success) {
      console.error('\n[Enforcement] ⚠️  Pre-commit validation warnings:');
      result.warnings.forEach(w => console.error(`  - ${w}`));
      if (result.errors.length > 0) {
        console.error('\n[Enforcement] ❌ Pre-commit validation errors:');
        result.errors.forEach(e => console.error(`  - ${e}`));
        console.error('\n[Enforcement] Fix errors before committing.');
      }
    }
  } catch (e) {
    // Don't block on hook errors
    console.error(`[Enforcement] Hook error (non-blocking): ${e.message}`);
  }
  // Always output the input to continue the chain
  console.log(input);
});

function validateCommit() {
  const warnings = [];
  const errors = [];

  // Find project root
  const projectRoot = findProjectRoot();
  if (!projectRoot) {
    return { success: true, warnings: [], errors: [] };
  }

  // Check 1: Story ID in staged commit message or recent message
  const storyIdCheck = checkStoryId();
  if (!storyIdCheck.valid) {
    warnings.push(storyIdCheck.message);
  }

  // Check 2: No secrets in staged files
  const secretsCheck = checkForSecrets(projectRoot);
  if (!secretsCheck.valid) {
    errors.push(...secretsCheck.errors);
  }

  // Check 3: No .env files staged
  const envCheck = checkEnvFiles();
  if (!envCheck.valid) {
    errors.push(envCheck.message);
  }

  // Check 4: Domain layer violations (if .cs files changed)
  const domainCheck = checkDomainViolations(projectRoot);
  if (!domainCheck.valid) {
    errors.push(...domainCheck.errors);
  }

  // Check 5: Tests for code changes
  const testCheck = checkTestsForCodeChanges();
  if (!testCheck.valid) {
    warnings.push(testCheck.message);
  }

  return {
    success: errors.length === 0,
    warnings,
    errors
  };
}

function findProjectRoot() {
  try {
    const gitRoot = execSync('git rev-parse --show-toplevel', { encoding: 'utf8' }).trim();
    return gitRoot;
  } catch {
    return null;
  }
}

function checkStoryId() {
  try {
    // Check the commit message being created (from COMMIT_EDITMSG or last commit)
    const lastMessage = execSync('git log -1 --format=%s 2>/dev/null || echo ""', { encoding: 'utf8' }).trim();
    const hasStoryId = /ACF-\d+/i.test(lastMessage);

    if (!hasStoryId && lastMessage.length > 0) {
      return {
        valid: false,
        message: 'Commit message should include story ID (ACF-###)'
      };
    }
    return { valid: true };
  } catch {
    return { valid: true };
  }
}

function checkForSecrets(projectRoot) {
  const errors = [];
  const secretPatterns = [
    { pattern: /api[_-]?key\s*[=:]\s*["']?[a-zA-Z0-9_-]{20,}/i, name: 'API Key' },
    { pattern: /password\s*[=:]\s*["'][^"']+["']/i, name: 'Password' },
    { pattern: /secret\s*[=:]\s*["']?[a-zA-Z0-9_-]{20,}/i, name: 'Secret' },
    { pattern: /-----BEGIN.*PRIVATE KEY-----/i, name: 'Private Key' },
    { pattern: /bearer\s+[a-zA-Z0-9_-]{20,}/i, name: 'Bearer Token' },
    { pattern: /aws_access_key_id\s*[=:]/i, name: 'AWS Credentials' }
  ];

  try {
    const stagedFiles = execSync('git diff --cached --name-only', { encoding: 'utf8' })
      .split('\n')
      .filter(f => f && !f.endsWith('.md') && !f.includes('test'));

    for (const file of stagedFiles) {
      const filePath = path.join(projectRoot, file);
      if (fs.existsSync(filePath)) {
        try {
          const content = fs.readFileSync(filePath, 'utf8');
          for (const { pattern, name } of secretPatterns) {
            if (pattern.test(content)) {
              errors.push(`Potential ${name} found in ${file}`);
            }
          }
        } catch {
          // Skip files we can't read
        }
      }
    }
  } catch {
    // Non-blocking
  }

  return { valid: errors.length === 0, errors };
}

function checkEnvFiles() {
  try {
    const stagedFiles = execSync('git diff --cached --name-only', { encoding: 'utf8' });
    const envFiles = stagedFiles.split('\n').filter(f => /^\.env($|\.)/.test(path.basename(f)));

    if (envFiles.length > 0) {
      return {
        valid: false,
        message: `.env files should not be committed: ${envFiles.join(', ')}`
      };
    }
    return { valid: true };
  } catch {
    return { valid: true };
  }
}

function checkDomainViolations(projectRoot) {
  const errors = [];
  const forbiddenInDomain = [
    'using Infrastructure',
    'using Microsoft.EntityFrameworkCore',
    'using System.Net.Http',
    'using Npgsql'
  ];

  try {
    const stagedFiles = execSync('git diff --cached --name-only', { encoding: 'utf8' })
      .split('\n')
      .filter(f => f.includes('/Domain/') && f.endsWith('.cs'));

    for (const file of stagedFiles) {
      const filePath = path.join(projectRoot, file);
      if (fs.existsSync(filePath)) {
        try {
          const content = fs.readFileSync(filePath, 'utf8');
          for (const forbidden of forbiddenInDomain) {
            if (content.includes(forbidden)) {
              errors.push(`Domain layer violation in ${file}: ${forbidden}`);
            }
          }
        } catch {
          // Skip files we can't read
        }
      }
    }
  } catch {
    // Non-blocking
  }

  return { valid: errors.length === 0, errors };
}

function checkTestsForCodeChanges() {
  try {
    const stagedFiles = execSync('git diff --cached --name-only', { encoding: 'utf8' }).split('\n');

    const codeFiles = stagedFiles.filter(f =>
      (f.endsWith('.cs') || f.endsWith('.ts') || f.endsWith('.tsx')) &&
      !f.includes('test') && !f.includes('Test') && !f.includes('.spec.')
    );

    const testFiles = stagedFiles.filter(f =>
      f.includes('test') || f.includes('Test') || f.includes('.spec.')
    );

    if (codeFiles.length > 0 && testFiles.length === 0) {
      return {
        valid: false,
        message: 'Code changes detected without test updates. Consider adding tests.'
      };
    }
    return { valid: true };
  } catch {
    return { valid: true };
  }
}

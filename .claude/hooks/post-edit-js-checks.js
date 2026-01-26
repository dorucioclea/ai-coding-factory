#!/usr/bin/env node
/**
 * Consolidated Post-Edit Checks for JS/TS files
 *
 * Combines: Prettier, TypeScript check, console.log warning
 * into a single hook to reduce process spawning.
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

let input = '';
process.stdin.on('data', (chunk) => input += chunk);
process.stdin.on('end', () => {
  try {
    const data = JSON.parse(input);
    const filePath = data.tool_input?.file_path;

    if (!filePath || !fs.existsSync(filePath)) {
      console.log(input);
      return;
    }

    const ext = path.extname(filePath);
    const isTS = ['.ts', '.tsx'].includes(ext);
    const isJS = ['.js', '.jsx'].includes(ext);

    if (!isTS && !isJS) {
      console.log(input);
      return;
    }

    // 1. Prettier (silent)
    try {
      execSync(`npx prettier --write "${filePath}"`, { stdio: ['pipe', 'pipe', 'pipe'] });
    } catch (e) {
      // Prettier not available or failed - continue
    }

    // 2. TypeScript check (TS files only)
    if (isTS) {
      let dir = path.dirname(filePath);
      while (dir !== path.dirname(dir) && !fs.existsSync(path.join(dir, 'tsconfig.json'))) {
        dir = path.dirname(dir);
      }
      if (fs.existsSync(path.join(dir, 'tsconfig.json'))) {
        try {
          execSync('npx tsc --noEmit --pretty false 2>&1', {
            cwd: dir,
            encoding: 'utf8',
            stdio: ['pipe', 'pipe', 'pipe']
          });
        } catch (e) {
          const lines = (e.stdout || '').split('\n')
            .filter(l => l.includes(filePath))
            .slice(0, 5);
          if (lines.length) {
            console.error('[Hook] TypeScript errors:');
            lines.forEach(l => console.error(l));
          }
        }
      }
    }

    // 3. console.log warning
    const content = fs.readFileSync(filePath, 'utf8');
    const lines = content.split('\n');
    const matches = [];
    lines.forEach((line, idx) => {
      if (/console\.log/.test(line)) {
        matches.push(`${idx + 1}: ${line.trim()}`);
      }
    });
    if (matches.length) {
      console.error(`[Hook] WARNING: console.log found in ${filePath}`);
      matches.slice(0, 3).forEach(m => console.error(m));
      console.error('[Hook] Remove before committing');
    }

  } catch (e) {
    // Silent fail
  }
  console.log(input);
});

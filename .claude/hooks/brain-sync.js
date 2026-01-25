#!/usr/bin/env node
/**
 * Brain Sync Hook - Auto-Memory System
 * Runs after every tool use, extracts AI memory to brain.jsonl
 *
 * @event PostToolUse
 */

const fs = require('fs');
const path = require('path');
const {
  findProjectRoot,
  getMaestroDir,
  getClaudeProjectsDir,
  normalizeProjectPath,
  ensureMaestroDir,
  loadState,
  saveState,
  isReadOnlyTool,
  logDebug,
  cleanAnsi,
  getTimestamp,
  readStdin,
  outputJson
} = require('./lib/utils');

const {
  readPreservedBrain,
  writeBrain,
  dedupe
} = require('./lib/brain');

const LOG_PREFIX = '[BRAIN-SYNC]';

const MAX_JSONL_SIZE = 50 * 1024 * 1024;
const STREAMING_THRESHOLD = 50 * 1024 * 1024;

/**
 * Detect active Claude CLI session from cwd.
 */
function getActiveSession(projectRoot) {
  logDebug(LOG_PREFIX, `Project root: ${projectRoot}`);

  try {
    if (fs.existsSync(projectRoot) && fs.lstatSync(projectRoot).isDirectory()) {
      const rootEntries = fs.readdirSync(projectRoot);
      const hasProjectFiles = rootEntries.some(e => !['.git', '.maestro', '.claude'].includes(e));
      if (!hasProjectFiles) {
        logDebug(LOG_PREFIX, 'Project directory empty (except meta) - treated as FRESH START. Skipping legacy session recovery.');
        return { sessionId: null, mainJsonl: null, subagentDir: null };
      }
    }

    const claudeProjectsDir = getClaudeProjectsDir();
    if (!fs.existsSync(claudeProjectsDir)) {
      return { sessionId: null, mainJsonl: null, subagentDir: null };
    }

    const cwdNormalized = normalizeProjectPath(projectRoot);
    let projectDir = null;

    const entries = fs.readdirSync(claudeProjectsDir);
    for (const entry of entries) {
      if (entry.toLowerCase() === cwdNormalized.toLowerCase()) {
        projectDir = path.join(claudeProjectsDir, entry);
        break;
      }
    }

    if (!projectDir) {
      for (const entry of entries) {
        const entryLower = entry.toLowerCase();
        const cwdLower = cwdNormalized.toLowerCase();
        if (cwdLower && (entryLower.startsWith(cwdLower) || cwdLower.startsWith(entryLower))) {
          projectDir = path.join(claudeProjectsDir, entry);
          break;
        }
      }
    }

    if (!projectDir) {
      return { sessionId: null, mainJsonl: null, subagentDir: null };
    }

    const jsonlFiles = fs.readdirSync(projectDir)
      .filter(f => f.endsWith('.jsonl'))
      .map(f => ({
        name: f,
        path: path.join(projectDir, f),
        mtime: fs.statSync(path.join(projectDir, f)).mtimeMs
      }))
      .sort((a, b) => b.mtime - a.mtime);

    if (jsonlFiles.length > 0) {
      const latestJsonl = jsonlFiles[0];
      const sessionId = latestJsonl.name.replace('.jsonl', '');
      const mainJsonl = latestJsonl.path;
      const subagentDir = path.join(projectDir, sessionId, 'subagents');

      logDebug(LOG_PREFIX, `Session: ${sessionId}`);
      return { sessionId, mainJsonl, subagentDir };
    }

    const indexFile = path.join(projectDir, 'sessions-index.json');
    if (fs.existsSync(indexFile)) {
      const indexData = JSON.parse(fs.readFileSync(indexFile, 'utf-8'));
      const sessions = indexData.entries || [];

      if (sessions.length > 0) {
        const latestSession = sessions.sort((a, b) =>
          (b.fileMtime || 0) - (a.fileMtime || 0)
        )[0];

        const sessionId = latestSession.sessionId;
        const subagentDir = path.join(projectDir, sessionId, 'subagents');
        const mainJsonl = path.join(projectDir, `${sessionId}.jsonl`);

        logDebug(LOG_PREFIX, `Session (from index): ${sessionId}`);
        return { sessionId, mainJsonl, subagentDir };
      }
    }

    return { sessionId: null, mainJsonl: null, subagentDir: null };

  } catch (err) {
    logDebug(LOG_PREFIX, `Session detection error: ${err.message}`);
    return { sessionId: null, mainJsonl: null, subagentDir: null };
  }
}

/**
 * Read JSONL file incrementally using offset tracking.
 */
function readJsonlIncremental(filePath, sessionId, callback) {
  const syncState = loadState('sync', findProjectRoot()) || {};
  const fileKey = `${sessionId}:${path.basename(filePath)}`;
  const startOffset = syncState[fileKey] || 0;

  const stats = fs.statSync(filePath);
  if (stats.size < startOffset) {
    logDebug(LOG_PREFIX, `File ${fileKey} shrunk, resetting offset`);
  }

  const currentStart = stats.size < startOffset ? 0 : startOffset;

  if (currentStart >= stats.size) {
    logDebug(LOG_PREFIX, `No new content in ${fileKey}`);
    return Promise.resolve();
  }

  logDebug(LOG_PREFIX, `Reading ${fileKey} from offset ${currentStart}`);

  const fd = fs.openSync(filePath, 'r');
  const bufferSize = 64 * 1024;
  const buffer = Buffer.alloc(bufferSize);
  let bytesRead;
  let leftover = '';
  let currentOffset = currentStart;

  while ((bytesRead = fs.readSync(fd, buffer, 0, bufferSize, currentOffset)) > 0) {
    currentOffset += bytesRead;
    const chunk = leftover + buffer.toString('utf-8', 0, bytesRead);
    const lines = chunk.split('\n');
    leftover = lines.pop();

    for (const line of lines) {
      if (line.trim()) {
        try {
          callback(JSON.parse(line));
        } catch (err) { }
      }
    }
  }

  fs.closeSync(fd);

  syncState[fileKey] = currentOffset;
  saveState('sync', syncState, findProjectRoot());

  return Promise.resolve();
}

/**
 * Process a single JSONL entry and extract relevant data.
 */
function processEntry(entry, data, toolIdToName = {}) {
  try {
    const entryType = entry.type;
    const timestamp = entry.timestamp || '';

    if (entryType === 'user') {
      const msgContent = entry.message?.content || [];
      if (Array.isArray(msgContent)) {
        for (const block of msgContent) {
          const blockType = block.type;
          const toolId = block.tool_use_id;
          const toolName = toolIdToName[toolId] || '';

          if (blockType === 'tool_result' && block.is_error) {
            let errorContent = cleanAnsi(block.content || '');
            data.errors.push({
              timestamp,
              error: errorContent,
              tool: toolName
            });
          }

          if (blockType === 'tool_result' && !block.is_error) {
            const currentToolName = toolName || block.name || '';
            if (!['run_command', 'browser_subagent', 'execute_python_code', 'Bash', 'Shell'].includes(currentToolName)) {
              continue;
            }

            const resultContent = cleanAnsi(block.content || '');
            const resultLower = resultContent.toLowerCase();

            const errorKeywords = [
              'exit code 127', 'exit code 1', 'exit code 2', 'exit code',
              'command not found', 'bash: command not found',
              'is not recognized as an internal or external command',
              'is not recognized as the name of a cmdlet',
              'failed to compile', 'build failed', 'compilation failed',
              'error:', 'typeerror:', 'syntaxerror:',
              'referenceerror:', 'cannot find module', 'module not found',
              'command failed', 'npm err!', 'npm error',
              'fatal:', 'exception:', 'traceback',
              'no such file or directory', 'permission denied',
              'access is denied', 'cannot find the path specified',
              'error: exit code', 'error exit code'
            ];

            if (errorKeywords.some(kw => resultLower.includes(kw))) {
              data.errors.push({
                timestamp,
                error: resultContent,
                tool: currentToolName
              });
            }
          }
        }
      }
    }

    if (entryType === 'assistant' || entryType === 'user') {
      const msgContent = entry.message?.content;
      const fullText = typeof msgContent === 'string' ? msgContent :
        (Array.isArray(msgContent) ? msgContent.map(b => b.text || '').join('') : '');
      const blocks = Array.isArray(msgContent) ? msgContent : [];

      for (const block of (blocks.length > 0 ? blocks : [{ type: 'text', text: fullText }])) {
        if (block.type === 'text' || typeof msgContent === 'string') {
          const text = block.text || (typeof msgContent === 'string' ? msgContent : '');
          if (!text) continue;

          const decisionIndicators = [
            'the user wants', 'user requested', 'requirements:',
            'architecture:', 'design decision:', 'chosen approach:',
            'will use', 'decided to', 'going with', 'selected',
            'because', 'reason:', 'rationale:', 'plan:', 'strategy:'
          ];
          const skipIndicators = [
            'Let me', 'Now let', "I will try", "I'll try",
            'First,', 'Next,', 'Then,', 'Now I'
          ];

          const textLower = text.toLowerCase();
          const isDecision = decisionIndicators.some(ind => textLower.includes(ind.toLowerCase()));
          const isTransient = skipIndicators.some(skip => text.startsWith(skip));

          const isSummary = (entry.isCompactSummary || entry.is_compact_summary || (
            text.length > 100 && (
              (text.includes('summary') && (text.includes('previous context') || text.includes('recent activity') || text.includes('compact') || text.includes('summarize'))) ||
              (text.includes('Primary Request') && text.includes('Key Technical')) ||
              (text.includes('This session is being continued from a previous conversation')) ||
              (text.includes('The following is a compact summary of our previous conversation'))
            )
          )) && !text.includes('<local-command-stdout>');

          if (isSummary) {
            data.decisions.push({
              timestamp,
              decision: `AUTO-SUMMARY: ${text.trim().replace(/\r\n/g, ' ').replace(/\n/g, ' ')}`
            });
          } else if (entryType === 'assistant' && isDecision && !isTransient && text.length > 30) {
            const sentences = text.split(/[.!?]\s+/);
            for (const sentence of sentences) {
              const sentLower = sentence.toLowerCase();
              if (decisionIndicators.some(ind => sentLower.includes(ind.toLowerCase()))) {
                if (sentence.trim().length > 20) {
                  data.decisions.push({
                    timestamp,
                    decision: sentence.trim().substring(0, 500)
                  });
                  break;
                }
              }
            }
          }
        }

        if (block.type === 'tool_use' && block.name === 'TodoWrite') {
          const todos = block.input?.todos || [];
          data.tasks.push({
            timestamp,
            todos
          });
        }

        if (block.type === 'tool_use') {
          const toolName = block.name || '';
          const toolInput = block.input || {};

          if (['Edit', 'StrReplace', 'replace_file_content', 'multi_replace_file_content', 'search_replace'].includes(toolName)) {
            const filePath = toolInput.file_path || toolInput.path || toolInput.AbsolutePath || toolInput.TargetFile || '';
            if (filePath) {
              let relPath = filePath;
              try {
                const projectRoot = findProjectRoot();
                relPath = path.relative(projectRoot, filePath);
                if (relPath.startsWith('.\\') || relPath.startsWith('./')) {
                  relPath = relPath.substring(2);
                }
              } catch (err) {
                relPath = path.basename(filePath);
              }

              const description = toolInput.Instruction || toolInput.Description ||
                (toolInput.old_string ? 'Modified content' : 'Edited');
              data.fileChanges.push({
                timestamp,
                file: relPath,
                action: 'edit',
                description: description.substring(0, 100)
              });
            }
          }

          if (['Write', 'write_to_file', 'write'].includes(toolName)) {
            const filePath = toolInput.file_path || toolInput.path || toolInput.AbsolutePath || '';
            if (filePath) {
              let relPath = filePath;
              try {
                const projectRoot = findProjectRoot();
                relPath = path.relative(projectRoot, filePath);
                if (relPath.startsWith('.\\') || relPath.startsWith('./')) {
                  relPath = relPath.substring(2);
                }
              } catch (err) {
                relPath = path.basename(filePath);
              }

              const description = toolInput.contents ? 'Created file' : 'Created';
              data.fileChanges.push({
                timestamp,
                file: relPath,
                action: 'create',
                description: description.substring(0, 100)
              });
            }
          }
        }
      }
    }
  } catch (err) {
    // Skip invalid entries
  }
}

/**
 * Extract brain data from JSONL files.
 */
async function extractBrainData(sessionId, mainJsonl, subagentDir) {
  const data = {
    tasks: [],
    decisions: [],
    errors: [],
    fileChanges: [],
    thinking: []
  };

  try {
    if (mainJsonl && fs.existsSync(mainJsonl)) {
      logDebug(LOG_PREFIX, `Extracting from main session: ${path.basename(mainJsonl)}`);
      await readJsonlIncremental(mainJsonl, sessionId, (entry) => {
        processEntry(entry, data, {});
      });
    }

    if (subagentDir && fs.existsSync(subagentDir)) {
      const subFiles = fs.readdirSync(subagentDir)
        .filter(f => f.endsWith('.jsonl'));

      const toolIdToName = {};
      for (const subFile of subFiles) {
        const subPath = path.join(subagentDir, subFile);
        if (!fs.existsSync(subPath)) continue;

        await readJsonlIncremental(subPath, sessionId, (entry) => {
          if (entry.type === 'assistant') {
            const msgContent = entry.message?.content || [];
            if (Array.isArray(msgContent)) {
              for (const block of msgContent) {
                if (block.type === 'tool_use') {
                  toolIdToName[block.id] = block.name;
                }
              }
            }
          }
        });
      }

      for (const subFile of subFiles) {
        const subPath = path.join(subagentDir, subFile);
        if (!fs.existsSync(subPath)) continue;

        await readJsonlIncremental(subPath, sessionId, (entry) => {
          processEntry(entry, data, toolIdToName);
        });
      }
    }

  } catch (err) {
    logDebug(LOG_PREFIX, `Extraction error: ${err.message}`);
  }

  return data;
}

/**
 * Compress verbose brain data into compact context summary.
 */
function compressToContext(data) {
  const context = {
    projectInfo: null,
    lastStatus: null,
    completed: [],
    inProgress: [],
    pending: [],
    blockers: [],
    keyDecisions: [],
    compactSummaries: [],
    errors: [],
    lastError: null
  };

  if (data.thinking.length > 0) {
    const thoughtsToCheck = [
      ...data.thinking.slice(0, 3),
      ...data.thinking.slice(-3)
    ];

    const indicators = [
      'next.js', 'react', 'vue', 'angular', 'express', 'django', 'flask',
      'kindle', 'e-reader', 'dashboard', 'landing', 'api', 'extension',
      'news', 'blog', 'portfolio', 'ecommerce', 'app', 'tool', 'script',
      'dotnet', 'aspnet', 'clean architecture', 'cqrs', 'expo', 'react native'
    ];

    const foundIndicators = [];
    for (const t of thoughtsToCheck) {
      const thought = t.thought.toLowerCase();
      for (const ind of indicators) {
        if (thought.includes(ind) && !foundIndicators.includes(ind)) {
          foundIndicators.push(ind);
        }
      }
    }

    if (foundIndicators.length > 0) {
      context.projectInfo = foundIndicators.slice(0, 3).join(' | ');
    }
  }

  if (data.tasks.length > 0) {
    const latestTasks = data.tasks[data.tasks.length - 1].todos || [];
    for (const todo of latestTasks) {
      const status = todo.status || 'pending';
      const content = (todo.content || '').trim();
      if (!content) continue;

      if (status === 'done' || status === 'completed') {
        context.completed.push(content);
      } else if (status === 'in_progress') {
        context.inProgress.push(content);
      } else {
        context.pending.push(content);
      }
    }
  }

  const seenErrors = new Set();
  for (const errItem of data.errors) {
    const errText = String(errItem.error || '').trim();
    const toolName = errItem.tool || '';

    if (errText.length < 5 || errText.startsWith('---') || errText.startsWith('import ')) {
      continue;
    }

    let errKey = null;

    if (errText.toLowerCase().includes('error:')) {
      for (const line of errText.split('\n')) {
        if (line.toLowerCase().includes('error') && line.includes(':')) {
          errKey = line.trim().substring(0, 500);
          break;
        }
      }
    }

    if (!errKey && (errText.toLowerCase().includes('exit code') ||
      errText.toLowerCase().includes('command not found') ||
      errText.toLowerCase().includes('bash:') ||
      errText.toLowerCase().includes('error: exit code'))) {
      const lines = errText.split('\n').filter(l => l.trim());
      const junkPatterns = ['starting', 'running', 'inspecting', '...', '---'];

      const meaningfulLines = lines.filter(line => {
        const lineLower = line.toLowerCase();
        const isJunk = junkPatterns.some(jp => lineLower.includes(jp));
        const hasError = ['error', 'fail', 'exception', 'invalid', 'not found', 'denied']
          .some(ew => lineLower.includes(ew));
        return !isJunk || hasError;
      });

      if (meaningfulLines.length > 0) {
        const captured = meaningfulLines.length > 5
          ? [...meaningfulLines.slice(0, 1), '...', ...meaningfulLines.slice(-4)]
          : meaningfulLines;
        errKey = captured.join(' | ').substring(0, 600);
      } else {
        errKey = errText.split('\n').pop()?.substring(0, 500);
      }
    }

    if (!errKey && errText.toLowerCase().includes('failed')) {
      errKey = errText.split('\n')[0]?.substring(0, 500);
    }

    if (errKey && !seenErrors.has(errKey)) {
      seenErrors.add(errKey);
      context.errors.push(toolName ? `[${toolName}] ${errKey}` : errKey);
    }
  }

  if (context.errors.length > 0) {
    context.lastError = context.errors[context.errors.length - 1];
    context.blockers.push(context.lastError);
  }

  for (const dec of data.decisions) {
    const decisionText = dec.decision || '';
    if (decisionText.startsWith('AUTO-SUMMARY:')) {
      context.compactSummaries.push(decisionText.substring(13).trim());
    } else {
      context.keyDecisions.push(decisionText.trim().substring(0, 1000));
    }
  }

  return context;
}

/**
 * Write brain.jsonl with consolidated data.
 */
function writeBrainJsonl(sessionId, data, projectRoot) {
  try {
    const preserved = readPreservedBrain(projectRoot);
    const context = compressToContext(data);

    const entries = [];

    entries.push(...preserved.tech);

    const allCompactsRaw = [...preserved.compacts, ...context.compactSummaries.map(s => ({ type: 'compact', summary: s, ts: getTimestamp() }))];
    const uniqueCompacts = [];
    const seenCompacts = new Set();
    for (const e of allCompactsRaw) {
      const txt = (e.summary || e.content || '').trim();
      if (txt && !seenCompacts.has(txt)) {
        seenCompacts.add(txt);
        uniqueCompacts.push(e);
      }
    }
    entries.push(...uniqueCompacts.slice(-10));

    const allGoalsRaw = [...preserved.goals];
    if (context.projectInfo) {
      const exists = allGoalsRaw.some(e => (e.content || '') === context.projectInfo);
      if (!exists) {
        allGoalsRaw.push({ type: 'goal', content: context.projectInfo, ts: getTimestamp() });
      }
    }
    entries.push(...allGoalsRaw.slice(-20));

    const allDecisionsRaw = [...preserved.decisions];
    for (const dec of context.keyDecisions) {
      const exists = allDecisionsRaw.some(e => (e.content || e.decision || '') === dec);
      if (!exists) {
        allDecisionsRaw.push({ type: 'decision', content: dec, session: sessionId });
      }
    }
    entries.push(...allDecisionsRaw.slice(-30));

    const allCompletedRaw = [...preserved.completed];
    for (const comp of context.completed) {
      const exists = allCompletedRaw.some(e => (e.content || '') === comp);
      if (!exists) {
        allCompletedRaw.push({ type: 'completed', content: comp });
      }
    }
    entries.push(...allCompletedRaw.slice(-30));

    const allErrorsRaw = [...preserved.errors];
    for (const err of [...context.errors, ...context.blockers]) {
      const exists = allErrorsRaw.some(e => (e.content || e.error || '') === err);
      if (!exists) {
        allErrorsRaw.push({ type: 'error', content: err });
      }
    }
    entries.push(...allErrorsRaw.slice(-20));

    entries.push(...preserved.others);

    writeBrain(entries, projectRoot);
    logDebug(LOG_PREFIX, `Updated LTM: ${entries.length} entries`);

  } catch (err) {
    logDebug(LOG_PREFIX, `Write error: ${err.message}`);
  }
}

/**
 * Extract errors from hook input (stdin).
 */
function extractErrorsFromHookInput(hookInput) {
  const errors = [];

  try {
    if (!hookInput || typeof hookInput !== 'object') {
      return errors;
    }

    const toolResult = hookInput.toolResult || hookInput.result || hookInput;
    const toolName = toolResult.tool_name || toolResult.toolName ||
      hookInput.toolName || hookInput.tool || 'unknown';

    let content = '';
    if (toolResult.content) {
      content = String(toolResult.content);
    } else if (toolResult.output) {
      content = String(toolResult.output);
    } else if (toolResult.stderr) {
      content = String(toolResult.stderr);
    } else if (hookInput.content) {
      content = String(hookInput.content);
    } else if (typeof toolResult === 'string') {
      content = toolResult;
    }

    content = cleanAnsi(content).trim();

    const isError = toolResult.is_error || toolResult.isError ||
      hookInput.isError || false;

    const contentLower = content.toLowerCase();
    const errorKeywords = [
      'exit code 127', 'exit code 1', 'exit code 2', 'exit code',
      'command not found', 'bash: command not found',
      'is not recognized as an internal or external command',
      'is not recognized as the name of a cmdlet',
      'failed to compile', 'build failed', 'compilation failed',
      'error:', 'typeerror:', 'syntaxerror:',
      'referenceerror:', 'cannot find module', 'module not found',
      'command failed', 'npm err!', 'npm error',
      'fatal:', 'exception:', 'traceback',
      'no such file or directory', 'permission denied',
      'access is denied', 'cannot find the path specified',
      'error: exit code', 'error exit code'
    ];

    const hasError = isError || errorKeywords.some(kw => contentLower.includes(kw));

    if (hasError && content.length > 5) {
      errors.push({
        timestamp: getTimestamp(),
        error: content.substring(0, 1000),
        tool: toolName
      });
      logDebug(LOG_PREFIX, `Captured error from hook input: ${toolName} - ${content.substring(0, 100)}`);
    }

  } catch (err) {
    logDebug(LOG_PREFIX, `Error extracting from hook input: ${err.message}`);
  }

  return errors;
}

/**
 * Write errors directly to brain.jsonl (fallback when session not found).
 */
function writeErrorsToBrain(errors, projectRoot) {
  if (errors.length === 0) {
    return;
  }

  try {
    const preserved = readPreservedBrain(projectRoot);
    const entries = [];

    for (const techEntry of preserved.techEntries || []) {
      entries.push(techEntry);
    }

    const existingErrors = preserved.errors || [];
    const allErrors = [...existingErrors];

    for (const err of errors) {
      const errText = err.error || '';
      const toolName = err.tool || '';
      const errorEntry = toolName ? `[${toolName}] ${errText}` : errText;

      if (errText.length > 10 && !allErrors.includes(errorEntry)) {
        allErrors.push(errorEntry);
      }
    }

    for (const err of allErrors.slice(-20)) {
      entries.push({
        type: 'error',
        content: err,
        ts: getTimestamp()
      });
    }

    writeBrain(entries, projectRoot);
    logDebug(LOG_PREFIX, `Wrote ${errors.length} errors to brain.jsonl (fallback)`);

  } catch (err) {
    logDebug(LOG_PREFIX, `Error writing errors: ${err.message}`);
  }
}

/**
 * Main hook entry point.
 */
async function main() {
  const projectRoot = findProjectRoot();

  try {
    const hookInput = await readStdin();
    logDebug(LOG_PREFIX, `Hook input keys: ${Object.keys(hookInput || {}).join(', ')}`);
    const eventName = hookInput.hook_event_name || hookInput.hookEventName || '';
    const toolName = hookInput.toolName || (hookInput.toolResult && hookInput.toolResult.tool_name) || '';

    logDebug(LOG_PREFIX, `Event: ${eventName}, Tool: ${toolName}`);

    if (toolName && isReadOnlyTool(toolName)) {
      logDebug(LOG_PREFIX, `Short-circuit: Skipping sync for read-only tool: ${toolName}`);
      outputJson({});
      return;
    }

    const immediateErrors = extractErrorsFromHookInput(hookInput);
    if (immediateErrors.length > 0) {
      logDebug(LOG_PREFIX, `Found ${immediateErrors.length} immediate errors`);
      writeErrorsToBrain(immediateErrors, projectRoot);
    }

    const { sessionId, mainJsonl, subagentDir } = getActiveSession(projectRoot);

    if (!sessionId) {
      if (immediateErrors.length > 0) {
        logDebug(LOG_PREFIX, 'Session not found, but errors written');
      }
      outputJson({});
      return;
    }

    const priorityEvents = ['UserPromptSubmit', 'Stop', 'SubagentStop', 'PreCompact', 'SessionStart'];
    const state = loadState('brain-sync', projectRoot) || {};
    const now = Date.now();
    const isPriorityEvent = priorityEvents.includes(eventName) || immediateErrors.length > 0;

    if (!isPriorityEvent && state.lastSync && (now - state.lastSync < 30000)) {
      logDebug(LOG_PREFIX, `Throttling sync (last one was < 30s ago). Event: ${eventName}`);
      outputJson({});
      return;
    }

    let data;
    if (eventName === 'Stop' || eventName === 'PreCompact') {
      logDebug(LOG_PREFIX, 'Stop/PreCompact event: using retry logic for transcript flush');
      for (let attempt = 1; attempt <= 3; attempt++) {
        data = await extractBrainData(sessionId, mainJsonl, subagentDir);
        if (data.compact && data.compact.length > 0) break;
        if (attempt < 3) await new Promise(r => setTimeout(r, 1000));
      }
    } else {
      data = await extractBrainData(sessionId, mainJsonl, subagentDir);
    }

    if (immediateErrors.length > 0) {
      data.errors = [...immediateErrors, ...(data.errors || [])];
    }

    writeBrainJsonl(sessionId, data, projectRoot);

    saveState('brain-sync', { lastSync: now, lastSessionId: sessionId }, projectRoot);

    outputJson({});

  } catch (err) {
    logDebug(LOG_PREFIX, `Hook error: ${err.message}`);
    logDebug(LOG_PREFIX, `Stack: ${err.stack}`);
    outputJson({});
  }
}

main();

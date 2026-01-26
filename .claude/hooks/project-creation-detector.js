#!/usr/bin/env node
/**
 * Project Creation Detector Hook
 *
 * Detects project creation intent in user messages and suggests
 * invoking the spec-driven-development skill automatically.
 *
 * Runs on: user-prompt-submit (before Claude processes the message)
 */

let input = '';
process.stdin.on('data', (chunk) => input += chunk);
process.stdin.on('end', () => {
  try {
    const data = JSON.parse(input);
    const userMessage = data.user_prompt || data.message || '';

    if (detectProjectCreationIntent(userMessage)) {
      console.error('\n[Hook] 🏗️  PROJECT CREATION DETECTED');
      console.error('[Hook] Auto-activating spec-driven-development workflow');
      console.error('[Hook] → Will use templates: clean-architecture-solution, react-frontend-template');
      console.error('[Hook] → Will ask max 3 clarifying questions');
      console.error('[Hook] → Will generate specification and implementation plan\n');
    }
  } catch (e) {
    // Silent fail - don't block on parse errors
  }
  console.log(input);
});

function detectProjectCreationIntent(message) {
  if (!message || typeof message !== 'string') return false;

  const lowerMessage = message.toLowerCase();

  // Direct trigger phrases
  const triggerPhrases = [
    'i want to build',
    'build me a',
    'create an app',
    'create a app',
    'i need a website',
    'i need a web app',
    'i need an app',
    'make me a',
    'i have an idea for',
    "let's build",
    'lets build',
    'help me create',
    'help me build',
    'i want to create',
    'i want to make',
    'build a platform',
    'create a platform',
    'develop an app',
    'develop a system',
    'new project for',
    'start a project'
  ];

  for (const phrase of triggerPhrases) {
    if (lowerMessage.includes(phrase)) {
      return true;
    }
  }

  // Pattern: "<verb> a/an <noun> for <purpose>"
  // e.g., "build a website for restaurants", "create an app for tracking"
  const buildPattern = /\b(build|create|make|develop|design)\s+(a|an)\s+\w+\s+(for|to|that|where|which)/i;
  if (buildPattern.test(message)) {
    return true;
  }

  // Pattern: "platform for <domain>"
  const platformPattern = /\bplatform\s+for\s+\w+/i;
  if (platformPattern.test(message)) {
    return true;
  }

  return false;
}

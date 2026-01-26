#!/usr/bin/env node
/**
 * Post-Edit Collaboration Hook
 *
 * Detects file patterns that require cross-cutting collaboration
 * and suggests appropriate reviewers based on orchestrator.yaml patterns.
 */

const fs = require('fs');
const path = require('path');

// Cross-cutting collaboration patterns
const COLLABORATIONS = [
  {
    patterns: [/\/auth\//i, /Auth\.(cs|ts|tsx)$/i, /authentication/i, /authorization/i],
    skill: 'authentication',
    owner: 'Developer',
    collaborators: ['Security Reviewer'],
    reason: 'Auth code requires security review',
    priority: 'HIGH'
  },
  {
    patterns: [/\/Controllers\//i, /\/api\//i, /Endpoints?\.(cs|ts)$/i],
    skill: 'api-implementation',
    owner: 'Developer',
    collaborators: ['Security Reviewer', 'Doc Updater'],
    reason: 'APIs need security review and documentation',
    priority: 'MEDIUM'
  },
  {
    patterns: [/\/Domain\//i],
    skill: 'domain-changes',
    owner: 'Developer',
    collaborators: ['Architect'],
    reason: 'Domain changes need architecture review',
    priority: 'HIGH'
  },
  {
    patterns: [/\/migrations?\//i, /\.sql$/i, /schema/i],
    skill: 'database-changes',
    owner: 'Developer',
    collaborators: ['Architect', 'Security Reviewer'],
    reason: 'Schema changes affect architecture and security',
    priority: 'HIGH'
  },
  {
    patterns: [/Token/i, /Credential/i, /Secret/i, /Password/i],
    skill: 'security-sensitive',
    owner: 'Developer',
    collaborators: ['Security Reviewer'],
    reason: 'Security-sensitive code detected',
    priority: 'CRITICAL'
  },
  {
    patterns: [/\.tsx$/i, /components?\//i, /screens?\//i],
    skill: 'mobile-feature',
    owner: 'RN Developer',
    collaborators: ['RN State Architect', 'RN A11y Enforcer'],
    reason: 'Mobile components need state and accessibility review',
    priority: 'MEDIUM',
    condition: (filePath) => filePath.includes('react-native') || filePath.includes('mobile')
  },
  {
    patterns: [/Infrastructure\//i, /Repository/i, /DbContext/i],
    skill: 'infrastructure',
    owner: 'Developer',
    collaborators: ['Architect'],
    reason: 'Infrastructure changes may affect persistence layer',
    priority: 'MEDIUM'
  }
];

// Read stdin for hook context
let input = '';
process.stdin.on('data', (chunk) => input += chunk);
process.stdin.on('end', () => {
  try {
    const data = JSON.parse(input);
    const filePath = data.tool_input?.file_path || '';

    if (filePath) {
      checkCollaborations(filePath);
    }
  } catch (e) {
    // Silent fail - don't interrupt workflow
  }

  // Always output the input to continue the chain
  console.log(input);
});

function checkCollaborations(filePath) {
  const triggered = [];

  for (const collab of COLLABORATIONS) {
    // Check if any pattern matches
    const matches = collab.patterns.some(pattern => pattern.test(filePath));

    if (!matches) continue;

    // Check additional condition if present
    if (collab.condition && !collab.condition(filePath)) continue;

    triggered.push(collab);
  }

  if (triggered.length === 0) return;

  // Sort by priority
  const priorityOrder = { 'CRITICAL': 0, 'HIGH': 1, 'MEDIUM': 2, 'LOW': 3 };
  triggered.sort((a, b) => priorityOrder[a.priority] - priorityOrder[b.priority]);

  // Output collaboration suggestions
  console.error('\n[Collaboration] 🤝 Cross-cutting review suggested:');

  for (const collab of triggered) {
    const priorityColor = collab.priority === 'CRITICAL' ? '\x1b[31m' :
                          collab.priority === 'HIGH' ? '\x1b[33m' : '\x1b[36m';
    const reset = '\x1b[0m';

    console.error(`  ${priorityColor}[${collab.priority}]${reset} ${collab.skill}`);
    console.error(`    Collaborators: ${collab.collaborators.join(', ')}`);
    console.error(`    Reason: ${collab.reason}`);
  }

  console.error('');
  console.error('[Collaboration] Consider running:');

  // Suggest specific commands based on triggered collaborations
  const hasSecurityCollab = triggered.some(t => t.collaborators.includes('Security Reviewer'));
  const hasArchCollab = triggered.some(t => t.collaborators.includes('Architect'));

  if (hasSecurityCollab) {
    console.error('  /security-review - Run security review on changes');
  }
  if (hasArchCollab) {
    console.error('  /code-review - Get architecture feedback');
  }

  console.error('');
}

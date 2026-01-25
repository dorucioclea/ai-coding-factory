/**
 * Adapter Registry
 *
 * Exports all system adapters and provides a factory function
 * for getting adapters by system ID.
 */

export * from "./base.js";
export { ClaudeAdapter } from "./claude.js";
export { OpenCodeAdapter } from "./opencode.js";
export { CursorAdapter } from "./cursor.js";
export { CodexAdapter } from "./codex.js";
export { WindsurfAdapter } from "./windsurf.js";

import type { SystemId } from "../models/types.js";
import type { SystemAdapter } from "./base.js";
import { ClaudeAdapter } from "./claude.js";
import { OpenCodeAdapter } from "./opencode.js";
import { CursorAdapter } from "./cursor.js";
import { CodexAdapter } from "./codex.js";
import { WindsurfAdapter } from "./windsurf.js";

/**
 * Registry of all available adapters
 */
const adapters: Record<SystemId, () => SystemAdapter> = {
  claude: () => new ClaudeAdapter(),
  opencode: () => new OpenCodeAdapter(),
  cursor: () => new CursorAdapter(),
  codex: () => new CodexAdapter(),
  windsurf: () => new WindsurfAdapter(),
  aider: () => {
    throw new Error("Aider adapter not yet implemented");
  },
  gemini: () => {
    throw new Error("Gemini adapter not yet implemented");
  },
  continue: () => {
    throw new Error("Continue adapter not yet implemented");
  },
  cody: () => {
    throw new Error("Cody adapter not yet implemented");
  },
};

/**
 * Get an adapter instance by system ID
 */
export function getAdapter(systemId: SystemId): SystemAdapter {
  const factory = adapters[systemId];
  if (!factory) {
    throw new Error(`Unknown system: ${systemId}`);
  }
  return factory();
}

/**
 * Get all available adapter system IDs
 */
export function getAvailableAdapters(): SystemId[] {
  return Object.keys(adapters).filter((id) => {
    try {
      adapters[id as SystemId]();
      return true;
    } catch {
      return false;
    }
  }) as SystemId[];
}

/**
 * Check if a system adapter is available
 */
export function isAdapterAvailable(systemId: SystemId): boolean {
  try {
    adapters[systemId]();
    return true;
  } catch {
    return false;
  }
}

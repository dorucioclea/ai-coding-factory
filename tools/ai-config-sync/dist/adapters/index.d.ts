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
/**
 * Get an adapter instance by system ID
 */
export declare function getAdapter(systemId: SystemId): SystemAdapter;
/**
 * Get all available adapter system IDs
 */
export declare function getAvailableAdapters(): SystemId[];
/**
 * Check if a system adapter is available
 */
export declare function isAdapterAvailable(systemId: SystemId): boolean;
//# sourceMappingURL=index.d.ts.map
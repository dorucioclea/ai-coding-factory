/**
 * Init Command
 *
 * Initialize AI config sync for a project.
 */
export interface InitCommandOptions {
    systems?: string[];
    force?: boolean;
}
export declare function initCommand(projectRoot: string, options: InitCommandOptions): Promise<void>;
//# sourceMappingURL=init.d.ts.map
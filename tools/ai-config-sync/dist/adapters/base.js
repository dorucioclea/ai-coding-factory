/**
 * Base Adapter Interface for AI Config Sync
 *
 * Defines the contract that all system adapters must implement.
 */
/**
 * Abstract base class with common functionality
 */
export class BaseAdapter {
    /**
     * Default transformation (identity)
     */
    transformArtifact(artifact, _options) {
        return artifact;
    }
    /**
     * Default validation
     */
    validateArtifact(artifact) {
        const errors = [];
        if (!artifact.id) {
            errors.push("Artifact ID is required");
        }
        if (!artifact.name) {
            errors.push("Artifact name is required");
        }
        if (!artifact.content) {
            errors.push("Artifact content is required");
        }
        return { valid: errors.length === 0, errors };
    }
    /**
     * Generate checksum for content
     */
    generateChecksum(content) {
        let hash = 0;
        for (let i = 0; i < content.length; i++) {
            const char = content.charCodeAt(i);
            hash = (hash << 5) - hash + char;
            hash = hash & hash;
        }
        return Math.abs(hash).toString(16).padStart(8, "0");
    }
    /**
     * Parse YAML frontmatter from markdown content
     */
    parseYamlFrontmatter(content) {
        const frontmatterRegex = /^---\n([\s\S]*?)\n---\n([\s\S]*)$/;
        const match = content.match(frontmatterRegex);
        if (!match) {
            return { frontmatter: {}, body: content };
        }
        try {
            // Simple YAML parsing for common cases
            const frontmatter = {};
            const lines = match[1].split("\n");
            for (const line of lines) {
                const colonIndex = line.indexOf(":");
                if (colonIndex > 0) {
                    const key = line.slice(0, colonIndex).trim();
                    let value = line.slice(colonIndex + 1).trim();
                    // Remove quotes
                    if ((value.startsWith('"') && value.endsWith('"')) ||
                        (value.startsWith("'") && value.endsWith("'"))) {
                        value = value.slice(1, -1);
                    }
                    // Parse arrays
                    if (value.startsWith("[") && value.endsWith("]")) {
                        frontmatter[key] = value
                            .slice(1, -1)
                            .split(",")
                            .map((s) => s.trim().replace(/['"]/g, ""));
                    }
                    else if (value === "true") {
                        frontmatter[key] = true;
                    }
                    else if (value === "false") {
                        frontmatter[key] = false;
                    }
                    else if (!isNaN(Number(value)) && value !== "") {
                        frontmatter[key] = Number(value);
                    }
                    else {
                        frontmatter[key] = value;
                    }
                }
            }
            return { frontmatter, body: match[2] };
        }
        catch {
            return { frontmatter: {}, body: content };
        }
    }
    /**
     * Generate YAML frontmatter string
     */
    generateYamlFrontmatter(data) {
        const lines = ["---"];
        for (const [key, value] of Object.entries(data)) {
            if (value === undefined || value === null)
                continue;
            if (Array.isArray(value)) {
                lines.push(`${key}: [${value.map((v) => `"${v}"`).join(", ")}]`);
            }
            else if (typeof value === "string") {
                lines.push(`${key}: "${value}"`);
            }
            else if (typeof value === "boolean" || typeof value === "number") {
                lines.push(`${key}: ${value}`);
            }
        }
        lines.push("---");
        return lines.join("\n");
    }
}
//# sourceMappingURL=base.js.map
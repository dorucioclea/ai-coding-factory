# /checkpoint - Save Verification State

Save the current state as a checkpoint for later recovery or reference.

## Usage

```
/checkpoint                      # Auto-named checkpoint
/checkpoint before-refactor      # Named checkpoint
/checkpoint ACF-042-milestone-1  # Story-linked checkpoint
```

## What Gets Saved

1. **Git State**:
   - Current branch name
   - Last commit hash
   - Uncommitted changes (stashed)

2. **Test State**:
   - Test results snapshot
   - Coverage percentage
   - Passing/failing test counts

3. **Session Context**:
   - Current working files
   - Recent commands
   - Active story ID

## Checkpoint Actions

### Create Checkpoint
```bash
# Stash uncommitted changes with checkpoint name
git stash push -m "checkpoint: $CHECKPOINT_NAME"

# Record state to checkpoint file
echo "branch: $(git branch --show-current)" >> .checkpoints/$CHECKPOINT_NAME
echo "commit: $(git rev-parse HEAD)" >> .checkpoints/$CHECKPOINT_NAME
echo "timestamp: $(date -Iseconds)" >> .checkpoints/$CHECKPOINT_NAME
```

### List Checkpoints
```bash
git stash list | grep "checkpoint:"
ls -la .checkpoints/
```

### Restore Checkpoint
```bash
# Find and restore stash
git stash list | grep "$CHECKPOINT_NAME"
git stash pop stash@{N}

# Or reset to checkpoint commit
git reset --hard $CHECKPOINT_COMMIT
```

## Checkpoint File Format

```yaml
# .checkpoints/ACF-042-milestone-1
checkpoint_id: chk-20240115-1430
name: ACF-042-milestone-1
created: 2024-01-15T14:30:00Z
branch: feature/ACF-042-user-auth
commit: abc123def456
stash_ref: stash@{0}
test_results:
  passed: 142
  failed: 0
  skipped: 3
coverage: 85.2
notes: "Completed authentication flow, starting authorization"
```

## When to Checkpoint

- Before risky refactoring
- After completing a logical unit of work
- Before merging or rebasing
- After tests pass for a milestone
- Before trying experimental approaches

## Related Commands

- `/verify` - Run verification before checkpointing
- `/tdd` - TDD workflow creates checkpoints automatically
- `/release` - Creates release checkpoint

## Recovery

If something goes wrong:
```bash
# List available checkpoints
/checkpoint --list

# Restore specific checkpoint
/checkpoint --restore ACF-042-milestone-1
```

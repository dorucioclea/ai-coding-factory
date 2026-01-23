#!/bin/bash
# Hook: Recommend tmux for dev servers
# Runs before: npm run dev, pnpm dev, yarn dev, bun run dev

if [ -z "$TMUX" ]; then
    echo "[Hook] Consider running dev server in tmux for session persistence" >&2
    echo "[Hook] tmux new -s dev  |  tmux attach -t dev" >&2
fi

exit 0

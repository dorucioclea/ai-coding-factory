---
name: tob-not-so-smart-contracts-scanners
description: Trail of Bits vulnerability scanners for smart contract platforms - specialized security scanning for Algorand, Cairo, Cosmos, Solana, Substrate, and TON blockchains
license: MIT
compatibility: opencode
metadata:
  audience: security-engineers
  category: security-scanning
---

# TOB Not-So-Smart-Contracts Scanners

A collection of Trail of Bits vulnerability scanners for various smart contract platforms and blockchains.

## Sub-Skills

This skill contains specialized vulnerability scanners for different blockchain platforms:

| Sub-Skill | Platform | Purpose |
|-----------|----------|---------|
| [algorand-vulnerability-scanner](algorand-vulnerability-scanner/skill.md) | Algorand | TEAL/PyTeal vulnerability scanning |
| [cairo-vulnerability-scanner](cairo-vulnerability-scanner/skill.md) | StarkNet | Cairo contract vulnerability detection |
| [cosmos-vulnerability-scanner](cosmos-vulnerability-scanner/skill.md) | Cosmos | CosmWasm/Cosmos SDK scanning |
| [solana-vulnerability-scanner](solana-vulnerability-scanner/skill.md) | Solana | Anchor/Solana program analysis |
| [substrate-vulnerability-scanner](substrate-vulnerability-scanner/skill.md) | Substrate | ink!/Substrate runtime scanning |
| [ton-vulnerability-scanner](ton-vulnerability-scanner/skill.md) | TON | FunC/TON contract analysis |

## When to Use

Use these scanners when:
- Auditing smart contracts on specific blockchain platforms
- Looking for platform-specific vulnerability patterns
- Performing security assessments on DeFi protocols
- Reviewing cross-chain applications

## Usage

Invoke the specific scanner based on your target blockchain:

```
# For Solana programs
Use solana-vulnerability-scanner skill

# For Cairo contracts
Use cairo-vulnerability-scanner skill

# For Cosmos/CosmWasm
Use cosmos-vulnerability-scanner skill
```

## Framework

Based on Trail of Bits' Not-So-Smart-Contracts repository and blockchain-specific security research.

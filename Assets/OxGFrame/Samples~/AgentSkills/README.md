# OxGFrame AI Agent Skill

An Agent Skill for AI coding agents (Claude Code, OpenAI Codex CLI, Cursor, Copilot, etc.) that teaches the agent how to develop Unity projects with **OxGFrame**.

The skill is a folder with a `SKILL.md` entry file (YAML frontmatter + instructions) and a `references/` folder with detailed, source-verified API usage documents. The format follows the Agent Skills convention (progressive disclosure: the agent reads `SKILL.md` first, then loads only the references it needs).

## What is included

| Skill | Purpose |
|---|---|
| `oxgframe-unity-skill` | OxGFrame framework usage: AssetLoader (patch/download/load), CoreFrame (UI/SR/US/CP), MediaFrame (Audio/Video), GSIFrame (game stages), NetFrame, CenterFrame (API/Event), Hotfixer (HybridCLR), plus coding style and production architecture guidance. All API usage is verified against the OxGFrame source and the official Samples. |

OxGFrame uses several OxGKit utility APIs internally (LoggingSystem, SaverSystem, TimeSystem, Utilities, ButtonSystem) — the skill briefly covers those. Dedicated skills for the individual OxGKit systems are provided separately by the OxGKit project (`https://github.com/michael811125/OxGKit`).

## How to install

1. Import this sample through **Package Manager > OxGFrame > Samples > AI Agent Skills**.
   The files are copied to `Assets/Samples/OxGFrame/<version>/AI Agent Skills/`.
2. Copy (or symlink) the skill folder to wherever your agent discovers skills:

   | Agent | Location |
   |---|---|
   | Claude Code | `<project>/.claude/skills/oxgframe-unity-skill/` (or `~/.claude/skills/` for all projects) |
   | OpenAI Codex CLI | `<project>/.agents/skills/oxgframe-unity-skill/` (or `~/.codex/skills/`) |
   | Other agents | Reference the folder from your agent context file (`AGENTS.md`, `CLAUDE.md`, `.cursorrules`, ...), e.g. "Read Assets/Samples/.../oxgframe-unity-skill/SKILL.md before working with OxGFrame APIs." |

3. Optionally delete the imported copy under `Assets/Samples/` afterwards — the skill files are plain Markdown and do not need to live inside the Unity project once copied. (Keeping them under `Assets/` is harmless; they import as TextAssets.)

## Notes

- The skill is written in English so it works well as agent context in any locale; the agent should reply in the user's language.
- API signatures were verified against OxGFrame v3.6.1+. When in doubt, the skill instructs the agent to re-verify against the installed package source — newer OxGFrame versions may differ.
- `references/source-map.md` lists the exact source files and search commands used for verification.

# Codex Commit Protocol (WordForge)

Repo: github.com/BstForge/WordForge
Default branch: main
Auth: Use secret env var **GITHUB_PAT** (Bearer token) for GitHub API calls.

## Workflow
1) For every change, work on a feature branch:
   codex/<phase>-<short-summary>
   e.g., codex/0.0.6-ui-spacing

2) Make **full-file replacements only** (no inline patches).
   Use the GitHub “Create or update file contents” API to commit to the branch.

3) Commit message format (exact):
   0.X.Y - <3–5 word description> - <what changed>

4) Open a Pull Request into `main`.
   - Title: [Codex] <same 3–5 word description>
   - Body: bullet list of files changed and why.

5) Wait for status check **“CI • Windows WPF Build”**.
   - If it fails, read logs, fix, and push more commits to the same branch until green.

6) Post back the PR URL, CI status, and artifact link.
7) Never push directly to `main`.

## Paths / build assumptions
- Solution: `WordForge.sln` at repo root.
- Project: `WordForge/WordForge.csproj`
- Windows CI must pass before merge.

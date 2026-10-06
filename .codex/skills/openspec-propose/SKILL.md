---
name: openspec-propose
description: Propose a new change with all artifacts generated in one step. Use when the user wants to quickly describe what they want to build and get a complete proposal with design, specs, and tasks ready for implementation.
allowed-tools: Bash(bit:*)
license: MIT
compatibility: Requires bit CLI.
metadata:
  author: openspec
  version: "1.0"
  generatedBy: "1.14.0"
---

**Execution metrics (never blocking):** Run `bit exec start --command propose --change "<name>" --tool codex` as soon as the change name is known (selected, inferred, or just created) and before doing the work. If you know which model you are, add `--model "<model>"`. If a `bit exec` call fails, say so in one line and continue — metrics never stop the work.

Propose a new change - create the change and generate all artifacts in one step.

I'll create a change with artifacts:
- proposal.md (what & why)
- design.md (how)
- tasks.md (implementation steps)

When ready to implement, run /opsx:apply

---

**Store selection:** If the user names a store (a store is a standalone OpenSpec repo registered on this machine) or the work lives in one, run `bit store list --json` to discover registered store ids, then pass `--store <id>` on the commands that read or write specs and changes (`new change`, `status`, `instructions`, `list`, `show`, `validate`, `archive`, `doctor`, `context`). Other commands do not take the flag. Hints printed by commands already carry the flag; keep it on follow-ups. Without a store, commands act on the nearest local `openspec/` root.

**Input**: The user's request should include a change name (kebab-case) OR a description of what they want to build.

**Steps**

1. **Guardrail validation — BLOCK if change violates locked rules**

   Read `.github/governance/guardrails/guardrails-01-global.md`.

   **Technology stack is LOCKED** — the stack defined in guardrails CANNOT be changed without explicit approval:

   | Locked | Value | Source |
   |--------|-------|--------|
   | Angular | 20 | guardrails-02-web-frontend.md |
   | .NET | 10 | guardrails-03-web-backend.md |
   | Flutter | 3.27 | guardrails-05-mobile-architect.md |
   | Architecture | Hexagonal + Clean | estructura-carpetas.md |
   | Infrastructure | Azure ONLY (no Docker/K8s) | estructura-carpetas.md |

   **If the user's request violates a guardrail:**
   - Identify WHICH guardrail is violated (e.g., `G-WEB-FE-01`)
   - BLOCK the change: Explain the violation and ask for explicit approval referencing the guardrail
   - Example: "The proposal changes Angular 20 → 19. This requires explicit approval per `estructura-carpetas.md` (Stack de plantilla: Angular 20 + .NET 10 + SQL Server). Do you have approval?"

   **If the user asks about changing locked items:** Always reference the guardrail and ask for approval before proceeding.

2. **If no clear input provided, ask what they want to build**

   Use the **AskUserQuestion tool** (open-ended, no preset options) to ask:
   > "What change do you want to work on? Describe what you want to build or fix."

   From their description, derive a kebab-case name (e.g., "add user authentication" → `add-user-auth`).

   **IMPORTANT**: Do NOT proceed without understanding what the user wants to build.

3. **Create the change directory**
   ```bash
   bit new change "<name>"
   ```
   This creates a scaffolded change in the planning home resolved by the CLI with `.openspec.yaml`.

4. **Get the artifact build order**
   ```bash
   bit status --change "<name>" --json
   ```
   Parse the JSON to get:
   - `applyRequires`: array of artifact IDs needed before implementation (e.g., `["tasks"]`)
   - `artifacts`: list of all artifacts with their status and dependencies
   - `planningHome`, `changeRoot`, `artifactPaths`, and `actionContext`: path and scope context. Use these instead of assuming repo-local paths.

5. **Create artifacts in sequence until apply-ready**

   Use the **TodoWrite tool** to track progress through the artifacts.

   Loop through artifacts in dependency order (artifacts with no pending dependencies first):

   a. **For each artifact that is `ready` (dependencies satisfied)**:
      - Get instructions:
        ```bash
        bit instructions <artifact-id> --change "<name>" --json
        ```
      - The instructions JSON includes:
        - `context`: Project background (constraints for you - do NOT include in output)
        - `rules`: Artifact-specific rules (constraints for you - do NOT include in output)
        - `template`: The structure to use for your output file
        - `instruction`: Schema-specific guidance for this artifact type
        - `resolvedOutputPath`: Resolved path or pattern to write the artifact
        - `dependencies`: Completed artifacts to read for context
      - Read any completed dependency files for context
      - Create the artifact file using `template` as the structure and write it to `resolvedOutputPath`
      - Apply `context` and `rules` as constraints - but do NOT copy them into the file
      - Show brief progress: "Created <artifact-id>"

   b. **Continue until all `applyRequires` artifacts are complete**
      - After creating each artifact, re-run `bit status --change "<name>" --json`
      - Check if every artifact ID in `applyRequires` has `status: "done"` in the artifacts array
      - Stop when all `applyRequires` artifacts are done

   c. **If an artifact requires user input** (unclear context):
      - Use **AskUserQuestion tool** to clarify
      - Then continue with creation

5. **Show final status**
   ```bash
   bit status --change "<name>"
   ```

**Output**

After completing all artifacts, summarize:
- Change name and location
- List of artifacts created with brief descriptions
- What's ready: "All artifacts created! Ready for implementation."
- Prompt: "Run `/opsx:apply` or ask me to implement to start working on the tasks."

**Artifact Creation Guidelines**

- Follow the `instruction` field from `bit instructions` for each artifact type
- The schema defines what each artifact should contain - follow it
- Read dependency artifacts for context before creating new ones
- Use `template` as the structure for your output file - fill in its sections
- **IMPORTANT**: `context` and `rules` are constraints for YOU, not content for the file
  - Do NOT copy `<context>`, `<rules>`, `<project_context>` blocks into the artifact
  - These guide what you write, but should never appear in the output

**Guardrails**
- Create ALL artifacts needed for implementation (as defined by schema's `apply.requires`)
- Always read dependency artifacts before creating a new one
- If context is critically unclear, ask the user - but prefer making reasonable decisions to keep momentum
- If a change with that name already exists, ask if user wants to continue it or create a new one
- Verify each artifact file exists after writing before proceeding to next

**Final step — execution metrics (never blocking):** Before your final message, including when you stop early or pause, record the end of this run:
```bash
bit exec end --command propose --change "<name>" --result <OK|FAIL|BLOCKED>
```
Use the same `--change` value as in the start call. `--result`: OK if the command did its job, BLOCKED if it stopped on a blocker or a question for the user, FAIL if it could not complete.


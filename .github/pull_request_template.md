# Pull Request — G-GLOBAL-02 Checklist

## Description
_Describe the change and why it's needed._

## Type of Change
- [ ] feat: new feature
- [ ] fix: bug fix
- [ ] docs: documentation
- [ ] refactor: code restructuring
- [ ] test: test addition/fix
- [ ] chore: maintenance

## Branch
- Branch name: `${BRANCH_NAME}`
- Convention: `{feature|bugfix}/short-kebab-case-description` (from develop) or `hotfix/short-kebab-case-description` (from main)

## Checklist (G-GLOBAL-02, G-GLOBAL-05)

### Branch & Commits
- [ ] Branch follows naming convention: `feature/desc`, `bugfix/desc` (from develop), or `hotfix/desc` (from main, merged back to both)
- [ ] All commits follow Conventional Commits format

### Code Quality
- [ ] Unit tests: ≥ 75% coverage (backend), ≥ 70% (frontend)
- [ ] No console.log / print statements in production code
- [ ] No TODO/FIXME without ticket reference
- [ ] Naming follows conventions (kebab-case files, PascalCase classes)

### Architecture
- [ ] Layers correctly isolated (no cross-layer imports)
- [ ] Dependencies point inward (unidirectional)
- [ ] No domain logic in presentation/infrastructure

### Security
- [ ] No hardcoded secrets
- [ ] Input validation present
- [ ] Error messages don't leak sensitive info
- [ ] CORS properly configured (if API changes)

### PR Requirements
- [ ] At least 1 code review approval required
- [ ] All CI checks passing
- [ ] No secrets in commits
- [ ] Squash merge to develop, merge commit to main
- [ ] Changeset added if user-facing change (for published packages)

## Test Plan
_Describe how this was tested._

## Screenshots (if UI change)
_Attach before/after screenshots._

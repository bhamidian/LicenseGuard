# Git Workflow

This repository uses `main` and `develop` as its two long-lived branches.

## Branches

- `main` contains reviewed, releasable code. Do not commit directly to it.
- `develop` is the integration branch for the next release. Do not commit directly to it.
- Work on short-lived branches created from `develop`: `feature/<short-name>`, `fix/<short-name>`, `docs/<short-name>`, `refactor/<short-name>`, or `chore/<short-name>`.
- Use lowercase kebab-case names, for example `feature/license-validation`.
- Merge completed work into `develop` through a pull request. Delete the short-lived branch after merge.
- Release by pull request from `develop` to `main`. Tag the release commit as `vMAJOR.MINOR.PATCH`.
- Do not rebase or force-push shared branches. Resolve conflicts on the work branch and update its pull request.

## Commits

Use Conventional Commits:

```text
<type>(<optional-scope>): <imperative summary>
```

Allowed common types: `feat`, `fix`, `docs`, `refactor`, `test`, `build`, `ci`, `chore`, and `perf`.

Examples:

```text
feat(licenses): add activation limit checks
fix(validation): reject licenses for the wrong product
docs: define Docker workflow
```

Keep each commit focused and independently understandable. Put breaking changes in the body and add `BREAKING CHANGE: <description>` in the footer.

## Pull requests and review

- PRs into `develop` must identify the change, explain relevant design decisions, and list verification performed.
- Keep PRs focused; link related issues or task references when available.
- Require at least one review before merging when repository hosting supports branch protection.
- Run the relevant build and tests before requesting review. Do not merge with unresolved review threads or known failing checks.
- Prefer squash merging work branches into `develop`; use a normal merge commit for release PRs into `main` so the release boundary remains visible.

## Repository setup

The repository starts with no commits, so Git cannot create a `develop` branch ref yet. After the initial commit on `main`, create `develop` from it:

```bash
git switch main
git switch -c develop
```

Configure the hosting service to protect both long-lived branches against direct pushes and force-pushes. Add the remote URL after the hosting location is selected.

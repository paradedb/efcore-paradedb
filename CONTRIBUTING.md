# Contributing to efcore-paradedb

Thanks for your interest in contributing to `efcore-paradedb`.

## Technical Info

Before opening a PR, please read this guide for workflow and quality expectations.
If anything is unclear, ask in the ParadeDB Slack community.

### Selecting Issues

External contributions should be linked to a GitHub issue.
If no issue exists for your bug/feature, please open one first.

Good starter issues are usually labeled `good first issue`.
Ideal issues for external contributors are well-scoped changes that are less
likely to conflict with core roadmap work. We welcome small documentation
contributions that accompany a feature, correct wrong information, or fix
typos, but we do not accept generic "documentation improvement" PRs.

### Claiming Issues

This repository supports self-assignment:

1. Confirm the issue is unassigned and not already being worked on.
2. Comment `/take` to assign it to yourself.

If you can no longer work on it, remove yourself from assignees.

### Development Setup

```bash
git clone https://github.com/paradedb/efcore-paradedb.git
cd efcore-paradedb

dotnet tool restore
dotnet restore build.slnf
```

### Running Tests

Run for against a single .NET version for speed:

```bash
dotnet test -f net10.0
```

Or against 8, 9, and 10:

```bash
dotnet test
```

### Linting and Formatting

This repository enforces format checks via CSharpier and lint workflows for
Markdown, YAML, Bash, and typos. Common commands:

```bash
# C# formatting
dotnet csharpier check .
dotnet csharpier format .

# Pre-commit hooks (markdownlint, codespell, etc.)
prek install -f
prek run --all-files
```

### Pull Request Workflow

1. Ensure your change has an issue.
2. Branch from `main`.
3. Add or update tests for behavior changes.
4. Update docs/examples when public API changes.
5. Open a PR to `main`.
6. Ensure CI passes.

The repository enforces PR title linting and follows Conventional Commits.
We will not merge a feature without appropriate tests.

### Documentation

If you add a user-facing feature, include docs updates in the same PR:

- `README.md` for public API behavior
- `examples/` for practical usage
- inline comments/XML doc comments when needed for maintainability

We will not merge a feature without appropriate documentation.

## Legal Info

### Contributor License Agreement

In order for us, ParadeDB, Inc., to accept patches and other contributions
from you, you need to adopt our ParadeDB Contributor License Agreement (the
"**CLA**"). The current version of the CLA can be found on the
[CLA Assistant website](https://cla-assistant.io/paradedb/paradedb).

ParadeDB uses a tool called CLA Assistant to help us track contributors' CLA
status. CLA Assistant will automatically post a comment to your pull request
indicating whether you have signed the CLA. If you have not signed the CLA, you
must do so before we can accept your contribution. Signing the CLA is a one-time
process for this repository, is valid for all future contributions to
efcore-paradedb, and can be done in under a minute by signing in with your
GitHub account.

If you have any questions about the CLA, please reach out to us in the
[ParadeDB Community Slack](https://paradedb.com/slack)
or via email at [legal@paradedb.com](mailto:legal@paradedb.com).

### License

By contributing to efcore-paradedb, you agree that your contributions will be
licensed under the [MIT License](LICENSE).

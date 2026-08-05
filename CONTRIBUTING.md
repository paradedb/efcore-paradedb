# Contributing to efcore-paradedb

Welcome! We're excited that you're interested in contributing to efcore-paradedb and want to make the process as smooth as possible.

## Technical Info

Before submitting a pull request, please review this document, which outlines what conventions to follow when submitting changes. If you have any questions not covered in this document, please reach out to us in the [ParadeDB Community Slack](https://paradedb.com/slack) or via [email](mailto:support@paradedb.com).

### Selecting GitHub Issues

All external contributions should be associated with a GitHub issue. If there is no open issue for the bug or feature that you'd like to work on, please open one first. When selecting an issue to work on, we recommend focusing on issues labeled `good first issue`.

Ideal issues for external contributors include well-scoped, individual features as those are less likely to conflict with our general development process. We welcome documentation contributions that accompany a feature, correct wrong information, improve clarity, or fix typos.

### Claiming GitHub Issues

This repository has a workflow to assign issues to new contributors automatically. This ensures that you don't need approval from a maintainer to pick an issue.

1. Before claiming an issue, ensure that:
   - It's not already assigned to someone else.
   - There are no comments indicating ongoing work.

2. To claim an unassigned issue, comment `/take` on the issue. This will automatically assign the issue to you.

If you find yourself unable to make progress, don't hesitate to seek help in the issue comments or the [ParadeDB Community Slack](https://paradedb.com/slack). If you no longer wish to work on the issue(s) you self-assigned, please remove yourself from the Assignees list in the sidebar to release it.

### Development Setup

efcore-paradedb is a NuGet package that provides Entity Framework Core integration for ParadeDB.

```bash
git clone https://github.com/paradedb/efcore-paradedb.git
cd efcore-paradedb

dotnet tool restore
dotnet restore build.slnf

# Install prek hooks
uvx prek install
```

### Running Tests

Run the tests to verify every change. Target a single .NET version for speed:

```bash
dotnet test -f net10.0
```

Or run against .NET 8, 9, and 10:

```bash
dotnet test
```

Prefer `dotnet test` over `dotnet build` when verifying changes.

The tests provision their own database with Testcontainers. The examples use `scripts/run_paradedb.sh`. The default container is `efcore-paradedb` on port `5432`.

Some tests require newer pg_search versions and are skipped automatically if the feature is not available.

### Linting and Formatting

```bash
# C# formatting
dotnet csharpier check .
dotnet csharpier format .

# Pre-commit hooks (markdownlint, codespell, etc.)
uvx prek install
uvx prek run --all-files
```

### API and Packaging Consistency Checks

Run these before opening a PR if your change touches SQL wrappers, API constants, packaging, or release metadata:

```bash
uv run --with json5 scripts/check_api_coverage.py
```

### Pull Request Workflow

All changes to efcore-paradedb happen through GitHub Pull Requests. Here is the recommended flow for making a change:

1. Before working on a change, please check if there is already a GitHub issue open for it.
2. If there is not, please open an issue first. This gives the community visibility into your work and allows others to make suggestions and leave comments.
3. Fork the efcore-paradedb repo and branch out from the `main` branch.
4. Install [prek](https://prek.j178.dev/quickstart/#already-using-pre-commit) hooks within your fork with `uvx prek install` to ensure code quality and consistency with upstream.
5. Make your changes. If you've added new functionality, please add tests. We will not merge a feature without appropriate tests.
6. Open a pull request towards the `main` branch. Ensure that all tests and checks pass. Note that the efcore-paradedb repository has pull request title linting in place and follows the [Conventional Commits spec](https://www.conventionalcommits.org/).
7. Congratulations! Our team will review your pull request.

### Changelog

When you make a change that a user of this project would care about, record it in the `Unreleased` section of [CHANGELOG.md](CHANGELOG.md). If the change is breaking, make sure to denote that.

### Documentation

If you are adding a new feature that requires new documentation, please add the documentation as part of your pull request. Documentation can be added to:

- The main README.md for user-facing features
- XML doc comments for API documentation
- The `examples/` directory for usage examples

We will not merge a feature without appropriate documentation.

## Legal Info

### Contributor License Agreement

In order for us, ParadeDB, Inc., to accept patches and other contributions from you, you need to adopt our ParadeDB Contributor License Agreement (the "**CLA**"). The current version of the CLA can be found on the [CLA Assistant website](https://cla-assistant.io/paradedb/efcore-paradedb).

ParadeDB uses a tool called CLA Assistant to help us track contributors' CLA status. CLA Assistant will automatically post a comment to your pull request indicating whether you have signed the CLA. If you have not signed the CLA, you must do so before we can accept your contribution. Signing the CLA is a one-time process for this repository, is valid for all future contributions to efcore-paradedb, and can be done in under a minute by signing in with your GitHub account.

If you have any questions about the CLA, please reach out to us in the [ParadeDB Community Slack](https://paradedb.com/slack) or via email at [legal@paradedb.com](mailto:legal@paradedb.com).

### License

By contributing to efcore-paradedb, you agree that your contributions will be licensed under the [MIT License](LICENSE).

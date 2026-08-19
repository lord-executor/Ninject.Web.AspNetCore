# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Ninject dependency injection integration for ASP.NET Core — replaces ASP.NET Core's default `IServiceProvider` with Ninject, including a compliance layer that mimics `Microsoft.Extensions.DependencyInjection` semantics (see README.md for the documented behavioral differences: null injection, scope handling, disposal ordering via `OrderedDisposalStrategy`, binding precedence via `IndexedBindingPrecedenceComparer`, generic constraint resolution via `ConstrainedGenericBindingResolver`).

Solution: `src/Ninject.Web.AspNetCore.slnx` (7 projects) — main library `Ninject.Web.AspNetCore`, hosting wrappers `Ninject.Web.AspNetCore.IIS` and `Ninject.Web.AspNetCore.Httpsys`, unit tests `Ninject.Web.AspNetCore.Test`, and `Ninject.Web.AspNetCore.ComplianceTest` which runs Microsoft's official `Microsoft.Extensions.DependencyInjection.Specification.Tests` suite against the Ninject-backed provider (some tests are intentionally skipped where not reasonable for a Ninject-based implementation). Two sample apps (`SampleApplication`, `SampleBlazorApplication`) demonstrate usage.

## Build & test

```
dotnet build src/Ninject.Web.AspNetCore.slnx
dotnet test src/Ninject.Web.AspNetCore.slnx
```

This matches CI (`.github/workflows/build.yml`), which runs on every push against .NET 10.0.x SDK.

## Code style

- **Tabs, not spaces** in `.cs` files (`.editorconfig`: `indent_style = tab`) — differs from the common 4-space .NET default.

## Packable projects (library, IIS, HttpSys)

- Multi-target `net10.0;net9.0;net8.0`. Test/compliance/sample projects target `net10.0` only.
- Strong-name signed with the committed key `src/Ninject.Web.AspNetCore.snk` (`SignAssembly`/`AssemblyOriginatorKeyFile`) — any new packable project needs the same setup.
- Package version numbers intentionally track the ASP.NET Core version they target, not independent semver (see README versioning table and CHANGELOG).

## Repo conventions

- Default/upstream branch is `main`.
- Feature branches use `feature/...` or `bugfix/...` prefixes, sometimes referencing an issue number (e.g. `feature/IssueNr11_IKeyedServiceProvider`).
- Commit messages are short, lowercase/imperative, no conventional-commit prefixes (e.g. "remove another warning", "fix open generics resolution").
- CHANGELOG is maintained manually in Keep-a-Changelog format — update it for user-visible changes.

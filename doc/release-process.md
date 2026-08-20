# Release Process
Releases are built, packed and published automatically by the [`NuGet Package Release`](../.github/publish.yml) GitHub Actions workflow.

To ship a release, push a tag matching `v*.*.*` (e.g. `v10.0.1`):
```
git tag v10.0.1
git push origin v10.0.1
```

The workflow then:
1. Restores, builds and tests the solution.
2. Derives the package version from the tag name (stripping the leading `v`, e.g. `v10.0.1` -> `10.0.1`) and packs all three packages with that version via `-p:Version`.
3. Exchanges the workflow's GitHub OIDC token for a short-lived NuGet API key ([NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)) and pushes the packages to nuget.org.

No manual build, pack or publish steps are required.

# Release Process
Build and package the project with the _Release_ configuration.

```
dotnet build .\src\Ninject.Web.AspNetCore.sln -c Release
dotnet pack .\src\Ninject.Web.AspNetCore.sln -c Release
```

Publish all three packages with the `publish.ps1` script

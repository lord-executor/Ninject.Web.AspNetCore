# Release Process
Build and package the project with the relevant _Release_ configuration - `Release22` for the ASP.NET Core 2.2 branch or `Release31` for the ASP.NET Core 3.x branch.

```
dotnet build .\src\Ninject.Web.AspNetCore.sln -c Release22
dotnet pack .\src\Ninject.Web.AspNetCore.sln -c Release22
```

```
dotnet build .\src\Ninject.Web.AspNetCore.sln -c Release31
dotnet pack .\src\Ninject.Web.AspNetCore.sln -c Release31
```

Publish all three packages with the `publish.ps1` script, replacing the API key and versions as needed.

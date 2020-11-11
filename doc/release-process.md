# Release Process
Build and package the project with the relevant _Release_ configuration - `Release22` for the ASP.NET Core 2.2 branch or `Release31` for the ASP.NET Core 3.x branch.

```
dotnet pack .\src\Ninject.Web.AspNetCore.sln -c Release22 -p:Version="2.2.X"
dotnet pack .\src\Ninject.Web.AspNetCore.sln -c Release31 -p:Version="3.0.X"
```

Publish all three packages with the `publish.ps1` script, replacing the API key and versions as needed.
```
.\doc\publish.ps1 -apiKey "NuGet APIKey" -v2 "2.2.X" -v3 "3.0.X"
```

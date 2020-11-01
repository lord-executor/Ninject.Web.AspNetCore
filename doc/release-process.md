# Release Process
Build and package the project with the _Release_ configuration.

```
dotnet build .\src\Ninject.Web.AspNetCore.sln -c Release
dotnet pack .\src\Ninject.Web.AspNetCore.sln -c Release
```

Publish all three packages with
```
dotnet nuget push src\Ninject.Web.AspNetCore\bin\Release\Ninject.Web.AspNetCore.[X.Y.Z].nupkg -k [API-Key] -s https://api.nuget.org/v3/index.json
dotnet nuget push src\Ninject.Web.AspNetCore.IIS\bin\Release\Ninject.Web.AspNetCore.IIS.[X.Y.Z].nupkg -k [API-Key] -s https://api.nuget.org/v3/index.json
dotnet nuget push src\Ninject.Web.AspNetCore.Httpsys\bin\Release\Ninject.Web.AspNetCore.Httpsys.[X.Y.Z].nupkg -k [API-Key] -s https://api.nuget.org/v3/index.json
```

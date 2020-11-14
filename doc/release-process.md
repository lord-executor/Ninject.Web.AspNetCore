# Release Process
Build and package the project with the  _Release_ configuration.

```
dotnet pack .\src\Ninject.Web.AspNetCore.sln -c Release -p:Version="5.0.X"
```

Publish all three packages with the `publish.ps1` script, replacing the API key and versions as needed.
```
.\doc\publish.ps1 -apiKey "NuGet APIKey" -version "5.0.X"
```

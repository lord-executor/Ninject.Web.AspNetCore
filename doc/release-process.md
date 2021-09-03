# Release Process
Build and package the project with the  _Release_ configuration.

```
dotnet build .\src\Ninject.Web.AspNetCore.sln -c Release -p:Version="5.0.X"
dotnet pack .\src\Ninject.Web.AspNetCore.sln -c Release -p:Version="5.0.X"
```

Publish all three packages with the `publish.ps1` script, replacing the API key and versions as needed.
```
.\doc\publish.ps1 -apiKey "NuGet APIKey" -version "5.0.X"
```

With the NuGet API key stored in the Windows credential manager, we can do
```
.\doc\publish.ps1 -apiKey $((Read-CredentialsStore -Target "NuGet:Ninject.Web.AspNetCore:APIKey").GetNetworkCredential().Password) -version "5.0.X"
```

# Release Process
Build and package the project with the  _Release_ configuration.

```
$version = "5.1.x"
dotnet build .\src\Ninject.Web.AspNetCore.sln -c Release -p:Version="$version"
dotnet pack .\src\Ninject.Web.AspNetCore.sln -c Release --include-symbols -p:SymbolPackageFormat=snupkg -p:Version="$version"
```

Publish all three packages with the `publish.ps1` script, replacing the API key and versions as needed.
```
.\doc\publish.ps1 -apiKey "NuGet APIKey" -version "$version"
```

With the NuGet API key stored in the Windows credential manager, we can do
```
.\doc\publish.ps1 -apiKey $((Read-CredentialsStore -Target "NuGet:Ninject.Web.AspNetCore:APIKey").GetNetworkCredential().Password) -version "$version"
```

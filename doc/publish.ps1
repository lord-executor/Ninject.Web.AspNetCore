$apiKey = "[API-Key]"
$version = "[X.Y.Z]"
$projects = (
    "Ninject.Web.AspNetCore",
    "Ninject.Web.AspNetCore.IIS",
    "Ninject.Web.AspNetCore.Httpsys"
)

$projects | ForEach-Object {
    dotnet nuget push "src\$_\bin\Release\$_.$version.nupkg" -k $apiKey -s https://api.nuget.org/v3/index.json
}

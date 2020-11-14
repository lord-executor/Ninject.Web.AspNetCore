param(
    [Parameter(Mandatory=$true)]
    [string] $apiKey,
    [Parameter(Mandatory=$true)]
    [string] $version = $null
)

$projects = (
    "Ninject.Web.AspNetCore",
    "Ninject.Web.AspNetCore.IIS",
    "Ninject.Web.AspNetCore.Httpsys"
)

$projects | ForEach-Object {
    dotnet nuget push "src\$_\bin\Release\$_.$version.nupkg" -k $apiKey -s https://api.nuget.org/v3/index.json
}

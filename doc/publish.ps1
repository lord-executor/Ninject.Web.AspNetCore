param(
    [Parameter(Mandatory=$true)]
    [string] $apiKey,
    [string] $v2 = $null,
    [string] $v3 = $null
)

$projects = (
    "Ninject.Web.AspNetCore",
    "Ninject.Web.AspNetCore.IIS",
    "Ninject.Web.AspNetCore.Httpsys"
)

function Publish() {
    param(
        [string] $release,
        [string] $version
    )

    $projects | ForEach-Object {
        dotnet nuget push "src\$_\bin\Release$release\$_.$version.nupkg" -k $apiKey -s https://api.nuget.org/v3/index.json
    }
}

if ($v2) {
    Publish "22" $v2
}

if ($v3) {
    Publish "31" $v3
}

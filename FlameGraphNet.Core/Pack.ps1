param(
    [string] $VersionSuffix,
    [switch] $official
)
if (!$VersionSuffix) {
    $VersionSuffix = Get-Date -Format "yyyyMMddHHmmss"
}
else {
    Write-Host "Version suffix not null";
}
Write-Host "Use version suffix: $VersionSuffix"

$versionPrefix = "1.0.2"

if ($official) {

    dotnet pack -o ../NuGets/ -c Release /p:Version="$versionPrefix"
}
else {
    Write-Host "Unofficial package:"
    dotnet pack -o ../NuGets/ -c Release /p:Version="$versionPrefix-$VersionSuffix"
}
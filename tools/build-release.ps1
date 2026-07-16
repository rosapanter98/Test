[CmdletBinding()]
param(
    [string] $Version = "0.1.0"
)

$ErrorActionPreference = "Stop"

if ($Version -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?$') {
    throw "Version '$Version' is not a valid SemVer 2 version."
}

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$artifactsRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts"))
$publishDirectory = [System.IO.Path]::GetFullPath((Join-Path $artifactsRoot "publish\win-x64"))
$releaseDirectory = [System.IO.Path]::GetFullPath((Join-Path $artifactsRoot "releases\win"))
$projectPath = Join-Path $repositoryRoot "src\CertPrep\CertPrep.csproj"
$solutionPath = Join-Path $repositoryRoot "CertPrep.sln"
$iconPath = Join-Path $repositoryRoot "src\CertPrep\Assets\App\certprep.ico"
$licensePath = Join-Path $repositoryRoot "LICENSE"

foreach ($path in @($publishDirectory, $releaseDirectory)) {
    if (-not $path.StartsWith($artifactsRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean a release path outside the artifacts directory: $path"
    }

    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
    }
}

New-Item -ItemType Directory -Path $publishDirectory, $releaseDirectory -Force | Out-Null

Push-Location $repositoryRoot
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "Tool restore failed." }

    dotnet test $solutionPath -c Release
    if ($LASTEXITCODE -ne 0) { throw "Release tests failed." }

    dotnet publish $projectPath `
        -c Release `
        -p:PublishProfile=win-x64 `
        -p:Version=$Version `
        -o $publishDirectory
    if ($LASTEXITCODE -ne 0) { throw "Release publish failed." }

    $symbols = @(Get-ChildItem -LiteralPath $publishDirectory -Filter *.pdb -File -Recurse)
    if ($symbols.Count -gt 0) {
        throw "Release publish contains PDB files: $($symbols.Name -join ', ')"
    }

    dotnet vpk pack `
        --packId ToreAndreVinsrygg.CertPrep `
        --packVersion $Version `
        --packDir $publishDirectory `
        --mainExe CertPrep.exe `
        --packTitle CertPrep `
        --packAuthors "Tore Andre Vinsrygg" `
        --runtime win-x64 `
        --channel win `
        --icon $iconPath `
        --instLicense $licensePath `
        --shortcuts StartMenuRoot `
        --outputDir $releaseDirectory `
        --yes
    if ($LASTEXITCODE -ne 0) { throw "VeloPack packaging failed." }

    Write-Host "Release $Version created in $releaseDirectory"
}
finally {
    Pop-Location
}

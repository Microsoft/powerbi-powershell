[CmdletBinding()]
param
(
    [ValidateNotNullOrEmpty()]
    [string] $Path,

    [int] $CatalogVersion = 2,

    [ValidateNotNullOrEmpty()]
    [string] $StagingDir = $env:Temp
)

Write-Output "Running $($MyInvocation.MyCommand.Name)"

if(!(Test-Path -Path $Path)) {
    throw "-Path not found: $Path"
}

$nuspecFiles = Get-ChildItem -Path $Path -Filter *.nuspec -Recurse
if(!$nuspecFiles) {
    throw "No *.nuspec files found under: $Path"
}

foreach($nuspecFile in $nuspecFiles) {
    $moduleName = $nuspecFile.BaseName
    $nuspecDirectory = $nuspecFile.Directory.FullName
    $nuspecFilePath = $nuspecFile.FullName
    
    Write-Output "Generating File Catalog (CAT) for module '$moduleName' in directory: $nuspecDirectory"
    $catalogFilePath = Join-Path -Path $nuspecDirectory -ChildPath "$moduleName.cat"
    
    # Move the Nuspec out temporary to get correct checksum
    $nuspecTemp = Join-Path -Path $StagingDir -ChildPath $nuspecFile.Name
    Move-Item -Path $nuspecFilePath -Destination $nuspecTemp -Force
    
    $newFileCatalogParams = @{
        Path = $nuspecDirectory
        CatalogFilePath = $catalogFilePath
        CatalogVersion = $CatalogVersion
        Verbose = $VerbosePreference
    }

    [void](Microsoft.PowerShell.Security\New-FileCatalog @newFileCatalogParams)

    # Move nuspec back
    Move-Item -Path $nuspecTemp -Destination $nuspecFilePath -Force

    if(!(Test-Path -Path $catalogFilePath)) {
        throw "Failed to generate CAT file: $catalogFilePath"
    }
}

Write-Output "Completed running $($MyInvocation.MyCommand.Name)"
function Test-UnitTestProjectLSL {

    $projectPC = New-ClassLibrary
    $projectPJ = New-Project BuildIntegratedClassLibrary
    $projectPR = New-Project PackageReferenceClassLibrary

    $projectT = New-Project UnitTestProject TestProject

    Add-ProjectReference $projectT $projectPC
    Add-ProjectReference $projectT $projectPJ
    Add-ProjectReference $projectT $projectPR

    # Build to restore
    Build-Solution

    Enable-LightweightSolutionLoad -Reload

    # Act
    $packageIds = Get-InstalledPackage | Select-Object -ExpandProperty Id

    Assert-True ($packageIds -contains 'MSTest.TestAdapter') -Message 'Test extension package is not found'
    Assert-True (Test-Project TestProject -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-PackagesConfigInstallPackageLSL {

    $projectT = New-ClassLibrary TestProject
    Enable-LightweightSolutionLoad -Reload

    $projectT = Get-Project TestProject

    # Act
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Assert-True -Value:($projectT | Test-InstalledPackage NuGet.Versioning) -Message:'Test package is not installed'
    Assert-True -Value:($projectT | Test-Project -IsDeferred) -Message:'Test project should stay in deferred mode'
}

function Test-PackagesConfigUninstallPackageLSL {

    $projectT = New-ClassLibrary TestProject
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7
    Enable-LightweightSolutionLoad -Reload

    $projectT = Get-Project TestProject

    # Act
    $projectT | Uninstall-Package NuGet.Versioning -Version 1.0.7

    Assert-True -Value:($projectT | Test-InstalledPackage NuGet.Versioning) -Message:'Test package is not installed'
    Assert-True -Value:($projectT | Test-Project -IsDeferred) -Message:'Test project should stay in deferred mode'
}

function Test-PackageReferenceInstallPackageLSL {

    $projectT = New-Project PackageReferenceClassLibrary TestProject
    Enable-LightweightSolutionLoad -Reload

    $projectT = Get-Project TestProject
    Assert-NotNull $projectT

    # Act
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Assert-True -Value:($projectT | Test-InstalledPackage NuGet.Versioning) -Message:'Test package is not installed'
    Assert-True -Value:($projectT | Test-Project -IsDeferred) -Message:'Test project should stay in deferred mode'
}

function Test-PackageReferenceUninstallPackageLSL {

    $projectT = New-Project PackageReferenceClassLibrary TestProject
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7
    Enable-LightweightSolutionLoad -Reload

    $projectT = Get-Project TestProject

    # Act
    $projectT | Uninstall-Package NuGet.Versioning -Version 1.0.7

    Assert-True -Value:($projectT | Test-InstalledPackage NuGet.Versioning) -Message:'Test package is not installed'
    Assert-True -Value:($projectT | Test-Project -IsDeferred) -Message:'Test project should stay in deferred mode'
}
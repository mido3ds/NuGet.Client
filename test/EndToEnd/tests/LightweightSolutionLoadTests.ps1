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

    $projectT = New-ClassLibrary | Select-Object FullName, ProjectName

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Assert-True ($projectT | Test-InstalledPackage NuGet.Versioning) -Message 'Test package is not installed'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-PackagesConfigUninstallPackageLSL {

    $projectT = New-ClassLibrary TestProject | Select-Object FullName, ProjectName
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Uninstall-Package NuGet.Versioning -Version 1.0.7

    Assert-True ($projectT | Test-InstalledPackage NuGet.Versioning) -Message 'Test package is not installed'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-PackageReferenceInstallPackageLSL {

    $projectT = New-Project PackageReferenceClassLibrary | Select-Object FullName, ProjectName

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Assert-True ($projectT | Test-InstalledPackage NuGet.Versioning) -Message 'Test package is not installed'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-PackageReferenceUninstallPackageLSL {

    $projectT = New-Project PackageReferenceClassLibrary | Select-Object FullName, ProjectName
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Enable-LightweightSolutionLoad -Reload

    $projectT = Get-Project TestProject

    # Act
    $projectT | Uninstall-Package NuGet.Versioning -Version 1.0.7

    Assert-True ($projectT | Test-InstalledPackage NuGet.Versioning) -Message 'Test package is not installed'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-ProjectJsonInstallPackageLSL {

    $projectT = New-Project BuildIntegratedClassLibrary | Select-Object FullName, ProjectName

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Assert-True ($projectT | Test-InstalledPackage NuGet.Versioning) -Message 'Test package is not installed'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-ProjectJsonUninstallPackageLSL {

    $projectT = New-Project BuildIntegratedClassLibrary | Select-Object FullName, ProjectName
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Enable-LightweightSolutionLoad -Reload

    $projectT = Get-Project TestProject

    # Act
    $projectT | Uninstall-Package NuGet.Versioning -Version 1.0.7

    Assert-True ($projectT | Test-InstalledPackage NuGet.Versioning) -Message 'Test package is not installed'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}
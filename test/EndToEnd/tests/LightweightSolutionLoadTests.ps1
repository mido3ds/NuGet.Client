function Test-UnitTestProjectLSL {

    $projectPC = New-ClassLibrary
    $projectPJ = New-Project BuildIntegratedClassLibrary
    $projectPR = New-Project PackageReferenceClassLibrary

    $projectT = New-Project UnitTestProject

    Add-ProjectReference $projectT $projectPC
    Add-ProjectReference $projectT $projectPJ
    Add-ProjectReference $projectT $projectPR

    # Build to restore
    Build-Solution

    $projectT = $projectT | Select-Object UniqueName, ProjectName

    Enable-LightweightSolutionLoad -Reload

    # Act
    $packageIds = Get-InstalledPackage | Select-Object -ExpandProperty Id

    Assert-True ($packageIds -contains 'MSTest.TestAdapter') -Message 'Test extension package is not found'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-PackagesConfigProjectInstallPackageLSL {

    $projectT = New-ClassLibrary | Select-Object UniqueName, ProjectName

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Assert-False ($projectT | Test-Project -IsDeferred) -Message 'Test project should not stay in deferred mode'
    Assert-True ($projectT | Test-InstalledPackage -Id NuGet.Versioning) -Message 'Test package is not installed'
}

function Test-PackagesConfigProjectUninstallPackageLSL {

    $projectT = New-ClassLibrary TestProject | Select-Object UniqueName, ProjectName
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Uninstall-Package NuGet.Versioning -Version 1.0.7

    Assert-False ($projectT | Test-Project -IsDeferred) -Message 'Test project should not stay in deferred mode'
    Assert-False ($projectT | Test-InstalledPackage -Id NuGet.Versioning) -Message 'Test package is still installed'
}

function Test-PackagesConfigProjectGetPackageLSL {

    $projectT = New-ClassLibrary | Select-Object UniqueName, ProjectName
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Enable-LightweightSolutionLoad -Reload

    # Act
    $packageIds = $projectT | Get-Package | Select-Object -ExpandProperty Id

    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
    Assert-True ($packageIds -contains 'NuGet.Versioning') -Message 'Test package is not installed'
}

function Test-PackagesConfigProjectInvokeInitScriptLSL {
    param(
        $context
    )

    $projectT = New-ConsoleApplication | Select-Object UniqueName, ProjectName
    $projectT | Install-Package PackageWithScripts -Source $context.RepositoryRoot

    Remove-Item function:\Get-World
    Assert-False (Test-Path function:\Get-World)

    Enable-LightweightSolutionLoad -Reload

    # This asserts init.ps1 gets called
    Assert-True (Test-Path function:\Get-World) -Message 'Test package function should be imported by init.ps1'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-PackagesConfigProjectGetProjectLSL {

    New-ClassLibrary TestProject

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT = Get-Project TestProject

    Assert-False ($projectT | Test-Project -IsDeferred) -Message 'Test project should not stay in deferred mode'
}

function Test-PackageReferenceProjectInstallPackageLSL {

    $projectT = New-Project PackageReferenceClassLibrary | Select-Object UniqueName, ProjectName

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Assert-False ($projectT | Test-Project -IsDeferred) -Message 'Test project should not stay in deferred mode'
    Assert-True ($projectT | Test-InstalledPackage -Id NuGet.Versioning) -Message 'Test package is not installed'
}

function Test-PackageReferenceProjectUninstallPackageLSL {

    $projectT = New-Project PackageReferenceClassLibrary | Select-Object UniqueName, ProjectName
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Uninstall-Package NuGet.Versioning -Version 1.0.7

    Assert-False ($projectT | Test-Project -IsDeferred) -Message 'Test project should not stay in deferred mode'
    Assert-False ($projectT | Test-InstalledPackage -Id NuGet.Versioning) -Message 'Test package is still installed'
}

function Test-PackageReferenceProjectGetPackageLSL {

    $projectT = New-Project PackageReferenceClassLibrary | Select-Object UniqueName, ProjectName
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Enable-LightweightSolutionLoad -Reload

    # Act
    $packageIds = $projectT | Get-Package | Select-Object -ExpandProperty Id

    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
    Assert-True ($packageIds -contains 'NuGet.Versioning') -Message 'Test package is not installed'
}

function Test-PackageReferenceProjectInvokeInitScriptLSL {
    param(
        $context
    )

    $projectT = New-Project PackageReferenceClassLibrary | Select-Object UniqueName, ProjectName
    $projectT | Install-Package PackageWithScripts -Source $context.RepositoryRoot

    Remove-Item function:\Get-World
    Assert-False (Test-Path function:\Get-World)

    Enable-LightweightSolutionLoad -Reload

    # This asserts init.ps1 gets called
    Assert-True (Test-Path function:\Get-World) -Message 'Test package function should be imported by init.ps1'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-PackageReferenceProjectGetProjectLSL {

    New-Project PackageReferenceClassLibrary TestProject

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT = Get-Project TestProject

    Assert-False ($projectT | Test-Project -IsDeferred) -Message 'Test project should not stay in deferred mode'
}

function Test-ProjectJsonProjectInstallPackageLSL {

    $projectT = New-Project BuildIntegratedClassLibrary | Select-Object UniqueName, ProjectName

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
    Assert-True ($projectT | Test-InstalledPackage -Id NuGet.Versioning) -Message 'Test package is not installed'
}

function Test-ProjectJsonProjectUninstallPackageLSL {

    $projectT = New-Project BuildIntegratedClassLibrary | Select-Object UniqueName, ProjectName
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT | Uninstall-Package NuGet.Versioning -Version 1.0.7

    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
    Assert-False ($projectT | Test-InstalledPackage -Id NuGet.Versioning) -Message 'Test package is still installed'
}

function Test-ProjectJsonProjectGetPackageLSL {

    $projectT = New-Project BuildIntegratedClassLibrary | Select-Object UniqueName, ProjectName
    $projectT | Install-Package NuGet.Versioning -Version 1.0.7

    Enable-LightweightSolutionLoad -Reload

    # Act
    $packageIds = $projectT | Get-Package | Select-Object -ExpandProperty Id

    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
    Assert-True ($packageIds -contains 'NuGet.Versioning') -Message 'Test package is not installed'
}

function Test-ProjectJsonProjectInvokeInitScriptLSL {
    param(
        $context
    )

    $projectT = New-Project BuildIntegratedClassLibrary | Select-Object UniqueName, ProjectName
    $projectT | Install-Package PackageWithScripts -Source $context.RepositoryRoot

    Remove-Item function:\Get-World
    Assert-False (Test-Path function:\Get-World)

    Enable-LightweightSolutionLoad -Reload

    # This asserts init.ps1 gets called
    Assert-True (Test-Path function:\Get-World) -Message 'Test package function should be imported by init.ps1'
    Assert-True ($projectT | Test-Project -IsDeferred) -Message 'Test project should stay in deferred mode'
}

function Test-ProjectJsonProjectGetProjectLSL {

    New-Project BuildIntegratedClassLibrary TestProject

    Enable-LightweightSolutionLoad -Reload

    # Act
    $projectT = Get-Project TestProject

    Assert-False ($projectT | Test-Project -IsDeferred) -Message 'Test project should not stay in deferred mode'
}

# Test-Project throws ObjectNotFound
# init.ps1 doesn't work
# update-package tests
# native projects
# uwp projects
# transitive get-package
# add-bindingredirects
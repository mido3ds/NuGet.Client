function Test-UnitTestProjectLSL {
    New-Project UnitTestProject
    Build-Solution
    Enable-LightweightSolutionLoad -Reload
    Wait-ForSolutionLoad

    $packageIds = Get-InstalledPackages | Select-Object -ExpandProperty Id
    Assert-True -Value:($packageIds -contains 'MSTest.TestAdapter') -Message:'Test extension package is not found'
}
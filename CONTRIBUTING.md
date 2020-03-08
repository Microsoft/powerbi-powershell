# Contribute Code to PowerShell Cmdlets for Power BI

## Developer Environment Requirements

* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
* [.NET Core SDK](https://www.microsoft.com/net/learn/get-started/windows)
    * Version driven by [global.json](src/Common/Commands.Common/global.json)
    * Cmdlets are designed to use .NET Core 2.0 SDK with .NET Standard 2.0
* [Visual Studio Code](https://code.visualstudio.com/download)
    * Experimental and optional, still need Visual Studio installed
    * Install the following extensions:
        * [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp): `code --install-extension ms-dotnettools.csharp`
        * [PowerShell](https://marketplace.visualstudio.com/items?itemName=ms-vscode.PowerShell): `code --install-extension ms-vscode.PowerShell`


> If you plan to build with Configuration=Release which Delay Signs the build output, call `.\scripts\DisableStrongName.ps1`.
> Add the -Enable switch parameter to re-enable strong name verification once developing.

### Optional requirements (for testing)
* [PowerShell Core (6.0.0)](https://github.com/powershell/powershell)
* Windows PowerShell v3 and up with .NET 4.7.1 or above

## Coding Style

Driven and enforced by [.editorconfig](src/.editorconfig). You can learn more about this config file by reading Visual Studio [documentation](https://docs.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options).

Optionally you can install the [EditorConfig](https://marketplace.visualstudio.com/items?itemName=EditorConfigTeam.EditorConfig) Visual Studio extension to give you intellisense editing the file.


## Design

<table width=500 border="1" frame="box">
    <tr>
        <th rowspan="5">Common</th>
        <td align="center" colspan="2">Common.Abstractions</td>
    </tr>
    <tr>
        <td align="center" colspan="2">Common.Authentication</td>
    </tr>
    <tr>
        <td align="center">Common.Commands</td>
        <td align="center">AzureADWindowsAuthenticator</td>
    </tr>
    <tr>
        <td align="center" colspan="2">Common.Api</td>
    </tr>
    <tr>
        <td align="center" colspan="2">Common.Client</td>
    </tr>
    <tr>
        <th rowspan="4">Modules</td>
        <td align="center" colspan="3">Commands.Profile</td>
    </tr>
    <tr>
        <td align="center">Commands.Workspaces</td>
        <td align="center">Commands.Reports</td>
        <td align="center">Commands.Data</td>
    </tr>
<table>

> * Test Projects are located next to implementation (module project).
> * AzureADWindowsAuthenticator has an independent dependency stack as it's compiled against .NET 4.6 (as opposed to .NET Core) to act as a bridge for ADAL on Windows (providing interactive login).



## CI Build
| Branch       | Status      |
| ------------ | ----------- |
| Master | [![Build status](https://ci.appveyor.com/api/projects/status/4kdopsyh3y70ad9w/branch/master?svg=true)](https://ci.appveyor.com/project/CodeCyclone/powerbi-powershell/branch/master) |
| Dev | [![Build status](https://ci.appveyor.com/api/projects/status/4kdopsyh3y70ad9w/branch/dev?svg=true)](https://ci.appveyor.com/project/CodeCyclone/powerbi-powershell/branch/dev) |

Defined by [appveyor.yml](appveyor.yml), see [AppVeyor Docs](https://www.appveyor.com/docs/) if you need to edit.

## Common Developer Scenarios

### Build and Packaging

#### Command line

Open PowerShell prompt and run (in repository root directory):

```powershell
.\scripts\Build.ps1

# Packaging
.\scripts\Build.ps1 -Pack

# To manually test module\package output
.\scripts\PrepManualTesting.ps1    # Optionally add -Build if you haven't previously built with -Pack from command-line or Visual Studio
# You can call cmdlets after this point such as Connect-PowerBIServiceAccount, their modules get auto-imported
```

#### Visual Studio 2019

1. Open [src/PowerBIPowerShell.sln](src/PowerBIPowerShell.sln).
2. Press `F6` or click `Build` menu and select `Build Solution`.

Binary output will be under each project's `bin\$(Configuration)\$(TargetFramework)` directory (TargetFramework is typically `netstandard2.0` unless its a test project then it's `netcoreapp2.0`)

For Packaging, right-click on a module project and select `Pack` in Visual Studio. Output is in `PkgOut` folder at the root.

To test package\module, rename extension from NuGet package `.nupkg` to Zip `.zip` and extract archive. Load the PSD1 file in the extracted directory by using `Import-Module -Path`.
> This is essentially what `.\scripts\PrepManualTesting.ps1` does with the caveat it appends `PkgOut` folder to `$env:PSModulePath` for your console session, when cmdlets get called their module will get auto-imported.

To execute unit tests, just open `Test Explorer` and run tests from the explorer. Solution supports Live Unit Testing, there are interactive tests in the suite that are auto excluded from running with this feature.

#### Visual Studio Code (experimental)

> Note: Due to some usage of .NET Framework by source code, building and executing tests have been customized and features such as run and debug test out-of-box in the IDE won't execute correctly (see workaround below).

Open  Visual Studio Code by navigating to the root directory of the repository in a PowerShell (or a command prompt) window and type `code .`, press `Enter`. Visual Studio Code should open with `POWERBI-POWERSHELL` in the Explorer pane. Settings, configurations, and tasks are auto-loaded by the files in the `.vscode` directory.

To build, first-time run task `Restore` by pressing `Ctrl+P`, type `task Restore`, press `Enter`.
After restoring packages you can execute build by pressing `Ctrl+Shift+B`.

To run tests, highlight the name of the test with your cursor and press `Ctrl+P`, type `task DebugTest`, press `Enter`. If you don't want to build do `task DebugTest (No Build)` instead. Set your breakpoint in the test and press `F5` to start debugging by picking the dotnet process ID shown from `DebugTest`. Press `F5` once more to trigger the breakpoint.

### Creating a new Cmdlet and Unit Test

1. Create a new class in a Module project.
2. Either extend `PowerBIClientCmdlet` from `Microsoft.PowerBI.Common.Client` project or extend `PowerBICmdlet` from `Microsoft.PowerBI.Commands.Common` project if a client is not needed.
3. Implement class and optionally add other interfaces from `Microsoft.PowerBI.Common.Abstractions` for common parameters.
4. Add `[Cmdlet(CmdletVerb, CmdletName)]` attribute to class and provide `public const string` for `CmdletVerb` and `CmdletName`.
    * For `CmdletVerb`, pick from the System.Management.Automation.Verbs* classes.
5. Optionally add `[OutputType(typeof(type))]` if your Cmdlet class writes an object to the output stream.
6. Optionally add `[Alias("Get-Alias1", "Get-Alias2")]` to your Cmdlet class.
7. Update PowerShell manifest file (*.ps1), add the new cmdlet to `CmdletsToExport` and if you added an alias update `AliasesToExport` too.
8. Add a test class to verify your cmdlet by adding (replacing with your new class name):
```csharp
using (var ps = System.Management.Automation.PowerShell.Create())
{
    ProfileTestUtilities.ConnectToPowerBI(ps); // If login is needed
    ps.AddCommand(new CmdletInfo($"{<class name>.CmdletVerb}-{<class name>.CmdletName}", typeof(<class name>))); // Optionally .AddParameter(), use intellisense to see options
    var result = ps.Invoke();
    // Add asserts to verify
    TestUtilities.AssertNoCmdletErrors(ps);
}
```
9. If your test is interactive, add method attributes `[TestCategory("Interactive")]` and `[TestCategory("SkipWhenLiveUnitTesting")]` (last tells Live Unit Testing to skip it).
10. Run test to verify cmdlet is being called correctly.
11. Add parameters to cmdlet class and implement cmdlet logic in `ExecuteCmdlet()` method.

### Creating a new PowerShell module

1. Open PowerShell and set your current directory to the root of this repository.
2. In PowerShell, execute the following by supplying your module name:
```powershell
$moduleName = '<fill out with module name>'
$root = "$pwd"
cd .\src\Modules
mkdir $moduleName
cd ".\$moduleName"
dotnet new classlib --name "Commands.$moduleName" --framework netstandard2.0
dotnet new mstest --name "Commands.$moduleName.Test"

cd ".\Commands.$moduleName"
dotnet add package PowerShellStandard.Library --version 3.0.0-preview-01
dotnet add reference "$root\src\Common\Commands.Common\Commands.Common.csproj"
dotnet add reference "$root\src\Common\Common.Abstractions\Common.Abstractions.csproj"
dotnet add reference "$root\src\Common\Common.Client\Common.Client.csproj"
dotnet add reference "$root\src\Modules\Profile\Commands.Profile\Commands.Profile.csproj"
Remove-Item .\Class1.cs
New-ModuleManifest -Path ".\MicrosoftPowerBIMgmt.$moduleName.psd1" `
    -Author 'Microsoft Corporation' `
    -CompanyName 'Microsoft Corporation' `
    -Copyright 'Microsoft Corporation. All rights reserved.' `
    -RootModule "Microsoft.PowerBI.Commands.$moduleName.dll" `
    -ModuleVersion '1.0.0' `
    -Description "Microsoft Power BI PowerShell - $moduleName cmdlets for Microsoft Power BI" `
    -PowerShellVersion '3.0' `
    -PrivateData @{
         PSData=@{
             Tags=@('PowerBI', $moduleName)
             ProjectUri='https://github.com/Microsoft/powerbi-powershell'
         }
     }

```
3. Edit CSProj file in the current directory and add (update with your module name):
```xml
<PropertyGroup>
    <AssemblyName>Microsoft.PowerBI.Commands.<fill out with module name></AssemblyName>
    <RootNamespace>Microsoft.PowerBI.Commands.<fill out with module name></RootNamespace>
    <ReferenceWindowsAuthenticator>true</ReferenceWindowsAuthenticator>
</PropertyGroup>

<!-- NuGet Package Properties -->
<PropertyGroup>
    <IsPackable>true</IsPackable>
    <PackageId>MicrosoftPowerBIMgmt.<fill out with module name></PackageId>
    <Description>Microsoft Power BI PowerShell - <fill out with module name> cmdlets for Microsoft Power BI</Description>
    <PackageTags>PowerBI;<fill out with module name></PackageTags>
</PropertyGroup>
```
4. In the same CSProj file, for any packages add `<PrivateAssets>All</PrivateAssets>` inside `<PackageReference></PackageReference>` as this will prevent those packages becoming dependencies for the module.
5. In the same CSProj file, for any project references outside of the Modules folder (such as Commands.Common and Commands.Abstractions, but don't do it for Commands.Profile), add    `<PrivateAssets>All</PrivateAssets>` inside `<ProjectReference></ProjectReference>`.
6. Save the CSProj file and close.
7. Execute in PowerShell (same console window as before):
```powershell
cd "..\Commands.$moduleName.Test"
dotnet add package PowerShellStandard.Library --version 3.0.0-preview-01
dotnet add package Microsoft.PowerShell.SDK --version 6.0.1
dotnet add package Microsoft.PowerShell.Commands.Diagnostics --version 6.0.1
dotnet add package Microsoft.WSMan.Management --version 6.0.1
dotnet add reference "$root\src\Common\Commands.Common.Test\Commands.Common.Test.csproj"
dotnet add reference "$root\src\Common\Commands.Common\Commands.Common.csproj"
dotnet add reference "$root\src\Common\Common.Abstractions\Common.Abstractions.csproj"
dotnet add reference "$root\src\Common\Common.Authentication\Common.Authentication.csproj"
dotnet add reference "$root\src\Common\Common.Client\Common.Client.csproj"
dotnet add reference "$root\src\Modules\Profile\Commands.Profile.Test\Commands.Profile.Test.csproj"
dotnet add reference "$root\src\Modules\Profile\Commands.Profile\Commands.Profile.csproj"
dotnet add reference "$root\src\Modules\$moduleName\Commands.$moduleName\Commands.$moduleName.csproj"
```
8. Edit CSProj file in the current directory and add (update with your module name):
```xml
<PropertyGroup>
    <AssemblyName>Microsoft.PowerBI.Commands.<fill out with module name>.Test</AssemblyName>
    <RootNamespace>Microsoft.PowerBI.Commands.<fill out with module name>.Test</RootNamespace>
    <ReferenceWindowsAuthenticator>true</ReferenceWindowsAuthenticator>
</PropertyGroup>
```
9. Execute in PowerShell (same console window as before):
```powershell
cd $root
cd .\src
dotnet sln add "$root\src\Modules\$moduleName\Commands.$moduleName\Commands.$moduleName.csproj"
dotnet sln add "$root\src\Modules\$moduleName\Commands.$moduleName.Test\Commands.$moduleName.Test.csproj"

ii .\PowerBIPowerShell.sln
```
10. Visual Studio should open for you, add a new class to your module project.
11. Make the class public and extend PowerBICmdlet (`Ctrl+.` to add using statement and implement)
12. Decorate class `[Cmdlet(CmdletVerb, CmdletName)]` and define `CmdletVerb` and `CmdletName` as `public const string` (for verb use the System.Management.Automation.Verbs* classes to pick an approved verb, you can see in the list in PowerShell by doing Get-Verb).
13. Optionally add `[OutputType(typeof(type))]` to your class if you cmdlet returns a certain output.
14. Click on psd1 file in the module project, change Build Action to `Content`, edit csproj and add the following to the psd1 file `<Content>` element:
```xml
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
```

If the content extension is not `.psd1` or `.types.ps1xml` then add the following to the `Content` element too:
```xml
    <Pack>true</Pack>
    <PackagePath></PackagePath>
```
15. Save the project file and build the solution.
16. A `help` folder should appear under your project after you build, the files contain the documenation for your module and cmdlets which you can fill out.

> Note: The PowerShell Manifest file (*.psd1) might get its encoding messed up when pushing and pulling from Git.
> After pushing changes to Git, pull down changes and verify encoding looks correct. This issue may be fixed in PowerShell Core with [PR 2048](https://github.com/PowerShell/PowerShell-Docs/pull/2048).

## Developer Resources

### PowerShell Cmdlet Design Reference
* [Creating a cross-platform binary module with the .NET Core command-line interface tools](https://github.com/PowerShell/PowerShell/blob/master/docs/cmdlet-example/command-line-simple-example.md)
* [Cmdlet Development Guidelines][1]

### MSBuild and NuGet

* https://docs.microsoft.com/en-us/nuget/reference/msbuild-targets
* https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files

[1]: https://msdn.microsoft.com/en-us/library/ms714657(v=vs.85).aspx
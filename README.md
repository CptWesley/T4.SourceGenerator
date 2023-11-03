[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/CptWesley/T4.SourceGenerator/blob/main/LICENSE.md)  
[![NuGet](https://img.shields.io/nuget/v/T4.SourceGenerator)![NuGet](https://img.shields.io/nuget/dt/T4.SourceGenerator)](https://www.nuget.org/packages/T4.SourceGenerator/)  

<img src="https://raw.githubusercontent.com/CptWesley/T4.SourceGenerator/master/logo_1024x1024.png" width="178" height="178">

# T4.SourceGenerator
Modern Roslyn Source Generator for T4 templates, which transforms [T4 templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022) during compilation using the [mono T4 templating engine (Mono.TextTemplating)](https://github.com/mono/t4/). This means that `C#11` features are available to be used in your [T4 templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022).

## Installing

### Through NuGet wizard
Look for the `T4.SourceGenerator` on [NuGet](https://www.nuget.org/packages/T4.SourceGenerator/) and install the package.

### With PowerShell
To install the package through PowerShell, use the following command:
``` PS
Install-Package DT4.SourceGenerator
```

### Manual installation
To install the package manually, include the following snippet in your `.csproj` file:
``` XML
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <PackageReference Include="T4.SourceGenerator" Version="1.0.0" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

## Usage
After installing the [package](https://www.nuget.org/packages/T4.SourceGenerator/), add your [T4 templates](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022) with `.tt` or `.t4` extensions to your project.
Adding your template files might add some items to your `.csproj` file that look like this (for `Bar.tt`):
``` XML
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <None Remove="templates\Bar.tt" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Update="Templates\Bar.tt">
      <Generator>TextTemplatingFileGenerator</Generator>
    </AdditionalFiles>
  </ItemGroup>

  <ItemGroup>
    <Service Include="{508349b6-6b84-4df5-91f0-309beebad82d}" />
  </ItemGroup>

</Project>
```
These entries are used by the regular T4 template engine and are not necessary (and will most likely lead to conflicts) with the code generation provided by this package. Thus these lines should be removed.

Instead, your templates should be included in your `.csproj` as `AdditionalFiles` as such (for `Bar.tt`):

``` XML
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <AdditionalFiles Include="Bar.tt" />
  </ItemGroup>

</Project>
```

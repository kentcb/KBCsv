using System;
using System.Reflection;
using System.Resources;

// this is used to version artifacts. AssemblyInformationalVersion should use semantic versioning (http://semver.org/)
// be sure to update the corresponding VB.NET AssemblyInfoCommon.vb
[assembly: AssemblyInformationalVersion("3.0.0-beta")]
[assembly: AssemblyVersion("3.0.0.0")]

[assembly: AssemblyCompany("Kent Boogaart")]
[assembly: AssemblyProduct("KBCsv")]
[assembly: AssemblyCopyright("© Copyright. Kent Boogaart.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
[assembly: NeutralResourcesLanguage("en-US")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
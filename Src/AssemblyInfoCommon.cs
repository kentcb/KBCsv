using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

[assembly: AssemblyCompany("Kent Boogaart")]
[assembly: AssemblyProduct("KBCsv")]
[assembly: AssemblyCopyright("© Copyright 2005. Kent Boogaart.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
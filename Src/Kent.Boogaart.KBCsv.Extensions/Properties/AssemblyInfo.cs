using System.Reflection;
using System.Security;

[assembly: AssemblyTitle("Kent.Boogaart.KBCsv.Extensions")]
[assembly: AssemblyDescription("Contains extensions for KBCsv.")]
[assembly: SecurityTransparent]

#if !SILVERLIGHT
[assembly: AllowPartiallyTrustedCallers]
#endif
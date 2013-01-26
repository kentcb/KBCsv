Imports System
Imports System.Reflection
Imports System.Resources
Imports System.Runtime.InteropServices

#If FX35 Then
Imports System.Security.Permissions
#End If

' this is used to version artifacts. AssemblyInformationalVersion should use semantic versioning (http://semver.org/)
' be sure to update the corresponding C# AssemblyInfoCommon.cs
<Assembly: AssemblyInformationalVersion("2.0.0-beta")> 
<Assembly: AssemblyVersion("2.0.0.0")> 

<Assembly: AssemblyCompany("Kent Boogaart")> 
<Assembly: AssemblyProduct("KBCsv")> 
<Assembly: AssemblyCopyright("© Copyright. Kent Boogaart.")> 
<Assembly: AssemblyTrademark("")> 
<Assembly: AssemblyCulture("")> 
<Assembly: CLSCompliant(True)> 
<Assembly: ComVisible(False)> 
<Assembly: NeutralResourcesLanguage("en-US")> 

#If FX35 Then
<Assembly: SecurityPermission(SecurityAction.RequestMinimum, Execution = true)>
#End If

#If DEBUG Then
<Assembly: AssemblyConfiguration("Debug")>
#Else
<Assembly: AssemblyConfiguration("Release")> 
#End If
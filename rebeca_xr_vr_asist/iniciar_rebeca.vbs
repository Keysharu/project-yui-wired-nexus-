' =============================================================================
' LAUNCHER VBS: CYBER-COMPANION (EL TALLER DE LA GATITA TECH)
' =============================================================================
' Inicia Rebeca en segundo plano sin ventana de consola negra visible.
' =============================================================================

On Error Resume Next

Set WshShell = CreateObject("WScript.Shell")
Set FSO = CreateObject("Scripting.FileSystemObject")

currentDir = FSO.GetParentFolderName(WScript.ScriptFullName)
WshShell.CurrentDirectory = currentDir
mainScript = currentDir & "\main.py"

If Not FSO.FileExists(mainScript) Then
    MsgBox "No se encontró el script principal:" & vbCrLf & mainScript, vbCritical, "Error Cyber-Companion"
    WScript.Quit 1
End If

' Detección del intérprete pythonw (sin consola)
pythonwPath = ""
If FSO.FileExists("C:\Python314\pythonw.exe") Then
    pythonwPath = "C:\Python314\pythonw.exe"
ElseIf FSO.FileExists("C:\Python312\pythonw.exe") Then
    pythonwPath = "C:\Python312\pythonw.exe"
ElseIf FSO.FileExists("C:\Python311\pythonw.exe") Then
    pythonwPath = "C:\Python311\pythonw.exe"
Else
    pythonwPath = "pythonw.exe"
End If

' Ejecutar en segundo plano (0 = Oculto, False = No bloquear)
cmd = """" & pythonwPath & """ """ & mainScript & """"
ret = WshShell.Run(cmd, 0, False)

If Err.Number <> 0 Then
    MsgBox "Error al iniciar Rebeca:" & vbCrLf & Err.Description & vbCrLf & vbCrLf & "Comando intentado: " & cmd, vbCritical, "Error Cyber-Companion"
End If

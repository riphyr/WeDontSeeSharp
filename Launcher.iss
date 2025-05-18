[Setup]
AppName=We Don't See Sharp
AppVersion=1.0
DefaultDirName={pf}\WeDontSeeSharp
DefaultGroupName=We Don't See Sharp
UninstallDisplayIcon={app}\WeDontSeeSharp.exe
OutputDir=.
OutputBaseFilename=Installer_WeDontSeeSharp

[Files]
Source: "D:\GameBuild\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\We Don't See Sharp"; Filename: "{app}\WeDontSeeSharp.exe"
Name: "{group}\Désinstaller"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\WeDontSeeSharp.exe"; Description: "Lancer le jeu"; Flags: nowait postinstall skipifsilent

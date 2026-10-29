; ============================================================
;  CFAS — сценарій інсталяції (Inno Setup 6)
;  Розташування: <репозиторій>\Installer\CFAS.iss
;  Структура папки Installer:
;     CFAS.iss
;     cfas.ico                        — іконка програми та ярликів
;     Database\create_db.sql          — скрипт створення БД
;     Prereq\SqlLocalDB.msi           — (необов'язково) інсталятор SQL Server LocalDB
;     Docs\Інструкція_користувача_CFAS.docx — (необов'язково) інструкція користувача
;  Перед компіляцією зберіть проєкт CFAS у конфігурації Release.
; ============================================================

#define MyAppName      "CFAS"
#define MyAppFullName  "CFAS — Комп’ютерна фірма «Тех-Сервіс»"
#define MyAppVersion   "0.9.2.0"
#define MyAppPublisher "Острянський Денис, група 581-ІПЗ"
#define MyAppExeName   "CFAS.exe"
#define MyAppIcon      "cfas.ico"
#define SourceDir      "..\bin\Release"

; Якщо поруч є SqlLocalDB.msi — він буде включений у пакет
#ifexist AddBackslash(SourcePath) + "Prereq\SqlLocalDB.msi"
  #define HasLocalDb
#endif

[Setup]
AppId={{ca9e4a79-003c-45be-9535-20bc83ad472b}
AppName={#MyAppFullName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=CFAS_Setup_{#MyAppVersion}
SetupIconFile=cfas.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}

[Languages]
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Програма та її налаштування (рядок підключення до БД)
Source: "{#SourceDir}\CFAS.exe";        DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\CFAS.exe.config"; DestDir: "{app}"; Flags: ignoreversion
; Іконка для ярликів
Source: "{#MyAppIcon}"; DestDir: "{app}"; Flags: ignoreversion
; Скрипт створення БД (виконується програмою при першому запуску)
Source: "Database\create_db.sql"; DestDir: "{app}\Database"; Flags: ignoreversion
; Документація
Source: "Docs\*"; DestDir: "{app}\Docs"; Flags: ignoreversion recursesubdirs skipifsourcedoesntexist
; Інсталятор SQL Server LocalDB (лише якщо він є в папці Prereq)
#ifdef HasLocalDb
Source: "Prereq\SqlLocalDB.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsLocalDbInstalled
#endif

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppIcon}"
Name: "{group}\Видалити {#MyAppName}"; Filename: "{uninstallexe}"; IconFilename: "{app}\{#MyAppIcon}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppIcon}"; Tasks: desktopicon

[Run]
#ifdef HasLocalDb
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\SqlLocalDB.msi"" /qn IACCEPTSQLLOCALDBLICENSETERMS=YES"; StatusMsg: "Встановлення SQL Server LocalDB..."; Flags: waituntilterminated; Check: not IsLocalDbInstalled
#endif
Filename: "{app}\{#MyAppExeName}"; Description: "Запустити {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
// Перевірка наявності SQL Server LocalDB
function IsLocalDbInstalled: Boolean;
begin
  Result := RegKeyExists(HKLM64, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions');
end;

// Перевірка наявності .NET Framework 4.8 (Release >= 528040)
function IsDotNet48Installed: Boolean;
var
  Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release)
            and (Release >= 528040);
end;

function InitializeSetup: Boolean;
begin
  Result := True;

  if not IsDotNet48Installed then
  begin
    MsgBox('Для роботи CFAS потрібен Microsoft .NET Framework 4.8. Встановіть його і запустіть інсталяцію повторно.',
           mbError, MB_OK);
    Result := False;
    Exit;
  end;

#ifndef HasLocalDb
  if not IsLocalDbInstalled then
  begin
    Result := MsgBox('На цьому комп’ютері не знайдено SQL Server LocalDB, який потрібен для роботи CFAS.' + #13#10 +
                     'Інсталятор його не містить. Встановіть LocalDB окремо (SqlLocalDB.msi).' + #13#10#13#10 +
                     'Продовжити інсталяцію CFAS?', mbConfirmation, MB_YESNO) = IDYES;
  end;
#endif
end;

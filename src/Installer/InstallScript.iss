﻿#define PluginBuildDir SourcePath + "..\bin\"
#define BepInEx32Dir SourcePath + "BepInEx32"
#define BepInEx64Dir SourcePath + "BepInEx64"
#define BepInExIl2cpp64Dir SourcePath + "BepInExIl2cpp64"
#define AppVersion GetVersionNumbersString(PluginBuildDir + "LoveMachine.Core\LoveMachine.Core.dll")

; We have a lot of plugins, so we just find them all and put them in here
; This way the script will handle new plugins by itself and we can forget about it
#dim Plugins[100]
#define PluginCount 0

#define GetPluginId(Index) Plugins[Index]
#define GetPluginInfoIni(Index) SourcePath + "..\" + GetPluginId(Index) + "\PluginInfo.ini"
#define GetGameNameEN(Index) ReadIni(GetPluginInfoIni(Index), GetPluginId(Index), "NameEN")
#define GetGameNameJP(Index) ReadIni(GetPluginInfoIni(Index), GetPluginId(Index), "NameJP")
#define GetGameRegSubKey(Index) ReadIni(GetPluginInfoIni(Index), GetPluginId(Index), "RegSubKey")
#define GetGameRegName(Index) ReadIni(GetPluginInfoIni(Index), GetPluginId(Index), "RegName")
#define GetGameArchitecture(Index) ReadIni(GetPluginInfoIni(Index), GetPluginId(Index), "Architecture")

#define I 0
#sub AddGameEntry
    #define PluginName FindGetFileName(FindHandle)
    #if Pos("LoveMachine.Core", PluginName) != 1
        #expr Plugins[I] = PluginName
        #expr I = I + 1
    #endif
#endsub

; Get all plugins from the build via file search
#define FindHandle
#define FindResult
#for {FindHandle = FindResult = FindFirst(PluginBuildDir + "LoveMachine*", faDirectory); \
    FindResult; \
    FindResult = FindNext(FindHandle)} \
        AddGameEntry
#if FindHandle
    #expr PluginCount = I
    #expr FindClose(FindHandle)
#endif

[Setup]
AppName=LoveMachine
AppPublisher=Sauceke
AppPublisherURL=sauceke.github.io
AppVersion={#AppVersion}
DefaultDirName={localappdata}\LoveMachine
DefaultGroupName=LoveMachine
UninstallDisplayIcon={app}\Inno_Setup_Project.exe
Compression=lzma2
SolidCompression=yes
OutputDir=bin
OutputBaseFilename=LoveMachineInstaller
WizardStyle=modern
DisableDirPage=yes
DisableWelcomePage=no
PrivilegesRequired=lowest
SetupLogging=yes

[Languages]
Name: "en"; MessagesFile: "EN.isl,compiler:Default.isl"
Name: "jp"; MessagesFile: "compiler:Languages/Japanese.isl,JP.isl"

[Files]
; BepInEx files
#sub BepInExFileEntry
    Source: "{#BepInEx32Dir}\*"; DestDir: {code:GetDir|{#I}}; \
        Flags: recursesubdirs; Check: ShouldInstallBepInEx({#I}, 'x86')
    Source: "{#BepInEx64Dir}\*"; DestDir: {code:GetDir|{#I}}; \
        Flags: recursesubdirs; Check: ShouldInstallBepInEx({#I}, 'x64')
    Source: "{#BepInExIl2cpp64Dir}\*"; DestDir: {code:GetDir|{#I}}; \
        Flags: recursesubdirs; Check: ShouldInstallBepInEx({#I}, 'il2cpp-x64')
#endsub
#if DirExists(BepInEx32Dir) && DirExists(BepInEx64Dir)
    #for {I = 0; I < PluginCount; I++} BepInExFileEntry
#endif

; LoveMachine files
#sub PluginFileEntry
    Source: "{#PluginBuildDir}{#GetPluginId(I)}\*"; DestDir: {code:GetDir|{#I}}; \
        Flags: recursesubdirs ignoreversion; Check: IsDirSelected({#I})
    Source: "..\{#GetPluginId(I)}\tweaks\*"; DestDir: {code:GetDir|{#I}}; \
        Flags: recursesubdirs ignoreversion skipifsourcedoesntexist onlyifdoesntexist;
#endsub
#for {I = 0; I < PluginCount; I++} PluginFileEntry

[Icons]
Name: "{group}\Inno_Setup_Project"; Filename: "{app}\Inno_Setup_Project.exe"

[Code]
const
    PageSize = 4;
    PluginCount = {#PluginCount};
var
    // The directory prompts don't fit all in one page, so we need more pages
    // This is way too many pages but whatever 
    DirPages: array[0..{#PluginCount}] of TInputDirWizardPage;
    Old_WizardForm_NextButton_OnClick: TNotifyEvent;
    PlaceholderDir: String;

// The ID of the plugin at the given index (e. g. 'LoveMachine.KK')
function GetPluginId(Index: Integer): String;
begin
    case Index of
        #sub IdMapping
            {#I}: Result := '{#GetPluginId(I)}';
        #endsub
        #for {I = 0; I < PluginCount; I++} IdMapping
    end;
end;

function GetGameNameEN(Index: Integer): String;
begin
    case Index of
        #sub EngNameMapping
            {#I}: Result := '{#GetGameNameEN(I)}';
        #endsub
        #for {I = 0; I < PluginCount; I++} EngNameMapping
    end;
end;

function GetGameNameJP(Index: Integer): String;
begin
    case Index of
        #sub JpNameMapping
            {#I}: Result := '{#GetGameNameJP(I)}';
        #endsub
        #for {I = 0; I < PluginCount; I++} JpNameMapping
    end;
end;

// The human-readable name of the game at the given index
function GetGameName(Index: Integer): String;
begin
    Result := GetGameNameEN(Index);
    if ActiveLanguage = 'jp' then Result := GetGameNameJP(Index);
    // this shouldn't happen, but whatever
    if Result = '' then Result := GetPluginId(Index);
end;

function GetGameArchitecture(Index: Integer): String;
begin
    case Index of
        #sub ArchitectureMapping
            {#I}: Result := '{#GetGameArchitecture(I)}';
        #endsub
        #for {I = 0; I < PluginCount; I++} ArchitectureMapping
    end;
end;

// Tries to guess the root directory of the game at the given index
function GuessGamePath(Index: Integer): String;
begin
    case Index of
        #sub PathMapping
            {#I}: RegQueryStringValue(HKCU, '{#GetGameRegSubKey(I)}', '{#GetGameRegName(I)}', Result);
        #endsub
        #for {I = 0; I < PluginCount; I++} PathMapping
    end;
    if not DirExists(Result) then
        Result := ''
end;

// Tells us where on which page the install dir box for the given index is located
procedure GetPageAndIndex(Index: Integer; out Page: Integer; out IndexInPage: Integer);
begin
    Page := Index / PageSize;
    IndexInPage := Index mod PageSize;
end;

function GetDir(Index: String): String;
var
    Page: Integer;
    IndexInPage: Integer;
begin
    GetPageAndIndex(StrToInt(Index), Page, IndexInPage);
    Result := DirPages[Page].Values[IndexInPage];
end;

function IsDirSelected(Index: Integer): Boolean;
begin
    Result := GetDir(IntToStr(Index)) <> '';
end;

function ShouldInstallBepInEx(Index: Integer; Architecture: String): Boolean;
var
    BepInExConfigDir: String;
begin
    BepInExConfigDir := AddBackslash(GetDir(IntToStr(Index))) + 'BepInEx\config';
    Result := (not DirExists(BepInExConfigDir)) and (GetGameArchitecture(Index) = Architecture);
end;

function GetPreviousDataKey(Index: Integer): String;
begin
    Result := 'GameDir.' + GetPluginId(Index);
end;

function ValidateGameDir(Path: String): Boolean;
var
    FindRec: TFindRec;
    WarningMsg: String;
begin
    Result := True;
    if (not FindFirst(AddBackslash(Path) + '*_Data', FindRec)) and (Path <> PlaceholderDir) then
    begin
        WarningMsg := Format(CustomMessage('NotAGameDir'), [Path]);
        MsgBox(WarningMsg, mbError, MB_OK);
        Result := False;
    end;
end;

function ValidateDirPage(Page: TWizardPage; DirCount: Integer): Boolean;
var
    DirPage: TInputDirWizardPage;
    IndexInPage: Integer;
begin
    Result := True;
    DirPage := Page as TInputDirWizardPage;
    for IndexInPage := 0 to DirCount - 1 do
    begin
        if not ValidateGameDir(DirPage.Values[IndexInPage]) then
        begin
            Result := False;
            break;
        end;
    end;
end;

function OnDirPageNextClick(Page: TWizardPage): Boolean;
begin
    Result := ValidateDirPage(Page, PageSize);
end;

function OnLastDirPageNextClick(Page: TWizardPage): Boolean;
var
    LastPage: Integer;
    LastIndex: Integer;
begin
    GetPageAndIndex(PluginCount - 1, LastPage, LastIndex);
    Result := ValidateDirPage(Page, LastIndex + 1);
end;

procedure AddDirPrompts;
var
    Index: Integer;
    Page: Integer;
    IndexInPage: Integer;
    PrevPageID: Integer;
begin
    for Index := 0 to PluginCount - 1 do
    begin
        GetPageAndIndex(Index, Page, IndexInPage);
        if Page = 0 then
            PrevPageID := wpSelectDir
        else
            PrevPageID := DirPages[Page - 1].ID;
        if IndexInPage = 0 then
            DirPages[Page] := CreateInputDirPage(PrevPageID,
                Format(CustomMessage('SelectPathTitle'), [Page + 1]),
                CustomMessage('SelectPath'),
                '', False, '');
        DirPages[Page].Add(GetGameName(Index));
        DirPages[Page].Values[IndexInPage] :=
            GetPreviousData(GetPreviousDataKey(Index), GuessGamePath(Index));
        DirPages[Page].OnNextButtonClick := @OnDirPageNextClick;
    end;
    DirPages[Page].OnNextButtonClick := @OnLastDirPageNextClick;
end;

// based on https://stackoverflow.com/a/31706698
procedure New_WizardForm_NextButton_OnClick(Sender: TObject);
var
    Index: Integer;
    Page: Integer;
    IndexInPage: Integer;
begin
    for Index := 0 to PluginCount - 1 do
    begin
        GetPageAndIndex(Index, Page, IndexInPage);
        if DirPages[Page].Values[IndexInPage] = '' then
            // Force value to pass validation
            DirPages[Page].Values[IndexInPage] := PlaceholderDir;
    end;
    Old_WizardForm_NextButton_OnClick(Sender);
    for Index := 0 to PluginCount - 1 do
    begin
        GetPageAndIndex(Index, Page, IndexInPage);
        if DirPages[Page].Values[IndexInPage] = PlaceholderDir then
            DirPages[Page].Values[IndexInPage] := '';
    end;
end;

procedure CheckIntiface;
var
    ErrorCode: Integer;
begin
    if not DirExists(AddBackslash(ExpandConstant('{commonpf32}')) + 'IntifaceCentral') then
        if MsgBox(CustomMessage('InstallIntiface'), mbConfirmation, MB_YESNO) = IDYES then
            if not ShellExec('open', 'https://intiface.com/central/', '', '', SW_SHOW, ewNoWait, ErrorCode) then
                MsgBox(SysErrorMessage(ErrorCode), mbError, MB_OK);
end;

procedure InitializeWizard;
begin
    PlaceholderDir := ExpandConstant('{%TEMP}');
    CheckIntiface;
    AddDirPrompts;
    Old_WizardForm_NextButton_OnClick := WizardForm.NextButton.OnClick;
    WizardForm.NextButton.OnClick := @New_WizardForm_NextButton_OnClick;
end;

procedure RegisterPreviousData(PreviousDataKey: Integer);
var
    Index: Integer;
    Page: Integer;
    IndexInPage: Integer;
    DirPath: String;
begin
    for Index := 0 to PluginCount - 1 do
    begin
        GetPageAndIndex(Index, Page, IndexInPage);
        DirPath := DirPages[Page].Values[IndexInPage];
        if DirExists(DirPath) then
            SetPreviousData(PreviousDataKey, GetPreviousDataKey(Index), DirPath); 
    end;
end;

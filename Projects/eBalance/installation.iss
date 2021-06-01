
#define VersionNumber '1.6.3'

#define pathEricCurrentVersion 'EricWrapper\ERIC_16_1_12_56434'
#define pathEbk 'eBalanceKit\eBalanceKit'
#define pathEbkBin 'eBalanceKit\eBalanceKit\bin\Release'
#define pathEbkConfigBin 'eBalanceKitConfig\bin\Release'
#define pathEbkDatabaseManagementBin 'DatabaseManagement\DatabaseManagement\bin\Release'
#define pathEbkLogviewerBin 'eBalanceKitManagement\eBalanceKitManagement\bin\Release'
#define pathEbkBackupServiceBin 'EBilanzBackupService\EBilanzBackupServiceSetup\Release'
#define pathEbkMappingTemplateUpgradeBin 'MappingTemplateUpgrade\MappingTemplateUpgrade\bin\Release'

#define pathInstallationResources 'InstallationResources'

#define defaultInstallationDestination 'AvenDATA\eBilanz-Kit'

#define exeConfig 'Config.exe'
#define exeLogviewer 'eBalanceKitManagement.exe'
#define exeDatabasemanagement 'DatabaseManagement.exe'
#define exeEbk 'eBalanceKit.exe'

#define appId 'AvenData-eBilanzKit-Installation'
#define regEntry 'eBalanceKit'


#define numberOfElements '25'

[Setup]
AppName=eBilanz-Kit
AppVersion={#VersionNumber}
AppCopyright=Copyright © AvenDATA GmbH 2011-2012
AppId={#appId}
ExtraDiskSpaceRequired=1
DefaultDirName={pf}\{#defaultInstallationDestination}
DefaultGroupName={#defaultInstallationDestination}
CreateUninstallRegKey=yes
AllowNoIcons=True
AlwaysShowGroupOnReadyPage=True
AlwaysShowDirOnReadyPage=True
UninstallDisplayName=eBilanz-Kit
UninstallDisplayIcon={app}\logo.ico
UninstallFilesDir={app}\Uninstall
AppPublisher=AvenDATA GmbH
AppPublisherURL=http://www.avendata.de
AppSupportURL=http://www.ebilanz-kit.de
AppComments=eBilanz-Kit einfach . effizient . systemunabhängig
AppContact=AvenDATA GmbH
AppSupportPhone=+49 30 700 157 500
SetupIconFile=eBalanceKit\eBalanceKit\logo.ico
VersionInfoCompany=AvenDATA GmbH
VersionInfoDescription=eBilanz-Kit einfach . effizient . systemunabhängig
VersionInfoCopyright=Copyright © AvenDATA GmbH 2012
VersionInfoProductName=eBilanz-Kit
VersionInfoProductVersion={#VersionNumber}
VersionInfoVersion={#VersionNumber}
OutputDir=Installation
WizardImageFile={#pathInstallationResources}\ebilanz-banner.bmp
WizardSmallImageFile={#pathInstallationResources}\logo.bmp
WizardImageBackColor=clWhite
WizardImageStretch=False
DisableDirPage=No
DisableProgramGroupPage=No
DirExistsWarning=no

[Types]
Name: "default"; Description: "default installation"

[Languages]
;Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"


[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Icons]
Name: "{group}\{cm:eBalanceKit}"; Filename: "{app}\{#exeEbk}"
Name: "{group}\{cm:Config}"; Filename: "{app}\{#exeConfig}"
Name: "{group}\{cm:DatabaseManagement}"; Filename: "{app}\{#exeDatabasemanagement}"
Name: "{group}\{cm:LogViewer}"; Filename: "{app}\{#exeLogviewer}"
Name: "{commondesktop}\{cm:eBalanceKit}"; Filename: "{app}\{#exeEbk}"; Tasks: desktopicon

[Run]
Filename: {app}\{#exeConfig}; Description: {cm:lblStartConfig}; Flags: nowait postinstall skipifsilent
Filename: {app}\Documents\Handbuch.pdf; Description: {cm:lblOpenManual}; Flags: shellexec nowait postinstall skipifsilent

[CustomMessages]
de.installRedistributable=Überprüfe und installiere MS Visual C++ Redistributable %1 ...
de.uninstallOldVersion=Entferne vorherige Installationen ...
de.eBalanceKit=eBilanz-Kit
de.LogViewer=eBilanz-Kit LogViewer
de.DatabaseManagement=eBilanz-Kit Datenbank-Verwaltung
de.Config=eBilanz-Kit Konfiguration
de.lblStartConfig=Konfiguration starten
de.lblOpenManual=Handbuch öffnen
MsgNewerVersionInstalled=It appears that the existing installation is newer than this update.%n%nThe existing installation is build %s.  This update will change the installation to build %s %n%nDo you want to continue with the update installation?
de.MsgNewerVersionInstalled=Es scheint bereits eine neuere Version dieses Programms installiert zu sein.%n%nDie bereits installierte Build-Version ist %s.  Diese Installation würde Build-Version %s installation.%n%nSoll der Vorgang fortgesetzt werden?%n(Die neuere Version wird deinstalliert.)

[Code]

procedure InstallLib2008;
var  
  ResultCode: Integer;            
  originalStatusLabel : string;
begin                                                  
  originalStatusLabel := WizardForm.STATUSLABEL.Caption;        
  WizardForm.STATUSLABEL.Caption := ExpandConstant('{cm:installRedistributable,2008}');
//Filename: "{app}\Setup\vcredist_x86 2008.exe"; WorkingDir: "{app}\Setup"; Description: "install cpp lib 2008"; Parameters: "/q"; StatusMsg: "{cm:installRedistributable,2008}";
  Exec(ExpandConstant('{tmp}\vcredist_x86 2008.exe'), '/q', '', SW_SHOW, ewWaitUntilTerminated, ResultCode); 
  WizardForm.STATUSLABEL.Caption := originalStatusLabel;
end;

procedure InstallLib2010;
var  
  ResultCode: Integer;           
  originalStatusLabel : string;
begin                          
  originalStatusLabel := WizardForm.STATUSLABEL.Caption;                                                                                               
  WizardForm.STATUSLABEL.Caption := ExpandConstant('{cm:installRedistributable,2010}');

//Filename: "{app}\Setup\vcredist_x86 2010.exe"; WorkingDir: "{app}\Setup"; Parameters: "/q /norestart"; Description: "install cpp lib 2010"; StatusMsg: "{cm:installRedistributable,2010}";
  Exec(ExpandConstant('{tmp}\vcredist_x86 2010.exe'), '/q /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);

  WizardForm.STATUSLABEL.Caption := originalStatusLabel;
  
end;


function GetUninstaller: string;
var
  i : integer;
  codes: array [0..{#numberOfElements}] of string;
  uninstallstring : string;
begin                                                            
  codes[0] := '{3A34DD72-0BA6-4C71-B0B0-0B7C21FA0830}';
  codes[1] := '{3497D0AF-26B9-4279-A28D-9C843E5C8C90}';
  codes[2] := '{E7B785B3-4B34-439A-A0F8-5600F07C72EC}';
  codes[3] := '{B955D797-5A1E-4A3C-822A-BF2368DDBCB0}';
  codes[4] := '{E0FD31F6-E146-40D1-8904-9539D76C95CA}';
  codes[5] := '{DDD23784-5D85-4E86-936F-AE35E9454ABD}';
  codes[6] := '{BED03B30-B9E4-4E03-BD88-3C0DB2185893}';
  codes[7] := '{611110CC-0C64-411C-AD8A-1D50DD0B1EB4}';
  codes[8] := '{CA97529C-1F1E-43EA-9DF0-152AC3195C04}';
  codes[9] := '{35784EFE-E9C0-461A-8B35-47615EAC63C7}';
  codes[10] := '{F3FC4B14-A63C-4728-8BE9-035D90F4CC3F}';
  codes[11] := '{DBC414D2-220F-4EF4-8A5E-0908F3312A96}';
  codes[12] := '{8CE4A49C-759E-47B9-99AA-87790CD69B82}';


  codes[13] := '{845C5099-E073-43D3-BB5D-22CB652BDC44}';
  codes[14] := '{F8E0A1DF-76A3-4868-B5A0-16E260C62505}';
  codes[15] := '{3A34DD72-0BA6-4C71-B0B0-0B7C21FA0830}';
  codes[16] := '{3497D0AF-26B9-4279-A28D-9C843E5C8C90}';
  codes[17] := '{1795B660-DDEE-4918-9E0E-E1355D5A1893}';
  codes[18] := '{26EF4C6C-51B8-45D5-BF60-5FF3F1DAAC1A}';
  codes[19] := '{CB94ECD6-F054-4894-9438-607E146850FA}';
  codes[20] := '{05B1BFD8-73D0-4E5D-80D6-B7A7B7385DC8}';
  codes[21] := '{818E228F-4804-41F8-A5E3-74DB476E1620}';
  codes[22] := '{E7B785B3-4B34-439A-A0F8-5600F07C72EC}';


  while i <= {#numberOfElements} do
  begin
    // find the Uninstaller of the current installed version ...
    if not RegQueryStringValue(HKLM,
        Format('Software\Microsoft\Windows\CurrentVersion\Uninstall\%s', [codes[i]]),
              'UninstallString',
              uninstallstring)
    then
    begin
      // ... or set the string to empty
      uninstallstring := '';
    end
    else
      break;

      
    // check if it is a 32bit installation on a 64bit computer ...
    if not RegQueryStringValue(HKLM,
        Format('Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\%s', [codes[i]]),
              'UninstallString',
              uninstallstring)
    then
    begin
      uninstallstring := '';
    end
    else
      break;
    
    i := i + 1;
  end;
  
  Result := uninstallstring;
  
end;


function SplitParameters(FileName: string; var Parameters: string): string;
var
  i : integer;
  InQuote : boolean;
begin
  // Standardergebnis, wenn nichts übergeben wird
  Result := '';
  Parameters := '';
  if (FileName = '') then exit;
 
  // loop to the first whitespace, but
  // take care of quotes!
  InQuote := false;
  i := 1;
  while i <= length(FileName) do
  begin
    if (FileName[i] = '"') then InQuote := not InQuote;
    if (FileName[i] = ' ') and (not Inquote) then break;
    i := i + 1;
  end;
 
  // split Filename & Parameter
  Result := RemoveQuotes(Copy(FileName, 1, i - 1));
  Parameters := Copy(FileName, i + 1, length(FileName));
end;
 
function CopyFileToTemp(FileName: string): boolean;
begin
  Result := FileCopy(FileName,
    Format('%s\%s', [ExpandConstant('{tmp}'), ExtractFileName(FileName)]),
    false);
end;
 
function GetTempName(FileName: string): string;
begin
  Result :=
    Format('%s\%s', [ExpandConstant('{tmp}'), ExtractFileName(FileName)]);
end; 


function UninstallOldVersion(const RegKeyName: string;
  const ShowErrorMsg, SilentMode: boolean): boolean;
var
  Uninstaller,
  UninstallerParams,
  UninstallerDatFile : string;
  ExecResult : integer;
begin
  Result := true;
     
  // find Uninstaller  ...

  Uninstaller :=
    SplitParameters(GetUninstaller, UninstallerParams)     
  // is it in silent mode?
  if SilentMode then
    UninstallerParams := Format('%s /passive', [UninstallerParams]);
     
  // ... & check if uninstaller found, ...
  if (Uninstaller <> '') then
  begin
     
  // The uninstall string has the /i parameter but we need the /x parameter for uninstallation
    StringChange(UninstallerParams, '/I', '/X');
     
  // ... & start
      Exec(
        Uninstaller,
        Format('%s', [UninstallerParams]),
        ExpandConstant('{tmp}'),
        SW_SHOWNORMAL,
        ewWaitUntilTerminated,
        ExecResult);
     
  // result
      Result := ExecResult = 0;
     
  // show messagebox with error?
      if (not Result) and (ShowErrorMsg) then
        MsgBox(Format('%s (%d)', [SysErrorMessage(ExecResult), ExecResult]),
          mbError, MB_OK);
    //end else
    //  Result := false;
  end;
end;

procedure UninstallPreviousEbk;
var
  originalStatusLabel : string;
begin                          
  originalStatusLabel := WizardForm.STATUSLABEL.Caption;  
  WizardForm.STATUSLABEL.Caption := ExpandConstant('{cm:uninstallOldVersion}');
  UninstallOldVersion('eBilanz-Kit', true, true);
  WizardForm.STATUSLABEL.Caption := originalStatusLabel;

end; 
    

[Files]
Source: "Q:\Softwareentwicklung\Projects\eBilanz-Kit\Resources\vc_redist\vcredist_x86 2008.exe"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall; AfterInstall: InstallLib2008
Source: "Q:\Softwareentwicklung\Projects\eBilanz-Kit\Resources\vc_redist\vcredist_x86 2010.exe"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall; AfterInstall: InstallLib2010
Source: "{#pathEbk}\logo.ico"; DestDir: "{app}"; Flags: ignoreversion; AfterInstall: UninstallPreviousEbk

;ERIC
Source: "{#pathEricCurrentVersion}\plugins\check34a2008.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\check34a2009.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\check34a2010.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkBilanz.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkDatenabholung.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkElsterLohn2.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkEst.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkEst2010.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkEuer.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkFein2009.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkFein2010.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkGewSt.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkGewStZ.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkKapesta2009.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkKapesta2010.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkKapesta2011.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkKapestin.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkKSt2007.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkKSt2008.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkKSt2009.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkKSt2010.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkLStA.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkUst.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\plugins\checkUStVA.dll"; DestDir: "{app}\ERIC_16_1_12_56434\plugins"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\1999_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\1999_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\1999_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\1999_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\1999_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2000_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2000_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2000_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2000_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2000_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2001_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2001_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2001_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2001_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2001_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2002_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2002_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2002_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2002_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2002_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2002_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2002_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2003_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2003_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2003_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2003_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2003_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2003_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2003_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2004_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2004_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2004_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2004_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2004_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2004_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2004_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2005_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2005_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2005_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2005_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2005_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2005_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2005_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2005_77.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2006_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2006_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2006_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2006_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2006_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2006_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2006_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2006_77.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2007_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2007_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2007_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2007_30.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2007_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2007_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2007_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2007_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2007_77.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_13.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_30.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_31.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2008_77.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_13.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_30.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_31.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_32.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_33.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_77.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_90.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2009_kapesta.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_13.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_30.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_31.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_32.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_33.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_77.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_90.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_95.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2010_kapesta.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011HJ1_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_10.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_13.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_20.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_21.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_30.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_31.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_32.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_33.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_50.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_77.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_90.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_95.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2011_kapesta.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2012_57.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2012_58.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2012_64.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\2012_kapesta.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\Bilanz.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\DUeAbmelden.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\DUeAnmelden.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\DUeUmmelden.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\kapestin_ab_2009.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\kapestin_ab_2011.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\kapestin_ab_2012.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\printxml\zmdo.xml"; DestDir: "{app}\ERIC_16_1_12_56434\printxml"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\ericapi.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\ericbasis.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\ericcrypt.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\ericplugin.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\ericprint.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\erictransfer.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\ericutil.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\ericxml.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\eSigner.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\icudt48.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\icuin48.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\icuuc48.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\log4cpp.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion
Source: "{#pathEricCurrentVersion}\xerces.dll"; DestDir: "{app}\ERIC_16_1_12_56434"; Flags: ignoreversion

;Taxonomy
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-calculation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-presentation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-reference-gaap-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-reference-gaap.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-reference.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2011-09-14\de-bra-2011-09-14.xsd"; DestDir: "{app}\Taxonomy\base\de-bra-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-calculation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-label-guidance-de.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-presentation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-reference-gaap-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-reference-gaap.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-reference.xml"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-bra-2012-06-01\de-bra-2012-06-01.xsd"; DestDir: "{app}\Taxonomy\base\de-bra-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-calculation-notes.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-dimensions-definition.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-dimensions-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-dimensions-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-dimensions.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-changesEquityStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-managementReport.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-notes.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-otherReportElements.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-presentation-transfersCommercialCodeToTax.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-reference.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-shell-fiscal-label.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-shell-fiscal-presentation.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16-shell.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2010-12-16\de-gaap-ci-2010-12-16.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-notes.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions-definition.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-label-fiscal-de.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-changesEquityStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-detailedInformation.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-determinationOfTaxableIncomeSpecialCases.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-managementReport.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-notes.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-otherReportElements.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-transfersCommercialCodeToTax.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-reference.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-shell.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-notes.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions-definition.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-fiscal-de.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-guidance-de.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-changesEquityStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-detailedInformation.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-determinationOfTaxableIncomeSpecialCases.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-managementReport.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-notes.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-otherReportElements.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-transfersCommercialCodeToTax.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-reference.xml"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-shell.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01.xsd"; DestDir: "{app}\Taxonomy\base\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2010-12-16\de-gcd-2010-12-16-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2010-12-16\de-gcd-2010-12-16-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2010-12-16\de-gcd-2010-12-16-presentation.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2010-12-16\de-gcd-2010-12-16-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2010-12-16\de-gcd-2010-12-16-shell.xsd"; DestDir: "{app}\Taxonomy\base\de-gcd-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2010-12-16\de-gcd-2010-12-16.xsd"; DestDir: "{app}\Taxonomy\base\de-gcd-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2010-12-16\hgbref-2010-02-19.xsd"; DestDir: "{app}\Taxonomy\base\de-gcd-2010-12-16"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2011-09-14\de-gcd-2011-09-14-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2011-09-14\de-gcd-2011-09-14-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2011-09-14\de-gcd-2011-09-14-presentation.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2011-09-14\de-gcd-2011-09-14-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2011-09-14\de-gcd-2011-09-14-shell.xsd"; DestDir: "{app}\Taxonomy\base\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2011-09-14\de-gcd-2011-09-14.xsd"; DestDir: "{app}\Taxonomy\base\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2011-09-14\hgbref-2010-02-19.xsd"; DestDir: "{app}\Taxonomy\base\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2012-06-01\de-gcd-2012-06-01-label-de.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2012-06-01\de-gcd-2012-06-01-label-en.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2012-06-01\de-gcd-2012-06-01-presentation.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2012-06-01\de-gcd-2012-06-01-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2012-06-01\de-gcd-2012-06-01-reference.xml"; DestDir: "{app}\Taxonomy\base\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2012-06-01\de-gcd-2012-06-01-shell.xsd"; DestDir: "{app}\Taxonomy\base\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2012-06-01\de-gcd-2012-06-01.xsd"; DestDir: "{app}\Taxonomy\base\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\base\de-gcd-2012-06-01\hgbref-2010-02-19.xsd"; DestDir: "{app}\Taxonomy\base\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-calculation-incomeStatement-staffelform.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-dimensions-calculation.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-dimensions-definition.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-dimensions-presentation.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-label-de.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-label-en.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-presentation-incomeStatement-staffelform.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-presentation-notes.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-reference.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14-shell-staffelform-fiscal.xsd"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2011-09-14\de-fi-2011-09-14.xsd"; DestDir: "{app}\Taxonomy\fi\de-fi-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-calculation-incomeStatement-staffelform.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-dimensions-calculation.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-dimensions-definition.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-dimensions-presentation.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-label-de.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-label-en.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-presentation-incomeStatement-staffelform.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-presentation-notes.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-reference.xml"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01-shell-staffelform-fiscal.xsd"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-fi-2012-06-01\de-fi-2012-06-01.xsd"; DestDir: "{app}\Taxonomy\fi\de-fi-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-notes.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions-definition.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions-label-de.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions-label-en.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions.xsd"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-label-de.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-label-en.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-label-fiscal-de.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-changesEquityStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-detailedInformation.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-determinationOfTaxableIncomeSpecialCases.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-managementReport.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-notes.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-otherReportElements.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-transfersCommercialCodeToTax.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-reference.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-shell.xsd"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14.xsd"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-notes.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions-definition.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions-label-de.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions-label-en.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions.xsd"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-de.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-en.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-fiscal-de.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-guidance-de.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-changesEquityStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-detailedInformation.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-determinationOfTaxableIncomeSpecialCases.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-managementReport.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-notes.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-otherReportElements.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-transfersCommercialCodeToTax.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-reference.xml"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-shell.xsd"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01.xsd"; DestDir: "{app}\Taxonomy\fi\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2011-09-14\de-gcd-2011-09-14-label-de.xml"; DestDir: "{app}\Taxonomy\fi\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2011-09-14\de-gcd-2011-09-14-label-en.xml"; DestDir: "{app}\Taxonomy\fi\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2011-09-14\de-gcd-2011-09-14-presentation.xml"; DestDir: "{app}\Taxonomy\fi\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2011-09-14\de-gcd-2011-09-14-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2011-09-14\de-gcd-2011-09-14-shell.xsd"; DestDir: "{app}\Taxonomy\fi\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2011-09-14\de-gcd-2011-09-14.xsd"; DestDir: "{app}\Taxonomy\fi\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2011-09-14\hgbref-2010-02-19.xsd"; DestDir: "{app}\Taxonomy\fi\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2012-06-01\de-gcd-2012-06-01-label-de.xml"; DestDir: "{app}\Taxonomy\fi\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2012-06-01\de-gcd-2012-06-01-label-en.xml"; DestDir: "{app}\Taxonomy\fi\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2012-06-01\de-gcd-2012-06-01-presentation.xml"; DestDir: "{app}\Taxonomy\fi\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2012-06-01\de-gcd-2012-06-01-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\fi\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2012-06-01\de-gcd-2012-06-01-reference.xml"; DestDir: "{app}\Taxonomy\fi\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2012-06-01\de-gcd-2012-06-01-shell.xsd"; DestDir: "{app}\Taxonomy\fi\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2012-06-01\de-gcd-2012-06-01.xsd"; DestDir: "{app}\Taxonomy\fi\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\fi\de-gcd-2012-06-01\hgbref-2010-02-19.xsd"; DestDir: "{app}\Taxonomy\fi\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-calculation-notes.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions-definition.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions-label-de.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions-label-en.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-dimensions.xsd"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-label-de.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-label-en.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-label-fiscal-de.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-changesEquityStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-detailedInformation.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-determinationOfTaxableIncomeSpecialCases.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-managementReport.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-notes.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-otherReportElements.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-presentation-transfersCommercialCodeToTax.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-reference.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14-shell.xsd"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2011-09-14\de-gaap-ci-2011-09-14.xsd"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-calculation-notes.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions-definition.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions-label-de.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions-label-en.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-dimensions.xsd"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-de.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-en.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-fiscal-de.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-label-guidance-de.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-adjustmentOfIncome.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-appropriationProfits.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-cashFlowStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-changesEquityAccounts.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-changesEquityStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-contingentLiabilities.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-detailedInformation.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-determinationOfTaxableIncome.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-determinationOfTaxableIncomeBusinessPartnership.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-determinationOfTaxableIncomeSpecialCases.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-managementReport.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-notes.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-otherReportElements.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-presentation-transfersCommercialCodeToTax.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-reference.xml"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01-shell.xsd"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gaap-ci-2012-06-01\de-gaap-ci-2012-06-01.xsd"; DestDir: "{app}\Taxonomy\ins\de-gaap-ci-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2011-09-14\de-gcd-2011-09-14-label-de.xml"; DestDir: "{app}\Taxonomy\ins\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2011-09-14\de-gcd-2011-09-14-label-en.xml"; DestDir: "{app}\Taxonomy\ins\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2011-09-14\de-gcd-2011-09-14-presentation.xml"; DestDir: "{app}\Taxonomy\ins\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2011-09-14\de-gcd-2011-09-14-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2011-09-14\de-gcd-2011-09-14-shell.xsd"; DestDir: "{app}\Taxonomy\ins\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2011-09-14\de-gcd-2011-09-14.xsd"; DestDir: "{app}\Taxonomy\ins\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2011-09-14\hgbref-2010-02-19.xsd"; DestDir: "{app}\Taxonomy\ins\de-gcd-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2012-06-01\de-gcd-2012-06-01-label-de.xml"; DestDir: "{app}\Taxonomy\ins\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2012-06-01\de-gcd-2012-06-01-label-en.xml"; DestDir: "{app}\Taxonomy\ins\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2012-06-01\de-gcd-2012-06-01-presentation.xml"; DestDir: "{app}\Taxonomy\ins\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2012-06-01\de-gcd-2012-06-01-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2012-06-01\de-gcd-2012-06-01-reference.xml"; DestDir: "{app}\Taxonomy\ins\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2012-06-01\de-gcd-2012-06-01-shell.xsd"; DestDir: "{app}\Taxonomy\ins\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2012-06-01\de-gcd-2012-06-01.xsd"; DestDir: "{app}\Taxonomy\ins\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-gcd-2012-06-01\hgbref-2010-02-19.xsd"; DestDir: "{app}\Taxonomy\ins\de-gcd-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-label-de.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-label-en.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-presentation-managementReport.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-presentation-notes.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-reference.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2011-09-14\de-ins-2011-09-14.xsd"; DestDir: "{app}\Taxonomy\ins\de-ins-2011-09-14"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-calculation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-calculation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-label-de.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-label-en.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-presentation-balanceSheet.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-presentation-incomeStatement.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-presentation-managementReport.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-presentation-notes.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-reference-fiscal.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-reference.xml"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01-shell-fiscal.xsd"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\ins\de-ins-2012-06-01\de-ins-2012-06-01.xsd"; DestDir: "{app}\Taxonomy\ins\de-ins-2012-06-01"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-bra-2011-09-14.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-fi-2011-09-14.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-fi-2012-06-01.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-gaap-2010-12-16.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-gaap-2011-09-14.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-gaap-2012-06-01.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-gcd-2010-12-16.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-gcd-2011-02-28.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-gcd-2011-09-14.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-gcd-2012-06-01.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-ins-2011-09-14.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\de-ins-2012-06-01.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy\styles\global.xml"; DestDir: "{app}\Taxonomy\styles"; Flags: ignoreversion

;Documents
Source: "{#pathEbkBin}\Documents\ChangeLog16.pdf"; DestDir: "{app}\Documents"; Flags: ignoreversion
Source: "{#pathEbkBin}\Documents\Feedback.pdf"; DestDir: "{app}\Documents"; Flags: ignoreversion
Source: "{#pathEbkBin}\Documents\Handbuch.pdf"; DestDir: "{app}\Documents"; Flags: ignoreversion
Source: "{#pathEbkBin}\Documents\Hinweis zum Aenderungsnachweis Taxonomie 5.0 zu 5.1.pdf"; DestDir: "{app}\Documents"; Flags: ignoreversion

;TestData
Source: "{#pathEbkBin}\TestData\TestSuSa.csv"; DestDir: "{app}\TestData"; Flags: ignoreversion

;de
Source: "{#pathEbkBin}\de\AvdWpfControls.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "{#pathEbkBin}\de\CustomResources.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "{#pathEbkBin}\de\DbAccess.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "{#pathEbkBin}\de\eBalanceKitBusiness.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "{#pathEbkBin}\de\eBalanceKitResources.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "{#pathEbkBin}\de\Taxonomy.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "{#pathEbkBin}\de\Utils.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion

Source: "{#pathEbkBin}\AvdCommon.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\AvdWpfControls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\AvdWpfStyles.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\CustomResources.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\DbAccess.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\eBalanceKit.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\eBalanceKit.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\eBalanceKitBase.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\eBalanceKitBusiness.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\eBalanceKitControls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\eBalanceKitResources.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\eFederalGazette.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\EricWrapper.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\itextsharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\MySql.Data.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\PdfGenerator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\Taxonomy.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBin}\Utils.dll"; DestDir: "{app}"; Flags: ignoreversion

Source: "{#pathEbkConfigBin}\de\Config.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "{#pathEbkConfigBin}\Config.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkConfigBin}\Config.exe.config"; DestDir: "{app}"; Flags: ignoreversion

Source: "{#pathEbkDatabaseManagementBin}\de\DatabaseManagement.resources.dll"; DestDir: "{app}\de"; Flags: ignoreversion
Source: "{#pathEbkDatabaseManagementBin}\DatabaseManagement.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkDatabaseManagementBin}\XmlDatabaseBackup.dll"; DestDir: "{app}"; Flags: ignoreversion

Source: "{#pathEbkLogviewerBin}\eBalanceKitManagement.exe"; DestDir: "{app}"; Flags: ignoreversion

Source: "{#pathEbkBackupServiceBin}\EBilanzBackupServiceSetup.msi"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#pathEbkBackupServiceBin}\setup.exe"; DestDir: "{app}"; Flags: ignoreversion

;Source: "{#pathEbkMappingTemplateUpgradeBin}\MappingTemplateUpgrade.exe"; DestDir: "{app}"; Flags: ignoreversion

Source: "..\..\Assemblies\32Bit\External\Oracle\Oracle.DataAccess.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Libraries\DbAccess\bin\Release\dao.dll"; DestDir: "{app}"; Flags: ignoreversion

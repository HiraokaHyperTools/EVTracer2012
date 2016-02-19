; example1.nsi
;
; This script is perhaps one of the simplest NSIs you can make. All of the
; optional settings are left to their default settings. The installer simply 
; prompts the user asking them where to install, and drops a copy of example1.nsi
; there. 

;--------------------------------

!define APP "EVTracer2012"
!system 'DefineAsmVer.exe bin\DEBUG\${APP}.exe "!define VER ""[SVER]"" " > TmpVer.nsh'
!include "TmpVer.nsh"

; The name of the installer
Name "${APP} Ver.${VER}"

; The file to write
OutFile "Setup_${APP}.exe"

; The default installation directory
InstallDir "$APPDATA\${APP}"

; Request application privileges for Windows Vista
RequestExecutionLevel user

!include "DotNetVer.nsh"

;--------------------------------

; Pages

Page directory
Page instfiles

;--------------------------------

; The stuff to install
Section "" ;No components page, name is not important

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ${IfNot} ${HasDotNet4.0}
  ${AndIfNot} ${Silent}
    MessageBox MB_ICONEXCLAMATION "必要: Microsoft .NET Framework 4"
  ${EndIf}
  
  ClearErrors

  ; Put file there
  File /r /x "*.vshost.*" "bin\DEBUG\*.*"
  
  StrCpy $1 "$SMPROGRAMS\${APP}"
  CreateDirectory "$1"
  CreateShortCut  "$1\XML整形する.lnk" "$INSTDIR\${APP}.exe" "/format"
  CreateShortCut  "$1\XML→csv.lnk" "$INSTDIR\${APP}.exe" "/conv"
  CreateShortCut  "$1\XML→csv(4659だけ).lnk" "$INSTDIR\${APP}.exe" "/convf"
  CreateShortCut  "$1\イベント ビューアー.lnk" "$SYSDIR\eventvwr.exe"

  ExecShell "open" "$1"
  
  IfErrors +2
    SetAutoClose true

SectionEnd ; end the section

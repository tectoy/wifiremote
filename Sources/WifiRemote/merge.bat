@echo off
IF EXIST WifiRemote_TMP.dll del WifiRemote_TMP.dll 
IF EXIST WifiRemote_TMP.pdb del WifiRemote_TMP.pdb 
ilmerge /out:WifiRemote_TMP.dll WifiRemote.dll ZeroconfService.dll Newtonsoft.Json.Net35.dll zxing.dll

IF EXIST WifiRemote.dll del WifiRemote.dll
IF EXIST WifiRemote.pdb del WifiRemote.pdb 

ren WifiRemote_TMP.dll WifiRemote.dll 
ren WifiRemote_TMP.pdb WifiRemote.pdb 

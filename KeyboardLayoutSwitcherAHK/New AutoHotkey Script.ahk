#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

a::
    Send {LCtrl down}
    Sleep 50
    Send {RCtrl down}
    Sleep 50
    Send {a down}
    Sleep 100
    Send {a Up}
    Sleep 50
    Send {RCtrl Up}
    Sleep 50
    Send {LCtrl Up}
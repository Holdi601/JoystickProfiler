#NoEnv  ; Recommended for performance and compatibility with future AutoHotkey releases.
; #Warn  ; Enable warnings to assist with detecting common errors.
SendMode Input  ; Recommended for new scripts due to its superior speed and reliability.
SetWorkingDir %A_ScriptDir%  ; Ensures a consistent starting directory.

switchModeActive := "false"

PressKeyManipulated(key)
{
  msgbox, %key%
  Send {LCtrl down}
  Sleep 50
  Send {RCtrl down}
  Sleep 50
  Send {%key% down}
  Sleep 100
  Send {%key% Up}
  Sleep 50
  Send {RCtrl Up}
  Sleep 50
  Send {LCtrl Up}
  return
}

ActivateHotkeys()
{
  msgbox, "Activated"
  HotKey, a, PressManA
  HotKey, b, PressManB  
  HotKey, c, PressManC  
  HotKey, d, PressManD  
  HotKey, e, PressManE  
  HotKey, f, PressManF  
  HotKey, g, PressManG  
  HotKey, h, PressManH  
  HotKey, i, PressManI  
  HotKey, j, PressManJ  
  HotKey, k, PressManK  
  HotKey, l, PressManL  
  HotKey, m, PressManM  
  HotKey, n, PressManN  
  HotKey, o, PressManO  
  HotKey, p, PressManP  
  HotKey, q, PressManQ  
  HotKey, r, PressManR  
  HotKey, s, PressManS  
  HotKey, t, PressManT  
  HotKey, u, PressManU  
  HotKey, v, PressManV  
  HotKey, w, PressManW 
  HotKey, x, PressManX
  HotKey, y, PressManY
  HotKey, z, PressManZ   
  HotKey, Left, PressManLeft
  HotKey, Right, PressManRight
  HotKey, Up, PressManUp
  HotKey, Down, PressManDown   
  HotKey, Numpad0, PressManNumpad0
  HotKey, Numpad1, PressManNumpad1  
  HotKey, Numpad2, PressManNumpad2  
  HotKey, Numpad3, PressManNumpad3  
  HotKey, Numpad4, PressManNumpad4  
  HotKey, Numpad5, PressManNumpad5  
  HotKey, Numpad6, PressManNumpad6  
  HotKey, Numpad7, PressManNumpad7  
  HotKey, Numpad8, PressManNumpad8  
  HotKey, Numpad9, PressManNumpad9
  HotKey, 0, PressManNumpad0
  HotKey, 1, PressManNumpad1  
  HotKey, 2, PressManNumpad2  
  HotKey, 3, PressManNumpad3  
  HotKey, 4, PressManNumpad4  
  HotKey, 5, PressManNumpad5  
  HotKey, 6, PressManNumpad6  
  HotKey, 7, PressManNumpad7  
  HotKey, 8, PressManNumpad8  
  HotKey, 9, PressManNumpad9    
  HotKey, NumpadDot, PressManNumpadDot 
  HotKey, NumpadDiv, PressManNumpadDiv  
  HotKey, NumpadMult, PressManNumpadMult
  HotKey, NumpadAdd, PressManNumpadAdd
  HotKey, NumpadSub, PressManNumpadSub
  HotKey, NumpadEnter, PressManNumpadEnter
  HotKey, ScrollLock, PressManScrollLock
  HotKey, Delete, PressManDelete
  HotKey, Insert, PressManInsert
  HotKey, Home, PressManHome
  HotKey, End, PressManEnd
  HotKey, PgUp, PressManPgUp
  HotKey, PgDn, PressManPgDn
  HotKey, CapsLock, PressManCapsLock
  HotKey, Space, PressManSpace
  HotKey, Tab, PressManSpace
  HotKey, BackSpace, PressManBackSpace
  HotKey, Enter, PressManNumpadEnter
;HotKey, VKBCSC033, PressManComma
;HotKey, VKBCSC029, PressManGrave
;HotKey, VKBCSC00C, PressManMinus
;HotKey, VKBCSC00D, PressManEqual
;HotKey, VKBCSC034, PressManDot
;HotKey, VKBCSC035, PressManSlash
;HotKey, VKBCSC02B, PressManBackSlash
;HotKey, VKBCSC01A, PressManLeftBracket
;HotKey, VKBCSC01B, PressManRightBracket
;HotKey, VKBCSC027, PressManSemiColon
;HotKey, VKBCSC028, PressManApostrophe
  return
}

DisableHotkeys()
{
  msgbox, "Disabled"
  HotKey, a, Off
  HotKey, b, Off  
  HotKey, c, Off 
  HotKey, d, Off  
  HotKey, e, Off  
  HotKey, f, Off  
  HotKey, g, Off  
  HotKey, h, Off  
  HotKey, i, Off  
  HotKey, j, Off  
  HotKey, k, Off  
  HotKey, l, Off  
  HotKey, m, Off  
  HotKey, n, Off  
  HotKey, o, Off  
  HotKey, p, Off  
  HotKey, q, Off  
  HotKey, r, Off  
  HotKey, s, Off  
  HotKey, t, Off  
  HotKey, u, Off  
  HotKey, v, Off  
  HotKey, w, Off 
  HotKey, x, Off
  HotKey, y, Off
  HotKey, z, Off  
  HotKey, Left, Off 
  HotKey, Right, Off
  HotKey, Up, Off
  HotKey, Down, Off   
  HotKey, Numpad0, Off
  HotKey, Numpad1, Off  
  HotKey, Numpad2, Off  
  HotKey, Numpad3, Off  
  HotKey, Numpad4, Off  
  HotKey, Numpad5, Off  
  HotKey, Numpad6, Off  
  HotKey, Numpad7, Off  
  HotKey, Numpad8, Off  
  HotKey, Numpad9, Off
  HotKey, 0, Off
  HotKey, 1, Off  
  HotKey, 2, Off  
  HotKey, 3, Off
  HotKey, 4, Off  
  HotKey, 5, Off  
  HotKey, 6, Off  
  HotKey, 7, Off  
  HotKey, 8, Off  
  HotKey, 9, Off    
  HotKey, NumpadDot, Off  
  HotKey, NumpadDiv, Off  
  HotKey, NumpadMult, Off  
  HotKey, NumpadAdd, Off  
  HotKey, NumpadSub, Off  
  HotKey, NumpadEnter, Off  
  HotKey, ScrollLock, Off  
  HotKey, Delete, Off  
  HotKey, Insert, Off  
  HotKey, Home, Off  
  HotKey, End, Off  
  HotKey, PgUp, Off  
  HotKey, PgDn, Off 
  HotKey, CapsLock, Off
  HotKey, Space, Off 
  HotKey, Tab, Off
  HotKey, BackSpace, Off
  HotKey, Enter, Off
  ;HotKey, VKBCSC033, Off
  ;HotKey, VKBCSC029, Off
  ;HotKey, VKBCSC00C, Off 
  ;HotKey, VKBCSC00D, Off 
  ;HotKey, VKBCSC034, Off 
  ;HotKey, VKBCSC035, Off 
  ;HotKey, VKBCSC02B, Off
  ;HotKey, VKBCSC01A, Off 
  ;HotKey, VKBCSC01B, Off 
  ;HotKey, VKBCSC027, Off 
  ;HotKey, VKBCSC028, Off  
  return
}

F24::
  if(switchModeActive="true")
  {
    switchModeActive:="false"
    msgbox, "DEACTIVATED"
  }  
  else
  {
    switchModeActive:="true"
    msgbox, "ACTIVATED"
  }
  return

F23::
  switchModeActive:="false"
  msgbox, "DEACTIVATED"
  return

F22::
  switchModeActive:="false"
  msgbox, "DEACTIVATED"
  return

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
  return

PressManA:
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
  return

PressManB:
  PressKeyManipulated("b")
  msgbox, "b pressed"
  return

PressManC:
  PressKeyManipulated("c")
  msgbox, "c pressed"
  return

PressManD:
  PressKeyManipulated("d")
  return

PressManE:
  PressKeyManipulated("e")
  return

PressManF:
  PressKeyManipulated("f")
  return

PressManG:
  PressKeyManipulated("g")
  return

PressManH:
  PressKeyManipulated("h")
  return

PressManI:
  PressKeyManipulated("i")
  return

PressManJ:
  PressKeyManipulated("j")
  return

PressManK:
  PressKeyManipulated("k")
  return

PressManL:
  PressKeyManipulated("l")
  return

PressManM:
  PressKeyManipulated("m")
  return

PressManN:
  PressKeyManipulated("n")
  return

PressManO:
  PressKeyManipulated("o")
  return

PressManP:
  PressKeyManipulated("p")
  return

PressManQ:
  PressKeyManipulated("q")
  return

PressManR:
  PressKeyManipulated("r")
  return

PressManS:
  PressKeyManipulated("s")
  return

PressManT:
  PressKeyManipulated("t")
  return

PressManU:
  PressKeyManipulated("u")
  return

PressManV:
  PressKeyManipulated("v")
  return

PressManW:
  PressKeyManipulated("w")
  return

PressManX:
  PressKeyManipulated("x")
  return

PressManY:
  PressKeyManipulated("y")
  return

PressManZ:
  PressKeyManipulated("z")
  return

PressManLeft:
  PressKeyManipulated("Left")
  return

PressManRight:
  PressKeyManipulated("Right")
  return

PressManUp:
  PressKeyManipulated("Up")
  return

PressManDown:
  PressKeyManipulated("Down")
  return

PressManNumpad0:
  PressKeyManipulated("Numpad0")
  return

PressManNumpad1:
  PressKeyManipulated("Numpad1")
  return

PressManNumpad2:
  PressKeyManipulated("Numpad2")
  return

PressManNumpad3:
  PressKeyManipulated("Numpad3")
  return

PressManNumpad4:
  PressKeyManipulated("Numpad4")
  return

PressManNumpad5:
  PressKeyManipulated("Numpad5")
  return

PressManNumpad6:
  PressKeyManipulated("Numpad6")
  return

PressManNumpad7:
  PressKeyManipulated("Numpad7")
  return

PressManNumpad8:
  PressKeyManipulated("Numpad8")
  return

PressManNumpad9:
  PressKeyManipulated("Numpad9")
  return

PressManNumpadDot:
  PressKeyManipulated("NumpadDot")
  return

PressManNumpadDiv:
  PressKeyManipulated("NumpadDiv")
  return

PressManNumpadMult:
  PressKeyManipulated("NumpadMult")
  return

PressManNumpadAdd:
  PressKeyManipulated("NumpadAdd")
  return

PressManNumpadSub:
  PressKeyManipulated("NumpadSub")
  return

PressManNumpadEnter:
  PressKeyManipulated("NumpadEnter")
  return

PressManScrollLock:
  PressKeyManipulated("ScrollLock")
  return

PressManDelete:
  PressKeyManipulated("Delete")
  return

PressManInsert:
  PressKeyManipulated("Insert")
  return

PressManHome:
  PressKeyManipulated("Home")
  return

PressManEnd:
  PressKeyManipulated("End")
  return

PressManPgUp:
  PressKeyManipulated("PgUp")
  return

PressManPgDn:
  PressKeyManipulated("PgDn")
  return

PressManCapsLock:
  PressKeyManipulated("CapsLock")
  return

PressManSpace:
  PressKeyManipulated("Space")
  return

PressManTab:
  PressKeyManipulated("Tab")
  return

PressManBackSpace:
  PressKeyManipulated("BackSpace")
  return

PressManGrave:
  PressKeyManipulated("VKBCSC029")
  return

PressManMinus:
  PressKeyManipulated("VKBCSC00C")
  return

PressManEqual:
  PressKeyManipulated("VKBCSC00D")
  return

PressManComma:
  PressKeyManipulated("VKBCSC033")
  return

PressManDot:
  PressKeyManipulated("VKBCSC034")
  return

PressManSlash:
  PressKeyManipulated("VKBCSC035")
  return

PressManBackSlash:
  PressKeyManipulated("VKBCSC02B")
  return

PressManLeftBracket:
  PressKeyManipulated("VKBCSC01A")
  return

PressManRightBracket:
  PressKeyManipulated("VKBCSC01B")
  return

PressManSemiColon:
  PressKeyManipulated("VKBCSC027")
  return

PressManApostrophe:
  PressKeyManipulated("VKBCSC028")
  return
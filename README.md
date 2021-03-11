# JoystickProfiler
***WARNING USING AT YOUR OWN RISK***

Joystick Profiler Utility

This utility is there in the current state to help, to quickly make the basic joystick Mappings for DCS world (Digital Combat Simulator).
It works by making relations between various inputs from various planes and bind them to the same place on the joystick.

You first create Relations. Within a single relation each airframe can only be once bound/active. Multiple different DCS key elements can conclude one relation. 
E.g. You have in some airframes Left Wheel Brake and a right Wheel brake. In some others you just have a Wheel break. So you can make now a relation that you would want on your
right paddle the right wheel brake and for those airframes that just have a general brake you want the general break there. 

You can export Relations to make other Peoples life easier, so they don't need to make Relations. 
Once you are happy with your Relation you can Bind an Axis or Button to it. 
However for simplicity sake the current version has some shortcomings, because I am not sure if i just coded that for me, or if more people find interest in such a utility.
Shortcomings:
-You cannot set Button Reformers(im not entirely sure how they work internally in DCS)
-It does not support complex User curvitures as seen in DCS. It offers the single curviture value as seen in DCS but not the 10 value array as of yet. If something exist *IT should keep it* though, test it at your own risk. 

If people show interest, I might update it and improve on the given shortcomings and improve the cleaness of the code and maybe extend to other games like Star citizen, but as I said for me it does the job right now. 
Use the program only at your own risk as you might overwrite some existing binds. Make some backup of your existing binds before you use the software, if you get disappointed, that you can roll back.

Anyways Good Success and Have a nice day.

Windows X64 Builds: https://github.com/Holdi601/JoystickProfiler/tree/master/Builds 


Requirements:
https://www.microsoft.com/en-us/download/confirmation.aspx?id=5555 - Microsoft Visual C++ 2010 Redistributable Package (x86)
https://www.microsoft.com/en-US/Download/confirmation.aspx?id=14632 Redist x64

https://dotnet.microsoft.com/download/dotnet-framework .NET 4.5 or higher (newest always best)


For updating yourself the Key Database
https://www.python.org/downloads/windows/ python3

Tutorials

How to create and edit Binds:
https://youtu.be/XxzfEFHpWxI

FAQ:
I have mods installed, but they do not show up in relations?
and
DCS got updated Keybinds but it is not showing them?

If DCS hits an update with updated binds, I will try to keep it up to date and push a new build. If its not up to date please feel free to contact me. I will update it asap. However if you care for installed mods or want to do the process yourself do the following:
All the existing Keybinds that exist are saved inf \DB\DCS inside the axis.csv and the btn.csv
These are generated from the python script that you find inside \Tools folder called DCSKeyAxisExtractor.py make sure to edit the file to fit your output path and DCS Saved Games InputLayoutsTxt path. 
The InputLayoutsTxt path exist, if you go into DCS go into the input option of each of your aircraft and click on "Make HTML". 
Once you have done it and adjusted your paths inside the python script, run the python script and replace the axis.csv and btn.csv in the \DB\DCS path and you are good to go.
At some point, I will probably make a video Tutorial about this as well. 


I have a bug?!

Contact me, I will try to fix it asap. 


What features might come?

-Maybe other games come like star citizen, as that game is in its joystick mapping a mess inside the game. But if so, thats far out. 
-AutoBackup on Startup
-Autoupdater
-Profile Validation:
--ID Validation
--Modifier Validation
--No double joystick use validation
-Profile from local Binds from
--ID
--JoyBinds

But lets see. Depends on time and motivation. 


Discord for more direct question answer or concerns: https://discord.gg/yK4gQA9xgk

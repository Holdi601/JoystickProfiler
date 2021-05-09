# JoystickProfiler
***WARNING USING AT YOUR OWN RISK***

# Joystick Profiler Utility

This utility is there in the current state to help, to quickly make the basic joystick Mappings for DCS world (Digital Combat Simulator).
It works by making relations between various inputs from various planes and bind them to the same place on the joystick.

You first create Relations. Within a single relation each airframe can only be once bound/active. Multiple different DCS key elements can conclude one relation. 
E.g. You have in some airframes Left Wheel Brake and a right Wheel brake. In some others you just have a Wheel break. So you can make now a relation that you would want on your
right paddle the right wheel brake and for those airframes that just have a general brake you want the general break there. 

You can export Relations to make other Peoples life easier, so they don't need to make Relations. 
Once you are happy with your Relation you can Bind an Axis or Button to it. 
However for simplicity sake the current version has some shortcomings, because I am not sure if i just coded that for me, or if more people find interest in such a utility.

If people show interest, I might update it and improve on the given shortcomings and improve the cleaness of the code and maybe extend to other games like Star citizen, but as I said for me it does the job right now. 
Use the program only at your own risk as you might overwrite some existing binds. Make some backup of your existing binds before you use the software, if you get disappointed, that you can roll back.

Anyways Good Success and Have a nice day.

# Windows X64 Builds: 

https://github.com/Holdi601/JoystickProfiler/tree/master/Builds 


# Requirements:
https://www.microsoft.com/en-us/download/confirmation.aspx?id=5555 - Microsoft Visual C++ 2010 Redistributable Package (x86)
https://www.microsoft.com/en-US/Download/confirmation.aspx?id=14632 Redist x64

https://dotnet.microsoft.com/download/dotnet-framework .NET 4.5 or higher (newest always best)

DirectX11
Windows 10

# Tutorials

How to create and edit Binds:

https://youtu.be/XxzfEFHpWxI

Other peoples Tutorials:

https://youtu.be/rKtNZfLImoM -Grim Reapers

https://www.youtube.com/watch?v=y0m1sVOM-JM -Lets play Indie Games Channel

# Templates and Profiles

You have a template or profile you want to share with others or see here? No Problem join the discord and upload it. I will attach it. 

# Common Issues:
-You see it in the task bar open but no windows? Then, when it tried to save its window state last time it got corrupted. To fix it, delete the Documents\JoyPro folder to fix.

-You crash on Binding ? You are missing c++ Redists.

-The Program does not start at all? You are probably lacking then .Net

# FAQ:
I have mods installed, but they do not show up in relations?

and

DCS got updated Keybinds but it is not showing them?

Go into DCS, Go into Controls for the Mod plane, click on make html. Restart joypro.


# I have a bug?!

Contact me, I will try to fix it asap. Join the discord or create an Issue. Discord preferred as the forth and back to figure out what is going wrong is faster.


# What features might come?

-Maybe other games come like star citizen, as that game is in its joystick mapping a mess inside the game. But if so, thats far out. 
(Right now im in the testing state how easy it is outside the game like IL2 or FS2020 or SC to change inputs)
-Refactoring at some point, prolly too much spaghetti for some :-D 


Discord for more direct question answer or concerns: https://discord.gg/SeCTXJHhJf

# To Do, Features I want to work on

-Grouping Relations

-Import update Database

-Device Manager

-Search function in Relation overview

-Filters in Relationview

-Select All / None in Import

-Asynchronus download and update

-Local DB Diffs if users want to exclude stuff from their DB

-instance saving into profile

-cancel an edit of a relation

-Keyboard support (lowest Priority for me, as after all this is the joystick profiler not input profiler. I see the use of this feature, but after all you dont exchange your keyboard and if you do you dont have IDs you need to work around, you got a fairly big different set of inputs in DCS of inputs for keyboard than the ones for joysticks. And other fairly big adjusmtents that need to be made for a feature which was originally not intendet. I see the benefit of it, but its low prio, just be clear.)


Other Games i want to support:

IL2 (WIP right now)

Star Citizen

MSFS2020

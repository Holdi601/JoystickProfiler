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
Microsoft C++ Redist 2010, 2011 and 2015 x64. I can't put a link here as they become deprecated after a few days. Google it yourself. Sorry.

https://dotnet.microsoft.com/download/dotnet-framework .NET 4.5 or higher (newest always best)

DirectX11
Windows 10

# Documentation / Tutorials

My own Video Short and Condense Series:

1. An Introduction https://www.youtube.com/watch?v=WY3UCZiRwro
2. Download & Create Relations https://youtu.be/CckEQUG7YGU
3. Bind an Button or Axis https://youtu.be/4zMcYzhgv7E
4. Create Modifier https://youtu.be/eG0npyq0gUo
5. Import Inputs
6. Manage your Relations
7. Validate your Relations
8. Export your Controls
9. Mod Support & Refresh DCS Input DB
10. Troubleshoot - Exported but default Profile binds still exist
11. Export Joystick Graphical Layouts
12. Exchange a device
13. Quick start guide
14. JoyPro doesn't start anymore

The entire playlist:
https://www.youtube.com/watch?v=WY3UCZiRwro&list=PLlzaQN7UpUfh3b98GOSMe3LHgXxLziFca

Older Video from me: How to create and edit Binds:

https://youtu.be/XxzfEFHpWxI

Other peoples Tutorials:

https://youtu.be/rKtNZfLImoM -Grim Reapers

https://www.youtube.com/watch?v=y0m1sVOM-JM -Lets play Indie Games Channel

# Templates and Profiles

You have a template or profile you want to share with others or see here? No Problem join the discord and upload it. I will attach it. 

# Common Issues:
-You see it in the task bar open but no windows? Then, when it tried to save its window state last time it got corrupted. To fix it, delete the Documents\JoyPro folder to fix.

-You crash on Binding or get an error? You are missing c++ Redists. Or other requirements of slimdx.dll. Try Installing the C++ Redist for x64bit Systems for 2010, 2011 and 2015 and if that doesn't work try to download the full slimdx sdk here: https://code.google.com/archive/p/slimdx/downloads . Linking to the c++ redist won't work as the download links always become depracated after a few days. What you might also want to try is installing Simple Radio as they also rely on c++ redist if i am not mistaken and possibly could fix it.

-The Program does not start at all? You are probably lacking then .Net

# IL2 Caveats
-the Map file gets recreated everytime the game starts and goes into options, so any Modifier or Custom curve created in JP will be ignored. If you reimport that data your relation will be on the bind without modifiers.
-Also IL2 doesn't allow outside the map file for Saturation changes, and the map file always gets recreated so it can't be changed.

# FAQ:
I have mods installed, but they do not show up in relations?

and

DCS got updated Keybinds but it is not showing them?

Go into DCS, Go into Controls for the Mod plane, click on make html. Restart joypro.


# I have a bug?!

Contact me, I will try to fix it asap. Join the discord or create an Issue. Discord preferred as the forth and back to figure out what is going wrong is faster.


# What features might come?

-Maybe other games come like star citizen, as that game is in its joystick mapping a mess inside the game. But if so, thats far out. 
(Right now im in the testing state how easy it is outside the game like  FS2020 or SC to change inputs)

-Refactoring at some point, prolly too much spaghetti for some :-D 


Discord for more direct question answer or concerns: https://discord.gg/SeCTXJHhJf

# To Do, Features I want to work on in that order

-Asynchronus download and update

-cancel an edit of a relation

-Keyboard support (lowest Priority for me, as after all this is the joystick profiler not input profiler. I see the use of this feature, but after all you dont exchange your keyboard and if you do you dont have IDs you need to work around, you got a fairly big different set of inputs in DCS of inputs for keyboard than the ones for joysticks. And other fairly big adjusmtents that need to be made for a feature which was originally not intendet. I see the benefit of it, but its low prio, just to be clear.)


#Other Games i want to support:

Star Citizen

MSFS2020

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
Windows 10 or higher

SlimDX Requirements:
X64 Runtime:
https://storage.googleapis.com/google-code-archive-downloads/v2/code.google.com/slimdx/SlimDX%20Runtime%20.NET%204.0%20x64%20(January%202012).msi

If that doesn’t help maybe the SDK:
https://storage.googleapis.com/google-code-archive-downloads/v2/code.google.com/slimdx/SlimDX%20SDK%20(January%202012).msi

And in general slimdx ressources can be found here in case the links are dead for you https://code.google.com/archive/p/slimdx/downloads but please when possible try to choose the x64 option as that is what JoyPro uses

# Documentation / Tutorials

Check the Wiki or in Video form:

My own Video Short and Condense Series:

1. An Introduction https://www.youtube.com/watch?v=WY3UCZiRwro
2. Download & Create Relations https://youtu.be/CckEQUG7YGU
3. Bind an Button or Axis https://youtu.be/4zMcYzhgv7E
4. Create Modifier https://youtu.be/eG0npyq0gUo
5. Import Inputs https://youtu.be/EZGPryuIWMM
6. Manage your Relations https://youtu.be/BqnDsQjF6Fw
7. Validate your Relations https://youtu.be/X3dzaRcWWZo
8. Export & Save your Controls https://youtu.be/aviKpu6Y6ks
9. Mod Support & Refresh DCS Input DB https://youtu.be/r-kyHykQCrs
10. Troubleshoot - Exported but default Profile binds still exist https://youtu.be/KkGLdN2cglU
11. Export Joystick Graphical Layouts https://youtu.be/kAWnWB3HDQ4 *Needs an Update*
12. Exchange a device https://youtu.be/Y8H9D9UX8KQ
13. Quick start guide https://youtu.be/Z030AiaJwg4
14. JoyPro doesn't start anymore https://youtu.be/EM1Ou2USzL0
15. Im unhappy with JoyPro, give me my old controls back! https://youtu.be/Kl-lvnkyeqU
16. Manually Adding Input Entries into Database https://youtu.be/L-bivat1-gU *Needs an Update*
17. Visual Assign Mode https://youtu.be/7LtPkeC2dL4
18. Ingame Overlay https://youtu.be/fDksVg-TnyU
19. Mass Operations
20. Keyboard support explained

The entire playlist:
https://www.youtube.com/watch?v=WY3UCZiRwro&list=PLlzaQN7UpUfh3b98GOSMe3LHgXxLziFca

Older Video from me: How to create and edit Binds:

https://youtu.be/XxzfEFHpWxI

Other peoples Tutorials:

https://youtu.be/rKtNZfLImoM -Grim Reapers

https://www.youtube.com/watch?v=y0m1sVOM-JM -Lets play Indie Games Channel

# Templates and Profiles

You have a template or profile you want to share with others or see here? No Problem join the discord and upload it. I will attach it. 


# I have a bug?!

Contact me, I will try to fix it asap. Join the discord or create an Issue. Discord preferred as the forth and back to figure out what is going wrong is faster.


# What features might come?

-Other Games. However I would need help by others as some games like MSFS2020 a lot of stuff looks like questionmarks to me.

-Refactoring at some point, prolly too much spaghetti for some :-D 


Discord for more direct question answer or concerns: https://discord.gg/SeCTXJHhJf

# To Do, Features I want to work on in that order

#Other Games i want to support:

Star Citizen

MSFS2020 (Someone please explain me the structure)

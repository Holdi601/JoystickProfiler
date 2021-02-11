# JoystickProfiler
***WARNING USING AT YOUR OWN RISK***

Joystick Profiler Utility

This utility is there in the current state to help, to quickly make the basic joystick Mappings for DCS world.
It works by making relations between various inputs from various planes and bind them to the same place on the joystick.

You first create Relations. Within a single relation each airframe can only be once bound/active. Multiple different DCS key elements can conclude one relation. 
E.g. You have in some airframes Left Wheel Brake and a right Wheel brake. In some others you just have a Wheel break. So you can make now a relation that you would want on your
right paddle the right wheel brake and for those airframes that just have a general brake you want the general break there. 

You can export Relations to make other Peoples life easier, so they don't need to make Relations. 
Once you are happy with your Relation you can Bind an Axis or Button to it. 
However for simplicity sake the current version has some shortcomings, because I am not sure if i just coded that for me, or if more people find interest in such a utility.
Shortcomings:
-You cannot set Custom Axis Graphs as in DCS(But you can set the curve value deadzone slider and saturations)
-You cannot set Button Reformers(im not entirely sure how they work internally in DCS)
-If you save over already over an existing profile with an existing custom Axis graph, it will overwrite it. (However if you use the simple curviture value, you should be fine)
-You cannot set the Buttons of the joystick by clicking it. For that I believe i would need to include some sort of directX to it to access the joy control data. And right now im not sure if thats needed, plus usually windows only allows 32 buttons per device, but many sticks like the Thrustmaster Warthog or Virpil or VKB or any Hardware has more Buttons than that, I m not sure if they just show up as Button69 or something, or ignores everything above 32.
-As it uses specific Joystick IDs as DCS does it, its not directly possible to make one X52 profile and then share it out as every x52 has their own specific user id, and maybe at some point i can catch the name of the loaded profile and check in the local DCS folder if a stick with a similar name but different ID exists. 

If people show interest, I might update it and improve on the given shortcomings and improve the cleaness of the code and maybe extend to other games like Star citizenm, but as I said for me it does the job right now. 
Use the program only at your own risk as you might overwrite some existing binds. 

Anyways Good Success and Have a nice day.

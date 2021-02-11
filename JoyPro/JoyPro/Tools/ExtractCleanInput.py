import os

inputpath = "C:\\Users\\reinh\\Saved Games\\DCS\\Config\\Input"
outputpath = os.path.abspath(os.getcwd())
file = open(outputpath+'\\clean.cf', mode='w')
joystickname = "LEFT VPC Throttle MT-50 CM {43DBF080-895E-11ea-8002-444553540000}"

subfolders = [f.path for f in os.scandir(inputpath) if f.is_dir()]
i=0;
for s in subfolders:
    dirparts = s.split('\\')
    plane = dirparts[len(dirparts)-1]
    firstline ='####################'+plane+'\n'
    if i>0:
        firstline='\n'+firstline
    file.write(firstline)
    try:
        sossefile = open(s+"\\joystick\\"+joystickname+".diff.lua", 'r')
        lines = sossefile.readlines()
        for line in lines:
            file.write(line)
    except Exception as e:
        print(type(e))
        print(e.args)
        print(e)
    i+=1
print("done")

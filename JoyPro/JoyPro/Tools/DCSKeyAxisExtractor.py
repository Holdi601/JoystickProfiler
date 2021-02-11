import os

Planes = []
path = "F:\\Dropbox\\DCS\\DCS.openbeta\\InputLayoutsTxt\\"
outptaxis ="F:\\Dropbox\\Programmierung\\dcs\\axis.csv"
outptbtn = "F:\\Dropbox\\Programmierung\\dcs\\btn.csv"

class Plane:
    def __init__(self, name, planePath):
        self.name = name
        self.path = planePath
        self.keybind = {}
        self.axis = {}
        self.filesToCheck = []


    def checkFoldersForFiles(self, fdir):
        AllItemsInPath = os.scandir(fdir)
        for item in AllItemsInPath:
            if item.is_dir():
                self.checkFoldersForFiles(item.path)
            else:
                if item.name.endswith(".html"):
                    self.filesToCheck.append(item.path)
                    #print(item.path)

    def analyzeFolder(self):
        self.checkFoldersForFiles(self.path)
        for fil in self.filesToCheck:
            self.analyzeFile(fil)

    def analyzeFile(self, filepath):
        file1 = open(filepath, 'r', encoding="utf8")
        isAxis = False
        isButton = False
        currentId =""
        currentName =""
        print("Read file: "+filepath)
        Lines = file1.readlines()
        block=0
        cachedlines = ["current", "last", "beforelast", "to be dropped"]
        for line in Lines:
            cachedlines[3]=cachedlines[2]
            cachedlines[2]=cachedlines[1]
            cachedlines[1]=cachedlines[0]
            cachedlines[0]=line
            if cachedlines[0].startswith("        </tr>"):
                id=cachedlines[1].replace("</td>","").replace("          <td>","").replace("\r","").replace("\n","")
                name=cachedlines[3].replace("</td>","").replace("          <td>","").replace("\r","").replace("\n","")
                isAxis = False
                if id.startswith("a"):
                    isAxis=True
                if block > 0:
                    if isAxis:
                        self.axis[name]=id
                    else:
                        self.keybind[name]=id
                block+=1
            

    

DirectoryContents = os.scandir(path)
for dirI in DirectoryContents:
    if dirI.is_dir():
        currentPlane = Plane(dirI.name, path+dirI.name+"\\")
        Planes.append(currentPlane)
        currentPlane.analyzeFolder()

IdDictAxis = {}
IdDictButts = {}
IdAxisStats = {}
IdBtnStats = {}
AxisSorted = []
BtnSorted = []
for pl in Planes:
    for aname in pl.axis:
        id= pl.axis[aname]
        if id not in IdDictAxis:
            IdDictAxis[id]= {}
            IdAxisStats[id] = 0
            AxisSorted.append(id)
        IdDictAxis[id][pl.name]=aname
        IdAxisStats[id]+=1
        
    for bname in pl.keybind:
        id=pl.keybind[bname]
        if id not in IdDictButts:
            IdDictButts[id]={}
            IdBtnStats[id] = 0
            BtnSorted.append(id)
        IdDictButts[id][pl.name]=bname
        IdBtnStats[id]+=1
        
        
isUnsorted= True
while isUnsorted:
    isUnsorted = False
    x = range (0, len(AxisSorted)-1)
    for i in x:
        if IdAxisStats[AxisSorted[i]] < IdAxisStats[AxisSorted[i+1]]:
            backupValue = AxisSorted[i]
            AxisSorted[i] = AxisSorted[i+1]
            AxisSorted[i+1] = backupValue
            isUnsorted = True

isUnsorted= True
while isUnsorted:
    isUnsorted = False
    x = range (0, len(BtnSorted)-1)
    for i in x:
        if IdBtnStats[BtnSorted[i]] < IdBtnStats[BtnSorted[i+1]]:
            backupValue = BtnSorted[i]
            BtnSorted[i] = BtnSorted[i+1]
            BtnSorted[i+1] = backupValue
            isUnsorted = True

        

print("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
axisfile = open(outptaxis, "w", encoding="utf8")
btnfile = open(outptbtn, "w", encoding="utf8")

axisfile.write("id")
btnfile.write("id")
for pl in Planes:
    axisfile.write(";"+pl.name)
    btnfile.write(";"+pl.name)
axisfile.write("\n")
btnfile.write("\n")

for id in AxisSorted:
    axisfile.write(id)
    for pl in Planes:
        cell = ""
        if pl.name in IdDictAxis[id]:
            cell = IdDictAxis[id][pl.name]
        axisfile.write(";"+cell)
    axisfile.write("\n")

for id in BtnSorted:
    btnfile.write(id)
    for pl in Planes:
        cell = ""
        if pl.name in IdDictButts[id]:
            cell = IdDictButts[id][pl.name]
        btnfile.write(";"+cell)
    btnfile.write("\n")
        

axisfile.close()
btnfile.close()
print("Done")

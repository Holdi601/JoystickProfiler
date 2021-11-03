local lfs = require('lfs')
LUA_PATH = "?;?.lua;"..lfs.currentdir().."/Scripts/?.lua"
local aircraft  = LoGetSelfData()["Name"]
local aircraft_old  = ''
function LuaExportStart()
	package.path  = package.path..";"..lfs.currentdir().."/LuaSocket/?.lua"
    package.cpath = package.cpath..";"..lfs.currentdir().."/LuaSocket/?.dll"
    socket = require("socket")
    host = "localhost"
    port = 1992
    c = socket.try(socket.connect(host, port)) -- connect to the listener socket
    c:setoption("tcp-nodelay",true) -- set immediate transmission mode
end
function LuaExportStop()
	socket.try(c:send("exit")) -- to close the listener socket
  	c:close()
end
function LuaExportAfterNextFrame()
    aircraft = LoGetSelfData()["Name"]
    if(aircraft ~= aircraft_old)
    then
        returnstring = aircraft.."\n"
	    socket.try(c:send(returnstring))
        aircraft_old=aircraft
    end
end
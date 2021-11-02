local lfs = require('lfs')
LUA_PATH = "?;?.lua;"..lfs.currentdir().."/Scripts/?.lua"
local aircraft  = LoGetObjectById(LoGetPlayerPlaneId())
local aircraft_old  = ''
function LuaExportStart()
	package.path  = package.path..";"..lfs.currentdir().."/LuaSocket/?.lua"
    package.cpath = package.cpath..";"..lfs.currentdir().."/LuaSocket/?.dll"
    socket = require("socket")
    host = "127.0.0.1"
    port = 1992
    c = socket.try(socket.connect(host, port)) -- connect to the listener socket
    c:setoption("tcp-nodelay",true) -- set immediate transmission mode
end
function debugWrite(text)
	socket.try(c:send(text))
end
function LuaExportStop()
	socket.try(c:send("quit")) -- to close the listener socket
  	c:close()
end
function LuaExportAfterNextFrame()
	aircraft = LoGetObjectById(LoGetPlayerPlaneId())
    if aircraft ~= aircraft_old then
        debugWrite(aircraft)
    end
    aircraft_old = aircraft
end
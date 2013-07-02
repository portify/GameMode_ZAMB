$ZAMB::Path = filePath(expandFileName("./description.txt")) @ "/";

if ($GameModeArg !$= "Add-Ons/GameMode_ZAMB/gamemode.txt") {
	exec("./server_custom.cs");
	return;
}

exec("./lib/ts-pathing.cs");
exec("./lib/vizard.cs");
exec("./lib/nodes.cs");

exec("./src/zamb/main.cs");
exec("./src/zombie/main.cs");
exec("./src/survivor/main.cs");

if (isFile("Add-Ons/GameMode_ZAMB/save.nav")) {
	schedule(0, 0, "loadNodes", "Add-Ons/GameMode_ZAMB/save.nav");
}
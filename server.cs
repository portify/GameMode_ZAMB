$ZAMB::Path = filePath(expandFileName("./description.txt")) @ "/";

if ($GameModeArg !$= "Add-Ons/GameMode_ZAMB/gamemode.txt") {
	// TODO: Provide minimal version of ZAMB in Custom game-mode for map building.
	error("ERROR: GameMode_ZAMB cannot be used in custom game-modes.");
	return;
}

exec("./lib/ts-pathing.cs");
exec("./lib/vizard.cs");
exec("./lib/nodes.cs");

exec("./src/zamb/main.cs");
exec("./src/zombie/main.cs");
exec("./src/survivor/main.cs");

if (isFile("Add-Ons/GameMode_ZAMB/save.nav")) {
	loadNodes("Add-Ons/GameMode_ZAMB/save.nav");
}
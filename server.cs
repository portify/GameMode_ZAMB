$ZAMB::Path = filePath(expandFileName("./description.txt")) @ "/";

if ($GameModeArg !$= "Add-Ons/GameMode_ZAMB/gamemode.txt") {
	// TODO: Provide minimal version of ZAMB in Custom game-mode for map building.
	error("ERROR: GameMode_ZAMB cannot be used in custom game-modes.");
	return;
}

exec("./lib/ts-pathing.cs");
exec("./lib/vizard.cs");
exec("./lib/nodes.cs");

exec("./src/datablocks.cs");
exec("./src/sound.cs");

exec("./src/main.cs");
exec("./src/soundController.cs");

exec("./src/zombie.cs");
exec("./src/survivor.cs");
exec("./src/tank.cs");
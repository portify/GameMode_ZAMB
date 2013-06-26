$ZAMB::Path = filePath(expandFileName("./description.txt")) @ "/";

if ($GameModeArg !$= "Add-Ons/GameMode_ZAMB/gamemode.txt") {
	// TODO: Provide minimal version of ZAMB in Custom game-mode for map building.
	error("ERROR: GameMode_ZAMB cannot be used in custom game-modes.");
	return;
}

exec( "./lib/ts-pathing.cs" );

exec( "./src/sounds.cs" );
exec( "./src/use.cs" );
exec( "./src/zombie.cs" );
$ZAMB::Path = filePath(expandFileName("./description.txt")) @ "/";

if ($GameModeArg !$= "Add-Ons/GameMode_ZAMB/gamemode.txt") {
	// TODO: Provide minimal version of ZAMB in Custom game-mode for map building.
	error("ERROR: GameMode_ZAMB cannot be used in custom game-modes.");
	return;
}

package zambPackage {
	function miniGameCanDamage(%a, %b) {
		%parent = parent::miniGameCanDamage(%a, %b);

		%m1 = getMiniGameFromObject(%a);
		%m2 = getMiniGameFromObject(%b);

		if (%m1 != %m2 || %m2 != $defaultMiniGame || !isObject($defaultMiniGame)) {
			return %parent;
		}

		%t1 = %a.getType() & $TypeMasks::PlayerObjectType;
		%t2 = %b.getType() & $TypeMasks::PlayerObjectType;

		if ( !%t1 || !%t2 )
		{
			return %parent;
		}

		return 1;
	}

	function player::removeBody(%this) {
		if (%this.getDataBlock().isZombie) {
			%this.delete();
		}
		else {
			parent::removeBody(%this);
		}
	}

	function player::playPain(%this) {
		if (!%this.getDataBlock().isZombie) {
			parent::playPain(%this);
		}
	}

	function player::emote(%this, %emote) {
		if (!%this.getDataBlock().isZombie || %emote !$= "PainLowImage") {
			parent::emote(%this, %emote);
		}
	}
};

activatePackage("zambPackage");

exec("./lib/ts-pathing.cs");
exec("./lib/vizard.cs");
exec("./lib/nodes.cs");

exec("./src/sounds.cs");
exec("./src/zombie.cs");
exec("./src/survivor.cs");

exec("./src/game/core.cs");
exec("./src/game/director.cs");
exec("./src/game/sound.cs");
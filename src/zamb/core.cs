function ZAMB_Core() {
	%obj = new ScriptObject() {
		class = ZAMB_Core;
		thinkLimit = 5;
	};

	MissionCleanup.add(%obj);
	return %obj;
}

function ZAMB_Core::onAdd(%this) {
	if ($Pref::Server::ZAMBDebug) {
		echo("ZAMB_Core(" @ %this @ ")::onAdd");
	}

	if (%this.difficulty $= "") {
		%this.difficulty = $Pref::Server::ZAMBDifficulty;
	}

	%this.zombies = ZAMB_Zombies();
	%this.director = ZAMB_Director(%this);

	%this.tick = %this.schedule(0, tick);
}

function ZAMB_Core::onRemove(%this) {
	if ($Pref::Server::ZAMBDebug) {
		echo("ZAMB_Core(" @ %this @ ")::onRemove");
	}

	%this.director.delete();
	%this.zombies.delete();
}

function ZAMB_Core::tick(%this) {
	cancel(%this.tick);

	%this.director.tick();
	%this.zombies.tick();

	if ($Pref::Server::ZAMBDebug && $Sim::Time - %this.lastDebugTime >= 0.2) {
		bottomPrintAll("<font:consolas:16>" @ %this.debug() @ "\n", 1, 5);
	}

	%this.tick = %this.schedule(100, tick);
}

function ZAMB_Core::debug(%this) {
	%wanderers = %this.zombies.wanderers.getCount() @ "/" @ %this.zombies.wanderers.limit;
	%hordes = %this.zombies.hordes.getCount() @ "/" @ %this.zombies.hordes.limit;
	%specials = %this.zombies.specials.getCount() @ "/" @ %this.zombies.specials.limit;
	%bosses = %this.zombies.bosses.getCount() @ "/" @ %this.zombies.bosses.limit;

	%str = "\c6wanderers:" SPC %wanderers;
	%str = %str @ "\n\c6hordes:" SPC %hordes;
	%str = %str @ "\n\c6specials:" SPC %specials;
	%str = %str @ "\n\c6bosses:" SPC %bosses;

	return %str;
}

function ZAMB_Core::end(%this, %message) {
	if ($Pref::Server::ZAMBDebug) {
		echo("ZAMB_Core(" @ %this @ ")::end()");
	}

	if (%this.ended) {
		return;
	}

	%this.ended = 1;
	%this.zombies.stop();

	cancel(%this.tick);

	for (%i = 0; %i < $defaultMiniGame.numMembers; %i++) {
		%client = $defaultMiniGame.member[%i];
		%player = %client.player;

		if (%message !$= "") {
			messageClient(%client, '', %message);
		}

		if (isObject(%player) && %player.getState() !$= "Dead") {
			%client.camera.setMode("Corpse", %player);
			%client.setControlObject(%client.camera);
		}
	}
}
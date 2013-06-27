function zambDirector::onAdd(%this) {
	if (!isObject(%this.core)) {
		error("ERROR: ZAMB director created without 'core' attribute.");
	}

	%this.state = 0;
}

function zambDirector::spawn(%this, %dataBlock, %transform) {
	if (%this.core.zombies.getCount() >= 50) {
		return -1;
	}

	if (!isObject(%dataBlock)) {
		error("ERROR: Unexistant datablock.");
		return -1;
	}

	if (!%datablock.isZombie) {
		error("ERROR: Datablock is not a zombie datablock.");
		return -1;
	}

	if (%transform $= "") {
		%transform = pickSpawnPoint();
	}

	%obj = new AIPlayer() {
		datablock = %dataBlock;
		isZombie = true;
	};

	%this.core.zombies.add(%obj);
	%obj.setTransform(%transform);

	if (!isEventPending(%this.zombieTick)) {
		%this.zombieTick = %this.schedule(0, "zombieTick");
	}

	return %obj;
}

function zambDirector::zombieTick(%this) {
	cancel(%this.zombieTick);
	%count = %this.core.zombies.getCount();

	if (!%count) {
		return;
	}

	for (%i = 0; %i < %count; %i++) {
		%obj = %this.core.zombies.getObject(%i);

		if (%obj.getState() $= "Dead") {
			%this.core.zombies.remove(%obj);

			%i--;
			%count--;

			continue;
		}

		if (%obj.lastZombieTick $= "") {
			%delta = 0;
		}
		else {
			%delta = $Sim::Time - %obj.lastZombieTick;
		}

		%obj.lastZombieTick = $Sim::Time;
		%obj.getDataBlock().zombieTick(%obj, %delta);
	}

	if ($Sim::Time - %this.lastZombieDebug >= 0.2) {
		%this.lastZombieDebug = $Sim::Time;

		%str = "\c6" @ %count SPC "zombies";

		commandToAll('BottomPrint', "<font:lucida console:14>" @ %str @ "\n", 1, 2);
	}

	%this.zombieTick = %this.schedule(100, "zombieTick");
}
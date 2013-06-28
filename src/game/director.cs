$ZOMBIE_LIMIT = 40;

$DIRECTOR_THINK_QUOTA = 5;
$DIRECTOR_TICK_RATE = 100;

function zambDirector::start(%this) {
	if (%this.isActive()) {
		error("ERROR: Director is already active.");
		return;
	}

	%this.tick = %this.schedule(0, "tick");
	%this.thinkIndex = 0;

	%this.nextHorde = %this.getHordeSpawnInterval();
	%this.prevCommon = "";
}

function zambDirector::stop(%this) {
	if (!%this.isActive()) {
		error("ERROR: Director is not active.");
		return;
	}

	cancel(%this.tick);
}

function zambDirector::isActive(%this) {
	return isEventPending(%this.tick);
}

function zambDirector::getZombieCount(%this) {
	return %this.core.zombies.getCount();
}

function zambDirector::tick(%this) {
	cancel(%this.tick);

	if (%this.nextQuarantineSound $= "") {
		%this.nextQuarantineSound = $Sim::Time + getRandom(60, 200);
	}
	else if ($Sim::Time > %this.nextQuarantineSound) {
		serverPlay2D("zamb_music_quarantine" @ getRandom(1, 3));
	}

	if (%this.nextHorde < $Sim::Time) {
		%this.spawnHorde();
		%this.nextHorde = $Sim::Time + %this.getHordeSpawnInterval();
	}

	%interval = %this.getCommonSpawnInterval();

	if (%interval != -1 && $Sim::Time - %this.prevCommon >= %interval) {
		%this.spawnCommon();
		%this.prevCommon = $Sim::Time;
	}

	%count = %this.getZombieCount();
	%usage = 0;

	while (%count && %usage < $DIRECTOR_THINK_QUOTA) {
		%this.thinkIndex = %this.thinkIndex % %count;
		%obj = %this.core.zombies.getObject(%this.thinkIndex);

		if (!isObject(%obj) || %seen[%obj]) {
			break;
		}

		if (%obj.getState() $= "Dead") {
			%this.core.zombies.remove(%obj);
			%this.thinkIndex--;

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

		%seen[%obj] = 1;

		%this.thinkIndex++;
		%usage++;
	}

	if ($Sim::Time - %this.lastZombieDebug >= 0.2) {
		%this.lastZombieDebug = $Sim::Time;

		%str = "\c6" @ %count SPC "zombies";

		if (%interval != -1) {
			%time = mCeil(%interval - ($Sim::Time - %this.prevCommon));
			%str = %str @ "\n\c6Common:" SPC %time @ "s";
		}

		if ($Sim::Time < %this.nextHorde) {
			%time = mCeil(%this.nextHorde - $Sim::Time);
			%str = %str @ "\n\c6Horde:" SPC %time @ "s";
		}

		commandToAll('BottomPrint', "<font:lucida console:14>" @ %str @ "\n", 1, 2);
	}

	%this.tick = %this.schedule($DIRECTOR_TICK_RATE, "tick");
}

function zambDirector::spawnHorde(%this) {
	if (getRandom() <= 0.3) {
		%n1 = 2;
		serverPlay2D(zamb_music_germs_high);
	}
	else {
		%n1 = 1;
		serverPlay2D(zamb_music_germs_low);
	}

	for (%i = 0; %i < %n1; %i++) {
		%n2 = getRandom(5, 10);

		for (%j = 0; %j < %n2; %j++) {
			%this.schedule(%j * 1000, "spawnZombie", baseZombieData);
		}
	}
}

function zambdirector::spawnCommon(%this) {
	%this.spawnZombie(baseZombieData);
	%this.prevCommon = $Sim::Time;
}

function zambDirector::getHordeSpawnInterval(%this) {
	return getRandom(60000, 120000) / 1000;
}

function zambDirector::getCommonSpawnInterval(%this) {
	%count = %this.getZombieCount();
	%limit = $ZOMBIE_LIMIT / 2;

	if (%count < %limit) {
		return 5 + (%count / %limit) * 30;
	}
	
	return -1;
}

function zambDirector::spawnZombie(%this, %dataBlock, %transform) {
	if (%this.core.zombies.getCount() >= $ZOMBIE_LIMIT) {
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

	return %obj;
}
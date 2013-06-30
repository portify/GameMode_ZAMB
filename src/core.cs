function ZAMB::onAdd(%this) {
	if (isObject(missionCleanup)) {
		missionCleanup.add(%this);
	}

	if (!isObject(%this.miniGame)) {
		error("ERROR: ZAMB created without a mini-game.");
		%this.delete();

		return;
	}

	%this.ended = 0;
	%this.index = 0;

	%this.quota = 5;
	%this.limit = 40;

	%this.tick = %this.scheduleNoQuota(0, "tick");
}

function ZAMB::end(%this) {
	if (%this.ended) {
		error("ERROR: ZAMB has already ended.");
		return;
	}

	%this.ended = 1;
	cancel(%this.tick);
}

function ZAMB::tick(%this) {
	cancel(%this.tick);

	if (%this.ended) {
		return;
	}

	%this.tickZombieSpawn();
	%this.tickZombieThink();

	if ($Sim::Time - %this.lastDebugTime >= 0.2) {
		%this.lastDebugTime = $Sim::Time;
		%this.tickDebug();
	}

	%this.tick = %this.schedule(100, "tick");
}

function ZAMB::tickZombieSpawn(%this) {
	if (%this.nextHorde $= "") {
		%this.nextHorde = $Sim::Time + %this.getHordeSpawnInterval();
	}

	if (%this.nextHorde <= $Sim::Time) {
		%this.spawnHorde();
		%this.nextHorde = $Sim::Time + %this.getHordeSpawnInterval();
	}

	%interval = %this.getCommonSpawnInterval();

	if (%interval != -1 && $Sim::Time - %this.prevCommon >= %interval) {
		%this.spawnCommon();
		%this.prevCommon = $Sim::Time;
	}
}

function ZAMB::tickZombieThink(%this) {
	%count = %this.getZombieCount();
	%usage = 0;

	while (%count && %usage < %this.quota) {
		%this.index = %this.index % %count;
		%obj = %this.miniGame.zombies.getObject(%this.index);

		if (!isObject(%obj) || %seen[%obj]) {
			break;
		}

		if (%obj.getState() $= "Dead") {
			%this.miniGame.zombies.remove(%obj);
			%this.index--;

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

		%this.index++;
		%usage++;
	}
}

function ZAMB::tickDebug(%this) {
	%str = "\c6" @ %this.getZombieCount() SPC "zombies";
	%interval = %this.getCommonSpawnInterval();

	if (%interval != -1 && $Sim::Time - %this.prevCommon < %interval) {
		%time = mCeil(%interval - ($Sim::Time - %this.prevCommon));
		%str = %str @ "\n\c6Common:" SPC %time @ "s";
	}

	if (%this.nextHorde !$= "" && $Sim::Time < %this.nextHorde) {
		%time = mCeil(%this.nextHorde - $Sim::Time);
		%str = %str @ "\n\c6Horde:" SPC %time @ "s";
	}

	commandToAll('BottomPrint', "<font:lucida console:14>" @ %str @ "\n", 1, 2);
}

function ZAMB::spawnHorde(%this) {
	if (getRandom() <= 0.4) {
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

function ZAMB::spawnCommon(%this) {
	%this.spawnZombie(baseZombieData);
	%this.prevCommon = $Sim::Time;
}

function ZAMB::getHordeSpawnInterval(%this) {
	return getRandom(60000, 120000) / 1000;
}

function ZAMB::getCommonSpawnInterval(%this) {
	%count = %this.getZombieCount();
	%limit = %this.limit / 2;

	if (%count < %limit) {
		return (%count / %limit) * 40;
	}
	
	return -1;
}

function ZAMB::spawnZombie(%this, %dataBlock, %transform) {
	if (%this.getZombieCount() >= %this.limit) {
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
		if (isObject(NodeGroup)) {
			%index = getRandom(0, NodeGroup.getCount() - 1);
			%node = NodeGroup.getObject(%index);

			%transform = %node.position;
		}
		else {
			%transform = pickSpawnPoint();
		}
	}

	%obj = new AIPlayer() {
		datablock = %dataBlock;
		isZombie = true;
	};

	%this.miniGame.zombies.add(%obj);
	%obj.setTransform(%transform);

	return %obj;
}

function ZAMB::getZombieCount(%this) {
	return %this.miniGame.zombies.getCount();
}
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

	%this.quota = 10;
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
	}

	%interval = %this.getCommonSpawnInterval();

	if (%interval != -1 && $Sim::Time - %this.prevCommon >= %interval) {
		%this.spawnCommon();
		%this.prevCommon = $Sim::Time;
	}

	if (%this.nextBoss $= "") {
		%this.nextBoss = $Sim::Time + %this.getBossSpawnInterval();
	}

	if (%this.nextBoss <= $Sim::Time) {
		%this.spawnBoss();
	}

	%this.handleSpecialZombie(boomerZombieData);
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

	if (%this.nextBoss !$= "" && $Sim::Time < %this.nextBoss) {
		%time = mCeil(%this.nextBoss - $Sim::Time);
		%str = %str @ "\n\c6Boss:" SPC %time @ "s";
	}

	commandToAll('BottomPrint', "<font:lucida console:14>" @ %str @ "\n", 1, 2);
}

function ZAMB::handleSpecialZombie(%this, %dataBlock) {
	if (%this.nextSpecial[%dataBlock] $= "") {
		%this.nextSpecial[%dataBlock] = $Sim::Time + %this.getSpecialSpawnInterval() + getRandom(15, 30);
	}

	if (%this.nextSpecial[%dataBlock] <= $Sim::Time) {
		%this.spawnZombie(%dataBlock);
		%this.nextSpecial[%dataBlock] = $Sim::Time + %this.getSpecialSpawnInterval();
	}
}

function ZAMB::getHordeSpawnInterval(%this) {
	return (4 - %this.difficulty) * 60 + getRandom(-20, 30);
}

function ZAMB::getCommonSpawnInterval(%this) {
	%count = %this.getZombieCount();
	%limit = %this.limit / 2;

	if (%count < %limit) {
		return (%count / %limit) * 10;
	}
	
	return -1;
}

function ZAMB::getBossSpawnInterval(%this) {
	return getRandom(120, 240);
}

function ZAMB::getZombieCount(%this) {
	return %this.miniGame.zombies.getCount();
}

function getZAMBDifficulty(%obj) {
	if (isObject(%obj)) {
		%miniGame = getMiniGameFromObject(%obj);
	}

	if (!isObject(%miniGame)) {
		%miniGame = $defaultMiniGame;
	}

	if (!isObject(%miniGame.zamb)) {
		return -1;
	}

	return %miniGame.zamb.difficulty;
}
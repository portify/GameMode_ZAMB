function ZAMB::spawnZombie(%this, %dataBlock, %node) {
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

	if (%node $= "") {
		if (isObject(NodeGroup)) {
			%node = NodeGroup.findZombieSpawn(%this.miniGame);
		}
		else {
			error("ERROR: No node specified and no NodeGroup to randomly choose from!");
			return -1;
		}
	}
	else if (!isObject(%node)) {
		error("ERROR: Invalid node specified!");
		return -1;
	}

	%obj = new AIPlayer() {
		datablock = %dataBlock;
		isZombie = true;
	};

	%this.miniGame.zombies.add(%obj);
	%obj.setTransform(%node.position);

	return %obj;
}

function ZAMB::spawnHorde(%this, %noSound) {
	if (getRandom() <= 0.4) {
		%n1 = 2;
	}
	else {
		%n1 = 1;
	}

	if (!%noSound) {
		if (%n1 == 1) {
			serverPlay2D(zamb_music_germs_low);
		}
		else if (%n1 == 2) {
			serverPlay2D(zamb_music_germs_high);
		}
	}

	for (%i = 0; %i < %n1; %i++) {
		%n2 = getRandom(5, 10);
		%node = NodeGroup.findZombieSpawn(%this.miniGame);

		for (%j = 0; %j < %n2; %j++) {
			%this.schedule(%j * 1000, "spawnZombie", baseZombieData, %node);
		}
	}

	%this.lastHorde = $Sim::Time;
	%this.nextHorde = $Sim::Time + %this.getHordeSpawnInterval();
}

function ZAMB::spawnCommon(%this) {
	%this.spawnZombie(baseZombieData);
	%this.prevCommon = $Sim::Time;
}

function ZAMB::spawnBoss(%this) {
	%this.nextBoss = "";
	%types = "none tank";

	if (%this.lastBossType !$= "") {
		%types = trim(strReplace(" " @ %types @ " ", " " @ %this.lastBossType @ " ", " "));
	}

	%type = pickRandomItemFromList(%types);

	if (%type $= "" || %type $= "none") {
		return;
	}

	switch$ (%type) {
		case "tank": %this.spawnZombie(tankZombieData);
	}
}

function NodeGroup::findZombieSpawn(%this, %miniGame, %objectBox) {
	return pickRandomItemFromList(%this.findAllZombieSpawns(%miniGame, %objectBox));
}

function NodeGroup::findAllZombieSpawns(%this, %miniGame, %objectBox) {
	%count = %this.getCount();

	for (%i = 0; %i < %count; %i++) {
		%node = %this.getObject(%i);

		if (%node.isValidZombieSpawn(%miniGame, %objectBox)) {
			if (%spawns $= "") {
				%spawns = %node;
			}
			else {
				%spawns = %spawns SPC %node;
			}
		}
	}

	return %spawns;
}

function NodeSO::isValidZombieSpawn(%this, %miniGame) {
	if (isObject(%miniGame)) {
		%min = 8;
		%max = 32;

		for (%i = 0; %i < %miniGame.numMembers; %i++) {
			%obj = %miniGame.member[%i].player;

			if (isObject(%obj) && %obj.getState() !$= "Dead") {
				%dist = vectorDist(%this.position, %obj.position);

				if (%dist <= %min) {
					return 0;
				}

				if (%lowest $= "" || %dist < %lowest) {
					%lowest = %dist;
				}

				if (%dist > %max) {
					continue;
				}

				%ray = containerRayCast(
					%obj.getHackPosition(),
					vectorAdd(%this.position, "0 0 0.1"),
					$TypeMasks::FxBrickObjectType
				);

				if (%ray $= "0") {
					return 0;
				}
			}
		}

		if (%lowest $= "" || %lowest > %max) {
			return 0;
		}
	}

	return 1;
}

function pickRandomItemFromList(%list) {
	%count = getWordCount(%list);

	if (!%count) {
		return "";
	}

	return getWord(%list, getRandom(0, %count - 1));
}
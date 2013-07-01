exec("./package.cs");

$ZAMB::TickRate = 100;
$ZAMB::ThinkLimit = 10;
$ZAMB::ZombieLimit = 40;

function ZAMB(%difficulty) {
	if (isObject(ZAMB)) {
		ZAMB.delete();
	}

	new ScriptGroup(ZAMB) {
		difficulty = %difficulty;
	};
}

function ZAMB::onAdd(%this) {
	if (%this.difficulty $= "") {
		if ($defaultMiniGame.difficulty !$= "") {
			%this.difficulty = %defaultMiniGame.difficulty;
		}
		else {
			%this.difficulty = $Pref::Server::ZAMBDifficulty;
		}
	}
}

function ZAMB::end(%this) {
	if (%this.ended) {
		return;
	}

	%this.ended = true;
	cancel(%this.tick);

	$defaultMiniGame.schedule(10000, "reset");
}

function ZAMB::tick(%this) {
	cancel(%this.tick);

	%this.updateSpawning();
	%this.updateAI();

	%this.tick = %this.schedule($ZAMB::TickRate, "tick");
}

function ZAMB::updateSpawning(%this) {
}

function ZAMB::updateAI(%this) {
	%count = %this.getCount();
	%usage = 0;

	while (%count && %usage < $ZAMB::ThinkLimit) {
		%this.index = %this.index % %count;
		%obj = %this.getObject(%this.index);

		if (!isObject(%obj) || %seen[%obj]) {
			break;
		}

		if (%obj.getState() $= "Dead") {
			%this.remove(%obj);
			%this.index--;

			%count--;
			continue;
		}

		// %obj.getDataBlock().updateAI(%obj);
		%seen[%obj] = 1;

		%this.index++;
		%usage++;
	}
}

function ZAMB::createZombie(%this, %dataBlock, %transform) {
	if (!isObject(%dataBlock) || !%dataBlock.isZombie) {
		error("ERROR: Invalid zombie datablock!");
		return -1;
	}

	if (%this.getCount() >= $ZAMB::ZombieLimit) {
		return -1;
	}

	%obj = new AIPlayer() {
		datablock = %dataBlock;
	};

	if (!isObject(%obj)) {
		error("ERROR: Object creation failed!");
		return -1;
	}

	%this.add(%obj);
	%obj.setTransform(%transform);

	return %obj;
}

function ZAMB::spawnZombie(%this, %dataBlock, %node) {
	if (!isObject(%dataBlock) || !%dataBlock.isZombie) {
		error("ERROR: Invalid zombie datablock!");
		return -1;
	}

	if (%this.getCount() >= $ZAMB::ZombieLimit) {
		return -1;
	}

	if (%node $= "") {
		if (isObject(NodeGroup)) {
			%node = NodeGroup.findZombieSpawn();
		}
	}

	if (!isObject(%node)) {
		return -1;
	}

	return %this.createZombie(%dataBlock, %node.position);
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
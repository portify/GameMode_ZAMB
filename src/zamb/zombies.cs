function ZAMB_Zombies(%zamb) {
	return new ScriptGroup() {
		class = ZAMB_Zombies;
		zamb = %zamb;
	};
}

function ZAMB_Zombies::onAdd(%this) {
	if ($Pref::Server::ZAMBDebug) {
		echo("ZAMB_Zombies(" @ %this @ ")::onAdd");
	}

	%this.wanderers = new SimSet() {
		limit = 10;

		spawnRange = 30;
		despawnRange = 40;
	};

	%this.hordes = new SimSet() {
		limit = 20;
		
		spawnRange = 24;
		despawnRange = 32;
	};

	%this.specials = new SimSet() {
		limit = 3;

		spawnRange = 48;
		despawnRange = 96;
	};

	%this.bosses = new SimSet() {
		limit = 8;

		spawnRange = 64;
		despawnRange = -1;
	};
}

function ZAMB_Zombies::onRemove(%this) {
	if ($Pref::Server::ZAMBDebug) {
		echo("ZAMB_Zombies(" @ %this @ ")::onRemove");
	}

	%this.wanderers.delete();
	%this.hordes.delete();
	%this.specials.delete();
	%this.bosses.delete();
}

function ZAMB_Zombies::stop(%this) {
	%count = %this.getCount();

	for (%i = 0; %i < %count; %i++) {
		%obj = %this.getObject(%i);

		%obj.stop();
		%obj.clearAim();

		%obj.setMoveX(0);
		%obj.setMoveY(0);

		%obj.setJumping(0);
		%obj.setCrouching(0);
	}
}

function ZAMB_Zombies::tick(%this) {
	%count = %this.getCount();

	while (%count && %usage < %this.zamb.thinkLimit) {
		%this.index = %this.index % %count;
		%obj = %this.getObject(%this.index);

		if (!isObject(%obj) || %seen[%obj]) {
			break;
		}

		if (%obj.getState() $= "Dead") {
			%this.remove(%obj);
			%obj.typeGroup.remove(%obj);

			MissionCleanup.add(%obj);
		}
		else if (!isSurvivorWithinRange(%obj.position, %obj.typeGroup.despawnRange)) {
			%obj.delete();
		}

		if (!isObject(%obj) || !%this.isMember(%obj)) {
			%this.index--;
			%count--;

			continue;
		}

		%obj.getDataBlock().updateAI(%obj);
		%seen[%obj] = 1;

		%this.index++;
		%usage++;
	}
}

function ZAMB_Zombies::spawn(%this, %dataBlock, %node) {
	if (!isObject(%dataBlock) || !%dataBlock.isZombie) {
		error("ERROR: Invalid zombie datablock!");
		return -1;
	}

	switch$ (%dataBlock.zombieType) {
		case 0: %group = %this.wanderers;
		case 1: %group = %this.hordes;
		case 2: %group = %this.specials;
		case 3: %group = %this.bosses;

		default:
			error("ERROR: Zombie datablock has an invalid zombieType attribute.");
			return -1;
	}

	if (%node $= "") {
		if (isObject(NodeGroup)) {
			%node = NodeGroup.findZombieSpawn(%group.spawnRange);
		}
	}

	if (!isObject(%node) || %group.getCount() >= %group.limit) {
		return -1;
	}

	%obj = new AIPlayer() {
		datablock = %dataBlock;
		typeGroup = %group;
	};

	if (!isObject(%obj)) {
		error("ERROR: Object creation failed!");
		return -1;
	}

	%this.add(%obj);
	%group.add(%obj);

	%obj.setTransform(%node.position);
	return %obj;
}

function NodeGroup::findZombieSpawn(%this, %range) {
	return pickRandomItemFromList(%this.findAllZombieSpawns(%range));
}

function NodeGroup::findAllZombieSpawns(%this, %range) {
	%count = %this.getCount();

	for (%i = 0; %i < %count; %i++) {
		%node = %this.getObject(%i);

		if (%node.isValidZombieSpawn(%range)) {
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

function NodeSO::isValidZombieSpawn(%this, %range) {
	for (%i = 0; %i < $defaultMiniGame.numMembers; %i++) {
		%obj = $defaultMiniGame.member[%i].player;

		if (isObject(%obj) && %obj.getState() !$= "Dead") {
			%dist = vectorDist(%this.position, %obj.position);

			if (%dist < 5 || %dist > %range) {
				continue;
			}

			%inRange = 1;

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

	return %inRange ? 1 : 0;
}

function pickRandomItemFromList(%list) {
	%count = getWordCount(%list);

	if (!%count) {
		return "";
	}

	return getWord(%list, getRandom(0, %count - 1));
}

function isSurvivorWithinRange(%point, %range) {
	if (%range == -1) {
		return 1;
	}

	initContainerRadiusSearch(%point, %range, $TypeMasks::PlayerObjectType);

	while (isObject(%obj = containerSearchNext())) {
		if (!%obj.getDataBlock().isSurvivor || %obj.getState() $= "Dead") {
			continue;
		}

		if (vectorDist(%point, %obj.position) <= %range) {
			return 1;
		}
	}

	return 0;
}
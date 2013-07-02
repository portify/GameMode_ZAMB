function ZAMB_Zombies(%zamb) {
	return new ScriptGroup() {
		class = ZAMB_Zombies;
		zamb = %zamb;
	};
}

function ZAMB_Zombies::onAdd(%this) {
	%this.wanderers = new SimSet() { limit = 10; };
	%this.hordes    = new SimSet() { limit = 30; };
	%this.specials  = new SimSet() { limit = 3;  };
	%this.bosses    = new SimSet() { limit = 8;  };
}

function ZAMB_Zombies::onRemove(%this) {
	%this.wanderers.delete();
	%this.hordes.delete();
	%this.specials.delete();
	%this.bosses.delete();
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
			%this.index--;

			%count--;
			continue;
		}

		//%obj.getDataBlock().updateAI(%obj);
		%seen[%obj] = 1;

		%this.index++;
		%usage++;
	}
}

function ZAMB_Zombies::create(%this, %dataBlock, %transform) {
	if (!isObject(%dataBlock) || !%dataBlock.isZombie) {
		error("ERROR: Invalid zombie datablock!");
		return -1;
	}

	switch$ (%dataBlock.zombieType) {
		case 0: %group = %this.wanderers;
		case 1: %group = %this.hordes;
		case 2: %group = %this.specials;
		case 3: %group = %this.bosses;
	}

	if (!isObject(%group)) {
		error("ERROR: Zombie datablock has an invalid zombieType attribute.");
		return -1;
	}

	if (%group.getCount() >= %group.limit) {
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
	%group.add(%obj);

	%obj.setTransform(%transform);
	return %obj;
}

function ZAMB_Zombies::spawn(%this, %dataBlock, %node) {
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

	return %this.create(%dataBlock, %node.position);
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
		%min = 4;
		%max = 16;

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
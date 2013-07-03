// -------
// General

function ZAMB_Director::maintainFullThreat(%this) {
	%this.maintainWanderers();
	%this.maintainHordes();
	%this.maintainSpecials();
}

// -----------------
// Wanderer spawning

function ZAMB_Director::maintainWanderers(%this) {
	%count = %this.zombies.wanderers.getCount();
	%limit = %this.zombies.wanderers.limit;

	if (%count >= %limit) {
		return;
	}

	%interval = 1 + (%count / %limit) * 20;

	if ($Sim::Time - %this.prevCommon >= %interval) {
		%this.zombies.spawn(wandererZombieData);
		%this.prevCommon = $Sim::Time;
	}
}

// --------------
// Horde spawning

function ZAMB_Director::maintainHordes(%this) {
	if (%this.nextHorde $= "") {
		%this.nextHorde = $Sim::Time + %this.getHordeSpawnInterval();
	}

	if (%this.nextHorde <= $Sim::Time) {
		%this.horde();
	}
}

function ZAMB_Director::horde(%this) {
	%size = getRandom(10, 30);
	%node = NodeGroup.findZombieSpawn(%this.zombies.hordes.spawnRange);

	%this.prevHorde = $Sim::Time;
	%this.nextHorde = $Sim::Time + %this.getHordeSpawnInterval();

	%this.hordeSpawner(%node, %size);
}

function ZAMB_Director::hordeSpawner(%this, %node, %size) {
	if (!isObject(%node) || !%size) {
		return;
	}

	%obj = %this.zombies.spawn(hordeZombieData, %node);

	if (isObject(%obj)) {
		// %obj.setRage(1);

		if (%size >= 2) {
			%this.schedule(1000, hordeSpawner, %node, %size - 1);
		}
	}
}

function ZAMB_Director::getHordeSpawnInterval(%this) {
	return (4 - %this.zamb.difficulty) * 60 + getRandom(-20, 30);
}

// ----------------
// Special spawning

function ZAMB_Director::maintainSpecials(%this) {
	if ($Sim::Time - %this.zamb.start < 30) {
		return;
	}

	if (%this.zombies.specials.getCount() >= %this.zombies.specials.limit) {
		return;
	}

	%types = "boomerZombieData";
	%count = %this.zombies.specials.getCount();

	for (%i = 0; %i < %count; %i++) {
		%obj = %this.zombies.specials.getObject(%i);
		%exists[%obj.getDataBlock()] = 1;
	}

	%count = getFieldCount(%types);

	for (%i = 0; %i < %count; %i++) {
		%type = getField(%types, %i).getID();

		if (%exists[%type]) {
		 	continue;
		}

		%object = %this.specialObject[%type];

		if (!isObject(%object) || %object.getState() $= "Dead") {
			if (%this.specialTimer[%type] $= "") {
				%this.specialTimer[%type] = $Sim::Time + getRandom(30, 60);
			}

			if (%this.specialTimer[%type] <= $Sim::Time) {
				if (%valid $= "") {
					%valid = %type;
				}
				else {
					%valid = %valid SPC %type;
				}
			}
		}
	}

	%type = pickRandomItemFromList(%valid);

	if (%type !$= "") {
		%obj = %this.zombies.spawn(%type);

		if (isObject(%obj)) {
			%this.specialObject[%type] = %obj;
			%this.specialTimer[%type] = "";
		}
	}
}

// -------------
// Boss spawning

function ZAMB_Director::maintainBosses(%this) {
	if (%this.nextBoss $= "") {
		%this.nextBoss = $Sim::Time + %this.getBossSpawnInterval();
	}

	if (%this.nextBoss <= $Sim::Time) {
		%this.boss();
	}
}

function ZAMB_Director::boss(%this) {
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
		case "tank": %this.zombies.spawn(tankZombieData);
	}
}

function ZAMB_Director::getBossSpawnInterval(%this) {
	return getRandom(150, 300);
}
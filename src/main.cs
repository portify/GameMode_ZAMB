package zambPackage {
	function miniGameSO::onAdd(%this) {
		if ($gameFlowDebug) {
			warn("MiniGameSO::onAdd");
		}

		parent::onAdd(%this);
	}

	function miniGameSO::onRemove(%this) {
		if ($gameFlowDebug) {
			warn("MiniGameSO::onRemove");
		}

		if (isObject(%this.zamb)) {
			%this.zamb.delete();
		}

		if (isObject(%this.sound)) {
			%this.sound.delete();
		}

		if (isObject(%this.zombies)) {
			%this.zombies.delete();
		}

		parent::onRemove(%this);
	}

	function miniGameSO::addMember(%this, %client) {
		if ($gameFlowDebug) {
			warn("MiniGameSO::addMember");
		}

		parent::addMember(%this, %client);

		if (%this.owner == 0 && !isObject(%this.zamb)) {
			%this.zambStart();
		}
	}

	function miniGameSO::removeMember(%this, %client) {
		if ($gameFlowDebug) {
			warn("MiniGameSO::removeMember");
		}

		parent::removeMember(%this, %client);

		if (!%this.numMembers && isObject(%this.zamb)) {
			%this.zambStop();
		}
	}

	function miniGameSO::reset(%this, %client) {
		if ($gameFlowDebug) {
			warn("MiniGameSO::reset");
		}

		if (%this.numMembers && isObject(%this.zamb)) {
			%this.zambStop();
			%this.zambStart();
		}

		parent::reset(%this, %client);
	}

	function miniGameCanDamage(%a, %b) {
		if (%b.getType() & $TypeMasks::PlayerObjectType) {
			if (%b.getDataBlock().isZombie) {
				return 1;
			}
		}

		%parent = parent::miniGameCanDamage(%a, %b);

		if (isObject(%m1)) {
			%m1 = getMiniGameFromObject(%a);
		}
		else {
			%m1 = -1;
		}

		if (isObject(%m2)) {
			%m2 = getMiniGameFromObject(%b);
		}
		else {
			%m2 = -1;
		}

		if (%m1 != %m2 || !isObject(%m1.zamb)) {
			return %parent;
		}

		%t1 = %a.getType() & $TypeMasks::PlayerObjectType;
		%t2 = %b.getType() & $TypeMasks::PlayerObjectType;

		if (!%t1 || !%t2) {
			return %parent;
		}

		return 1;
	}

	function gameConnection::onDeath(%this, %obj, %killer, %type, %area) {
		parent::onDeath(%this, %obj, %killer, %type, %area);
		%miniGame = %this.miniGame;

		if (!isObject(%miniGame.zamb) || %miniGame.zamb.ended) {
			return;
		}

		%this.zambCameraTarget = -1;
		%this.zambCameraSched = %this.schedule(2000, "getZAMBAutoCameraTarget");

		%alive = 0;
		messageClient(%this, 'MsgYourSpawn');

		for (%i = 0; %i < %miniGame.numMembers; %i++) {
			%player = %miniGame.member[%i].player;

			if (isObject(%player) && %player.getState() !$= "Dead") {
				%alive = 1;
				break;
			}
		}

		if (!%alive) {
			%miniGame.zambEnd();
		}
	}

	function Observer::onTrigger(%this, %obj, %slot, %state) {
		%client = %obj.getControllingClient();

		if (!isObject(%client)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		%miniGame = %client.miniGame;

		if (!isObject(%miniGame.zamb)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		if (%miniGame.zamb.ended) {
			return;
		}

		if (isObject(%client.player) || %state == 0) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		%client.zambCameraTarget = mFloor(%client.zambCameraTarget);

		if (%slot != 1 && %slot != 3) {
			cancel(%client.zambCameraSched);
		}

		switch (%slot) {
			case 0: // Switch Target Negatively
				%i = %client.zambCameraTarget - 1;
				%l = %client.zambCameraTarget - %miniGame.numMembers;

				for (%i; %i > %l; %i--) {
					%ei = %i < 0 ? %miniGame.numMembers - %i : %i;
					%cl = %miniGame.member[%ei];

					if (isObject(%cl.player) && %cl.player.getState() !$= "Dead") {
						%obj.setMode("Corpse", %cl.player);
						%client.zambCameraTarget = %ei;

						break;
					}
				}

			case 4: // Switch Target Positively
				%i = %client.zambCameraTarget + 1;
				%l = %client.zambCameraTarget + %miniGame.numMembers;

				for (%i; %i < %l; %i++) {
					%ei = %i % %miniGame.numMembers;
					%cl = %miniGame.member[%ei];

					if (isObject(%cl.player) && %cl.player.getState() !$= "Dead") {
						%obj.setMode("Corpse", %cl.player);
						%client.zambCameraTarget = %ei;

						break;
					}
				}

			case 2: // Switch Camera Mode
				cancel(%client.zambCameraSched);

				if (%obj.mode $= "Corpse") {
					%obj.setMode("Observer");
					return;
				}

				%i = %client.zambCameraTarget + 1;
				%limit = %miniGame.numMembers + %client.zambCameraTarget;

				for (%i; %i < %limit; %i++) {
					%cl = %miniGame.member[%i % %miniGame.numMembers];

					if (isObject(%cl.player) && %cl.player.getState() !$= "Dead") {
						%obj.setMode("Corpse", %cl.player);
						%client.zambCameraTarget = %i;
					}
				}
		}
	}

	function Armor::onTrigger(%this, %obj, %slot, %state) {
		%client = %obj.getControllingClient();

		if (!isObject(%client)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		%miniGame = %client.miniGame;

		if (!isObject(%miniGame.zamb)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		if (%miniGame.zamb.ended) {
			return;
		}

		return parent::onTrigger(%this, %obj, %slot, %state);
	}

	function player::removeBody(%this) {
		if (%this.getDataBlock().isZombie) {
			%this.delete();
		}
		else {
			parent::removeBody(%this);
		}
	}

	function player::playPain(%this) {
		if (!%this.getDataBlock().isZombie) {
			parent::playPain(%this);
		}
	}

	function player::emote(%this, %emote) {
		if (!%this.getDataBlock().isZombie || %emote !$= "PainLowImage") {
			parent::emote(%this, %emote);
		}
	}
};

activatePackage("zambPackage");

function gameConnection::getZAMBAutoCameraTarget(%this) {
	%miniGame = %this.miniGame;

	if (!isObject(%miniGame.zamb) || %miniGame.zamb.ended || !isObject(%this.camera)) {
		return;
	}

	if (isObject(%this.player) && %this.player.getState() !$= "Dead") {
		return;
	}

	for (%i = 0; %i < %miniGame.numMembers; %i++) {
		%client = %mini.member[%i];

		if (isObject(%client.player) && %client.player.getState() !$= "Dead") {
			%this.zambCameraTarget = %i;
			%this.camera.setMode("Corpse", %cl.player);

			%this.setControlObject(%this.camera);
			return;
		}
	}
}

function miniGameSO::zambStart(%this) {
	if ($gameFlowDebug) {
		warn("MiniGameSO::zambStart");
	}

	if (%this.owner != 0) {
		error("ERROR: You cannot start ZAMB on a player-owned mini-game.");
		return;
	}

	if (isObject(%this.zamb)) {
		error("ERROR: This mini-game already has a running ZAMB session.");
		return;
	}

	if (isObject(%this.sound)) {
		%this.sound.delete();
	}

	if (isObject(%this.zombies)) {
		%this.zombies.delete();
	}

	%this.zamb = new ScriptObject() {
		class = ZAMB;
		miniGame = %this;
	};

	%this.sound = new ScriptGroup() {
		class = zambSoundController;
		miniGame = %this;
	};

	%this.zombies = new SimGroup() {
		miniGame = %this;
	};

	missionCleanup.add(%this.zamb);
}

function miniGameSO::zambStop(%this) {
	if ($gameFlowDebug) {
		warn("MiniGameSO::zambStop");
	}

	if (!isObject(%this.zamb)) {
		error("This mini-game does not have a running ZAMB session.");
		return;
	}

	%this.zamb.delete();
	%this.sound.delete();
	%this.zombies.delete();
}

function miniGameSO::zambEnd(%this, %message) {
	if ($gameFlowDebug) {
		warn("MiniGameSO::zambEnd");
	}

	if (!isObject(%this.zamb) || %this.zamb.ended) {
		return;
	}

	%this.zamb.end();
	%this.schedule(5000, "reset", 0);

	if (%message !$= "") {
		%this.chatMessageAll(%message);
	}
}

function ZAMB::onAdd(%this) {
	if ($gameFlowDebug) {
		warn("ZAMB::onAdd");
	}

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

	%this.tick = %this.schedule(0, "tick");
}

function ZAMB::onRemove(%this) {
	if ($gameFlowDebug) {
		warn("ZAMB::onRemove");
	}
}

function ZAMB::getZombieCount(%this) {
	return %this.miniGame.zombies.getCount();
}

function ZAMB::end(%this) {
	if ($gameFlowDebug) {
		warn("ZAMB::end");
	}

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
		%transform = pickSpawnPoint();
	}

	%obj = new AIPlayer() {
		datablock = %dataBlock;
		isZombie = true;
	};

	%this.miniGame.zombies.add(%obj);
	%obj.setTransform(%transform);

	return %obj;
}
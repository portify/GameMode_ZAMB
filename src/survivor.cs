function playerSurvivorArmor::onAdd(%this, %obj) {
	parent::onAdd(%this, %obj);
	%obj.footstepUpdateTick();
}

function playerSurvivorArmor::onTrigger(%this, %obj, %slot, %state) {
	parent::onTrigger(%this, %obj, %slot, %state);
	%obj.trigger[%slot] = %state;

	if (%slot != 4 || !%state || $Sim::Time - %obj.lastShove < 0.8) {
		return;
	}

	%obj.lastShove = $Sim::Time;
	%obj.playThread(3, "shiftUp");

	%start = %obj.getHackPosition();
	%eye = %obj.getEyeVector();

	initContainerRadiusSearch(%obj.getHackPosition(), 4, $TypeMasks::PlayerObjectType);
	%miniGame = getMiniGameFromObject(%obj);

	while (isObject(%col = containerSearchNext())) {
		if (getMiniGameFromObject(%col) != %miniGame) {
			continue;
		}

		%end = %col.getHackPosition();

		if (vectorDist(%start, %end) >= 4) {
			continue;
		}

		%line = vectorNormalize(vectorSub(%end, %start));

		if (vectorDot(%eye, %line) >= 0.75) {
			if (%col.getDataBlock().noZombiePush) {
				continue;
			}

			if (!%col.getDataBlock().isZombie) {
				%hitSurvivor = true;
				continue;
			}

			%col.setVelocity(vectorAdd(vectorScale(%line, 10), "0 0 6"));
			%col.damage(%obj, %end, getRandom(-5, -15), $DamageType::Suicide);

			%hitInfected = true;
		}
	}

	if (%hitInfected) {
		%profile = zamb_survivor_swing_infected;
	}
	else if (%hitSurvivor) {
		%profile = zamb_survivor_swing_survivor;
	}
	else {
		%start = %obj.getEyePoint();

		%ray = containerRayCast(%start,
			vectorAdd(%start, vectorScale(%eye, 4)),
			$TypeMasks::FxBrickObjectType | $TypeMasks::TerrainObjectType
		);

		if (%ray !$= "0") {
			%profile = zamb_survivor_swing_world;
		}
		else {
			%profile = zamb_survivor_swing_miss;
		}
	}

	if (%profile !$= "zamb_survivor_swing_miss") {
		%obj.playThread(2, "plant");
	}

	if (isObject(%profile)) {
		serverPlay3D(%profile, %obj.getHackPosition());
	}
}

function playerSurvivorArmor::damage(%this, %obj, %src, %origin, %damage, %type) {
	if (isObject(%src) && %src != %obj && %src.getDataBlock().isSurvivor) {
		%difficulty = $defaultMiniGame.zamb.difficulty;

		switch (%difficulty) {
			case 0: %mod = 0;
			case 1: %mod = 0.1;
			case 2: %mod = 0.5;
			case 3: %mod = 1;
		}

		%damage *= %mod;
	}

	parent::damage(%this, %obj, %src, %origin, %damage, %type);
}

datablock fxLightData(playerFlashlightData : playerLight) {
	uiName = "";
	flareOn = 0;

	radius = 16; // 10
	brightness = 3;
};

package zambSurvivorPackage {
	function serverCmdLight(%client) {
		%player = %client.player;

		if (!isObject(%player) || %player.getState() $= "Dead") {
			parent::serverCmdLight(%client);
			return;
		}

		%dataBlock = %player.getDataBlock();

		if (%dataBlock != nameToID("playerSurvivorArmor")) {
			if (%dataBlock.isZombie) {
				parent::serverCmdLight(%client);
			}

			return;
		}

		if (getSimTime() - %player.lastLightTime < 250) {
			return;
		}

		%player.lastLightTime = getSimTime();
		serverPlay3D(zamb_survivor_flashlight, %player.getHackPosition());

		if (isObject(%player.light)) {
			%player.light.delete();
		}
		else {
			%player.light = new fxLight() {
				datablock = playerFlashlightData;
				iconSize = 1;

				player = %player;
				enable = 1;
			};

			missionCleanup.add(%player.light);
			%player.light.setTransform(%player.getTransform());

			if (!isEventPending(%player.flashlightTick)) {
				%player.flashlightTick();
			}
		}
	}
};

activatePackage("zambSurvivorPackage");

function player::flashlightTick(%this) {
	cancel(%this.flashlightTick);

	if (%this.getState() $= "Dead" || !isObject(%this.light)) {
		return;
	}

	// %start = %this.getEyePoint();
	%start = %this.getMuzzlePoint(0);
	%vector = %this.getEyeVector();
	// %vector = %this.getMuzzleVector(0);

	%range = 50;
	%limit = $EnvGuiServer::VisibleDistance / 2;

	if (%range > %limit) {
		%range = %limit;
	}

	%end = vectorAdd(%start, vectorScale(%vector, %range));
	%end = vectorAdd(%end, %this.getVelocity());
	%ray = containerRayCast(%start, %end, $TypeMasks::All, %this);

	if (%ray $= "0") {
		%pos = %end;
	}
	else {
		%pos = vectorSub(getWords(%ray, 1, 3), %vector);
	}

	%path = vectorSub(%pos, %this.light.position);
	%length = vectorLen(%path);

	%speed = 0.35;

	if (%length < %speed) {
		%pos = %pos;
	}
	else {
		%moved = vectorScale(%path, %speed);
		%pos = vectorAdd(%this.light.position, %moved);
	}

	%this.light.setTransform(%pos);
	%this.light.inspectPostApply();

	%this.flashlightTick = %this.schedule(32, "flashlightTick");
}

function gameConnection::updateZAMBVignette(%this) {
	cancel(%this.updateZAMBVignette);

	%r = 0;
	%g = 0;
	%b = 0;
	%a = 1;

	%player = %this.player;

	if (isObject(%player) && %player.getState() !$= "Dead") {
		%damage = %player.getDamageLevel() / %player.getDataBlock().maxDamage;
		%boomer = $Sim::Time - %player.lastBoomerVictimTime;

		if (%damage > 0) {
			%r += %player.getDamageLevel() / %player.getDataBlock().maxDamage;
		}

		if (%boomer < 15) {
			%g += %boomer <= 10 ? 1 : 1 - (%boomer - 10) / 5;

			if (%boomer < 10) {
				%this.updateZAMBVignette = %this.schedule((10 - %boomer) * 1000, "updateZAMBVignette");
			}
			else {
				%this.updateZAMBVignette = %this.schedule(100, "updateZAMBVignette");
			}
		}
	}

	commandToClient(%this, 'SetVignette', 1, %r SPC %g SPC %b SPC %a);
}
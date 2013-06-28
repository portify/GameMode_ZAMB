datablock playerData(playerSurvivorArmor : playerStandardArmor) {
	uiName = "Survivor Player";
	maxTools = 3;

	canJet = false;
	isSurvivor = true;

	firstPersonOnly = true;
	thirdPersonOnly = false;

	mass = 150;
	runForce = 4800;
	airControl = 0.025;

	jumpForce = 1332;
	jumpDelay = 15;

	minLookAngle = -1.4;
	maxLookAngle = 1.4;

	maxForwardSpeed = 9;
	maxBackwardSpeed = 6;
	maxSideSpeed = 7;
	
	maxForwardCrouchSpeed = 4;
	maxBackwardCrouchSpeed = 3;
	maxSideCrouchSpeed = 3;

	runSurfaceAngle = 55;
	jumpSurfaceAngle = 55;
};

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
	%obj.playThread(2, "activate2");

	initContainerRadiusSearch(%obj.getHackPosition(), 4, $TypeMasks::PlayerObjectType);
	%eye = %obj.getEyeVector();

	while (isObject(%col = containerSearchNext())) {
		if (getMiniGameFromObject(%col) !$= $defaultMiniGame) {
			continue;
		}

		%line = vectorNormalize(vectorSub(%col.position, %obj.position));

		if (vectorDot(%eye, %line) >= 0.75) {
			if (!%col.getDataBlock().isZombie) {
				%hitSurvivor = true;
				continue;
			}

			%path = vectorNormalize(vectorSub(%col.position, %obj.position));
			%velocity = vectorAdd(vectorScale(%path, 10), "0 0 6");

			%col.setVelocity(%velocity);
			%col.addHealth(getRandom(-5, -15));
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

	if (isObject(%profile)) {
		serverPlay3D(%profile, %obj.getHackPosition());
	}
}

function playerSurvivorArmor::onDisabled(%this, %obj) {
	%miniGame = getMiniGameFromObject(%obj);

	if (%miniGame $= $defaultMiniGame && isObject(%obj.client)) {
		if ($Sim::Time - %obj.lastDeathMusic >= 7.5) {
			%obj.client.play2D(zamb_music_death);
			%obj.lastDeathMusic = $Sim::Time;
		}
	}

	parent::onDisabled(%this, %obj);
}

datablock fxLightData(playerFlashlightData : playerLight) {
	uiName = "";
	flareOn = 0;

	radius = 10;
	brightness = 3;
};

package zambSurvivorPackage {
	function serverCmdLight(%client) {
		if (%client.miniGame !$= $defaultMiniGame) {
			parent::serverCmdLight(%client);
			return;
		}

		%player = %client.player;

		if (!isObject(%player)) {
			parent::serverCmdLight(%client);
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

	function player::activateStuff(%this) {
		%miniGame = getMiniGameFromObject(%this);

		if (%miniGame !$= $defaultMiniGame) {
			parent::activateStuff(%this);
			return;
		}

		if ($Sim::Time - %this.lastUseTime < 0.1) {
			return;
		}

		%this.lastUseTime = $Sim::Time;

		%start = %this.getEyePoint();
		%vector = %this.getEyeVector();

		%distance = 5;

		%ray = containerRayCast(%start,
			vectorAdd(%start, vectorScale(%vector, %distance)),
			$TypeMasks::All, %this
		);

		%col = firstWord(%ray);
		%use = isObject(%col);

		if (%use) {
			%use = %this.useObject(%col);
		}

		if (!%use && isObject(%this.client)) {
			%this.client.play2D(zamb_cannot_use);
		}
	}
};

activatePackage("zambSurvivorPackage");

function player::flashlightTick(%this) {
	cancel(%this.flashlightTick);

	if (%this.getState() $= "Dead" || !isObject(%this.light)) {
		return;
	}

	%start = %this.getEyePoint();
	%vector = %this.getEyeVector();

	%end = vectorAdd(%start, vectorScale(%vector, 50));
	%ray = containerRayCast(%start, %end, $TypeMasks::All, %this);

	if (%ray $= "0") {
		%pos = %end;
	}
	else {
		%pos = vectorSub(getWords(%ray, 1, 3), %vector);
	}

	if (vectorDist(%pos, %this.light.position) >= 0.05) {
		%this.light.setTransform(%pos);
		%this.light.inspectPostApply();
	}

	%this.flashlightTick = %this.schedule(32, "flashlightTick");
}

function player::useObject(%this, %obj) {
	return false;
}
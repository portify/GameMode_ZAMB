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

	if (%slot != 4 || !%val || $Sim::Time - %obj.lastShove < 0.5) {
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
			$TypeMasks::FxBrickObjectType
		);

		if (%ray !$= "0") {
			%profile = zamb_survivor_swing_world;
		}
		else {
			%profile = zamb_survivor_swing_miss;
		}
	}

	if (isObject(%profile)) {
		serverPlay3D(%profile, %this.getHackPosition());
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
datablock playerData(baseZombieData : playerStandardArmor) {
	uiName = "";

	mass = 150;
	maxDamage = 30;
	jumpForce = 1332;

	minLookAngle = -1.6;
	maxLookAngle = 1.6;

	maxForwardSpeed = 10;
	maxSideSpeed = 10;
	maxBackwardSpeed = 8;

	maxForwardCrouchSpeed = 6;
	maxSideCrouchSpeed = 6;
	maxBackwardCrouchSpeed = 4;

	minImpactSpeed = 24;
	speedDamageScale = 3.9;

	canJet = false;
	isZombie = true;

	enableRandomMoveSpeed = true;
	targetImprovementTolerance = 0.15;

	moveSpeedMin = 0.6;
	moveSpeedMax = 1;
};

function baseZombieData::onAdd(%this, %obj) {
	parent::onAdd(%this, %obj);
	// %obj.footstepUpdateTick();

	if (%this.enableRandomMoveSpeed && %obj.getClassName() $= "AIPlayer") {
		%obj.setMoveSpeed(%this.moveSpeedMin + getRandom() * (%this.moveSpeedMax - %this.moveSpeedMin));
	}

	%obj.setMoveTolerance(1.78);
	%obj.setMoveSlowdown(0);

	%obj.maxYawSpeed = 6;
	%obj.maxPitchSpeed = 3;

	%this.applyZombieAppearance(%obj);
}

function baseZombieData::onRemove(%this, %obj) {
	if (isObject(%obj.path)) {
		%obj.path.delete();
	}
}

function baseZombieData::applyZombieAppearance(%this, %obj) {
	%skin = "0.541 0.698 0.552 1";

	%obj.setNodeColor("headSkin", %skin);
	%obj.setFaceName("asciiTerror");

	%hat = getRandom(0, 7);
	%gender = getRandom(0, 1);

	%obj.hideNode($Chest[0]);
	%obj.playThread(1, "armReadyBoth");

	%obj.mountImage(zambClawRight, 0);
	%obj.mountImage(zambClawLeft, 1);

	%obj.hideNode("LHand");
	%obj.hideNode("RHand");

	%obj.hideNode("LHook");
	%obj.hideNode("RHook");

	if (%hat != 4 && %hat != 6 && %hat != 7) {
		%hat = 0;
	}

	for (%i = 0; %i < 7; %i++) {
		switch (%i) {
			case 0: %garment = $Hat[%hat];
			case 1: %garment = $Chest[%gender];
			case 2: %garment = $LArm[%gender];
			case 3: %garment = $RArm[%gender];
			case 4: %garment = "pants";
			case 5: %garment = "lshoe";
			case 6: %garment = "rshoe";
			default: continue;
		}

		if (%i != 4 && %i != 5 && (%i == 0 || %i == 1 || %i == 6 || getRandom() >= 0.5)) {
			%r = getRandom(50, 125) / 255;
			%g = getRandom(50, 125) / 255;
			%b = getRandom(50, 125) / 255;

			%color = %r SPC %g SPC %b SPC "1";
		}
		else {
			%color = %skin;
		}

		%obj.unHideNode(%garment);
		%obj.setNodeColor(%garment, %color);
	}
}

function baseZombieData::zombifyClientAppearance(%this, %obj, %client) {
	%this.applyZombieAppearance(%obj);
}

function baseZombieData::zombieTick(%this, %obj, %delta) {
	%obj.stop();
	%obj.clearAim();
	%obj.setMoveX(0);

	%this.updateZombieTarget(%obj);

	if (%obj.target $= "") {
		return;
	}

	%move = %this.directZombieMovement(%obj) || %this.pathedZombieMovement(%obj);

	if (%move) {
		%obj.updateLOA();

		%this.updateJump(%obj);
		%this.updateCrouch(%obj);
	}
	else {
		if (vectorLen(%obj.getVelocity()) < 0.15) {
			%obj.setActionThread("root");
		}

		if (isObject(%obj.path)) {
			%obj.path.delete();
			%obj.path = "";

			%obj.pathIndex = "";
			%obj.pathTarget = "";
		}
	}
}

function baseZombieData::updateZombieTarget(%this, %obj) {
	if (%obj.target !$= "" && !%this.isValidTarget(%obj, %obj.target)) {
		%obj.target = "";
	}

	if (%this.shouldHaveTarget(%obj)) {
		%bestScore = 0;
		%bestTarget = -1;

		for (%i = 0; %i < $defaultMiniGame.numMembers; %i++) {
			%client = $defaultMiniGame.member[%i];
			%player = %client.player;

			if (%this.isValidTarget(%obj, %player)) {
				%score = %this.getTargetScore(%obj, %player);

				if (%score > %bestScore) {
					%bestScore = %score;
					%bestTarget = %player;
				}
			}
		}

		if (%bestTarget != -1) {
			if (%obj.target $= "") {
				%obj.target = %bestTarget;
			}
			else {
				%improvement = %bestScore - %this.getTargetScore(%obj, %obj.target);

				if (%improvement >= %this.targetImprovementTolerance) {
					%obj.target = %bestTarget;
				}
			}
		}
	}
	else if (%obj.target !$= "") {
		%obj.target = "";
	}
}

function baseZombieData::shouldHaveTarget(%this, %obj) {
	return 1;
}

function baseZombieData::isValidTarget(%this, %obj, %target) {
	return isObject(%target) && %target.getState() !$= "Dead";
}

function baseZombieData::getTargetScore(%this, %obj, %target) {
	%sum = 0;

	%sum += mClampF(1 - (vectorDist(%obj.position, %target.position) / 150), 0, 1);
	%sum += %target.getDamageLevel() / %target.getDataBlock().maxDamage;

	return %sum / 2;
}

function baseZombieData::directZombieMovement(%this, %obj) {
	%ray = containerRayCast(
		vectorAdd(%obj.getEyePoint()),
		vectorAdd(%obj.target.getEyePoint()),
		$TypeMasks::FxBrickObjectType
	);

	if (%ray !$= "0") {
		return 0;
	}

	%obj.setAimObject(%obj.target);
	%obj.setMoveObject(%obj.target);

	if (isObject(%obj.path)) {
		%obj.path.delete();
		%obj.path = "";

		%obj.pathIndex = "";
		%obj.pathTarget = "";
	}

	return 1;
}

function baseZombieData::onReachDestination(%this, %obj) {
	parent::onReachDestination(%this, %obj);

	if (%obj.path !$= "" && %obj.pathTarget !$= "") {
		%obj.pathIndex = %obj.pathTarget;
		%obj.pathTarget = "";

		%this.zombieTick(%obj, $Sim::Time - %obj.lastZombieTick);
	}
}

function baseZombieData::pathedZombieMovement(%this, %obj) {
	if (!isObject(NodeGroup) || !NodeGroup.getCount()) {
		return 0;
	}

	%b = NodeGroup.findNearest(%obj.target.position, 16, 1);

	if (!isObject(%b)) {
		return 0;
	}

	%a = NodeGroup.findNearest(%obj.position, 16, 1);

	if (!isObject(%a)) {
		return false;
	}

	if (%obj.path.b != %b) {
		if (isObject(%obj.path)) {
			%obj.path.delete();
		}

		%obj.path = findPath(%a, %b);

		%obj.pathIndex = 0;
		%obj.pathTarget = "";
	}

	if (!%obj.path.done || %obj.path.result $= "error") {
		return 0;
	}

	if (%obj.pathIndex >= getWordCount(%obj.path.result)) {
		return 0;
	}

	%lookAhead = 5;

	for (%i = %obj.pathIndex + %lookAhead; %i >= %obj.pathIndex; %i--) {
		%node = getWord(%obj.path.result, %i);

		%ray = containerRayCast(
			vectorAdd(%obj.position, "0 0" SPC %this.maxStepHeight),
			vectorAdd(%node.position, "0 0" SPC %this.maxStepHeight),
			$TypeMasks::FxBrickObjectType
		);

		if (%ray $= "0") {
			%obj.pathTarget = %i + 1;
			%obj.setMoveDestination(%node.position);

			return 1;
		}
	}

	return 0;
}

function baseZombieData::updateJump(%this, %obj) {
	%obj.setJumping(%obj._zfRay(%this.maxStepHeight, 2));
}

function baseZombieData::updateCrouch(%this, %obj) {
	%a = %obj._zfRay(%this.maxStepHeight, 2);

	if (%a) {
		%b = %obj._zfRay(2.8, 2);

		if (%b) {
			%obj.setCrouching(!%obj._zfRay(1.1, 2));
		}
	}
}

function player::_zfRay(%this, %z, %f) {
	%scale = getWord(%this.getScale(), 2);
	%forward = vectorScale(%this.getForwardVector(), %f * %scale);

	%start = vectorAdd(%this.position, "0 0" SPC %z * %scale);
	%end = vectorAdd(%start, %forward);

	%ray = debugContainerRayCast(%start, %end, $TypeMasks::FxBrickObjectType);
	return %ray !$= 0;
}
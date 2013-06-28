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

	if (isObject(%obj.client)) {
		%this.zombifyClientAppearance(%obj, %obj.client);
	}
	else {
		%this.applyZombieAppearance(%obj);
	}
}

function baseZombieData::onRemove(%this, %obj) {
	if (isObject(%obj.path)) {
		%obj.path.delete();
	}
}

function baseZombieData::applyZombieAppearance(%this, %obj) {
	%obj.setFaceName("asciiTerror");
	// %obj.setDecalName("HCZombie");

	%obj.setNodeColor("headSkin", "0 0.5 0.25 1");

	%obj.hideNode("lhand");
	%obj.hideNode("rhand");

	%obj.mountImage(zambClawRight, 0);
	%obj.mountImage(zambClawLeft, 1);

	%obj.playThread(3, "armReadyBoth");
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

	if (!%this.directZombieMovement(%obj)) {
		if (!%this.pathedZombieMovement(%obj)) {
			if (vectorLen(%obj.getVelocity()) < 0.15) {
				%obj.setActionThread("root");
			}

			if (isObject(%obj.path)) {
				%obj.path.delete();
				%obj.path = "";

				%obj.pathIndex = "";
				%obj.pathTarget = "";
			}

			return;
		}
	}

	%obj.updateLOA();
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
		vectorAdd(%obj.position, "0 0" SPC %this.maxStepHeight),
		vectorAdd(%obj.target.position, "0 0" SPC %this.maxStepHeight),
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
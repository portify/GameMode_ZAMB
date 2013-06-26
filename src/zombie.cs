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
	%obj.footstepUpdateTick();

	if (%this.enableRandomMoveSpeed && %obj.getClassName() $= "AIPlayer") {
		%obj.setMoveSpeed(%this.moveSpeedMin + getRandom() * (%this.moveSpeedMax - %this.moveSpeedMin));
	}

	%obj.setMoveTolerance(1.78);
	%obj.setMoveSlowdown(0);
}

function baseZombieData::onNewDataBlock(%this, %obj) {
	parent::onNewDataBlock(%this, %obj);

	if (isObject(%obj.client)) {
		%this.zombifyClientAppearance(%obj, %obj.client);
	}
	else {
		%this.applyZombieAppearance(%obj);
	}
}

function baseZombieData::applyZombieAppearance(%this, %obj) {
	%obj.setFaceName("asciiTerror");

	%obj.mountImage(zombieClawLeftImage, 0);
	%obj.mountImage(zombieClawRightImage, 1);

	%obj.playThread(3, "armReadyBoth");
}

function baseZombieData::zombifyClientAppearance(%this, %obj, %client) {
	%this.applyZombieAppearance(%obj);
}

function baseZombieData::zombieTick(%this, %obj, %delta) {
	%obj.stop();
	%obj.clearAim();
	%obj.setMoveX(0);

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
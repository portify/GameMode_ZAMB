// $skinColorRedMin = 0;
// $skinColorRedMax = 0;

$skinColorRedMin = 0.1;
$skinColorRedMax = 0.6;

// $skinColorGreenMin = 0.150;
// $skinColorGreenMax = 0.450;

$skinColorGreenMin = 0.15;
$skinColorGreenMax = 0.75;

// $skinColorBlueMin = 0.150;
// $skinColorBlueMax = 0.450;

$skinColorBlueMin = 0.15;
$skinColorBlueMin = 0.60;

$skinColorSym = "gb";

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

	%obj.setActionThread("root");
	%this.applyZombieAppearance(%obj);
}

function baseZombieData::onRemove(%this, %obj) {
	if (isObject(%obj.path)) {
		%obj.path.delete();
	}
}

function baseZombieData::applyZombieAppearance(%this, %obj) {
	// %skin = "0.541 0.698 0.552 1";
	%skin = getZombieSkinColor();

	%obj.setNodeColor("headSkin", %skin);
	%obj.setNodeColor("lhand", %skin);
	%obj.setNodeColor("rhand", %skin);

	%obj.setFaceName("asciiTerror");

	%hat = getRandom(0, 7);
	%gender = getRandom(0, 1);

	%obj.hideNode($Chest[0]);
	%obj.playThread(1, "armReadyBoth");

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
		}

		if (%garment $= "" || %garment $= "none") {
			continue;
		}

		if (%i == 0 || %i == 1 || %i == 4 || %i == 6 || getRandom() >= 0.5) {
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

function getZombieSkinColor()
{
	%redMin = $skinColorRedMin * 100;
	%redMax = $skinColorRedMax * 100;
	%greenMin = $skinColorGreenMin * 100;
	%greenMax = $skinColorGreenMax * 100;
	%blueMin = $skinColorblueMin * 100;
	%blueMax = $skinColorblueMax * 100;
	
	%red = getRandom(%redMin, %redMax) / 100;
	%blue = getRandom(%blueMin, %blueMax) / 100;
	%green = getRandom(%greenMin, %greenMax) / 100;

	if ($skinColorSym !$= "") {
		switch$ ($skinColorSym) {
			case "rg":
				%sym = (%red + %green) / 2;
				%red = %sym;
				%green = %sym;
			
			case "rb":
				%sym = (%red + %blue) / 2;
				%red = %sym;
				%blue = %sym;

			case "gb":
				%sym = (%green + %blue) / 2;
				%green = %sym;
				%blue = %sym;

			case "rgb":
				%sym = (%red + %green + %blue) / 3;
				%red = %sym;
				%green = %sym;
				%blue = %sym;
		}
	}

	return %red SPC %green SPC %blue SPC 1;
}

function baseZombieData::zombieTick(%this, %obj, %delta) {
	%obj.stop();
	%obj.clearAim();

	%this.updateZombieTarget(%obj);

	if (%obj.target !$= "") {
		%move = %this.directZombieMovement(%obj) || %this.pathedZombieMovement(%obj);
	}

	%obj.setMoveX(%move ? %this.determineLOA(%obj, 1.75) : 0);
	%obj.setJumping(%move && %this.determineJump(%obj, 2));
	%obj.setCrouching(%move && %this.determineCrouch(%obj, 2));

	if (!%move) {
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
		for (%i = 0; %i < $defaultMiniGame.numMembers; %i++) {
			%client = $defaultMiniGame.member[%i];
			%player = %client.player;

			if (%this.isValidTarget(%obj, %player)) {
				%score = %this.getTargetScore(%obj, %player);

				if (%bestScore $= "" || %score > %bestScore) {
					%bestScore = %score;
					%bestTarget = %player;
				}
			}
		}

		if (%bestTarget !$= -1) {
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

	%sum += 1 - (vectorDist(%obj.position, %target.position) / 50);
	%sum += %target.getDamageLevel() / %target.getDataBlock().maxDamage;

	return %sum / 2;
}

function baseZombieData::directZombieMovement(%this, %obj) {
	%ray = containerRayCast(
		%obj.getEyePoint(),
		%obj.target.getEyePoint(),
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

function baseZombieData::determineLOA(%this, %obj, %dist) {
	%a = %obj._loaProbe("0 0 -1", %dist);
	%b = %obj._loaProbe("0 0 1", %dist);

	if (%a && %b) {
		if (%obj.loaStuck) {
			if ($Sim::Time - %obj.loaStuckTime >= 1.5) {
				%obj.loaStuckDir *= -1;
				%obj.loaStuckTime = $Sim::Time;

				return %obj.loaStuckDir;
			}
		}
		else {
			%obj.loaStuck = 1;
			%obj.loaStuckDir = getRandom() >= 0.5 ? 1 : -1;
			%obj.loaStuckTime = $Sim::Time;

			return %obj.loaStuckDir;
		}
	}
	else {
		%obj.loaStuck = "";
		%obj.loaStuckDir = "";
		%obj.loaStuckTime = "";

		return %a + %b * -1; // return %a ? 1 : (%b ? -1 : 0));
	}
}

function baseZombieData::determineJump(%this, %obj, %dist) {
	return %obj._zfRay(%this.maxStepHeight, %dist);
}

function baseZombieData::determineCrouch(%this, %obj, %dist) {
	if (%obj._zfRay(%this.maxStepHeight, %dist)) {
		return 0;
	}

	%low = 1.2 + 0.1;
	%high = 2.4 + 0.1;

	%step = 0.2;

	if (%obj._zfRay(%low - %step, %dist)) {
		return 0;
	}

	for (%i = %high; %i >= %high; %i -= %step) {
		if (%obj._zfRay(%i, %dist)) {
			return 1;
		}
	}

	return 0;
}

function player::_zfRay(%this, %z, %f) {
	%scale = getWord(%this.getScale(), 2);
	%forward = vectorScale(%this.getForwardVector(), %f * %scale);

	%start = vectorAdd(%this.position, "0 0" SPC %z * %scale);
	%end = vectorAdd(%start, %forward);

	%ray = containerRayCast(%start, %end, $TypeMasks::FxBrickObjectType);
	return %ray !$= 0;
}

function AIPlayer::_loaProbe(%this, %up, %dist) {
	%start = %this.getHackPosition();
	%forward = %this.getForwardVector();

	%cross = vectorCross(%forward, %up);
	%length = %dist * getWord(%this.getScale(), 2);

	%stop = vectorAdd(%start, vectorScale(%cross, 1));
	%stop = vectorAdd(%stop, vectorScale(%forward, %length));

	return containerRayCast(%start, %stop, $TypeMasks::All, %this) !$= 0;
}
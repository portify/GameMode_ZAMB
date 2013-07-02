function zombieData::onReachDestination(%this, %obj) {
	parent::onReachDestination(%this, %obj);

	if (%obj.path !$= "" && %obj.pathTarget !$= "") {
		%obj.pathIndex = %obj.pathTarget;
		%obj.pathTarget = "";

		%this.updateAI(%obj);
	}
}

function Player::setAIDebug(%this, %text) {
	if (!isObject(%this.camera)) {
		%this.camera = new Camera() {
			datablock = Observer;
		};
	}

	%this.camera.setTransform(%this.position);
	%this.camera.setShapeName(%text);
}

function zombieData::updateAI(%this, %obj) {
	%obj.stop();
	%obj.clearAim();

	%this.updateTarget(%obj);

	if (%obj.target !$= "") {
		if (%this.directMovement(%obj)) {
			%type = 1;
		}
		else if (%this.pathedMovement(%obj)) {
			%type = 2;
		}
		else {
			%type = 0;

			%obj.target.lastReachFail = $Sim::Time;
			%obj.target = "";
		}
	}

	%obj.setMoveX(%type ? %this.shouldStrafe(%obj, 2) : 0);
	%obj.setJumping(%type && %this.shouldJump(%obj, 2));
	%obj.setCrouching(%type && %this.shouldCrouch(%obj, 2));

	%this.zombieAttack(%obj);
}

function zombieData::updateTarget(%this, %obj) {
	if (isObject(%obj.target) && %obj.getState() $= "Dead") {
		%obj.target = "";
	}

	for (%i = 0; %i < $defaultMiniGame.numMembers; %i++) {
		%player = $defaultMiniGame.member[%i].player;

		if (isObject(%player) && %player.getState() !$= "Dead") {
			%score = %this.getTargetScore(%obj, %player);

			if (%bestScore $= "" || %score > %bestScore) {
				%bestScore = %score;
				%bestTarget = %player;
			}
		}
	}

	if (%bestTarget !$= "") {
		if (isObject(%obj.target)) {
			%improvement = %bestScore - %this.getTargetScore(%obj, %obj.target);

			if (%improvement >= %this.targetImprovementTolerance) {
				%obj.target = %bestTarget;
			}
		}
		else {
			%obj.target = %bestTarget;
		}
	}
}

function zombieData::getTargetScore(%this, %obj, %target) {
	if ($Sim::Time - %obj.lastReachFail <= 1) {
		return -1;
	}

	%boomer = $Sim::Time - %obj.lastBoomerVictimTime;

	if ($Sim::Time - %obj.lastBoomerVictimTime < 15) {
		%sum = 5;
	}
	else {
		%sum = 0;
	}

	%sum += 1 - (vectorDist(%obj.position, %target.position) / 50);
	%sum += 1 - (%target.getDamageLevel() / %target.getDataBlock().maxDamage);

	return %sum / 2;
}

function zombieData::directMovement(%this, %obj) {
	%mask = $TypeMasks::FxBrickObjectType;
	%ray = containerRayCast(
		vectorAdd(%obj.position, "0 0" SPC %this.maxStepHeight),
		vectorAdd(%obj.target.position, "0 0" SPC %this.maxStepHeight),
		$TypeMasks::FxBrickObjectType
	);

	if (%ray $= "0") {
		%obj.setAimObject(%obj.target);
		%obj.setMoveObject(%obj.target);

		return 1;
	}

	return 0;
}

function zombieData::pathedMovement(%this, %obj) {
	if (!isObject(NodeGroup) || !NodeGroup.getCount()) {
		return 0;
	}

	%a = %obj.getNearestNode();
	%b = %obj.target.getNearestNode();

	if (!isObject(%a) || !isObject(%b)) {
		return;
	}

	if (%obj.path.b !$= %b) {
		if (isObject(%obj.path)) {
			%obj.path.delete();
		}

		%obj.path = findPath(%obj.node, %obj.target.node);

		%obj.pathIndex = 0;
		%obj.pathTarget = "";
	}

	if (!%obj.path.done || %obj.path.result $= "error") {
		return 0;
	}

	if (%obj.pathIndex >= getWordCount(%obj.path.result)) {
		return 0;
	}

	if (%this.rpfLookAhead !$= "") {
		for (%i = %obj.pathIndex + %this.rpfLookAhead; %i >= %obj.pathIndex; %i--) {
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
	}

	%obj.pathTarget = %obj.pathIndex + 1;
	%obj.setMoveDestination(getWord(%obj.path.result, %obj.pathIndex).position);

	return 1;
}

function zombieData::shouldStrafe(%this, %obj, %dist) {
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

function zombieData::shouldJump(%this, %obj, %dist) {
	return %obj._zfRay(%this.maxStepHeight, %dist) && !%obj._zfRay(%this.jumpForce / %this.mass, %dist);
}

function zombieData::shouldCrouch(%this, %obj, %dist) {
	%pos = vectorAdd(%obj.position, vectorScale(%obj.getForwardVector(), 0.5));

	%ray = containerRayCast(
		vectorAdd(%pos, "0 0 0.1"),
		vectorAdd(%pos, "0 0 1.6"),
		$TypeMasks::FxBrickObjectType
	);

	if (%ray !$= "0") {
		return 1;
	}

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

function Player::getNearestNode(%this) {
	if ($Sim::Time - %this.lastNode > 0.5) {
		%this.node = NodeGroup.findNearest(%this.position, 16, 1);
		
		if (!isObject(%this.node)) {
			%this.node = NodeGroup.findNearest(%this.position, 16);
		}

		%this.lastNode = $Sim::Time;
	}

	return %obj.node;
}
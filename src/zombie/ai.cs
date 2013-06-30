function baseZombieData::zombieTick(%this, %obj, %delta) {
	%obj.stop();
	%obj.clearAim();

	%this.updateZombieTarget(%obj);

	if (%obj.target !$= "") {
		%move = %this.directZombieMovement(%obj) || %this.pathedZombieMovement(%obj);
		%this.zombieAttack(%obj);

		// %obj.setLine(%obj.getHackPosition(), %obj.target.getHackPosition());
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
	%boomer = $Sim::Time - %obj.lastBoomerVictimTime;

	if ($Sim::Time - %obj.lastBoomerVictimTime < 15) {
		%sum = 5;
	}
	else {
		%sum = 0;
	}

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

	if ($Sim::Time - %obj.lastNode > 0.5) {
		%obj.node = NodeGroup.findNearest(%obj.position, 16, 1);
		%obj.lastNode = $Sim::Time;
	}

	if (!isObject(%obj.node)) {
		return 0;
	}

	if ($Sim::Time - %obj.target.lastNode > 0.5) {
		%obj.target.node = NodeGroup.findNearest(%obj.target.position, 16, 1);
		%obj.target.lastNode = $Sim::Time;
	}

	if ( !isObject(%obj.target.node)) {
		return false;
	}

	if (%obj.path.b !$= %obj.target.node) {
		if (isObject(%obj.path)) {
			%obj.path.delete();
		}

		%obj.path = findPath(%obj.node, %obj.target.node);

		%obj.pathIndex = 0;
		%obj.pathTarget = "";
	}

	if (!%obj.path.done) {
		return -1;
	}

	if ( %obj.path.result $= "error" ) {
		return false;
	}

	if ( %obj.index >= %length = getWordCount( %obj.path.result ) ) {
		return false;
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

	%i = %obj.pathIndex;
	%node = getWord(%obj.path.result, %i);

	%obj.pathTarget = %i + 1;
	%obj.setMoveDestination(%node.position);

	return 1;
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
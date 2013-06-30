datablock playerData(tankZombieData : baseZombieData) {
	mass = 300;
	maxDamage = 3000;

	runForce = 3000;
	jumpForce = 2000;

	maxForwardSpeed = 5;
	maxBackwardSpeed = 4;
	maxSideSpeed = 2;

	noZombiePush = 1;
	enableRandomMoveSpeed = 0;
	targetImprovementTolerance = 0.25;

	footstepMaterial = "tank_walk";

	alternateFootstepMaterial["slosh"] = "tank_water";
	alternateFootstepMaterial["wade"] = "tank_water";
};

function tankZombieData::onAdd(%this, %obj) {
	baseZombieData::onAdd(%this, %obj);
	%obj.footstepUpdateTick();

	if (isObject($defaultMiniGame.sound)) {
		$defaultMiniGame.sound.play(zamb_music_tank, 1, 60, 1);
	}
}

function tankZombieData::applyZombieAppearance(%this, %obj) {
	baseZombieData::applyZombieAppearance(%this, %obj);
	%obj.setScale("1.3 1.45 1.45");
}

function tankZombieData::onRemove(%this, %obj) {
	baseZombieData::onRemove(%this, %obj);
}

function tankZombieData::zombieTick(%this, %obj, %delta) {
	baseZombieData::zombieTick(%this, %obj, %delta);
}

function tankZombieData::updateZombieTarget(%this, %obj) {
	baseZombieData::updateZombieTarget(%this, %obj);
}

function tankZombieData::shouldHaveTarget(%this, %obj) {
	return baseZombieData::shouldHaveTarget(%this, %obj);
}

function tankZombieData::isValidTarget(%this, %obj, %target) {
	return baseZombieData::isValidTarget(%this, %obj, %target);
}

function tankZombieData::getTargetScore(%this, %obj, %target) {
	return baseZombieData::getTargetScore(%this, %obj, %target);
}

function tankZombieData::isValidTarget(%this, %obj, %target) {
	return baseZombieData::isValidTarget(%this, %obj, %target);
}

function tankZombieData::directZombieMovement(%this, %obj) {
	return baseZombieData::directZombieMovement(%this, %obj);
}

function tankZombieData::onReachDestination(%this, %obj) {
	baseZombieData::onReachDestination(%this, %obj);
}

function tankZombieData::pathedZombieMovement(%this, %obj) {
	return baseZombieData::pathedZombieMovement(%this, %obj);
}

function tankZombieData::determineLOA(%this, %obj, %dist) {
	return baseZombieData::determineLOA(%this, %obj, %dist);
}

function tankZombieData::determineJump(%this, %obj, %dist) {
	return baseZombieData::determineJump(%this, %obj, %dist);
}

function tankZombieData::determineCrouch(%this, %obj, %dist) {
	return 0;
	// return baseZombieData::determineCrouch(%this, %obj, %dist);
}

function tankZombieData::zombieAttack(%this, %obj) {
	if ($Sim::Time < %obj.attackReadyTime) {
		return;
	}

	if (!isObject(%obj.getControllingClient())) {
		%start = %obj.getHackPosition();
		%eye = %obj.getEyeVector();

		initContainerRadiusSearch(%start, 6, $TypeMasks::PlayerObjectType);

		while (isObject(%col = containerSearchNext())) {
			if (%col.getDataBlock().isZombie || %col.getState() $= "Dead") {
				continue;
			}

			%end = %col.getHackPosition();

			if (vectorDist(%start, %end) >= 6) {
				continue;
			}

			%line = vectorNormalize(vectorSub(%col.position, %obj.position));

			if (vectorDot(%eye, %line) >= 0.5) {
				%ray = containerRayCast(%start, %end, $TypeMasks::FxBrickObjectType);

				if (%ray $= "0") {
					%hit = true;
					break;
				}
			}
		}
	}

	if (isObject(%obj.getControllingClient()) || %hit) {
		%obj.attackReadyTime = $Sim::Time + 2;
		%obj.playThread(0, "activate2");

		%this.schedule(500, "doZombieAttack", %obj);
	}
}

function tankZombieData::doZombieAttack(%this, %obj) {
	if (!isObject(%obj) || %obj.getState() $= "Dead") {
		return;
	}

	%start = %obj.getHackPosition();
	%eye = %obj.getEyeVector();

	initContainerRadiusSearch(%start, 6, $TypeMasks::PlayerObjectType);

	while (isObject(%col = containerSearchNext())) {
		if (%col.getDataBlock().isZombie || %col.getState() $= "Dead") {
			continue;
		}

		%end = %col.getHackPosition();

		if (vectorDist(%start, %end) >= 6) {
			continue;
		}

		%line = vectorNormalize(vectorSub(%col.position, %obj.position));

		if (vectorDot(%eye, %line) >= 0.5) {
			%ray = containerRayCast(%start, %end, $TypeMasks::FxBrickObjectType);

			if (%ray $= "0") {
				%hit = true;

				%col.setVelocity(vectorAdd(vectorScale(%line, 20), "0 0 12"));
				%col.damage(%obj, %end, 40, $DamageType::Suicide);
			}
		}
	}
}
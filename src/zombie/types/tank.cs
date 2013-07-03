datablock playerData(tankZombieData : zombieData) {
	zombieType = 3;

	mass = 300;
	maxDamage = 3000;

	runForce = 3000;
	jumpForce = 2000;

	maxForwardSpeed = 5;
	maxBackwardSpeed = 4;
	maxSideSpeed = 2;

	noZombiePush = 1;
	targetImprovementTolerance = 0.25;

	footstepMaterial = "tank_walk";

	alternateFootstepMaterial["slosh"] = "tank_water";
	alternateFootstepMaterial["wade"] = "tank_water";
};

function tankZombieData::onAdd(%this, %obj) {
	zombieData::onAdd(%this, %obj);
}

function tankZombieData::onRemove(%this, %obj) {
	zombieData::onRemove(%this, %obj);
}

function tankZombieData::onReachDestination(%this, %obj) {
	zombieData::onReachDestination(%this, %obj);
}

function tankZombieData::applyZombieAppearance(%this, %obj) {
	zombieData::applyZombieAppearance(%this, %obj);
	%obj.setScale("1.3 1.45 1.45");
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
		if (%col.getState() $= "Dead") {
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

function tankZombieData::updateAI(%this, %obj) {
	zombieData::updateAI(%this, %obj);
}

function tankZombieData::updateTarget(%this, %obj) {
	zombieData::updateTarget(%this, %obj);
}

function tankZombieData::getTargetScore(%this, %obj, %target) {
	return zombieData::getTargetScore(%this, %obj, %target);
}

function tankZombieData::directMovement(%this, %obj) {
	return zombieData::directMovement(%this, %obj);
}

function tankZombieData::pathedMovement(%this, %obj) {
	return zombieData::pathedMovement(%this, %obj);
}

function tankZombieData::shouldStrafe(%this, %obj, %dist) {
	return zombieData::shouldStrafe(%this, %obj, %dist);
}

function tankZombieData::shouldJump(%this, %obj, %dist) {
	return zombieData::shouldJump(%this, %obj, %dist);
}

function tankZombieData::shouldCrouch(%this, %obj, %dist) {
	return zombieData::shouldCrouch(%this, %obj, %dist);
}
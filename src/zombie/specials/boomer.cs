datablock playerData(boomerZombieData : baseZombieData) {
	maxDamage = 50;

	maxForwardSpeed = 5;
	maxBackwardSpeed = 4;
	maxSideSpeed = 2;

	enableRandomMoveSpeed = 0;
};

function boomerZombieData::onAdd(%this, %obj) {
	baseZombieData::onAdd(%this, %obj);
	%obj.footstepUpdateTick();

	serverPlay3D(zamb_music_bacteria_boomer, %obj.getHackPosition());
}

function boomerZombieData::applyZombieAppearance(%this, %obj) {
	baseZombieData::applyZombieAppearance(%this, %obj);
	%obj.setScale("1.3 1.6 1");
}

function boomerZombieData::onRemove(%this, %obj) {
	baseZombieData::onRemove(%this, %obj);
}

function boomerZombieData::onDisabled(%this, %obj) {
	%hack = %obj.getHackPosition();
	initContainerRadiusSearch(%hack, 6, $TypeMasks::PlayerObjectType);

	while (isObject(%col = containerSearchNext())) {
		if (%col.getDataBlock().isSurvivor) {
			%distance = vectorDist(%hack, %col.getHackPosition());

			if (%distance <= 6) {
				%col.applyBoomerVomit();
			}
		}
	}

	%profile = "zamb_boomer_explode" @ getRandom(1, 3);
	serverPlay3D(%profile, %obj.getHackPosition());

	%obj.spawnExplosion(spawnProjectile, 2);
	baseZombieData::onDisabled(%this, %obj);
}

function boomerZombieData::zombieTick(%this, %obj, %delta) {
	baseZombieData::zombieTick(%this, %obj, %delta);
}

function boomerZombieData::updateZombieTarget(%this, %obj) {
	baseZombieData::updateZombieTarget(%this, %obj);
}

function boomerZombieData::shouldHaveTarget(%this, %obj) {
	return baseZombieData::shouldHaveTarget(%this, %obj);
}

function boomerZombieData::isValidTarget(%this, %obj, %target) {
	return baseZombieData::isValidTarget(%this, %obj, %target);
}

function boomerZombieData::getTargetScore(%this, %obj, %target) {
	return baseZombieData::getTargetScore(%this, %obj, %target);
}

function boomerZombieData::isValidTarget(%this, %obj, %target) {
	return baseZombieData::isValidTarget(%this, %obj, %target);
}

function boomerZombieData::directZombieMovement(%this, %obj) {
	return baseZombieData::directZombieMovement(%this, %obj);
}

function boomerZombieData::onReachDestination(%this, %obj) {
	baseZombieData::onReachDestination(%this, %obj);
}

function boomerZombieData::pathedZombieMovement(%this, %obj) {
	return baseZombieData::pathedZombieMovement(%this, %obj);
}

function boomerZombieData::determineLOA(%this, %obj, %dist) {
	return baseZombieData::determineLOA(%this, %obj, %dist);
}

function boomerZombieData::determineJump(%this, %obj, %dist) {
	return baseZombieData::determineJump(%this, %obj, %dist);
}

function boomerZombieData::determineCrouch(%this, %obj, %dist) {
	return baseZombieData::determineCrouch(%this, %obj, %dist);
}

function player::applyBoomerVomit(%this) {
	if (!%this.isBoomerVictim()) {
		%this.lastBoomerVictimTime = $Sim::Time;

		if (isObject(%this.client)) {
			%this.client.play2D(zamb_music_pukricide);
			%this.client.updateZAMBVignette();
		}

		%miniGame = getMiniGameFromObject(%this);

		if (isObject(%miniGame.zamb) && !%miniGame.zamb.ended) {
			if ($Sim::Time - %miniGame.zamb.lastHorde >= 0.5) {
				%miniGame.zamb.spawnHorde(1);
			}
		}
	}
}

function player::isBoomerVictim(%this) {
	return $Sim::Time - %this.lastBoomerVictimTime < 15;
}
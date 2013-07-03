datablock playerData(boomerZombieData : zombieData) {
	zombieType = 2;
};

function boomerZombieData::onAdd(%this, %obj) {
	zombieData::onAdd(%this, %obj);
}

function boomerZombieData::onRemove(%this, %obj) {
	zombieData::onRemove(%this, %obj);
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

	%obj.spawnExplosion(spawnProjectile, 2);
	parent::onDisabled(%this, %obj);
}

function boomerZombieData::onReachDestination(%this, %obj) {
	zombieData::onReachDestination(%this, %obj);
}

function boomerZombieData::applyZombieAppearance(%this, %obj) {
	zombieData::applyZombieAppearance(%this, %obj);
	%obj.setScale("1.3 1.6 1");
}

function boomerZombieData::zombieAttack(%this, %obj) {
	zombieData::zombieAttack(%this, %obj);
}

function boomerZombieData::updateAI(%this, %obj) {
	zombieData::updateAI(%this, %obj);
}

function boomerZombieData::updateTarget(%this, %obj) {
	zombieData::updateTarget(%this, %obj);
}

function boomerZombieData::getTargetScore(%this, %obj, %target) {
	return zombieData::getTargetScore(%this, %obj, %target);
}

function boomerZombieData::directMovement(%this, %obj) {
	return zombieData::directMovement(%this, %obj);
}

function boomerZombieData::pathedMovement(%this, %obj) {
	return zombieData::pathedMovement(%this, %obj);
}

function boomerZombieData::shouldStrafe(%this, %obj, %dist) {
	return zombieData::shouldStrafe(%this, %obj, %dist);
}

function boomerZombieData::shouldJump(%this, %obj, %dist) {
	return zombieData::shouldJump(%this, %obj, %dist);
}

function boomerZombieData::shouldCrouch(%this, %obj, %dist) {
	return zombieData::shouldCrouch(%this, %obj, %dist);
}

function player::applyBoomerVomit(%this) {
	if (!%this.isBoomerVictim()) {
		%this.lastBoomerVictimTime = $Sim::Time;

		if (isObject(%this.client)) {
			%this.client.updateZAMBVignette();
		}

		%miniGame = getMiniGameFromObject(%this);

		if (isObject(%miniGame.zamb) && !%miniGame.zamb.ended) {
			if ($Sim::Time - %miniGame.zamb.director.prevHorde >= 0.5) {
				%miniGame.zamb.director.horde();
			}
		}
	}
}

function player::isBoomerVictim(%this) {
	return $Sim::Time - %this.lastBoomerVictimTime < 15;
}
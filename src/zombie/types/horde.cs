datablock playerData(hordeZombieData : zombieData) {
	zombieType = 1;
};

function hordeZombieData::onAdd(%this, %obj) {
	zombieData::onAdd(%this, %obj);
}

function hordeZombieData::onRemove(%this, %obj) {
	zombieData::onRemove(%this, %obj);
}

function hordeZombieData::onReachDestination(%this, %obj) {
	zombieData::onReachDestination(%this, %obj);
}

function hordeZombieData::applyZombieAppearance(%this, %obj) {
	zombieData::applyZombieAppearance(%this, %obj);
}

function hordeZombieData::zombieAttack(%this, %obj) {
	zombieData::zombieAttack(%this, %obj);
}

function hordeZombieData::updateAI(%this, %obj) {
	zombieData::updateAI(%this, %obj);
}

function hordeZombieData::updateTarget(%this, %obj) {
	zombieData::updateTarget(%this, %obj);
}

function hordeZombieData::getTargetScore(%this, %obj, %target) {
	return zombieData::getTargetScore(%this, %obj, %target);
}

function hordeZombieData::directMovement(%this, %obj) {
	return zombieData::directMovement(%this, %obj);
}

function hordeZombieData::pathedMovement(%this, %obj) {
	return zombieData::pathedMovement(%this, %obj);
}

function hordeZombieData::shouldStrafe(%this, %obj, %dist) {
	return zombieData::shouldStrafe(%this, %obj, %dist);
}

function hordeZombieData::shouldJump(%this, %obj, %dist) {
	return zombieData::shouldJump(%this, %obj, %dist);
}

function hordeZombieData::shouldCrouch(%this, %obj, %dist) {
	return zombieData::shouldCrouch(%this, %obj, %dist);
}
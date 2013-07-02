datablock playerData(wandererZombieData : playerStandardArmor) {
	zombieType = 0;
};

function wandererZombieData::onAdd(%this, %obj) {
	zombieData::onAdd(%this, %obj);
}

function wandererZombieData::onRemove(%this, %obj) {
	zombieData::onRemove(%this, %obj);
}

function wandererZombieData::applyZombieAppearance(%this, %obj) {
	zombieData::applyZombieAppearance(%this, %obj);
}

function wandererZombieData::zombieAttack(%this, %obj) {
	zombieData::zombieAttack(%this, %obj);
}

function wandererZombieData::onReachDestination(%this, %obj) {
	zombieData::onReachDestination(%this, %obj);
}

function wandererZombieData::updateAI(%this, %obj) {
	zombieData::updateAI(%this, %obj);
}

function wandererZombieData::updateTarget(%this, %obj) {
	zombieData::updateTarget(%this, %obj);
}

function wandererZombieData::getTargetScore(%this, %obj, %target) {
	return zombieData::getTargetScore(%this, %obj, %target);
}

function wandererZombieData::directMovement(%this, %obj) {
	return zombieData::directMovement(%this, %obj);
}

function wandererZombieData::pathedMovement(%this, %obj) {
	return zombieData::pathedMovement(%this, %obj);
}

function wandererZombieData::shouldStrafe(%this, %obj, %dist) {
	return zombieData::shouldStrafe(%this, %obj, %dist);
}

function wandererZombieData::shouldJump(%this, %obj, %dist) {
	return zombieData::shouldJump(%this, %obj, %dist);
}

function wandererZombieData::shouldCrouch(%this, %obj, %dist) {
	return zombieData::shouldCrouch(%this, %obj, %dist);
}
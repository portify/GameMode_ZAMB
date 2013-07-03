datablock playerData(exampleZombieData : zombieData) {
	attribute = 1;
};

function exampleZombieData::onAdd(%this, %obj) {
	zombieData::onAdd(%this, %obj);
}

function exampleZombieData::onRemove(%this, %obj) {
	zombieData::onRemove(%this, %obj);
}

function exampleZombieData::applyZombieAppearance(%this, %obj) {
	zombieData::applyZombieAppearance(%this, %obj);
}

function exampleZombieData::zombieAttack(%this, %obj) {
	zombieData::zombieAttack(%this, %obj);
}

function exampleZombieData::onReachDestination(%this, %obj) {
	zombieData::onReachDestination(%this, %obj);
}

function exampleZombieData::updateAI(%this, %obj) {
	zombieData::updateAI(%this, %obj);
}

function exampleZombieData::updateTarget(%this, %obj) {
	zombieData::updateTarget(%this, %obj);
}

function exampleZombieData::getTargetScore(%this, %obj, %target) {
	return zombieData::getTargetScore(%this, %obj, %target);
}

function exampleZombieData::directMovement(%this, %obj) {
	return zombieData::directMovement(%this, %obj);
}

function exampleZombieData::pathedMovement(%this, %obj) {
	return zombieData::pathedMovement(%this, %obj);
}

function exampleZombieData::shouldStrafe(%this, %obj, %dist) {
	return zombieData::shouldStrafe(%this, %obj, %dist);
}

function exampleZombieData::shouldJump(%this, %obj, %dist) {
	return zombieData::shouldJump(%this, %obj, %dist);
}

function exampleZombieData::shouldCrouch(%this, %obj, %dist) {
	return zombieData::shouldCrouch(%this, %obj, %dist);
}
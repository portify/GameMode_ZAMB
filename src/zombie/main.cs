datablock playerData(zombieData : playerStandardArmor) {
	uiName = "";
	canJet = 0;

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
	targetImprovementTolerance = 0.15;

	isZombie = 1;
	isSurvivor = 0;
};

exec("./ai.cs");

function zombieData::onAdd(%this, %obj) {
	parent::onAdd(%this, %obj);

	%obj.setMoveTolerance(1.78);
	%obj.setMoveSlowdown(0);

	%obj.setActionThread("root");
}

function zombieData::onRemove(%this, %obj) {
	if (isObject(%obj.path)) {
		%obj.path.delete();
	}
}
datablock playerData(playerSurvivorArmor : playerStandardArmor) {
	uiName = "Survivor Player";
	maxTools = 3;

	canJet = false;
	isSurvivor = true;

	firstPersonOnly = true;
	thirdPersonOnly = false;

	mass = 150;
	runForce = 4800;
	airControl = 0.025;

	jumpForce = 1332;
	jumpDelay = 15;

	minLookAngle = -1.4;
	maxLookAngle = 1.4;

	maxForwardSpeed = 9;
	maxBackwardSpeed = 6;
	maxSideSpeed = 7;
	
	maxForwardCrouchSpeed = 4;
	maxBackwardCrouchSpeed = 3;
	maxSideCrouchSpeed = 3;

	runSurfaceAngle = 55;
	jumpSurfaceAngle = 55;
};

datablock playerData(baseZombieData : playerStandardArmor) {
	uiName = "";

	mass = 150;
	maxDamage = 50;
	jumpForce = 1332;

	minLookAngle = -1.6;
	maxLookAngle = 1.6;

	maxForwardSpeed = 16;
	maxSideSpeed = 10;
	maxBackwardSpeed = 8;

	maxForwardCrouchSpeed = 6;
	maxSideCrouchSpeed = 6;
	maxBackwardCrouchSpeed = 4;

	minImpactSpeed = 24;
	speedDamageScale = 3.9;

	canJet = false;
	isZombie = true;

	enableRandomMoveSpeed = true;
	targetImprovementTolerance = 0.15;

	moveSpeedMin = 0.25;
	moveSpeedMax = 0.75;
};
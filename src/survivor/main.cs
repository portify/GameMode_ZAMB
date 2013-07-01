datablock playerData(survivorData : playerStandardArmor) {
	uiName = "Survivor Player";
	canJet = 0;
	maxTools = 3;

	firstPersonOnly = 1;
	thirdPersonOnly = 0;

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

	isZombie = 0;
	isSurvivor = 1;
};
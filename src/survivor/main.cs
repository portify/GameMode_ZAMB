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

exec("./flashlight.cs");
exec("./shove.cs");

package zambSurvivorPackage {
	function serverCmdLight(%client) {
		if (!isObject(%client.miniGame) || %client.miniGame != $defaultMiniGame) {
			parent::serverCmdLight(%client);
			return;
		}

		%player = %client.player;

		if (!isObject(%player) || %player.getState() $= "Dead") {
			parent::serverCmdLight(%client);
			return;
		}

		if (!%player.getDataBlock().isSurvivor) {
			return;
		}

		if (getSimTime() - %player.lastLightTime < 250) {
			return;
		}

		%player.lastLightTime = getSimTime();

		if (isObject(%player.light)) {
			%player.light.delete();
		}
		else {
			%player.light = new fxLight() {
				datablock = playerFlashlightData;
				iconSize = 1;

				player = %player;
				enable = 1;
			};

			missionCleanup.add(%player.light);
			%player.light.setTransform(%player.getTransform());

			if (!isEventPending(%player.flashlightTick)) {
				%player.flashlightTick();
			}
		}
	}
};

activatePackage("zambSurvivorPackage");
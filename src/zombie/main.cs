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

	isZombie = 1;
	isSurvivor = 0;

	zombieType = 0;
	targetImprovementTolerance = 0.15;
};

exec("./ai.cs");
exec("./appearance.cs");

exec("./types/wanderer.cs");
exec("./types/horde.cs");

function zombieData::onAdd(%this, %obj) {
	parent::onAdd(%this, %obj);

	%obj.setMoveTolerance(0.75); // 1.78
	%obj.setMoveSlowdown(0);

	%obj.setActionThread("root");
	%this.applyZombieAppearance(%obj);
}

function zombieData::onRemove(%this, %obj) {
	if (isObject(%obj.path)) {
		%obj.path.delete();
	}

	if (isObject(%obj.camera)) {
		%obj.camera.delete();
	}
}

function zombieData::zombieAttack(%this, %obj) {
	if ($Sim::Time < %obj.attackReadyTime) {
		return;
	}

	%start = %obj.getHackPosition();
	%eye = %obj.getEyeVector();

	initContainerRadiusSearch(%start, 3, $TypeMasks::PlayerObjectType);

	switch (ZAMB.difficulty) {
		case 0: %damage = 1;
		case 1: %damage = 2;
		case 2: %damage = 5;
		case 3: %damage = 20;
	}

	while (isObject(%col = containerSearchNext())) {
		if (%col.getDataBlock().isZombie || %col.getState() $= "Dead") {
			continue;
		}

		%end = %col.getHackPosition();

		if (vectorDist(%start, %end) >= 3) {
			continue;
		}

		%line = vectorNormalize(vectorSub(%col.position, %obj.position));

		if (vectorDot(%eye, %line) >= 0.75) {
			%ray = containerRayCast(%start, %end, $TypeMasks::FxBrickObjectType);

			if (%ray $= "0") {
				%hit = true;

				%col.setVelocity("0 0 0.1");
				%col.schedule(150, "setVelocity", "0 0 0.1");

				%eye2 = %col.getEyeVector();
				%line2 = vectorNormalize(vectorSub(%col.position, %obj.position));

				if (vectorDot(%eye2, %line2) >= 0) {
					%col.damage(%obj, %end, %damage, $DamageType::Suicide);
				}
				else {
					%col.damage(%obj, %end, %damage / 2, $DamageType::Suicide);
				}
			}
		}
	}

	if (isObject(%obj.getControllingClient()) || %hit) {
		%obj.attackReadyTime = $Sim::Time + 1;
		%obj.playThread(0, "activate2");

		if (%hit) {
			%profile = "zamb_infected_claw_flesh" @ getRandom(1, 4);
		}
		else {
			%profile = "zamb_infected_claw_miss" @ getRandom(1, 2);
		}

		if (isObject(%profile)) {
			serverPlay3D(%profile, %obj.getHackPosition());
		}
	}
}
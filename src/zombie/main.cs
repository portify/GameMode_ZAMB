exec("./ai.cs");
exec("./appearance.cs");

exec("./specials/boomer.cs");
exec("./specials/tank.cs");

function baseZombieData::onAdd(%this, %obj) {
	parent::onAdd(%this, %obj);

	if (%this.enableRandomMoveSpeed && %obj.getClassName() $= "AIPlayer") {
		%obj.setMoveSpeed(%this.moveSpeedMin + getRandom() * (%this.moveSpeedMax - %this.moveSpeedMin));
	}

	%obj.setMoveTolerance(1.78);
	%obj.setMoveSlowdown(0);

	%obj.maxYawSpeed = 6;
	%obj.maxPitchSpeed = 3;

	%obj.setActionThread("root");
	%this.applyZombieAppearance(%obj);
}

function baseZombieData::onRemove(%this, %obj) {
	if (isObject(%obj.path)) {
		%obj.path.delete();
	}

	if (isObject(%obj.line)) {
		%obj.line.delete();
	}
}

function player::setLine(%this, %a, %b) {
	if (%this.line.a !$= %a || %this.line.b !$= %b) {
		if (isObject(%this.line)) {
			%this.line.delete();
		}

		%this.line = createLine(%a, %b, 0.005, "1 1 1 0.2");
	}
}

function baseZombieData::zombieAttack(%this, %obj) {
	if ($Sim::Time < %obj.attackReadyTime) {
		return;
	}

	%start = %obj.getHackPosition();
	%eye = %obj.getEyeVector();

	initContainerRadiusSearch(%start, 3, $TypeMasks::PlayerObjectType);

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

				%col.damage(%obj, %end, 2, $DamageType::Suicide);
			}
		}
	}

	if (isObject(%obj.getControllingClient()) || %hit) {
		%obj.attackReadyTime = $Sim::Time + 0.75 + getRandom() * 0.25;
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
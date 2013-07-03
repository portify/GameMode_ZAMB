function survivorData::onTrigger(%this, %obj, %slot, %state) {
	parent::onTrigger(%this, %obj, %slot, %state);
	%obj.trigger[%slot] = %state;

	if (%slot != 4 || !%state || $Sim::Time - %obj.lastShove < 1) {
		return;
	}

	%hit = 0;

	%start = %obj.getHackPosition();
	%eye = %obj.getEyeVector();

	initContainerRadiusSearch(%obj.getHackPosition(), 4, $TypeMasks::PlayerObjectType);

	while (isObject(%col = containerSearchNext())) {
		%end = %col.getHackPosition();

		if (vectorDist(%start, %end) >= 4) {
			continue;
		}

		%line = vectorNormalize(vectorSub(%end, %start));

		if (vectorDot(%eye, %line) >= 0.75) {
			%dataBlock = %col.getDataBlock();

			if (!%dataBlock.isZombie) {
				if (%dataBlock.isSurvivor && %hit < 2) {
					%hit = 2;
				}

				continue;
			}

			if (%dataBlock.noZombiePush) {
				continue;
			}

			%col.setVelocity(vectorAdd(vectorScale(%line, 10), "0 0 6"));
			%col.damage(%obj, %end, getRandom(-5, -15), $DamageType::Suicide);

			if (%hit < 3) {
				%hit = 3;
			}
		}
	}

	if (%hit == 0) {
		%start = %obj.getEyePoint();

		%ray = containerRayCast(%start,
			vectorAdd(%start, vectorScale(%eye, 4)),
			$TypeMasks::FxBrickObjectType | $TypeMasks::TerrainObjectType
		);

		if (%ray !$= "0") {
			%hit = 1;
		}
	}

	if (%hit) {
		%obj.playThread(2, "plant");
	}

	if (isObject(%obj.getControllingClient()) || %hit >= 2) {
		%obj.lastShove = $Sim::Time;
		%obj.playThread(3, "shiftUp");
	}
}
package zambBloodPackage {
	function armor::onDamage(%this, %obj, %delta) {
		parent::onDamage(%this, %obj, %delta);

		if (%obj.getState() $= "Dead") {
			%count = getRandom(5, 8);
		}
		else {
			%count = mCeil((%delta / %this.maxDamage) * 3);
		}

		%obj.sprayBlood(%count);
	}
};

activatePackage("zambBloodPackage");

datablock itemData(bloodItem) {
	shapeFile = $ZAMB::Path @ "res/models/blood2.dts";

	doColorShift = true;
	colorShiftColor = "1 0 0 1";

	friction = 1;
	elasticity = 0;
};

function spawnBlood(%transform, %velocity) {
	if (%transform $= "") {
		%transform = "0 0 1 0 0 0 1";
	}

	if (%velocity $= "") {
		%velocity = "0 0 0";
	}

	%size = 0.75 + getRandom() * 1.25;

	%obj = new Item() {
		datablock = bloodItem;
		scale = %size SPC %size SPC 0.01;
	};

	missionCleanup.add(%obj);

	%obj.setTransform(%transform);
	%obj.setVelocity(%velocity);

	%obj.schedulePop();
	return %obj;
}

function player::sprayBlood(%this, %count) {
	%position = %this.getHackPosition();
	%velocity = %this.getVelocity();

	for (%i = 0; %i < %count; %i++) {
		%x = getRandom() * (getRandom(0, 1) ? 1 : -1) * 6;
		%y = getRandom() * (getRandom(0, 1) ? 1 : -1) * 6;
		%z = getRandom() * 1;

		%obj = spawnBlood(%position, vectorAdd(%velocity, %x SPC %y SPC %z));

		if (isObject(%obj)) {
			%obj.setCollisionTimeout(%this);
		}
	}
}

function bloodItem::onPickup(%this, %obj, %col) {}
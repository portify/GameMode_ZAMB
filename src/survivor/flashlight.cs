datablock fxLightData(playerFlashlightData : playerLight) {
	uiName = "";
	flareOn = 0;

	radius = 16; // 10
	brightness = 3;
};

function player::flashlightTick(%this) {
	cancel(%this.flashlightTick);

	if (%this.getState() $= "Dead" || !isObject(%this.light)) {
		return;
	}

	// %start = %this.getEyePoint();
	%start = %this.getMuzzlePoint(0);
	%vector = %this.getEyeVector();
	// %vector = %this.getMuzzleVector(0);

	%range = 50;

	if ($EnvGuiServer::VisibleDistance !$= "") {
		%limit = $EnvGuiServer::VisibleDistance / 2;

		if (%range > %limit) {
			%range = %limit;
		}
	}

	%end = vectorAdd(%start, vectorScale(%vector, %range));
	%end = vectorAdd(%end, %this.getVelocity());

	%ray = containerRayCast(%start, %end, $TypeMasks::All, %this);

	if (%ray $= "0") {
		%pos = %end;
	}
	else {
		%pos = vectorSub(getWords(%ray, 1, 3), %vector);
	}

	%path = vectorSub(%pos, %this.light.position);
	%length = vectorLen(%path);

	%speed = 0.35;

	if (%length < %speed) {
		%pos = %pos;
	}
	else {
		%moved = vectorScale(%path, %speed);
		%pos = vectorAdd(%this.light.position, %moved);
	}

	%this.light.setTransform(%pos);
	%this.light.inspectPostApply();

	%this.flashlightTick = %this.schedule(32, "flashlightTick");
}
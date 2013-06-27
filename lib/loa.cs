function AIPlayer::updateLOA(%this) {
	%bump1 = %this.loaProbe("0 0 -1");
	%bump2 = %this.loaProbe("0 0 1");

	if (%bump1 && %bump2) {
		if (%this.loaStuck) {
			if ($Sim::Time - %this.loaStuckTime >= 1.5) {
				%this.loaStuckDir *= -1;
				%this.loaStuckTime = $Sim::Time;

				%this.setMoveX(%this.loaStuckDir);
			}
		}
		else {
			%this.loaStuck = 1;
			%this.loaStuckDir = getRandom() >= 0.5 ? 1 : -1;
			%this.loaStuckTime = $Sim::Time;

			%this.setMoveX(%this.loaStuckDir);
		}
	}
	else {
		%this.loaStuck = false;
		%this.setMoveX(%bump1 ? 1 : (%bump2 ? -1 : 0));
	}
}

function AIPlayer::loaProbe(%this, %up) {
	%start = %this.getHackPosition();
	%forward = %this.getForwardVector();

	%cross = vectorCross(%forward, %up);
	%length = 1.75 * getWord(%this.getScale(), 2);

	%stop = vectorAdd(%start, vectorScale(%cross, 1));
	%stop = vectorAdd(%stop, vectorScale(%forward, %length));

	return containerRayCast(%start, %stop, $TypeMasks::All, %this) !$= 0;
}
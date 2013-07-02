// Tempos
//  0 => make
//  1 => keep
//  2 => fade
//  3 => wait

exec("./tempos.cs");

function ZAMB_Director::onAdd(%this) {
	%this.tempo = 0;
	%this.start = $Sim::Time;
}

function ZAMB_Director::tick(%this) {
	%this.intensity = 0;

	for (%i = 0; %i < $defaultMiniGame.numMembers; %i++) {
		%player = $defaultMiniGame.member[%i].player;

		if (!isObject(%player) || %player.getState() $= "Dead") {
			continue;
		}

		if (!%player.getDataBlock().isSurvivor) {
			continue;
		}

		// %intensity = %player.updateIntensity(0.1);
		%intensity = (mSin($Sim::Time) + 1) / 2;

		if (%intensity > %this.intensity) {
			%this.intensity = %intensity;
		}
	}

	switch (%this.tempo) {
		case 0: %this.tickMake();
		case 1: %this.tickKeep();
		case 2: %this.tickFade();
		case 3: %this.tickWait();
	}
}

function ZAMB_Director::setTempo(%this, %tempo, %a, %b, %c) {
	switch (%tempo) {
		case 0: %name = "PEAK_MAKE";
		case 1: %name = "PEAK_KEEP";
		case 2: %name = "PEAK_FADE";
		case 3: %name = "PEAK_WAIT";

		default: return 0;
	}

	warn( "AID: setting tempo to '" @ %name @ "'" );

	%this.tempo = %tempo;
	%this.start = $Sim::Time;

	%this.length = "";

	%this.param0 = %a;
	%this.param1 = %b;
	%this.param2 = %c;

	return 1;
}

function ZAMB_Director::setTempoTimed(%this, %tempo, %length, %a, %b, %c) {
	if (%this.setTempo(%tempo, %a, %b, %c)) {
		%this.length = %length;
	}
}

function ZAMB_Director::isTempoDone(%this) {
	if (%this.length !$= "") {
		return $Sim::Time - %this.start >= %this.length;
	}

	return 0;
}
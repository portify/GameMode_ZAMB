$ZAMB::IntensityThreshold = 0.8;

function ZAMB_Director(%zamb) {
	return new ScriptObject() {
		class = ZAMB_Director;
		zamb = %zamb;
	};
}

function ZAMB_Director::onAdd(%this) {
	if ($Pref::Server::ZAMBDebug) {
		echo("ZAMB_Director(" @ %this @ ")::onAdd");
	}

	%this.tempo = 0;
	%this.start = $Sim::Time;
}

function ZAMB_Director::onRemove(%this) {
	if ($Pref::Server::ZAMBDebug) {
		echo("ZAMB_Director(" @ %this @ ")::onRemove");
	}
}

function ZAMB_Director::tick(%this) {
	%intensity = 0;

	for (%i = 0; %i < $defaultMiniGame.numMembers; %i++) {
		%player = $defaultMiniGame.member[%i].player;

		if (!isObject(%player) || %player.getState() $= "Dead") {
			continue;
		}

		if ($Sim::Time - %player.lastIntensityUpdateTime >= 10) {
			%player.intensity -= (1 / 30) * %delta;
		}

		%local = mClampF(%player.intensity, 0, 1);

		if (%local > %intensity) {
			%intensity = %local;
		}
	}

	switch (%this.tempo) {
		case 0: %this.tickMake(%intensity);
		case 1: %this.tickKeep(%intensity);
		case 2: %this.tickFade(%intensity);
		case 3: %this.tickWait(%intensity);
	}
}

function ZAMB_Director::setTempo(%this, %tempo, %length, %a, %b, %c) {
	if ($Pref::Server::ZAMBDebug) {
		switch (%tempo) {
			case 0: %name = "PEAK_MAKE";
			case 1: %name = "PEAK_KEEP";
			case 2: %name = "PEAK_FADE";
			case 3: %name = "PEAK_WAIT";

			default: %name = "#INVALID";
		}

		echo("ZAMB_Core(" @ %this @ ")::setTempo(" @ %tempo @ ")");
	}

	%this.start = $Sim::Time;

	%this.tempo = %tempo;
	%this.length = %length;

	%this.param0 = %a;
	%this.param1 = %b;
	%this.param2 = %c;

	return 1;
}

function ZAMB_Director::isTempoDone(%this) {
	if (%this.length !$= "") {
		return $Sim::Time - %this.start >= %this.length;
	}

	return 0;
}

function ZAMB_Director::isInCombat(%this, %tolerance) {
	if (!strLen(%tolerance)) {
		%tolerance = 5;
	}

	return $Sim::Time - $defaultMiniGame.lastCombatTime < %tolerance;
}

// Tempos

function ZAMB_Director::tickMake(%this, %intensity) {
	if (%intensity >= $ZAMB::IntensityThreshold) {
		%this.setTempo(1, 3 + getRandom() * 2);
	}
	else {
		//%this.maintainFullThreat();
	}
}

function ZAMB_Director::tickKeep(%this, %intensity) {
	if (%this.isTempoDone()) {
		%this.setTempo(2);
	}
	else {
		//%this.maintainFullThreat();
	}
}

function ZAMB_Director::tickFade(%this, %intensity) {
	if (!%this.isInCombat() && %intensity < $ZAMB:IntensityThreshold) {
		%this.setTempoTimed(3, 3 + getRandom() * 2);
	}
}

function ZAMB_Director::tickWait(%this, %intensity) {
	if (%this.isTempoDone()) {
		%this.setTempo(0);
	}
}
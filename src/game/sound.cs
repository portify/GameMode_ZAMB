function zambSound::onAdd(%this) {
	if (%this.type !$= "") {
		%type = %this.type;

		%this.type = "";
		%this.setType(%type);
	}
}

function zambSound::setType(%this, %type) {
	if (%this.type !$= "") {
		switch$ (%this.type) {
			case "rain":
				%this.stopEmitLoop(zamb_ambience_rumble_rain);
			case "thunderstorm":
				%this.stopEmitLoop(zamb_ambience_rumble_rain);
				cancel(%this.lightningStrikeController);
		}
	}

	%this.type = %type;

	switch$ (%type) {
		case "rain":
			%this.emitLoop(zamb_ambience_rumble_rain, 0.75);
		case "thunderstorm":
			%this.emitLoop(zamb_ambience_rumble_rain, 0.65);
			%this.lightningStrikeController();
	}
}

function zambSound::lightningStrike(%this) {
	%n = 9;

	%p0 = zamb_ambience_thunderstorm_lightning1;
	%p1 = zamb_ambience_thunderstorm_lightning2;
	%p2 = zamb_ambience_thunderstorm_lightning3;
	%p3 = zamb_ambience_thunderstorm_lightning4;
	%p4 = zamb_ambience_thunderstorm_thunder1;
	%p5 = zamb_ambience_thunderstorm_thunder2;
	%p6 = zamb_ambience_thunderstorm_thunder3;
	%p7 = zamb_ambience_thunderstorm_thunderFar1;
	%p8 = zamb_ambience_thunderstorm_thunderFar1;

	if (isObject(Sky)) {
		Sky.stormClouds(1, 0.75 + getRandom() * 0.5);
	}

	%this.emit(%p[getRandom(0, %n - 1)], 0.75 + getRandom() * 0.25);
}

function zambSound::lightningStrikeController(%this) {
	cancel(%this.lightningStrikeController);
	%time = getRandom(5500, 10000);

	%this.lightningStrike();
	%this.lightningStrikeController = %this.schedule(%time, "lightningStrikeController");
}

function zambSound::emit(%this, %profile, %volume, %time) {
	if (!isObject(%profile)) {
		error("ERROR: Unexistant AudioProfile.");
		return;
	}

	if (%volume $= "") {
		%volume = 1;
	}
	else {
		%volume = mClampF(%volume, 0, 1);
	}

	if (!$Server::Dedicated) {
		%time = alxGetWaveLen( %profile.fileName );
	}
	else if (%time $= "") {
		%time = 30000;
	}

	%emitter = new audioEmitter() {
		profile = %profile;
		volume = %volume;

		is3D = false;
		isLooping = false;

		type = 0;
		useProfileDescription = false;
	};

	missionCleanup.add(%emitter);
	%emitter.scheduleNoQuota(%time, "delete");

	return %emitter;
}

function zambSound::emitLoop(%this, %profile, %volume) {
	if (!isObject(%profile)) {
		error("ERROR: Unexistant AudioProfile.");
		return;
	}

	if (%volume $= "") {
		%volume = 1;
	}
	else {
		%volume = mClampF(%volume, 0, 1);
	}

	%name = "_loopingAmbient_" @ %profile.getID();

	if (isObject(%name)) {
		%name.delete();
	}

	%emitter = new audioEmitter() {
		profile = %profile;
		volume = %volume;

		is3D = false;
		isLooping = true;

		type = 0;
		useProfileDescription = false;
	};

	missionCleanup.add(%emitter);

	%emitter.setName(%name);
	%emitter.scheduleNoQuota(%time, "delete");

	return %emitter;
}

function zambSound::stopEmitLoop(%this, %profile) {
	%name = "_loopingAmbient_" @ %profile.getID();

	if (isObject(%name)) {
		%name.delete();
	}
}
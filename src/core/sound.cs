function zambSoundController::onAdd(%this) {
	if (isObject(missionCleanup)) {
		missionCleanup.add(%this);
	}

	if (!isObject(%this.miniGame)) {
		error("ERROR: ZAMB created without a mini-game.");
		%this.delete();

		return;
	}

	if (%this.rainA) {
		%this.setRain(%this.rainA, %this.rainB);
	}

	if (%this.wind) {
		%this.setWind(%this.wind);
	}

	if (%this.windHits) {
		%this.setWindHits(%this.windHits);
	}

	if (%this.thunderHits) {
		%this.setThunderHits(%this.thunderHits);
	}
}

function zambSoundController::play(%this, %profile, %volume, %time, %once) {
	if (!isObject(%profile)) {
		error("ERROR: Unexistant AudioProfile.");
		return;
	}

	if (%once && %this.playing(%profile)) {
		return;
	}

	if (%volume $= "") {
		%volume = 1;
	}
	else {
		%volume = mClampF(%volume, 0, 1);
	}

	if (!$Server::Dedicated) {
		%time = alxGetWaveLen(%profile.fileName);
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

	%this.add(%emitter);
	%emitter.scheduleNoQuota(%time, "delete");

	return %emitter;
}

function zambSoundController::playLoop(%this, %profile, %volume) {
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

	%this.stop(%profile);

	%emitter = new audioEmitter() {
		profile = %profile;
		volume = %volume;

		is3D = false;
		isLooping = true;

		type = 0;
		useProfileDescription = false;
	};

	%this.add(%emitter);
	return %emitter;
}

function zambSoundController::playing(%this, %profile) {
	if (!isObject(%profile)) {
		return 0;
	}

	%profile = %profile.getID();
	%count = %this.getCount();

	for (%i = 0; %i < %count; %i++) {
		if (%this.getObject(0).profile.getID() == %profile) {
			return 1;
		}
	}

	return 0;
}

function zambSoundController::stop(%this, %profile) {
	if (!isObject(%profile)) {
		return;
	}

	%profile = %profile.getID();
	%count = %this.getCount();

	for (%i = 0; %i < %count; %i++) {
		%emitter = %this.getObject(%i);

		if (%emitter.profile.getID() == %profile) {
			%emitter.delete();

			%count--;
			%i--;
		}
	}
}

function zambSoundController::setRain(%this, %a, %b) {
	%this.stop("zamb_ambience_rain_" @ %this.rainA @ "_" @ %this.rainB);

	%this.rainA = %a;
	%this.rainB = %b;

	%profile = "zamb_ambience_rain_" @ %a @ "_" @ %b;

	if (isObject(%profile)) {
		%this.playLoop(%profile, 0.8);
	}
}

function zambSoundController::setWind(%this, %wind) {
	%this.stop("zamb_ambience_wind_" @ %this.wind);
	%this.wind = %wind;

	%profile = "zamb_ambience_wind_" @ %wind;

	if (isObject(%profile)) {
		%this.playLoop(%profile, 0.85);
	}
}

function zambSoundController::setWindHits(%this, %bool) {
	%pending = isEventPending(%this.windHitController);
	%this.windHits = %bool ? 1 : 0;

	if (%bool && !%pending) {
		%this.windHitController();
	}
	else if (!%bool && %pending) {
		cancel(%this.windHitController);
	}
}

function zambSoundController::setThunderHits(%this, %bool) {
	%pending = isEventPending(%this.thunderHitController);
	%this.thunderHits = %bool ? 1 : 0;

	if (%bool && !%pending) {
		%this.thunderHitController();
	}
	else if (!%bool && %pending) {
		cancel(%this.thunderHitController);
	}
}

function zambSoundController::windHitController(%this) {
	cancel(%this.windHitController);
	%time = getRandom(10000, 15000);

	%this.windHit();
	%this.windHitController = %this.schedule(%time, "windHitController");
}

function zambSoundController::thunderHitController(%this) {
	cancel(%this.thunderHitController);
	%time = getRandom(5500, 10000);

	%this.thunderHit();
	%this.thunderHitController = %this.schedule(%time, "thunderHitController");
}

function zambSoundController::windHit(%this) {
	%p0 = zamb_ambience_wind_hit1;
	%p1 = zamb_ambience_wind_hit2;
	%p2 = zamb_ambience_wind_hit3;
	%p3 = zamb_ambience_wind_med1;
	%p4 = zamb_ambience_wind_med2;
	%p5 = zamb_ambience_wind_gust1;
	%p6 = zamb_ambience_wind_gust2;
	%p7 = zamb_ambience_wind_snippet1;
	%p8 = zamb_ambience_wind_snippet2;
	%p9 = zamb_ambience_wind_snippet3;

	echo(getSimTime() SPC "windHit");
	%this.play(%p[getRandom(0, 9)], 0.8 + getRandom() * 0.2, 20);
}

function zambSoundController::thunderHit(%this) {
	%p0 = zamb_ambience_thunder_lightning1;
	%p1 = zamb_ambience_thunder_lightning2;
	%p2 = zamb_ambience_thunder_lightning3;
	%p3 = zamb_ambience_thunder_lightning4;
	%p4 = zamb_ambience_thunder_thunder1;
	%p5 = zamb_ambience_thunder_thunder2;
	%p6 = zamb_ambience_thunder_thunder3;
	%p7 = zamb_ambience_thunder_far1;
	%p8 = zamb_ambience_thunder_far2;

	if (isObject(Sky)) {
		%time = 0.1 + getRandom() * 0.2;

		Sky.stormClouds(1, %time);
		Sky.schedule(%time * 1000 + 100, "stormClouds", 0, %time);
	}

	echo(getSimTime() SPC "thunderHit");
	%this.play(%p[getRandom(0, 8)], 0.75 + getRandom() * 0.25);
}
function zambRegisterSound(%name, %path, %description) {
	if (%description $= "") {
		%description = "audioDefault3D";
	}

	%_path = %path;
	
	if (getSubStr(%path, 0, 8) !$= "Add-Ons/") {
		%path = $ZAMB::Path @ "res/sounds/" @ %path;
	}

	if (!isFile(%path)) {
		%path = %path @ ".wav";

		if (!isFile(%path)) {
			error("ERROR: '" @ %_path @ "' does not exist!");
			return -1;
		}
	}

	%db = nameToID(%name);

	if (isObject(%db)) {
		%db.filePath = %path;
		%db.description = %description;

		return %db;
	}
	else {
		datablock audioProfile(temporaryZAMBAudioProfile) {
			preload = true;
			fileName = %path;
			description = %description;
		};

		%db = nameToID("temporaryZAMBAudioProfile");

		if (!isObject(%db)) {
			error("ERROR: Failed to create AudioProfile!");
			return -1;
		}

		%db.setName(%name);
	}

	return %db;
}

function zambLoadFootsteps()
{
	%pattern = $ZAMB::Path @ "res/sounds/footsteps/*.wav";
	
	for (%file = findFirstFile(%pattern); %file !$= ""; %file = findNextFile(%pattern)) {
		%base = fileBase(%file);
		%length = strLen(%base);

		%mat = getSubStr(%base, 0, %length - 1);
		%num = getSubStr(%base, %length - 1, 1);

		if (%num > mFloor($ZAMB::MaterialNum[%mat])) {
			$ZAMB::MaterialNum[%mat] = %num;
		}

		zambRegisterSound("zamb_footstep_" @ %base, %file);
	}
}

package zambFootstepPackage {
	function armor::onNewDataBlock(%this, %obj) {
		parent::onNewDataBlock(%this, %obj);

		if (!isEventPending(%obj.footstepTick)) {
			%obj.footstepUpdateTick();
		}
	}

	function armor::onTrigger(%this, %obj, %slot, %state) {
		parent::onTrigger(%this, %obj, %slot, %state);
		%obj.trigger[%slot] = %state;
	}

	function armor::onDisabled(%this, %obj) {
		%miniGame = getMiniGameFromObject(%obj);

		if (%miniGame $= $defaultMiniGame && isObject(%obj.client)) {
			%obj.client.play2D(zamb_music_death);
		}

		parent::onDisabled(%this, %obj);
	}
};

activatePackage("zambFootstepPackage");

function Player::footstepUpdateTick(%this) {
	cancel(%this.footstepUpdateTick);

	if (%this.getState() $= "Dead") {
		return;
	}

	%tick = 0;
	%pending = isEventPending(%this.footstepTick);

	if (vectorLen(%this.getVelocity()) >= 0.5 && !%this.isMounted()) {
		if (!%this.trigger[3] && !(%this.trigger[4] && %this.getDataBlock().canJet)) {
			%ray = containerRayCast(
				vectorAdd(%this.position, "0 0 0.05"),
				vectorSub(%this.position, "0 0 0.05"),
				$TypeMasks::All, %this
			);

			if (%ray !$= "0") {
				%tick = 1;
			}
		}
	}

	if (%tick && !%pending) {
		%this.footstepTick = %this.schedule(100, "footstepTick");
	}
	else if (!%tick && %pending) {
		cancel(%this.footstepTick);
	}

	%this.footstepUpdateTick = %this.schedule(50, "footstepUpdateTick");
}

function Player::footstepTick(%this) {
	cancel(%this.footstepTick);

	if (%this.getState() $= "Dead") {
		return;
	}

	%this.playFootstepSound();
	%this.footstepTick = %this.schedule(310, "footstepTick");
}

function Player::playFootstepSound(%this, %material) {
	if (%material $= "") {
		%material = $f;
	}

	if (%material $= "") {
		%material = "concrete";
	}

	if (mFloor($ZAMB::MaterialNum[%material]) <= 0) {
		return;
	}

	%profile = nameToID("zamb_footstep_" @ %material @ getRandom(1, $ZAMB::MaterialNum[%material]));

	if (!isObject(%profile)) {
		return;
	}

	serverPlay3D(%profile, %this.getPosition());
}

function Player::playJumpLanding(%this) {
	if (%this.getDataBlock().isZombie) {
		%profile = zamb_infected_jumplanding;
	}
	else {
		%profile = zamb_survivor_jumplanding;
	}

	if (isObject(%profile)) {
		serverPlay3D(%profile, %this.getPosition());
	}
}

function playAmbientSoundSingular(%profile, %volume) {
	%emitter = new audioEmitter() {
		profile = %profile;
		volume = %volume;

		is3D = false;
		isLooping = false;

		type = 0;
		useProfileDescription = false;
	};

	// %time = alxGetWaveLen( %profile.fileName );
	%time = 30000;

	missionCleanup.add(%emitter);
	%emitter.scheduleNoQuota(%time, "delete");

	return %emitter;
}

function playAmbientSoundLooping(%profile, %volume) {
	%name = "loopingAmbient_" @ %profile;

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

	%emitter.setName(%name);
	missionCleanup.add(%emitter);

	return %emitter;
}

function lightningStrikeController() {
	cancel($lightingStrikeController);
	%thunderStrikeCount = 9;

	%thunderStrike0 = zamb_ambience_thunderstorm_lightning1;
	%thunderStrike1 = zamb_ambience_thunderstorm_lightning2;
	%thunderStrike2 = zamb_ambience_thunderstorm_lightning3;
	%thunderStrike3 = zamb_ambience_thunderstorm_lightning4;
	%thunderStrike4 = zamb_ambience_thunderstorm_thunder1;
	%thunderStrike5 = zamb_ambience_thunderstorm_thunder2;
	%thunderStrike6 = zamb_ambience_thunderstorm_thunder3;
	%thunderStrike7 = zamb_ambience_thunderstorm_thunderFar1;
	%thunderStrike8 = zamb_ambience_thunderstorm_thunderFar1;

	%profile = %thunderStrike[getRandom(0, %thunderStrikeCount - 1)];
	%volume = 0.75 + getRandom() * 0.25;
	// %time = alxGetWaveLen( %profile.fileName ) + getRandom( 1000, 6500 );
	// %time = getRandom( 7500, 9000 ) + getRandom( 500, 2500 );
	%time = getRandom(5000, 7500) + getRandom(500, 2500);
	// %time = getRandom( 1000, 3500 );

	if (isObject(Sky)) {
		Sky.stormClouds(1, 0.75 + getRandom() * 0.5);
	}

	playAmbientSoundSingular(%profile, %volume);
	$lightingStrikeController = schedule(%time, 0, "lightningStrikeController");
}

function startAmbience() {
	playAmbientSoundLooping(zamb_ambience_rumble_rain, 0.65);
	lightningStrikeController();
}

zambRegisterSound(zamb_cannot_use, "cannot_use");
zambRegisterSound(zamb_damage1, "damage1");
zambRegisterSound(zamb_damage2, "damage2");

zambRegisterSound(zamb_music_death, "music/death");
zambRegisterSound(zamb_music_germs_low, "music/germs_low");
zambRegisterSound(zamb_music_germs_high, "music/germs_high");
zambRegisterSound(zamb_music_pukricide, "music/pukricide");
zambRegisterSound(zamb_music_quarantine1, "music/quarantine1");
zambRegisterSound(zamb_music_quarantine2, "music/quarantine2");
zambRegisterSound(zamb_music_quarantine3, "music/quarantine3");
zambRegisterSound(zamb_music_win, "music/win");

zambRegisterSound(zamb_survivor_flashlight, "survivor/flashlight");
zambRegisterSound(zamb_survivor_jumplanding, "survivor/jumplanding");
zambRegisterSound(zamb_survivor_pickup, "survivor/pickup");
zambRegisterSound(zamb_survivor_swing_infected, "survivor/swing_infected");
zambRegisterSound(zamb_survivor_swing_miss, "survivor/swing_miss");
zambRegisterSound(zamb_survivor_swing_survivor, "survivor/swing_survivor");
zambRegisterSound(zamb_survivor_swing_world, "survivor/swing_world");

zambRegisterSound(zamb_ambience_rumble_rain, "ambience/rumble_rain");

zambRegisterSound(zamb_ambience_thunderstorm_lightning1, "ambience/thunderstorm/lightning1");
zambRegisterSound(zamb_ambience_thunderstorm_lightning2, "ambience/thunderstorm/lightning2");
zambRegisterSound(zamb_ambience_thunderstorm_lightning3, "ambience/thunderstorm/lightning3");
zambRegisterSound(zamb_ambience_thunderstorm_lightning4, "ambience/thunderstorm/lightning4");

zambRegisterSound(zamb_ambience_thunderstorm_thunder1, "ambience/thunderstorm/thunder1");
zambRegisterSound(zamb_ambience_thunderstorm_thunder2, "ambience/thunderstorm/thunder2");
zambRegisterSound(zamb_ambience_thunderstorm_thunder3, "ambience/thunderstorm/thunder3");

zambRegisterSound(zamb_ambience_thunderstorm_thunderFar1, "ambience/thunderstorm/thunder_far1");
zambRegisterSound(zamb_ambience_thunderstorm_thunderFar2, "ambience/thunderstorm/thunder_far2");

zambLoadFootsteps();
schedule(0, 0, "startAmbience");
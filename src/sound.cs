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

function zambLoadFootsteps() {
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
	function armor::onTrigger(%this, %obj, %slot, %state) {
		parent::onTrigger(%this, %obj, %slot, %state);
		%obj.trigger[%slot] = %state;
	}
};

activatePackage("zambFootstepPackage");

function Player::footstepUpdateTick(%this) {
	cancel(%this.footstepUpdateTick);

	if (%this.getState() $= "Dead") {
		return;
	}

	%vz = getWord(%this.getVelocity(), 2);

	if (%this.lastVZ < -6 && %vz >= -0.75) {
		%this.playJumpLanding();
	}

	%this.lastVZ = %vz;

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

	%this.footstepUpdateTick = %this.schedule(40, "footstepUpdateTick");
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

	%dataBlock = %this.getDataBlock();

	if (%dataBlock.footstepMaterial !$= "") {
		if (%dataBlock.alternateFootstepMaterial[%material] $= "") {
			%material = %dataBlock.footstepMaterial;
		}
		else {
			%material = %dataBlock.alternateFootstepMaterial[%material];
		}
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

function zambEmit(%profile, %volume, %time, %once) {
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

	return %emitter;
}

function zambSound::stopEmitLoop(%this, %profile) {
	if (isObject(%profile)) {
		%name = "_loopingAmbient_" @ %profile.getID();

		if (isObject(%name)) {
			%name.delete();
		}
	}
}

datablock audioDescription(audioQuiet3D : audioDefault3D) {
	volume = 0.5;
};

datablock audioDescription(audioVeryQuiet3D : audioDefault3D) {
	volume = 0.25;
};

zambRegisterSound(zamb_music_death, "music/death");
zambRegisterSound(zamb_music_germs_low, "music/germs_low");
zambRegisterSound(zamb_music_germs_high, "music/germs_high");
zambRegisterSound(zamb_music_pukricide, "music/pukricide");
zambRegisterSound(zamb_music_tank, "music/tank");
zambRegisterSound(zamb_music_win, "music/win");

zambRegisterSound(zamb_music_bacteria_boomer, "music/bacteria/boomer");
zambRegisterSound(zamb_music_bacteria_boomer_multiple, "music/bacteria/boomer_multiple");

zambRegisterSound(zamb_music_zombiechoir1, "music/zombiechoir/hit1");
zambRegisterSound(zamb_music_zombiechoir2, "music/zombiechoir/hit2");
zambRegisterSound(zamb_music_zombiechoir3, "music/zombiechoir/hit3");
zambRegisterSound(zamb_music_zombiechoir4, "music/zombiechoir/hit4");
zambRegisterSound(zamb_music_zombiechoir5, "music/zombiechoir/hit5");
zambRegisterSound(zamb_music_zombiechoir6, "music/zombiechoir/hit6");
zambRegisterSound(zamb_music_zombiechoir7, "music/zombiechoir/hit7");

zambRegisterSound(zamb_survivor_flashlight, "survivor/flashlight");
zambRegisterSound(zamb_survivor_jumplanding, "survivor/jumplanding");
zambRegisterSound(zamb_survivor_swing_infected, "survivor/swing_infected");
zambRegisterSound(zamb_survivor_swing_miss, "survivor/swing_miss");
zambRegisterSound(zamb_survivor_swing_survivor, "survivor/swing_survivor");
zambRegisterSound(zamb_survivor_swing_world, "survivor/swing_world");

zambRegisterSound(zamb_infected_jumplanding, "infected/jumplanding");
zambRegisterSound(zamb_infected_claw_miss1, "infected/claw_miss1", audioQuiet3D);
zambRegisterSound(zamb_infected_claw_miss2, "infected/claw_miss2", audioQuiet3D);
zambRegisterSound(zamb_infected_claw_flesh1, "infected/claw_flesh1", audioQuiet3D);
zambRegisterSound(zamb_infected_claw_flesh2, "infected/claw_flesh2", audioQuiet3D);
zambRegisterSound(zamb_infected_claw_flesh3, "infected/claw_flesh3", audioQuiet3D);
zambRegisterSound(zamb_infected_claw_flesh4, "infected/claw_flesh4", audioQuiet3D);

zambRegisterSound(zamb_boomer_explode1, "infected/boomer/explode1");
zambRegisterSound(zamb_boomer_explode2, "infected/boomer/explode2");
zambRegisterSound(zamb_boomer_explode3, "infected/boomer/explode3");
zambRegisterSound(zamb_boomer_vomit1, "infected/boomer/vomit1");
zambRegisterSound(zamb_boomer_vomit2, "infected/boomer/vomit2");
zambRegisterSound(zamb_boomer_vomit3, "infected/boomer/vomit3");

zambLoadFootsteps();
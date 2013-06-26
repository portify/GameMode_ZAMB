function zambRegisterSound(%name, %file, %description) {
	if (%description $= "") {
		%description = "audioDefault3D";
	}

	%path = $ZAMB::Path @ "res/sounds/" @ %file;

	if (!isFile(%path)) {
		%path = %path @ ".wav";

		if (!isFile(%path)) {
			error("ERROR: '" @ %file @ "' does not exist!");
			return -1;
		}
	}

	%db = nameToID(%name);

	if (isObject(%db)) {
		%db.filePath = %path;
		%db.description = %description;

		return %db;
	}

	echo(%path);

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
	return %db;
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
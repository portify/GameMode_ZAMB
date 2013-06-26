function zambRegisterSound(%name, %file, %description) {
	if (%description $= "") {
		%description = "audioDefault3D";
	}

	%path = $ZAMB::Path @ "/res/sounds/" @ %file;

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

	%db = datablock audioProfile() {
		preload = true;
		filePath = %path;
		description = %description;
	}

	%db.setName(%name);
	return %db;
}

zambRegisterSound(zamb_damage1, "damage1");
zambRegisterSound(zamb_damage2, "damage2");

zambRegisterSound(zamb_music_death, "music/death");
zambRegisterSound(zamb_music_win, "music/win");

zambRegisterSound(zamb_music_germs_low, "music/germs_low");
zambRegisterSound(zamb_music_germs_high, "music/germs_high");

zambRegisterSound(zamb_survivor_flashlight, "survivor/flashlight");
zambRegisterSound(zamb_survivor_pickup, "survivor/pickup");
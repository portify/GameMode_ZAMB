exec("./package.cs");
exec("./zombies.cs");

exec("./director/main.cs");

$ZAMB::TickRate = 100;
$ZAMB::ThinkLimit = 10;
$ZAMB::ZombieLimit = 40;

function ZAMB(%difficulty) {
	if (isObject(ZAMB)) {
		ZAMB.delete();
	}

	new ScriptGroup(ZAMB) {
		difficulty = %difficulty;
	};
}

function ZAMB::onAdd(%this) {
	if (%this.difficulty $= "") {
		if ($defaultMiniGame.difficulty !$= "") {
			%this.difficulty = %defaultMiniGame.difficulty;
		}
		else {
			%this.difficulty = $Pref::Server::ZAMBDifficulty;
		}
	}

	%this.zombies = new ScriptGroup() {
		class = ZAMB_Zombies;
		zamb = %this;
	};

	%this.add(%this.zombies);

	%this.items = new ScriptGroup() {
		class = ZAMB_Items;
		zamb = %this;
	};

	%this.add(%this.items);

	%this.director = new ScriptObject() {
		class = ZAMB_Director;
		zamb = %this;

		zombies = %this.zombies;
		items = %this.items;
	};

	%this.add(%this.director);
}

function ZAMB::end(%this) {
	if (%this.ended) {
		return;
	}

	%this.ended = true;
	cancel(%this.tick);

	$defaultMiniGame.schedule(10000, "reset");
}

function ZAMB::tick(%this) {
	cancel(%this.tick);

	if (%this.zombies.getCount()) {
		%this.zombies.tick();
	}

	%this.director.tick();

	if ($Sim::Time - %this.lastDebugTime >= 0.2) {
		%this.debug();
	}

	%this.tick = %this.schedule($ZAMB::TickRate, "tick");
}

function ZAMB::debug(%this) {
	switch (%this.director.tempo) {
		case 0: %tempo = "PEAK_MAKE";
		case 1: %tempo = "PEAK_KEEP";
		case 2: %tempo = "PEAK_FADE";
		case 3: %tempo = "PEAK_WAIT";
	}

	if (%this.director.length !$= "") {
		%time = mCeil(%this.director.length - ($Sim::Time - %this.director.start));
		%tempo = %tempo SPC "(" @ %time @ "s)";
	}

	%color = gc(%this.director.intensity);
	%intensity = mFloatLength(%this.director.intensity * 100, 0);

	%str = "\c6" @ %tempo SPC "at <color:" @ %color @ ">" @ %intensity @ "%";
	%str = %str @ "\n\c6n = " @ %this.zombies.getCount();

	bottomPrintAll("<font:consolas:16>" @ %str @ "\n", 1, 5);
}

function gc(%v)
{
	%g="0123456789ABCDEF";

	if(%v>=0.5)
	{
		%v=mfloatlength((1 - ((%v-0.5) / 0.5))*15, 0);
		%c=getsubstr(%g,%v,1);
		return "FF"@%c@%c@"00";
	}
	else
	{
		%v=mfloatlength((%v / 0.5)*15, 0);
		%c=getsubstr(%g,%v,1);
		return %c@%c@"FF00";
	}

	return "FFFFFF";
}
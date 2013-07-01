exec("./package.cs");
exec("./zombies.cs");

exec("./director/tick.cs");

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
	%this.tick = %this.schedule($ZAMB::TickRate, "tick");
}
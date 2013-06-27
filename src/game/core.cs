function _createZAMB() {
	if (!isObject(ZAMB)) {
		new scriptObject(ZAMB);
	}
}

function ZAMB::onAdd(%this) {
	if (isObject(missionCleanup)) {
		missionCleanup.add(%this);
	}

	if (!isObject(%this.zombies)) {
		%this.zombies = new simGroup() {
			core = %this;
		};
	}

	if (!isObject(%this.director)) {
		%this.director = new scriptObject() {
			class = zambDirector;
			core = %this;
		};
	}

	if (!isObject(%this.sound)) {
		%this.sound = new scriptObject() {
			class = zambSound;
			core = %this;
			
			wind = 1;
			windHits = 1;
			thunderHits = 1;
		};
	}
}

function ZAMB::onRemove(%this) {
	if (isObject(%this.zombies)) {
		%this.zombies.delete();
	}

	if (isObject(%this.director)) {
		%this.director.delete();
	}

	if (isObject(%this.sound)) {
		%this.sound.delete();
	}
}

schedule(0, 0, "_createZAMB");
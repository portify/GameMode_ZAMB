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

	if (!isObject(%this.soundDirector)) {
		%this.soundDirector = new scriptObject() {
			class = zambSoundDirector;
			core = %this;
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

	if (isObject(%this.soundDirector)) {
		%this.soundDirector.delete();
	}
}

schedule(0, 0, "_createZAMB");
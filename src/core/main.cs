exec("./zamb.cs");
exec("./sound.cs");

package zambPackage {
	function miniGameSO::onRemove(%this) {
		if (isObject(%this.zamb)) {
			%this.zamb.delete();
		}

		if (isObject(%this.sound)) {
			%this.sound.delete();
		}

		if (isObject(%this.zombies)) {
			%this.zombies.delete();
		}

		parent::onRemove(%this);
	}

	function miniGameSO::addMember(%this, %client) {
		parent::addMember(%this, %client);

		if (%this.owner == 0 && !isObject(%this.zamb)) {
			%this.zambStart();
		}
	}

	function miniGameSO::removeMember(%this, %client) {
		parent::removeMember(%this, %client);

		if (!%this.numMembers && isObject(%this.zamb)) {
			%this.zambStop();
		}
	}

	function miniGameSO::reset(%this, %client) {
		if (%this.numMembers && isObject(%this.zamb)) {
			%this.zambStop();
			%this.zambStart();
		}

		parent::reset(%this, %client);
	}

	function miniGameCanDamage(%a, %b) {
		if (%b.getType() & $TypeMasks::PlayerObjectType) {
			if (%b.getDataBlock().isZombie) {
				return 1;
			}
		}

		%parent = parent::miniGameCanDamage(%a, %b);

		if (isObject(%m1)) {
			%m1 = getMiniGameFromObject(%a);
		}
		else {
			%m1 = -1;
		}

		if (isObject(%m2)) {
			%m2 = getMiniGameFromObject(%b);
		}
		else {
			%m2 = -1;
		}

		if (%m1 != %m2 || !isObject(%m1.zamb)) {
			return %parent;
		}

		%t1 = %a.getType() & $TypeMasks::PlayerObjectType;
		%t2 = %b.getType() & $TypeMasks::PlayerObjectType;

		if (!%t1 || !%t2) {
			return %parent;
		}

		return 1;
	}

	function gameConnection::onDeath(%this, %obj, %killer, %type, %area) {
		parent::onDeath(%this, %obj, %killer, %type, %area);
		%miniGame = %this.miniGame;

		if (!isObject(%miniGame.zamb) || %miniGame.zamb.ended) {
			return;
		}

		%this.zambCameraTarget = -1;
		%this.zambCameraSched = %this.schedule(2000, "getZAMBAutoCameraTarget");

		%alive = 0;
		messageClient(%this, 'MsgYourSpawn');

		for (%i = 0; %i < %miniGame.numMembers; %i++) {
			%player = %miniGame.member[%i].player;

			if (isObject(%player) && %player.getState() !$= "Dead") {
				%alive = 1;
				break;
			}
		}

		if (!%alive) {
			%miniGame.sound.play(zamb_music_death, 1, 10);
			%miniGame.zambEnd("\c5The survivors were overwhelmed.");
		}
	}

	function Observer::onTrigger(%this, %obj, %slot, %state) {
		%client = %obj.getControllingClient();

		if (!isObject(%client)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		%miniGame = %client.miniGame;

		if (!isObject(%miniGame.zamb)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		if (%miniGame.zamb.ended) {
			return;
		}

		if (isObject(%client.player) || %state == 0) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		%client.zambCameraTarget = mFloor(%client.zambCameraTarget);

		if (%slot != 1 && %slot != 3) {
			cancel(%client.zambCameraSched);
		}

		switch (%slot) {
			case 0: // Switch Target Negatively
				%i = %client.zambCameraTarget - 1;
				%l = %client.zambCameraTarget - %miniGame.numMembers;

				for (%i; %i > %l; %i--) {
					%ei = %i < 0 ? %miniGame.numMembers - %i : %i;
					%cl = %miniGame.member[%ei];

					if (isObject(%cl.player) && %cl.player.getState() !$= "Dead") {
						%obj.setMode("Corpse", %cl.player);
						%client.zambCameraTarget = %ei;

						break;
					}
				}

			case 4: // Switch Target Positively
				%i = %client.zambCameraTarget + 1;
				%l = %client.zambCameraTarget + %miniGame.numMembers;

				for (%i; %i < %l; %i++) {
					%ei = %i % %miniGame.numMembers;
					%cl = %miniGame.member[%ei];

					if (isObject(%cl.player) && %cl.player.getState() !$= "Dead") {
						%obj.setMode("Corpse", %cl.player);
						%client.zambCameraTarget = %ei;

						break;
					}
				}

			case 2: // Switch Camera Mode
				cancel(%client.zambCameraSched);

				if (%obj.mode $= "Corpse") {
					%obj.setMode("Observer");
					return;
				}

				%i = %client.zambCameraTarget + 1;
				%limit = %miniGame.numMembers + %client.zambCameraTarget;

				for (%i; %i < %limit; %i++) {
					%cl = %miniGame.member[%i % %miniGame.numMembers];

					if (isObject(%cl.player) && %cl.player.getState() !$= "Dead") {
						%obj.setMode("Corpse", %cl.player);
						%client.zambCameraTarget = %i;
					}
				}
		}
	}

	function Armor::onTrigger(%this, %obj, %slot, %state) {
		%client = %obj.getControllingClient();

		if (!isObject(%client)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		%miniGame = %client.miniGame;

		if (!isObject(%miniGame.zamb)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		if (%miniGame.zamb.ended) {
			return;
		}

		return parent::onTrigger(%this, %obj, %slot, %state);
	}

	function player::removeBody(%this) {
		if (%this.getDataBlock().isZombie) {
			%this.delete();
		}
		else {
			parent::removeBody(%this);
		}
	}

	function player::playPain(%this) {
		if (!%this.getDataBlock().isZombie) {
			parent::playPain(%this);
		}
	}

	function player::emote(%this, %emote) {
		if (!%this.getDataBlock().isZombie || %emote !$= "PainLowImage") {
			parent::emote(%this, %emote);
		}
	}
};

activatePackage("zambPackage");

function gameConnection::getZAMBAutoCameraTarget(%this) {
	%miniGame = %this.miniGame;

	if (!isObject(%miniGame.zamb) || %miniGame.zamb.ended || !isObject(%this.camera)) {
		return;
	}

	if (isObject(%this.player) && %this.player.getState() !$= "Dead") {
		return;
	}

	for (%i = 0; %i < %miniGame.numMembers; %i++) {
		%client = %mini.member[%i];

		if (isObject(%client.player) && %client.player.getState() !$= "Dead") {
			%this.zambCameraTarget = %i;
			%this.camera.setMode("Corpse", %cl.player);

			%this.setControlObject(%this.camera);
			return;
		}
	}
}

function miniGameSO::zambStart(%this, %difficulty) {
	if (%this.owner != 0) {
		error("ERROR: You cannot start ZAMB on a player-owned mini-game.");
		return;
	}

	if (isObject(%this.zamb)) {
		error("ERROR: This mini-game already has a running ZAMB session.");
		return;
	}

	if (isObject(%this.sound)) {
		%this.sound.delete();
	}

	if (isObject(%this.zombies)) {
		%this.zombies.delete();
	}

	if (%difficulty $= "") {
		%difficulty = 1;
	}

	%this.zamb = new ScriptObject() {
		class = ZAMB;
		miniGame = %this;
		difficulty = %difficulty;
	};

	%this.sound = new ScriptGroup() {
		class = zambSoundController;
		miniGame = %this;
	};

	%this.zombies = new SimGroup() {
		miniGame = %this;
	};

	missionCleanup.add(%this.zamb);
}

function miniGameSO::zambStop(%this) {
	if (!isObject(%this.zamb)) {
		error("This mini-game does not have a running ZAMB session.");
		return;
	}

	%this.zamb.delete();
	%this.sound.delete();
	%this.zombies.delete();
}

function miniGameSO::zambEnd(%this, %message) {
	if (!isObject(%this.zamb) || %this.zamb.ended) {
		return;
	}

	%this.zamb.end();
	%this.schedule(10000, "reset", 0);

	if (%message !$= "") {
		%this.chatMessageAll(%message);
	}
}

function miniGameSO::zambWin(%this) {
	if (!isObject(%this.zamb) || %this.zamb.ended) {
		return;
	}

	serverPlay2D(zamb_music_win);
	%this.zambEnd("\c5The survivors won!");
}

registerOutputEvent("MiniGameSO", "zambWin");
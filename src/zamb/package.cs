package zambPackage {
	function miniGameSO::addMember(%this, %client) {
		parent::addMember(%this, %client);

		if (%this.owner == 0 && !isObject(%this.zamb) && %this.numMembers) {
			%this.zamb = ZAMB_Core();
		}
	}

	function miniGameSO::removeMember(%this, %client) {
		parent::removeMember(%this, %client);

		if (%this.owner == 0 && isObject(%this.zamb)) {
			if (%this.numMembers) {
				for (%i = 0; %i < %this.numMembers; %i++) {
					%player = %this.member[%i].player;

					if (isObject(%player) && %player.getState() !$= "Dead") {
						%alive = 1;
						break;
					}
				}

				if (!%alive) {
					%this.zamb.end("\c5The survivors were overwhelmed.");
				}
			}
			else {
				%this.zamb.delete();
			}
		}
	}

	function miniGameSO::reset(%this, %client) {
		parent::reset(%this, %client);

		if (%this.owner == 0 && isObject(%this.zamb)) {
			%this.zamb.delete();

			if (%this.numMembers) {
				%this.zamb = ZAMB_Core();
			}
		}
	}

	function miniGameCanDamage(%a, %b) {
		%m1 = getMiniGameFromObject(%a);
		%m2 = getMiniGameFromObject(%b);

		if (!isObject(%m2)) {
			if (%b.getType() && $TypeMasks::PlayerObjectType) {
				if (%b.getDataBlock().isZombie) {
					%m2 = $defaultMiniGame;
				}
			}
		}

		if (%m1 != $defaultMiniGame || %m2 != $defaultMiniGame) {
			return parent::miniGameCanDamage(%a, %b);
		}

		return 1;

		%t1 = %a.getType() & $TypeMasks::PlayerObjectType;
		%t2 = %b.getType() & $TypeMasks::PlayerObjectType;

		if (!%t1 || !%t2) {
			return %parent;
		}

		return %t1 && %t2 ? 1 : %parent;
	}

	function GameConnection::onDeath(%this, %obj, %src, %type, %area) {
		parent::onDeath(%this, %obj, %src, %type, %area);
		%miniGame = %this.miniGame;

		if (isObject(%miniGame) && %miniGame !$= $defaultMiniGame) {
			return;
		}

		messageClient(%this, 'MsgYourSpawn');

		if (!isObject(%miniGame.zamb) || %miniGame.zamb.ended) {
			return;
		}

		for (%i = 0; %i < %miniGame.numMembers; %i++) {
			%player = %miniGame.member[%i].player;

			if (isObject(%player) && %player.getState() !$= "Dead") {
				%alive = 1;
				break;
			}
		}

		if (!%alive) {
			%miniGame.zamb.end("\c5The survivors were overwhelmed.");
		}
	}

	function GameConnection::setControlObject(%this, %obj) {
		parent::setControlObject(%this, %obj);

		%client = %obj.client;
		%miniGame = %client.miniGame;

		if (isObject(%miniGame) && %miniGame == $defaultMiniGame) {
			%client.updateZAMBVignette();
		}
	}

	function Observer::onTrigger(%this, %obj, %slot, %state) {
		%client = %obj.getControllingClient();

		if (!isObject(%client)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		%miniGame = %client.miniGame;

		if (!isObject(%miniGame) || %miniGame != $defaultMiniGame) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		if (!isObject(%miniGame.zamb) || %miniGame.zamb.ended) {
			return;
		}

		if (isObject(%client.player) && %client.player.getState() !$= "Dead") {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}
	}
};

activatePackage("zambPackage");
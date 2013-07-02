package zambPackage {
	function miniGameSO::addMember(%this, %client) {
		parent::addMember(%this, %client);

		if (%this.owner == 0 && !isObject(ZAMB) && %this.numMembers) {
			ZAMB();
		}
	}

	function miniGameSO::removeMember(%this, %client) {
		parent::removeMember(%this, %client);

		if (%this.owner == 0 && isObject(ZAMB) && !%this.numMembers) {
			ZAMB.delete();
		}
	}

	function miniGameSO::reset(%this, %client) {
		parent::addMember(%this, %client);

		if (%this.owner == 0 && isObject(ZAMB) && %this.numMembers) {
			ZAMB();
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

	function gameConnection::onDeath(%this, %obj, %src, %type, %area) {
		parent::onDeath(%this, %obj, %src, %type, %area);

		if (%this.miniGame !$= $defaultMiniGame) {
			return;
		}

		messageClient(%this, 'MsgYourSpawn');

		if (!isObject(ZAMB) || ZAMB.ended) {
			return;
		}

		%alive = 0;

		for (%i = 0; %i < %miniGame.numMembers; %i++) {
			%player = %miniGame.member[%i].player;

			if (isObject(%player) && %player.getState() !$= "Dead") {
				%alive = 1;
				break;
			}
		}

		if (!%alive) {
			ZAMB.end("\c5The survivors were overwhelmed.");
		}
	}

	function Observer::onTrigger(%this, %obj, %slot, %state) {
		%client = %obj.getControllingClient();

		if (!isObject(%client)) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		if (%client.miniGame != $defaultMiniGame) {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}

		if (!isObject(ZAMB) || ZAMB.ended) {
			return;
		}

		if (isObject(%client.player) && %client.player.getState() !$= "Dead") {
			return parent::onTrigger(%this, %obj, %slot, %state);
		}
	}
};

activatePackage("zambPackage");
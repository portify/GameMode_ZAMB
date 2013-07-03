package zambDamagePackage {
	function Armor::damage(%this, %obj, %src, %pos, %type) {
		%t1 = %this.isSurvivor ? 1 : (%this.isZombie ? 0 : -1);
		%t2 = %obj.isSurvivor ? 1 : (%obj.isZombie ? 0 : -1);

		if (%t1 >= 0 && %t2 >= 0 && %t1 != %t2) {
			$defaultMiniGame.lastCombatTime = $Sim::Time;
		}

		if (isObject(%src) && %src != %obj && %t1 == 1 && %t2 == 1) {
			switch ($defaultMiniGame.zamb.difficulty) {
				case 0: %damage = 0;
				case 1: %damage *= 0.1;
				case 2: %damage *= 0.5;
			}
		}

		parent::damage(%this, %obj, %src, %pos, %type);

		%client = %obj.client;
		%miniGame = %client.miniGame;

		if (isObject(%miniGame) && %miniGame == $defaultMiniGame) {
			%client.updateZAMBVignette();
		}
	}

	function Armor::onDisabled(%this, %obj) {
		parent::onDisabled(%this, %obj);

		%client = %obj.client;
		%miniGame = %client.miniGame;

		if (isObject(%miniGame) && %miniGame == $defaultMiniGame) {
			%client.updateZAMBVignette();
		}
	}

	function Armor::onNewDataBlock(%this, %obj) {
		parent::onNewDataBlock(%this, %obj);

		%client = %obj.client;
		%miniGame = %client.miniGame;

		if (isObject(%miniGame) && %miniGame == $defaultMiniGame) {
			%client.updateZAMBVignette();
		}
	}
};

activatePackage("zambDamagePackage");
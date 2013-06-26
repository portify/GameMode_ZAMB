package zambUsePackage {
	function player::activateStuff(%this) {
		%miniGame = getMiniGameFromObject(%this);

		if (!isObject(%miniGame) || %miniGame !$= $defaultMiniGame) {
			parent::activateStuff(%this);
			return;
		}

		if ($Sim::Time - %this.lastUseTime < 0.1) {
			return;
		}

		%this.lastUseTime = $Sim::Time;

		%start = %this.getEyePoint();
		%vector = %this.getEyeVector();

		%distance = 5;

		%ray = containerRayCast(%start,
			vectorAdd(%start, vectorScale(%vector, %distance)),
			$TypeMasks::All, %this
		);

		%col = firstWord(%ray);
		%use = isObject(%col);

		if (%use) {
			%use = %this.useObject(%col);
		}

		if (!%use && isObject(%this.client)) {
			%this.client.play2D(zamb_cannot_use);
		}
	}
};

activatePackage("zambUsePackage");

function player::useObject(%this, %obj) {
	return false;
}
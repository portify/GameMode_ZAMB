$skinColorRedMin = 0.1;
$skinColorRedMax = 0.6;

$skinColorGreenMin = 0.15;
$skinColorGreenMax = 0.75;

$skinColorBlueMin = 0.15;
$skinColorBlueMin = 0.60;

$skinColorSym = "gb";

function baseZombieData::applyZombieAppearance(%this, %obj) {
	%skin = getZombieSkinColor();

	%obj.setNodeColor("headSkin", %skin);
	%obj.setNodeColor("lhand", %skin);
	%obj.setNodeColor("rhand", %skin);

	%obj.setFaceName("asciiTerror");

	%hat = getRandom(0, 7);
	%gender = getRandom(0, 1);

	%obj.hideNode($Chest[0]);
	%obj.playThread(1, "armReadyBoth");

	if (%hat != 4 && %hat != 6 && %hat != 7) {
		%hat = 0;
	}

	for (%i = 0; %i < 7; %i++) {
		switch (%i) {
			case 0: %garment = $Hat[%hat];
			case 1: %garment = $Chest[%gender];
			case 2: %garment = $LArm[%gender];
			case 3: %garment = $RArm[%gender];
			case 4: %garment = "pants";
			case 5: %garment = "lshoe";
			case 6: %garment = "rshoe";
		}

		if (%garment $= "" || %garment $= "none") {
			continue;
		}

		if (%i == 0 || %i == 1 || %i == 4 || %i == 6 || getRandom() >= 0.5) {
			%r = getRandom(50, 125) / 255;
			%g = getRandom(50, 125) / 255;
			%b = getRandom(50, 125) / 255;

			%color = %r SPC %g SPC %b SPC "1";
		}
		else {
			%color = %skin;
		}

		%obj.unHideNode(%garment);
		%obj.setNodeColor(%garment, %color);
	}
}

function getZombieSkinColor()
{
	%redMin = $skinColorRedMin * 100;
	%redMax = $skinColorRedMax * 100;
	%greenMin = $skinColorGreenMin * 100;
	%greenMax = $skinColorGreenMax * 100;
	%blueMin = $skinColorblueMin * 100;
	%blueMax = $skinColorblueMax * 100;
	
	%red = getRandom(%redMin, %redMax) / 100;
	%blue = getRandom(%blueMin, %blueMax) / 100;
	%green = getRandom(%greenMin, %greenMax) / 100;

	if ($skinColorSym !$= "") {
		switch$ ($skinColorSym) {
			case "rg":
				%sym = (%red + %green) / 2;
				%red = %sym;
				%green = %sym;
			
			case "rb":
				%sym = (%red + %blue) / 2;
				%red = %sym;
				%blue = %sym;

			case "gb":
				%sym = (%green + %blue) / 2;
				%green = %sym;
				%blue = %sym;

			case "rgb":
				%sym = (%red + %green + %blue) / 3;
				%red = %sym;
				%green = %sym;
				%blue = %sym;
		}
	}

	return %red SPC %green SPC %blue SPC 1;
}
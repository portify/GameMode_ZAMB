$ZAMB::TickRate = 100;
$ZAMB::ThinkLimit = 5;
$ZAMB::ZombieLimit = 40;

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

function ZAMB_Director::onAdd(%this) {
	// Populate with a few starting wanderers.
	%count = getRandom(6, 10);

	for (%i = 0; %i < %count; %i++) {
		%this.zombies.spawn(zombieData);
	}
}

function Player::increaseIntensity(%this, %value, %temp) {
	%this.intensity = mClampF(%this.intensity + %value, 0, 1);

	if (!%temp) {
		%this.lastIntensityUpdateTime = $Sim::Time;
	}
}
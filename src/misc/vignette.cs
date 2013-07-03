function gameConnection::updateZAMBVignette(%this) {
	cancel(%this.updateZAMBVignette);

	%r = 0;
	%g = 0;
	%b = 0;
	%a = 1;

	%player = %this.player;

	if (isObject(%player) && %player.getState() !$= "Dead") {
		%damage = %player.getDamageLevel() / %player.getDataBlock().maxDamage;
		%boomer = $Sim::Time - %player.lastBoomerVictimTime;

		if (%damage > 0) {
			%r += %player.getDamageLevel() / %player.getDataBlock().maxDamage;
		}

		if (%boomer < 15) {
			%g += %boomer <= 10 ? 1 : 1 - (%boomer - 10) / 5;

			if (%boomer < 10) {
				%this.updateZAMBVignette = %this.schedule((10 - %boomer) * 1000, "updateZAMBVignette");
			}
			else {
				%this.updateZAMBVignette = %this.schedule(100, "updateZAMBVignette");
			}
		}
	}

	commandToClient(%this, 'SetVignette', 1, %r SPC %g SPC %b SPC %a);
}
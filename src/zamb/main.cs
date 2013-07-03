exec("./package.cs");

exec("./core.cs");
exec("./zombies.cs");

exec("./director/main.cs");
exec("./director/maintain.cs");

function MiniGameSO::zambWin(%this) {
	if (isObject(%this.zamb)) {
		%this.zamb.end("\c5The survivors won!");
	}
}

registerOutputEvent("MiniGame", "zambWin");
function zambDirector::onAdd(%this) {
	if (!isObject(%this.core)) {
		error("ERROR: ZAMB director created without 'core' attribute.");
	}

	%this.state = 0;
}
datablock shapeBaseImageData(zambClawLeft)
{
	shapeFile = $ZAMB::Path @ "res/models/claw_left.dts";
	mountPoint = 0;

	doColorShift = true;
	colorShiftColor = "0.541 0.698 0.552 1";

	rotation = "1 0 0 -60";
};

datablock shapeBaseImageData(zambClawRight)
{
	shapeFile = $ZAMB::Path @ "res/models/claw_right.dts";
	mountPoint = 1;

	doColorShift = true;
	colorShiftColor = "0.541 0.698 0.552 1";

	rotation = "1 0 0 -60";
};
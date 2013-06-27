datablock staticShapeData(cubeShapeData) {
	shapeFile = $ZAMB::Path @ "res/models/cube.dts";
};

function createCube(%a, %scale, %color) {
	if (%scale $= "") {
		%scale = "0.05 0.05 0.05";
	}

	if (%color $= "") {
		%color = "1 1 1 1";
	}

	%obj = new staticShape() {
		datablock = cubeShapeData;
		position = %a;
		scale = %scale;
	};

	missionCleanup.add(%obj);
	%obj.setNodeColor("ALL", %color);

	return %obj;
}

function createLine(%a, %b, %size, %color) {
	if (%size $= "") {
		%size = 0.05;
	}

	if (%color $= "") {
		%color = "1 1 1 1";
	}

	%offset = vectorSub(%a, %b);
	%normal = vectorNormalize(%offset);

	%xyz = vectorNormalize(vectorCross("1 0 0", %normal));
	%pow = mRadToDeg(mACos(vectorDot("1 0 0", %normal))) * -1;

	%obj = new staticShape() {
		datablock = cubeShapeData;
		scale = vectorLen(%offset) SPC %size SPC %size;

		position = vectorScale(vectorAdd(%a, %b), 0.5);
		rotation = %xyz SPC %pow;

		a = %a;
		b = %b;
	};

	missionCleanup.add(%obj);
	%obj.setNodeColor("ALL", %color);

	return %obj;
}
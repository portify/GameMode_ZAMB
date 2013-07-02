exec("./lib/vizard.cs");
exec("./lib/nodes.cs");

if (isFile("Add-Ons/GameMode_ZAMB/save.nav")) {
	// schedule(0, 0, "loadNodes", "Add-Ons/GameMode_ZAMB/save.nav");
}

function fixd() {
	NodeGroup.setDebug(0);
	%data = nameToID("cubeShapeData");

	for (%i = 0; %i < MissionGroup.getCount(); %i++) {
		%obj = MissionGroup.getObject(%i);

		if (%obj.getClassName() $= "StaticShape") {
			%obj.delete();
			%i--;
		}
	}

	NodeGroup.setDebug(1);
}

datablock itemData( nodeEditorItem : printGun )
{
	colorShiftColor = "1 0.75 0 1";

	uiName = "Node Editor";
	image = nodeEditorImage;
};

datablock shapeBaseImageData( nodeEditorImage : printGunImage )
{
	colorShiftColor = "1 0.75 0 1";
	item = nodeEditorItem;

	stateName[ 0 ] = "Ready";
	stateAllowImageChange[ 0 ] = true;
	stateTransitionOnTimeout[ 0 ] = "";
};

function nodeEditorImage::onMount( %this, %obj, %slot )
{
	if ( %slot != 0 || !isObject( %client = %obj.client ) )
	{
		%obj.unmountImage( %slot );
		return;
	}

	if ( !%client.isAdmin || !%client.isSuperAdmin )
	{
		%obj.unmountImage( %slot );
		%client.centerPrint( "\c6Only \c3Super Admins \c6can use the \c3Node Editor\c6.", 2 );

		return;
	}

	%obj.nodeEditorTick();
}

function nodeEditorImage::onUnMount( %this, %obj, %slot )
{
	cancel( %obj.nodeEditorTick );
	%obj.nodeEditorCleanup();
}

function player::nodeEditorTick( %this )
{
	cancel( %this.nodeEditorTick );

	if ( %this.getState() $= "Dead" )
	{
		return;
	}

	if ( %this.getMountedImage( 0 ) != nameToID( "nodeEditorImage" ) )
	{
		return;
	}

	if ( !isObject( %client = %this.client ) )
	{
		return;
	}

	if ( !%client.isAdmin || !%client.isSuperAdmin )
	{
		%this.unmountImage( 0 );
		%this.nodeEditorCleanup();

		%client.centerPrint( "\c6Only \c3Super Admins \c6can use the \c3Node Editor\c6.", 2 );
		return;
	}

	%start = %this.getEyePoint();
	%eye = %this.getEyeVector();

	%path = vectorScale( %eye, 100 );
	%stop = vectorAdd( %start, %path );

	%mask = $TypeMasks::TerrainObjectType | $TypeMasks::FxBrickAlwaysObjectType;
	%ray = containerRayCast( %start, %stop, %mask );

	if ( %ray !$= 0 )
	{
		%pos = getWords( %ray, 1, 3 );
		%pos = vectorSub( %pos, vectorScale( %eye, 0.25 ) );

		%start = vectorAdd( %pos, "0 0 0.25" );
		%stop = vectorSub( %pos, "0 0 100" );

		%ray = containerRayCast( %start, %stop, %mask );
	}

	%this.nodeEditorHover = "";

	if ( %ray !$= 0 )
	{
		%pos = getWords( %ray, 1, 3 );

		%px = getWord( %pos, 0 );
		%py = getWord( %pos, 1 );
		%pz = getWord( %pos, 2 );

		%px = mFloatLength( %px / 0.25, 0 ) * 0.25;
		%py = mFloatLength( %py / 0.25, 0 ) * 0.25;
		%pz = mFloatLength( %pz / 0.1, 0 ) * 0.1;

		%pos = %px SPC %py SPC %pz;

		if ( isObject( %group = nodeGroup ) )
		{
			%near = %group.findNearest( %pos, 1 );
		}

		%enableCube = !isObject( %near );
		%enableLine = isObject( %this.nodeEditorLink );

		%cubePoint = %pos;
		%linePoint = %pos;

		if ( isObject( %near ) )
		{
			%this.nodeEditorHover = %near;
			%linePoint = %near.position;
		}
	}

	%flash = 0.1 + ( mSin( $Sim::Time * 5 ) + 1 ) / 4;

	if ( %enableCube )
	{
		if ( isObject( %this.nodeEditorCube ) )
		{
			%this.nodeEditorCube.setTransform( %cubePoint SPC "0 0 0 1" );
			%this.nodeEditorCube.setNodeColor( "ALL", "1 1 1" SPC %flash );
		}
		else
		{
			%this.nodeEditorCube = createCube( %cubePoint, "0.5 0.5 0.4", "1 1 1" SPC %flash );
		}
	}
	else if ( isObject( %this.nodeEditorCube ) )
	{
		%this.nodeEditorCube.delete();
		%this.nodeEditorCube = "";
	}

	if ( isObject( %this.nodeEditorLink ) )
	{
		if ( isObject( %this.nodeEditorLine ) )
		{
			%this.nodeEditorLine.delete();
			%this.nodeEditorLine = "";
		}

		%this.nodeEditorLine = createLine( %this.nodeEditorLink.position, %linePoint, 0.05, "1 1 1" SPC %flash );
	}
	else if ( isObject( %this.nodeEditorLine ) )
	{
		%this.nodeEditorLine.delete();
		%this.nodeEditorLine = "";
	}

	if ( $Sim::Time - %this.lastNodeEditorMessage > 0.15 )
	{
		%this.lastNodeEditorMessage = $Sim::Time;
		%message = "<font:palatino linotype:24>";

		if ( isObject( %this.nodeEditorHover ) )
		{
			%message = %message @ "\c3Light\c6: Delete Node\n";
		}
		else if ( isObject( %this.nodeEditorCube ) )
		{
			%message = %message @ "\c3Light\c6: Create Node\n";
		}

		if ( isObject( %this.nodeEditorLink ) )
		{
			if ( isObject( %this.nodeEditorHover ) && %this.nodeEditorLink != %this.nodeEditorHover )
			{
				if ( %this.nodeEditorLink.isNeighbor[ %this.nodeEditorHover ] )
				{
					%message = %message @ "\c3Fire\c6: Destroy Link\n";
				}
				else
				{
					%message = %message @ "\c3Fire\c6: End Link\n";
				}
			}

			%message = %message @ "\c3Numpad 0\c6: Cancel Link\n";
		}
		else if ( isObject( %this.nodeEditorHover ) )
		{
			%message = %message @ "\c3Fire\c6: Start Link\n";
		}

		%client.bottomPrint( %message, 0.6, true );
	}

	%this.nodeEditorTick = %this.schedule( 50, "nodeEditorTick" );
}

function player::nodeEditorCleanup( %this )
{
	if ( isObject( %this.nodeEditorCube ) )
	{
		%this.nodeEditorCube.delete();
		%this.nodeEditorCube = "";
	}

	if ( isObject( %this.nodeEditorLine ) )
	{
		%this.nodeEditorLine.delete();
		%this.nodeEditorLine = "";
	}

	%this.nodeEditorLink = "";
	%this.nodeEditorHover = "";
}

function serverCmdNodeEditor( %client )
{
	if ( !%client.isAdmin || !%client.isSuperAdmin )
	{
		return;
	}

	if ( !isObject( %player = %client.player ) || %player.getState() $= "Dead" )
	{
		return;
	}

	%player.mountImage( nodeEditorImage, 0 );
	fixArmReady( %player );
}

package nodeEditorPackage
{
	function armor::onDisabled( %this, %obj )
	{
		if ( %obj.getMountedImage( 0 ) == nameToID( "nodeEditorImage" ) )
		{
			%obj.nodeEditorCleanup();
		}

		parent::onDisabled( %this, %obj );
	}

	function serverCmdLight( %client )
	{
		if ( !isObject( %player = %client.player ) || %player.getState() $= "Dead" )
		{
			parent::serverCmdLight( %client );
			return;
		}

		if ( %player.getMountedImage( 0 ) != nameToID( "nodeEditorImage" ) )
		{
			parent::serverCmdLight( %client );
			return;
		}

		if ( !%client.isAdmin || !%client.isSuperAdmin )
		{
			return;
		}

		if ( $Sim::Time - %player.lastNodeCreateDestroy < 0.5 )
		{
			return;
		}

		if ( isObject( %player.nodeEditorHover ) )
		{
			%player.nodeEditorHover.delete();
			%player.nodeEditorHover = "";

			%player.nodeEditorTick();
			%player.lastNodeCreateDestroy = $Sim::Time;
		}
		else if ( isObject( %player.nodeEditorCube ) )
		{
			nodeSO( %player.nodeEditorCube.position );

			%player.nodeEditorTick();
			%player.lastNodeCreateDestroy = $Sim::Time;
		}
	}

	function serverCmdCancelBrick( %client )
	{
		if ( !isObject( %player = %client.player ) || %player.getState() $= "Dead" )
		{
			parent::serverCmdCancelBrick( %client );
			return;
		}

		if ( %player.getMountedImage( 0 ) != nameToID( "nodeEditorImage" ) )
		{
			parent::serverCmdCancelBrick( %client );
			return;
		}

		if ( !%client.isAdmin || !%client.isSuperAdmin )
		{
			return;
		}

		if ( isObject( %player.nodeEditorLink ) )
		{
			%player.nodeEditorLink = "";
			%player.nodeEditorTick();
		}
	}

	function armor::onTrigger( %this, %obj, %slot, %val )
	{
		if ( %slot != 0 || !%val || !isObject( %client = %obj.client ) )
		{
			parent::onTrigger( %this, %obj, %slot, %val );
			return;
		}

		if ( %obj.getMountedImage( 0 ) != nameToID( "nodeEditorImage" ) )
		{
			parent::onTrigger( %this, %obj, %slot, %val );
			return;
		}

		if ( !%client.isAdmin || !%client.isSuperAdmin )
		{
			return;
		}

		if ( !isObject( %obj.nodeEditorHover ) )
		{
			return;
		}

		if ( isObject( %obj.nodeEditorLink ) )
		{
			if ( %obj.nodeEditorLink == %obj.nodeEditorHover )
			{
				return;
			}

			if ( %obj.nodeEditorLink.isNeighbor[ %obj.nodeEditorHover ] )
			{
				%obj.nodeEditorLink.removeNeighbor( %obj.nodeEditorHover );
			}
			else
			{
				%obj.nodeEditorLink.addNeighbor( %obj.nodeEditorHover );
			}

			%obj.nodeEditorLink = "";
			%obj.nodeEditorTick();
		}
		else
		{
			%obj.nodeEditorLink = %obj.nodeEditorHover;
			%obj.nodeEditorTick();
		}
	}
};

activatePackage( "nodeEditorPackage" );
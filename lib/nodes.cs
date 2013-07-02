function nodeSO( %position, %owner )
{
	if ( strLen( %owner ) && !isObject( %owner ) )
	{
		error( "ERROR: Invalid node owner." );
		return;
	}

	%obj = new scriptObject()
	{
		class = "nodeSO";

		position = %position;
		neighbors = 0;
	};

	if ( strLen( %owner ) )
	{
		if ( !isObject( %group = %owner.nodeGroup ) )
		{
			%group = %owner.nodeGroup = new scriptGroup()
			{
				class = "nodeSG";
				debug = true;
			};

			missionCleanup.add( %group );
		}
	}
	else
	{
		if ( !isObject( %group = nodeGroup ) )
		{
			%group = new scriptGroup( nodeGroup )
			{
				class = "nodeSG";
				debug = true;
			};

			missionCleanup.add( %group );
		}
	}
	
	%group.add( %obj );

	if ( %group.debug )
	{
		%obj.createDebugObjects();
	}

	return %obj;
}

function nodeSO::onRemove( %this )
{
	%this.destroyDebugObjects();

	for ( %i = %this.neighbors - 1 ; %i >= 0 ; %i-- )
	{
		if ( !isObject( %this.neighbor[ %i ] ) )
		{
			continue;
		}

		%this.removeNeighbor( %this.neighbor[ %i ] );
	}
}

function nodeSO::addNeighbor( %this, %node )
{
	if ( %node.class !$= "nodeSO" )
	{
		error( "ERROR: Invalid node." );
		return;
	}

	if ( ( %group = %this.getGroup() ) != %node.getGroup() )
	{
		error( "ERROR: Node belongs to another group." );
		return;
	}

	%node = %node.getID();

	if ( %this == %node )
	{
		error( "ERROR: Cannot connect a node to itself." );
		return;
	}

	if ( !%this.isNeighbor[ %node ] )
	{
		%this.neighbor[ %this.neighbors ] = %node;
		%this.neighbors++;

		%this.isNeighbor[ %node ] = true;

		if ( %group.debug )
		{
			%this.createDebugObjects();
		}
	}

	if ( !%node.isNeighbor[ %this ] )
	{
		%node.addNeighbor( %this );
	}
}

function nodeSO::removeNeighbor( %this, %node )
{
	if ( %node.class !$= "nodeSO" )
	{
		error( "ERROR: Invalid node." );
		return;
	}

	%node = %node.getID();

	if ( %this.isNeighbor[ %node ] )
	{
		for ( %i = 0 ; %i < %this.neighbors ; %i++ )
		{
			if ( %found )
			{
				%this.neighbor[ %i ] = %this.neighbors[ %i + 1 ];
			}
			else if ( %this.neighbor[ %i ] == %node )
			{
				%found = true;
				%i--;

				continue;
			}
		}

		if ( %found )
		{
			%this.neighbors--;
			%this.neighbor[ %this.neighbors ] = "";
		}

		%this.isNeighbor[ %node ] = "";

		if ( isObject( %this.debugLine[ %node ] ) )
		{
			%this.debugLine[ %node ].delete();
			%this.debugLine[ %node ] = "";
		}
	}

	if ( %node.isNeighbor[ %this ] )
	{
		%node.removeNeighbor( %this );
	}
}

function nodeSO::createDebugObjects( %this )
{
	%color = "0 0.75 0";

	if ( !isObject( %this.debugCube ) )
	{
		%this.debugCube = createCube( %this.position, "0.5 0.5 0.4", %color SPC 1 );
	}

	%line = vectorScale( %color, 0.5 );
	%line = vectorAdd( %line, "0.5 0.5 0.5" );

	for ( %i = 0 ; %i < %this.neighbors ; %i++ )
	{
		%neighbor = %this.neighbor[ %i ];

		if ( !isObject( %neighbor ) )
		{
			continue;
		}

		if ( !isObject( %this.debugLine[ %neighbor ] ) )
		{
			if ( isObject( %neighbor.debugLine[ %this ] ) )
			{
				%this.debugLine[ %neighbor ] = %neighbor.debugLine[ %this ];
				continue;
			}

			%a = %this.position;
			%b = %neighbor.position;

			%this.debugLine[ %neighbor ] = createLine( %a, %b, 0.005, %line SPC 1 );
			%neighbor.debugLine[ %this ] = %this.debugLine[ %neighbor ];
		}
	}
}

function nodeSO::destroyDebugObjects( %this )
{
	if ( isObject( %this.debugCube ) )
	{
		%this.debugCube.delete();
		%this.debugCube = "";
	}

	for ( %i = 0 ; %i < %this.neighbors ; %i++ )
	{
		%neighbor = %this.neighbor[ %i ];

		if ( isObject( %this.debugLine[ %neighbor ] ) )
		{
			%this.debugLine[ %neighbor ].delete();

			%this.debugLine[ %neighbor ] = "";
			%neighbor.debugLine[ %this ] = "";
		}
	}
}

function nodeSG::setDebug( %this, %bool )
{
	%this.debug = %bool ? true : false;
	%count = %this.getCount();

	for ( %i = 0 ; %i < %count ; %i++ )
	{
		%node = %this.getObject( %i );

		if ( %bool )
		{
			%node.createDebugObjects();
		}
		else
		{
			%node.destroyDebugObjects();
		}
	}
}

function nodeSG::findNearest( %this, %position, %range, %visible )
{
	%node = -1;
	%best = 256;

	%count = %this.getCount();
	%start = vectorAdd( %position, "0 0 2" );

	for ( %i = 0 ; %i < %count ; %i++ )
	{
		%test = %this.getObject( %i );
		%dist = vectorDist( %position, %test.position );

		if ( %dist >= %range || %dist >= %best )
		{
			continue;
		}

		if ( %visible )
		{
			%end = vectorAdd( %test.position, "0 0 2" );
			%ray = containerRayCast( %position, %end, $TypeMasks::FxBrickAlwaysObjectType );

			if ( %ray !$= "0" )
			{
				continue;
			}
		}

		%best = %dist;
		%node = %test;
	}

	return %node;
}

function loadNodes( %file, %debug )
{
	%mode = 0;
	%nodes = 0;

	if ( !isFile( %file ) )
	{
		error( "ERROR: Cannot find file." );
		return;
	}

	%fo = new fileObject();
	%fo.openForRead( %file );

	if ( isObject( nodeGroup ) )
	{
		nodeGroup.delete();
	}

	%group = new scriptGroup( nodeGroup )
	{
		class = "nodeSG";
		debug = false;
	};

	missionCleanup.add( %group );

	while ( !%fo.isEOF() )
	{
		%line = %fo.readLine();

		if ( %line $= "neighbors" )
		{
			%mode = 1;
			continue;
		}

		if ( %mode == 0 )
		{
			%node[ %nodes ] = nodeSO( %line );
			%nodes++;
		}
		else if ( %mode == 1 )
		{
			%node[ getWord( %line, 0 ) ].addNeighbor( %node[ getWord( %line, 1 ) ] );
		}
	}

	%fo.close();
	%fo.delete();

	if ( %debug )
	{
		NodeGroup.setDebug( %debug );
	}
}

function saveNodes( %file )
{
	if ( !isWriteableFileName( %file ) )
	{
		error( "ERROR: Cannot write to file." );
		return;
	}

	%fo = new fileObject();
	%fo.openForWrite( %file );

	%count = nodeGroup.getCount();

	for ( %i = 0 ; %i < %count ; %i++ )
	{
		%node = nodeGroup.getObject( %i );
		%id[ %node ] = %i;

		%fo.writeLine( %node.position );
	}

	%fo.writeLine( "neighbors" );

	for ( %i = 0 ; %i < %count ; %i++ )
	{
		%node = nodeGroup.getObject( %i );

		for ( %j = 0 ; %j < %node.neighbors ; %j++ )
		{
			%neighbor = %node.neighbor[ %j ];

			if ( !isObject( %neighbor ) || %rel[ %node, %neighbor ] || %rel[ %neighbor, %node ] )
			{
				continue;
			}

			%rel[ %node, %neighbor ] = true;
			%fo.writeLine( %id[ %node ] SPC %id[ %neighbor ] );
		}
	}

	%fo.close();
	%fo.delete();
}
function findPath( %a, %b, %callback )
{
	if ( !isObject( %a ) || !isObject( %b ) )
	{
		error( "ERROR: Invalid targets." );
		return;
	}

	%a = %a.getID();
	%b = %b.getID();

	%obj = new scriptObject()
	{
		class = "pathSO";
		callback = %callback;

		done = false;
		result = "";

		a = %a;
		b = %b;
	};

	%obj.opened = %a;
	%obj.score[ %a ] = pathDistance( %a, %b );

	%obj.schedule( 0, "tick" );
	return %obj;
}

function pathSO::tick( %this )
{
	cancel( %this.tick );

	if ( %this.done )
	{
		return;
	}

	if ( !strLen( %this.opened ) )
	{
		%this.end( "error" );
		return;
	}

	%length = getWordCount( %this.opened );

	for ( %i = 0 ; %i < %length ; %i++ )
	{
		%test = getWord( %this.opened, %i );

		if ( %this.score[ %test ] < %best || !strLen( %best ) )
		{
			%best = %this.score[ %test ];
			%node = %test;
		}
	}

	if ( !isObject( %node ) )
	{
		%this.end( "error" );
		return;
	}

	%this.run( %node );
	%this.tick = %this.schedule( 0, "tick" );
}

function pathSO::run( %this, %node )
{
	if ( %node == %this.b )
	{
		while ( strLen( %parent = %this.from[ %node ] ) )
		{
			%path = trim( %node SPC %path );
			%node = %parent;
		}

		%this.end( trim( %this.a SPC %path ) );
	}

	%this.closed = listAppend( %this.closed, %node );
	%this.opened = listRemove( %this.opened, %node );

	for ( %i = 0 ; %i < %node.neighbors ; %i++ )
	{
		%neighbor = %node.neighbor[ %i ];

		if ( !listContains( %this.closed, %neighbor ) )
		{
			%this.score[ %neighbor ] = pathDistance( %neighbor, %this.b );
			%this.from[ %neighbor ] = %node;

			if ( !listContains( %this.opened, %neighbor ) )
			{
				%this.opened = listAppend( %this.opened, %neighbor );
			}
		}
	}
}

function pathSO::end( %this, %result )
{
	if ( !%this.done )
	{
		%this.done = true;
		%this.result = %result;

		if ( isFunction( %this.callback ) )
		{
			call( %this.callback, %this, %result );
		}
	}
}

function findPathBlocking( %a, %b )
{
	if ( !isObject( %a ) || !isObject( %b ) )
	{
		error( "ERROR: Invalid targets." );
		return;
	}

	%a = %a.getID();
	%b = %b.getID();

	%opened = %a;
	%score[ %a ] = pathDistance( %a, %b );

	while ( strLen( %opened ) && %length = getWordCount( %opened ) )
	{
		for ( %i = 0 ; %i < %length ; %i++ )
		{
			%test = getWord( %opened, %i );

			if ( %score[ %test ] < %best || !strLen( %best ) )
			{
				%best = %score[ %test ];
				%node = %test;
			}
		}

		if ( %node == %b )
		{
			while ( strLen( %parent = %from[ %node ] ) )
			{
				%path = trim( %node SPC %path );
				%node = %parent;
			}

			return trim( %a SPC %path );
		}

		%closed = listAppend( %closed, %node );
		%opened = listRemove( %opened, %node );

		for ( %i = 0 ; %i < %node.neighbors ; %i++ )
		{
			%neighbor = %node.neighbor[ %i ];

			if ( !listContains( %closed, %neighbor ) )
			{
				%score[ %neighbor ] = pathDistance( %neighbor, %b );
				%from[ %neighbor ] = %node;

				if ( !listContains( %this.opened, %neighbor ) )
				{
					%opened = listAppend( %opened, %neighbor );
				}
			}
		}
	}

	return "error";
}

function pathDistance( %a, %b )
{
	return vectorDist( %a.position, %b.position );
}

function listAppend( %list, %item )
{
	return trim( %list SPC %item );
}

function listContains( %list, %item )
{
	return strPos( " " @ %list @ " ", " " @ %item @ " " ) >= 0;
}

function listRemove( %list, %item )
{
	return trim( strReplace( " " @ %list @ " ", " " @ %item @ " ", " " ) );
}
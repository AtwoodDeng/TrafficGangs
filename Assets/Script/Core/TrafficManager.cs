using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficManager : MBehavior {

	static public TrafficManager Instance{
		get { return m_Instance; }
		set { if ( m_Instance == null ) m_Instance = value ; }
	}
	static private TrafficManager m_Instance;
	public TrafficManager() { Instance = this; }

	public CarSpawner[] targets;
	public Location[] locations;

	protected override void MStart () {
		base.MStart ();

		targets = GetComponentsInChildren<CarSpawner>();

		foreach( CarSpawner cs in targets )
			cs.SetTarget( targets );

		locations = GetComponentsInChildren<Location>();
	}

	/// <summary>
	/// Get the next location according to the route
	/// </summary>
	/// <returns>The next location.</returns>
	/// <param name="tem">Tem.</param>
	/// <param name="target">Target.</param>
	public Location GetNextLocation( Location tem , Location target ) {
		Location[] route = GetRoute( tem , target ); 

		if ( route != null && route.Length > 1 )
			return route[1];

		return null;
	}

	/// <summary>
	/// Get the location list from the source to target
	/// result starts with source ( index = 0 )
	/// and ends with target
	/// </summary>
	/// <returns>The route.</returns>
	/// <param name="source">Source location.</param>
	/// <param name="target">Target location.</param>
	public Location[] GetRoute( Location source , Location target ) {
		if ( source == null || target == null )
			return null;
		
		Dictionary<Location,float> dist = new Dictionary<Location, float>(); // distance from source to location
		Dictionary<Location,Location> prev = new Dictionary<Location, Location>(); // previous location
		List<Location> unvisited = new List<Location>();

		// set up the map
		foreach( Location l in locations ) {
			dist[l] = Mathf.Infinity;
			prev[l] = null;
			unvisited.Add( l );
		}

		dist[source] = 0;

		// search for the nearest
		while( unvisited.Count > 0 ) {

			Location uLocation = null; //Assign the location with Least wait time to u
			{
				float minDistance = Mathf.Infinity;
				foreach( Location l in unvisited )
				{
					if ( dist[l] < minDistance )
					{
						minDistance = dist[l];
						uLocation = l;
					}
				}
			}
			unvisited.Remove( uLocation );

			foreach( Road r in uLocation.GetRoads())
			{
				float WaittingTime = dist[uLocation];
				WaittingTime += r.GetWaittingTime() + r.Target.GetWaittingTime(r);
				if ( WaittingTime < dist[r.Target] )
				{
					dist[r.Target] = WaittingTime;
					prev[r.Target] = uLocation;
				}
			}
		}

		// make the route from the search result
		List<Location> route = new List<Location>();
		Location vLocation = target;
		route.Add( vLocation);
		while( prev[vLocation] != null )
		{
			route.Insert( 0 , prev[vLocation]);
			vLocation = prev[vLocation];
		}

		return route.ToArray();
	}

}


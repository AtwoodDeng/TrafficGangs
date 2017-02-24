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
	public int MaxCarNumber = 50;

	public CarSpawner[] targets;
	public Location[] locations;
	public List<Car> carList = new List<Car>();
	public float AverageWaittingRate
	{
		get {
			float totalRate = 0;
			foreach (Car c in carList)
				totalRate += c.WaittingRate;
			return totalRate / carList.Count;
		}
	}

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
	public Location GetNextLocation( Location tem , Location target , Road fromRoad ) {
		Location[] route = GetRoute( tem , target , fromRoad); 

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
	public Location[] GetRoute( Location source , Location target , Road fromRoad ) {
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

			// search from uLocation
			foreach( Road r in uLocation.GetRoads())
			{
				float WaittingTime = dist[uLocation];
				WaittingTime += r.GetWaittingTime();
				if ( prev[uLocation] == null )
					WaittingTime += r.Original.GetWaittingTimeFromToRoad( fromRoad , r );
				else {
					Road fRoad = prev[uLocation].GetRoadToward( uLocation );
					if ( fRoad != null )
						WaittingTime += r.Original.GetWaittingTimeFromToRoad( fRoad , r );
				}
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

	public static void RegisterCar( Car car )
	{
		Instance.carList.Add (car);
	}

	public static void UnregisterCar( Car car )
	{
		Instance.carList.Remove (car);
	}

	void OnGUI()
	{
		GUILayout.Label ("Average Wait Time : " + (AverageWaittingRate * 100f ) + "%" );
	}

	public static bool IsCarMaximum()
	{
		if ( Instance == null )
			return true;
		return Instance.carList.Count >= Instance.MaxCarNumber;
	}
}


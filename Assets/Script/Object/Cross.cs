using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class Cross : Location {

	[SerializeField] float PassCycle = 2f;
	[SerializeField] float PassRate = 0.5f;
	[SerializeField] float yellowRate = 0.1f;
//	[SerializeField] LimitedTurnDirection turnDirectLimitation = LimitedTurnDirection.All;
	[SerializeField] LimitedTurnDirection moveLimitFromNorth = LimitedTurnDirection.All;
	[SerializeField] LimitedTurnDirection moveLimitFromSouth = LimitedTurnDirection.All;
	[SerializeField] LimitedTurnDirection moveLimitFromWest = LimitedTurnDirection.All;
	[SerializeField] LimitedTurnDirection moveLimitFromEast = LimitedTurnDirection.All;
	[SerializeField] bool ResetNorthSouth = false;
	[SerializeField] bool ResetEastWest = false;
	[SerializeField] TrafficLight trafficLightScript;

	/// <summary>
	/// Get the limit direction according to the type of the road
	/// the limit direction is setted by where the road is from
	/// so it is oppose to the road type of the road
	/// </summary>
	/// <returns>The turn limitaion.</returns>
	/// <param name="fromRoad">From road.</param>
	public LimitedTurnDirection GetTurnLimitaion( RoadType fromRoad )
	{
		
		if ( fromRoad == RoadType.North )
			return moveLimitFromSouth;
		if ( fromRoad == RoadType.South )
			return moveLimitFromNorth;
		if ( fromRoad == RoadType.West )
			return moveLimitFromEast;
		if ( fromRoad == RoadType.East )
			return moveLimitFromWest;
		return LimitedTurnDirection.All;
	}
	private float timer = 0;
	public float TimeRateInCycle
	{
		get {
			float realTime = Mathf.Repeat( timer , PassCycle );
			return realTime / PassCycle;
		}
	}
	List<Car> waittingCar = new List<Car>();

	protected override void MAwake ()
	{
		base.MAwake ();
		timer = 0;
	}
		
	protected override void MUpdate ()
	{
		base.MUpdate ();
		timer += Time.deltaTime;
		UpdateWaittingCar();

		// reset the cross to let north south road be passiable
		if ( ResetNorthSouth )
		{
			ResetNorthSouth = false;
			ResetToPass( RoadType.North );
		}
		// reset the cross to let east west road be passiable
		if ( ResetEastWest )
		{
			ResetEastWest = false;
			ResetToPass( RoadType.East );
		}
	}

	bool isPassing = false;
	virtual public void UpdateWaittingCar()
	{

		// pass the first priority cars
		for( int i = 0 ; i < waittingCar.Count ; ++ i )
		{
			if ( waittingCar[i].IsWaitting )
			{
				if ( waittingCar[i].IsIgnoreTrafficLight() )
				{
					PassCar( waittingCar[i] );
					waittingCar.Remove (waittingCar[i]);
					return;
				}
			}
		}

		if ( !isPassing )
		{

			// pass the straight cars
			for( int i = 0 ; i < waittingCar.Count ; ++ i )
			{
				if ( waittingCar[i].IsWaitting )
				{
					if (  IsPassiable( waittingCar[i].GetTemRoad() , waittingCar[i].GetNextRoad() ) )
//						&& IsStraight( waittingCar[i].GetTemRoad().type , waittingCar[i].GetNextRoad().type ))
					{
						PassCar( waittingCar[i] );
						waittingCar.Remove (waittingCar[i]);
						return;
					}
				}
			}
//			// pass turn left cars
//			for( int i = 0 ; i < waittingCar.Count ; ++ i )
//			{
//				if ( waittingCar[i].IsWaitting )
//				{
//					if ( IsPassiable( waittingCar[i].GetTemRoad() , waittingCar[i].GetNextRoad() )
//						&& IsLeftTurn( waittingCar[i].GetTemRoad().type , waittingCar[i].GetNextRoad().type ))
//					{
//						PassCar( waittingCar[i] );
//						waittingCar.Remove (waittingCar[i]);
//						return;
//
//					}
//				}
//			}
//			// pass turn right cars
//			for( int i = 0 ; i < waittingCar.Count ; ++ i )
//			{
//				if ( waittingCar[i].IsWaitting )
//				{
//					if ( IsPassiable( waittingCar[i].GetTemRoad() , waittingCar[i].GetNextRoad() )
//						&& IsRightTurn( waittingCar[i].GetTemRoad().type , waittingCar[i].GetNextRoad().type ))
//					{
//						PassCar( waittingCar[i] );
//						waittingCar.Remove (waittingCar[i]);
//						return;
//
//					}
//				}
//			}

		}
	}

	public void PassCar( Car c )
	{
		c.WaitToPass();
		isPassing = true;
		c.CrossMoveTo( c.GetNextRoad().GetStartPosition() , delegate {
			c.PassToMoveForward();
			isPassing = false;
		});
	}

	public override bool IsPassiable ( Road fromRoad , Road toRoad , float timeRate = -1f)
	{
//		Debug.Log ("This " + name + " from " + fromRoad.Target.name + " to " + toRoad.Original.name);
		Assert.AreEqual<Location> (this, fromRoad.Target);
		Assert.AreEqual<Location> (this, toRoad.Original);

//		Debug.Log(" Road Type " + r.type + " " + TimeRateInCycle + " " + PassRate );
		if ( IsPassing( toRoad.type , timeRate ) )
		{
			if ( GetTurnLimitaion( fromRoad.type ) == LimitedTurnDirection.All )
				return true;
			if ( IsRightTurn (fromRoad.type, toRoad.type) && GetTurnLimitaion( fromRoad.type ) == LimitedTurnDirection.Right )
				return true;
			if ( IsLeftTurn (fromRoad.type, toRoad.type) && GetTurnLimitaion( fromRoad.type ) == LimitedTurnDirection.Left )
				return true;
			if ( IsStraight (fromRoad.type, toRoad.type) && GetTurnLimitaion( fromRoad.type ) == LimitedTurnDirection.Straight )
				return true;
		}

		return false;
	}

	public bool IsRightTurn( RoadType fromRoad , RoadType toRoad )
	{
		if (fromRoad == RoadType.South && toRoad == RoadType.West)
			return true;
		if (fromRoad == RoadType.West && toRoad == RoadType.North)
			return true;
		if (fromRoad == RoadType.North && toRoad == RoadType.East)
			return true;
		if (fromRoad == RoadType.East && toRoad == RoadType.South)
			return true;
		return false;
	}

	public bool IsLeftTurn( RoadType fromRoad , RoadType toRoad )
	{
		if (toRoad == RoadType.South && fromRoad == RoadType.West)
			return true;
		if (toRoad == RoadType.West && fromRoad == RoadType.North)
			return true;
		if (toRoad == RoadType.North && fromRoad == RoadType.East)
			return true;
		if (toRoad == RoadType.East && fromRoad == RoadType.South)
			return true;
		return false;
	}

	public bool IsStraight( RoadType fromRoad , RoadType toRoad )
	{
		if (toRoad == RoadType.South && fromRoad == RoadType.South)
			return true;
		if (toRoad == RoadType.West && fromRoad == RoadType.West)
			return true;
		if (toRoad == RoadType.North && fromRoad == RoadType.North)
			return true;
		if (toRoad == RoadType.East && fromRoad == RoadType.East)
			return true;
		return false;
	}

	public bool IsTurnBack( RoadType fromRoad , RoadType toRoad )
	{
		if (toRoad == RoadType.South && fromRoad == RoadType.North)
			return true;
		if (toRoad == RoadType.West && fromRoad == RoadType.East)
			return true;
		if (toRoad == RoadType.North && fromRoad == RoadType.South)
			return true;
		if (toRoad == RoadType.East && fromRoad == RoadType.West)
			return true;
		return false;
	}

	public bool IsPassing( RoadType type , float timeRate = -1f )
	{
		if (timeRate < 0 || timeRate > 1f )
			timeRate = TimeRateInCycle;
		switch( type ) {
		case RoadType.North:
		case RoadType.South:
			if ( timeRate < PassRate - yellowRate  && timeRate > yellowRate)
				return true;
			else
				return false;
			break;
		case RoadType.West:
		case RoadType.East:
			if ( timeRate > PassRate + yellowRate && timeRate < 1f - yellowRate )
				return true;
			else
				return false;
			break;
		default:
			break;
		};
		return false;
	}

	/// <summary>
	/// Reset the road to make the road of type can pass
	/// </summary>
	/// <returns>The to pass.</returns>
	/// <param name="type">Type.</param>
	public void ResetToPass( RoadType type )
	{
		if ( type == RoadType.North || type == RoadType.South )
		{
			timer = 0;
			trafficLightScript.isPassingNS = true;

		}
		if ( type == RoadType.West || type == RoadType.East )
		{
			timer = PassRate * PassCycle;
			trafficLightScript.isPassingNS = false;
		}
	}

	public float GetTimeToWait( RoadType type )	{
		if ( IsPassing( type ))
			return 0;
		if ( type == RoadType.North || type == RoadType.South ) {
			return Mathf.Repeat( - TimeRateInCycle , 1f) * PassCycle;
		}
		if ( type == RoadType.West || type == RoadType.East ) {
			return Mathf.Repeat( PassRate - TimeRateInCycle , 1f) * PassCycle;
		}
		return PassCycle * 0.5f;
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.Lerp( Color.red , Color.yellow , 0.5f ) ;
		Gizmos.DrawWireCube( transform.position , Vector3.one * Width  );

		Gizmos.color = Color.cyan;

		foreach( Road r in roads )
		{
			if (r.type == RoadType.North)
				Gizmos.color = Color.yellow;
			else if (r.type == RoadType.South)
				Gizmos.color = Color.red;
			else if (r.type == RoadType.East)
				Gizmos.color = Color.blue;
			else
				Gizmos.color = Color.cyan;

			Gizmos.DrawLine( r.GetStartPosition() , r.GetEndPosition() );
		}

		// show the passing situation
		Gizmos.color = Color.green;
		if ( IsPassing( RoadType.North ) || IsPassing( RoadType.South ) )
		{
			Gizmos.DrawLine( transform.position + transform.forward * Width / 2f 
				, transform.position - transform.forward * Width / 2f );
			trafficLightScript.DisplayPassingNS();
		} 
		else if ( IsPassing( RoadType.East) || IsPassing( RoadType.West) ) 
		{
			Gizmos.DrawLine( transform.position + transform.right * Width / 2f 
				, transform.position - transform.right * Width / 2f );
			trafficLightScript.DisplayPassingWE();
		}
		else 
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine( transform.position + transform.forward * Width / 2f + transform.right * Width / 2f ,
				transform.position - transform.forward * Width / 2f - transform.right * Width / 2f);
			Gizmos.DrawLine( transform.position - transform.forward * Width / 2f + transform.right * Width / 2f ,
				transform.position + transform.forward * Width / 2f - transform.right * Width / 2f );
		}
	}

	public override float GetWaittingTimeFromRoad (Road fromRoad)
	{
		if ( fromRoad.type == RoadType.North || fromRoad.type == RoadType.South )
		{
			return PassRate * PassCycle;
		}

		return ( 1f - PassRate ) * PassCycle;
	}

	public override float GetWaittingTimeToRoad (Road toRoad)
	{
		return GetTimeToWait( toRoad.type );
	}


	public override float GetWaittingTimeFromToRoad( Road fromRoad , Road toRoad )
	{
		if ( GetTurnLimitaion( fromRoad.type ) == LimitedTurnDirection.Left ) {
			if ( !IsLeftTurn( fromRoad.type , toRoad.type ) )
				return 99999f;
		}else if ( GetTurnLimitaion( fromRoad.type ) == LimitedTurnDirection.Right ) {
			if ( !IsRightTurn( fromRoad.type , toRoad.type ) )
				return 99999f;
		}
		else if ( GetTurnLimitaion( fromRoad.type ) == LimitedTurnDirection.Straight ) {
			if ( !IsStraight( fromRoad.type , toRoad.type ) ) {
//				Debug.Log("999");
				return 99999f;
			}
		}

		return GetWaittingTimeToRoad( toRoad );
	}

	public override void OnArrive (Car car)
	{
		base.OnArrive (car);
		waittingCar.Add( car );
//		Debug.Log("Add Waitting Car " + car );
	}

	public override void OnLeave (Car car)
	{
		base.OnLeave (car);
	}

	public override Road GetNeastestPassible ( Road fromRoad )
	{
		float timeRate = TimeRateInCycle;
		List<Road> newRoads = new List<Road> (roads);
		newRoads.Sort (delegate(Road x, Road y) {
			return Random.Range(0,2);	
		});
		int count = 0;
		while (true) {
			foreach (Road r in newRoads) {
				if (IsPassiable (fromRoad, r , timeRate) && fromRoad.Original != r.Target ) {
					return r;
				}
			}
			timeRate = Mathf.Repeat (timeRate + 0.05f, 1f);
			count++;
			if (count > 100)
				break;
		}
		return newRoads [0];
	}
		
	public void AllowPassingNS()
	{
		ResetToPass(RoadType.North);
	}
	public void AllowPassingWE()
	{
		ResetToPass(RoadType.East);
	}

	public void SetNorthDirection(LimitedTurnDirection direction)
	{
		moveLimitFromNorth = direction;
	}


	public void SetSouthDirection(LimitedTurnDirection direction)
	{
		moveLimitFromSouth = direction;
	}

	public void SetEastDirection(LimitedTurnDirection direction)
	{
		moveLimitFromEast = direction;
	}

	public void SetWestDirection(LimitedTurnDirection direction)
	{
		moveLimitFromWest = direction;
	}




}
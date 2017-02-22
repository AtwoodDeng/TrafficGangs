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
	[SerializeField] LimitedTurnDirection turnDirectLimitation = LimitedTurnDirection.Both;
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
	}

	bool isPassing = false;
	virtual public void UpdateWaittingCar()
	{
		if ( !isPassing )
		{
			foreach( Car c in waittingCar )
			{
				if ( c.IsWaitting )
				{
//					Debug.Log ("=== car " + c.name + " === ");
					if ( IsPassiable( c.GetTemRoad() , c.GetNextRoad() )) {
						PassCar( c );
						waittingCar.Remove (c);
						break;
					}
				}
			}
		}
	}

	public void PassCar( Car c )
	{
		Debug.Log("Set to Pass " + c );
		c.WaitToPass();
		isPassing = true;
		c.CrossMoveTo( c.GetNextRoad().GetStartPosition() , delegate {
			c.PassToMoveForward();
			isPassing = false;
		});

//		c.PassToMoveForward();
	}

	public override bool IsPassiable ( Road fromRoad , Road toRoad , float timeRate = -1f)
	{
//		Debug.Log ("This " + name + " from " + fromRoad.Target.name + " to " + toRoad.Original.name);
		Assert.AreEqual<Location> (this, fromRoad.Target);
		Assert.AreEqual<Location> (this, toRoad.Original);

//		Debug.Log(" Road Type " + r.type + " " + TimeRateInCycle + " " + PassRate );
		if ( IsPassing( toRoad.type , timeRate ) )
		{
			if ( turnDirectLimitation == LimitedTurnDirection.Both )
				return true;
			if (IsRightTurn (fromRoad.type, toRoad.type) && turnDirectLimitation == LimitedTurnDirection.Right)
				return true;
			if (IsLeftTurn (fromRoad.type, toRoad.type) && turnDirectLimitation == LimitedTurnDirection.Left)
				return true;
		}

		return false;
	}

	public bool IsLeftTurn( RoadType fromRoad , RoadType toRoad )
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

	public bool IsRightTurn( RoadType fromRoad , RoadType toRoad )
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

	public bool IsPassing( RoadType type , float timeRate = -1f )
	{
		if (timeRate < 0)
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
		} else if ( IsPassing( RoadType.East) || IsPassing( RoadType.West) ) {
			Gizmos.DrawLine( transform.position + transform.right * Width / 2f 
				, transform.position - transform.right * Width / 2f );
		}else {
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
		if (IsPassing (toRoad.type))
			return 0;

		return GetWaittingTimeFromRoad (toRoad);
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
				if (IsPassiable (fromRoad, r , timeRate)) {
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
}
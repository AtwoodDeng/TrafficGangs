using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[ExecuteInEditMode]
public class Cross : Location {

	[SerializeField] float PassCycle = 2f;
	[SerializeField] float PassRate = 0.5f;
	[SerializeField] float yellowRate = 0.1f;
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
					if ( IsPassiable( c.GetNextRoad() )) {
						PassCar( c );
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

	public override bool IsPassiable (Road r)
	{
		//		Debug.Log(" road from " + r.Original.name + " to " + r.Target.name );
		if ( r.Original != this )
			return true;

//		Debug.Log(" Road Type " + r.type + " " + TimeRateInCycle + " " + PassRate );
		if ( IsPassing( r.type ) )
		{
			return true;
		}

		return false;
	}

	public bool IsPassing( RoadType type )
	{
		switch( type ) {
		case RoadType.NorthSouth:
			if ( TimeRateInCycle < PassRate - yellowRate )
				return true;
			else
				return false;
			break;
		case RoadType.WestEast:
			if ( TimeRateInCycle > PassRate + yellowRate )
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
			if ( r.type == RoadType.NorthSouth )
				Gizmos.color = Color.Lerp( Color.white , Color.magenta , 0.7f );
			else
				Gizmos.color = Color.cyan;

			Gizmos.DrawLine( r.GetStartPosition() , r.GetEndPosition() );
		}

		// show the passing situation
		Gizmos.color = Color.green;
		if ( IsPassing(RoadType.WestEast ) )
		{
			Gizmos.DrawLine( transform.position + transform.forward * Width / 2f 
				, transform.position - transform.forward * Width / 2f );
		} else if ( IsPassing(RoadType.NorthSouth) ) {
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

	public override float GetWaittingTime (Road fromRoad)
	{
		if ( fromRoad.type == RoadType.NorthSouth )
		{
			return PassRate * PassCycle;
		}

		return ( 1f - PassRate ) * PassCycle;
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
}
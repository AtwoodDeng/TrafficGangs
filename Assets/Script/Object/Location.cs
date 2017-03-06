using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Location : MBehavior {

	[SerializeField] protected List<Road> roads = new List<Road>();

	public float Width
	{
		get { return m_width ; }
	}
	[SerializeField] float m_width = 1;

	BoxCollider m_collider;
	public delegate void EndPassHandler();

	protected override void MAwake ()
	{
		base.MAwake ();
		if ( m_collider == null )
			m_collider = gameObject.GetComponent<BoxCollider>();
		m_collider.size = Vector3.one * Width;
	}

	protected override void MUpdate ()
	{
		base.MUpdate ();

		foreach( Road r in roads ) {
			if ( r.Original == null )
				r.Original = this;
		}
	}

	public Road GetRoadToward( Location toward )
	{
		foreach( Road r in roads ) {
			if ( r.Target == toward )
				return r;
		}
		return null;
	}

	public Road GetRandom()
	{
		return roads[ Random.Range( 0 ,  roads.Count) ];
	}

	virtual public bool IsPassiable ( Road fromRoad , Road toRoad , float time = -1f )
	{
		return true;
	}

	virtual public float GetWaittingTimeFromRoad( Road fromRoad )
	{
		return 0;
	}

	virtual public float GetWaittingTimeToRoad( Road toRoad )
	{
		return 0;
	}

	virtual public float GetWaittingTimeFromToRoad( Road fromRoad , Road toRoad )
	{
		return 0;
	}

	virtual public void OnLeave( Car car )
	{
	}

	virtual public void OnArrive( Car car )
	{
		
	}

	public Road[] GetRoads()
	{
		return roads.ToArray();
	}

	virtual public Road GetNeastestPassible( Road fromRoad)
	{
		return roads [0];
	}

	public bool IsInLocation( Vector3 position )
	{
		Vector3 offset = position - transform.position;
		return ( Mathf.Abs( offset.x ) < Width / 2f ) && ( Mathf.Abs( offset.y ) < Width / 2f );
	}

}

public enum LimitedTurnDirection
{
	Straight,
	Left,
	Right,
	All,
}

public enum RoadType
{
	North,
	South,
	West,
	East,
	None = 10,
}

[System.Serializable]
public class Road
{
	public Location Target;
	public Location Original;
	public RoadType type;
	private List<Car> carOnRoad = new List<Car>();


	public Vector3 Toward
	{
		get { 
			if ( Target != null && Original != null )
				return Target.transform.position - Original.transform.position;
			return Vector3.zero;
		}
	}

	public Vector3 GetStartPosition()
	{
		if ( Toward != Vector3.zero)
		{
			Vector3 offset = Vector3.Cross( Toward.normalized , Vector3.up ) * Original.Width * 0.25f ;
			return Toward.normalized * 0.5f * Original.Width - offset + Original.transform.position;
		}

		return Vector3.zero;
	}

	public Vector3 GetEndPosition()
	{
		if ( Toward != Vector3.zero)
		{
			Vector3 offset = Vector3.Cross( Toward.normalized , Vector3.up ) * Target.Width * 0.25f ;
			return - Toward.normalized * 0.5f * Target.Width - offset + Target.transform.position;
		}

		return Vector3.zero;
	}

	public Vector3 GetDirection()
	{
		return (GetEndPosition() - GetStartPosition()).normalized;
	}

	public float GetWaittingTime()
	{
		float waittingTime = 0;
		foreach( Car c in carOnRoad )
		{
			if ( c.IsSlow )
				waittingTime += c.AccelerationTime;
		}
		return waittingTime;
	}
	public void OnLeave( Car car )
	{
		carOnRoad.Remove( car );
	}

	public void OnArrive( Car car )
	{
		carOnRoad.Add( car );
	}

//	public void AllStopByFirstPriority()
//	{
//		foreach( Car c in carOnRoad )
//		{
//			Debug.Log("Set " + c + " to stop ");
//			c.StopByFirstPriority();
//		}
//	}

//	public void AllRecoverMoving()
//	{
//		foreach( Car c in carOnRoad )
//		{
//			c.EndStop();
//		}
//	}

	public Car GetPoliceCar()
	{
		foreach( Car c in carOnRoad )
		{
			if ( c.IsIgnoreTrafficLight() )
				return c;
		}
		return null;
	}

	public float GetDistanceToRoad( Vector3 pos )
	{
		Ray ray = new Ray( GetStartPosition() , GetDirection() );
		return Vector3.Cross(ray.direction, pos - ray.origin).magnitude;
	}

	public Vector3 GetNearestPoint ( Vector3 pos )
	{
		Ray ray = new Ray( GetStartPosition() , GetDirection() );
		return GetStartPosition() + Vector3.Dot( ray.direction , pos - ray.origin ) * ray.direction; 
	}
}

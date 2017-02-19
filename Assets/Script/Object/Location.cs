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

	virtual public bool IsPassiable( Road r )
	{
		return true;
	}

	virtual public float GetWaittingTime( Road fromRoad )
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

}


public enum RoadType
{
	NorthSouth,
	WestEast,
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
}

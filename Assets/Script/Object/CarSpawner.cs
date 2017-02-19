using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : Location {

	[SerializeField] GameObject[] carPrefabs;
	[SerializeField] List<Location> targets;
	[SerializeField] MinMax spawTime;
	[SerializeField] bool CreateOnStart = false;
	[SerializeField] float TestDistance;
	[SerializeField] LayerMask carTestMask;
	float timer = 0;

	protected override void MAwake ()
	{
		base.MAwake ();
		timer = CreateOnStart? 0 : spawTime.rand;
	}

	protected override void MUpdate ()
	{
		base.MUpdate ();
		timer -= Time.deltaTime;
		if ( timer < 0 ){
			SpawCar();
			timer = spawTime.rand;
		}
	}

	public void SpawCar()
	{
		// test if all the road is empty
		foreach( Road r in roads )
		{
			if ( TestForward( r ) != null )
				return;
		}

		GameObject carObj = Instantiate( carPrefabs[Random.Range( 0 , carPrefabs.Length) ] );

		Car carCom = carObj.GetComponent<Car>();
		carCom.SetFromToLocation( this , targets[Random.Range( 0 , targets.Count) ] );
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
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

		// TestLine
		Gizmos.color = Color.Lerp (Color.red, Color.blue, 0.5f);
		foreach( Road r in roads )
		{
			Vector3 side = Vector3.Cross( r.GetDirection() , Vector3.up );
			Gizmos.DrawLine( r.GetStartPosition() - r.GetDirection().normalized * Width / 2f + side * 0.05f 
				, r.GetDirection().normalized * ( TestDistance ) + r.GetStartPosition() + side * 0.05f);

		}
	}

	public void SetTarget( CarSpawner[] _targets )
	{
		if ( targets.Count <= 0 ) {
			targets.AddRange( _targets );
			targets.Remove( this );
		}
	}

	/// <summary>
	/// Test if there is any car in the front
	/// </summary>
	/// <returns> the car right before </returns>
	public Car TestForward ( Road r )
	{
		Car res = null;
		RaycastHit hitInfo;
		if ( Physics.Raycast ( r.GetStartPosition()  - r.GetDirection().normalized * Width / 2f 
			, r.GetDirection() , out hitInfo, TestDistance + Width / 2f  , carTestMask.value))
		{
			GameObject obj = hitInfo.collider.gameObject;
			res = obj.GetComponent<Car>();
		}

		return res;

	}

	public override void OnArrive (Car car)
	{
		base.OnArrive (car);
		car.Fade();
		car.gameObject.SetActive(false);
	}
}

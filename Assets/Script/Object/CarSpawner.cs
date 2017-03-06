using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : Location {

	[SerializeField] GameObject[] carPrefabs;
	[SerializeField] GameObject policeCarPrefab;
	[SerializeField] List<Location> targets;
	[SerializeField] MinMax spawTime;
	[SerializeField] bool CreateOnStart = false;
	[SerializeField] float TestDistance;
	[SerializeField] LayerMask carTestMask;
	public bool IsSpawPolicCar = false;
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
		if ( IsSpawPolicCar )
		{
			IsSpawPolicCar = !SpawPoliceCar();
		}
	}

	/// <summary>
	/// Spaws the police car.
	/// </summary>
	/// <returns><c>true</c>, if police car was spawed, <c>false</c> otherwise.</returns>
	public bool SpawPoliceCar()
	{

		// test if all the road is empty
		foreach( Road r in roads )
		{
			if ( TestForward( r ) != null )
				return false;
		}

		if ( policeCarPrefab == null ) 
			return false;

		GameObject carObj = Instantiate( policeCarPrefab );

		CarPolice carCom = carObj.GetComponent<CarPolice>();
		if ( carCom == null )
		{
			Destroy( carObj );
			return false;
		}
		Car chaseCar = TrafficManager.Instance.carList[Random.Range(0,TrafficManager.Instance.carList.Count)];
		chaseCar.AffectedByFirstPriority = false;
		carCom.SetFromTo( this, chaseCar );
		TrafficManager.RegisterCar (carCom);

		return true;
	}

	public void SpawCar()
	{
		// if the car numbers hit the max,
		// do not create more cars
		if ( TrafficManager.IsCarMaximum() )
			return;
		
		// test if all the road is empty
		foreach( Road r in roads )
		{
			if ( TestForward( r ) != null )
				return;
		}

		GameObject carObj = Instantiate( carPrefabs[Random.Range( 0 , carPrefabs.Length) ] );

		Car carCom = carObj.GetComponent<Car>();
		carCom.SetFromToLocation( this , targets[Random.Range( 0 , targets.Count) ] );
		TrafficManager.RegisterCar (carCom);
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
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
		TrafficManager.UnregisterCar (car);
		car.Fade();
	}
}

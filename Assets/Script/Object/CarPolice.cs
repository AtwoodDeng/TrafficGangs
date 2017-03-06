using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPolice : Car {

	public Car TargetCar{
		get {
			return m_targetCar;
		}
	}
	[SerializeField] private Car m_targetCar;
	public Location originalLocation;

	public void SetTargetCar( Location _from, Car car )
	{
		originalLocation = _from;
		m_targetCar = car;
		SetFromToLocation( _from , null );
	}

	public override Location CalculateNext ()
	{

		if ( TargetCar != null )
		{
			targetLocation = ( temLocation == TargetCar.temLocation)? TargetCar.nextLocation : TargetCar.temLocation;
		}else {
			targetLocation = originalLocation;
			Debug.Log("Original Location ");
		}

		// if the police car and the target car are in the same location
		// then the police car go to the same road with it
				route = TrafficManager.Instance.GetRoute( temLocation , targetLocation , temRoad );
		if ( route != null && route.Length > 1 )
			return route[1];
		
		return base.CalculateNext ();
	}

	protected override void OnMoveForwardUpdate ()
	{

		// Update Speed
		Car forwardCar = TestForward();

		if ( forwardCar != null )
			forwardCar.StopByFirstPriority(this);
		
		if ( (transform.position - temRoad.GetEndPosition()).magnitude < SafeDistance ) {
			m_forwardSpeed = Mathf.Clamp( m_forwardSpeed - Acceleration * Time.deltaTime , SlowSpeed , MaxSpeed );
		}else {
			m_forwardSpeed = Mathf.Clamp( m_forwardSpeed + Acceleration * Time.deltaTime , 0 , MaxSpeed );
		}

		if ( m_forwardSpeed < SlowSpeed )
			waittingTime += Time.deltaTime;

		// Update Direction
		forwardDirection = GetForwardDirection().normalized;
		transform.forward = Speed.normalized;

		// Update Position
		transform.position += Speed * Time.deltaTime;

		// Test If Arrive the end position
		if ( Vector3.Dot( ( transform.position - temRoad.GetEndPosition() ) , temRoad.GetDirection() ) > 0 ) {
			m_stateMachine.State = State.Wait;
		}

		if ( TargetCar != null )
		{
			if ( TargetCar.GetTemRoad() == temRoad )
			{
				if ( ( TargetCar.transform.position - transform.position ).magnitude < 
					( TargetCar.Length / 2f + this.Length / 2f ) )
					OnCatch();
			}
		}


	}

	/// <summary>
	/// What happen when catch the target car
	/// </summary>
	public void OnCatch()
	{
		TargetCar.Fade();

		m_targetCar = null;
		Debug.Log("On Catch " + nextLocation );
	}

	public override bool IsIgnoreTrafficLight ()
	{
		return true;
	}


	void OnDrawGizmosSelected ()
	{

		// draw the route to target
		Gizmos.color = Color.cyan;
		if ( route != null )
		{
			for( int i = 1 ; i < route.Length ; ++ i )
			{
				Vector3 start = ( i == 1 )? transform.position : route[i-1].transform.position;
				Vector3 end = route[i].transform.position;

				Gizmos.DrawLine( start , end );

				if ( i == route.Length - 1 && TargetCar != null )
					Gizmos.DrawLine( end , TargetCar.transform.position );
			}
		}

		if ( temRoad != null  )
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere( temRoad.GetEndPosition() , 0.2f );
			Gizmos.color = Color.green;
			if ( temRoad.Target != null )
				Gizmos.DrawWireSphere( temRoad.Target.transform.position , 0.3f );
			Gizmos.color = Color.cyan;
			if ( GetNextRoad() != null && GetNextRoad().Target != null )
				Gizmos.DrawWireSphere( GetNextRoad().Target.transform.position , 0.4f );
		}

	}


}

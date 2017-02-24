using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MBehavior
{
	int CarID;
	public Location targetLocation;
	public Location temLocation;
	public Location nextLocation;
	private float totalMoveTime = 0.001f;
	private float waittingTime;
	public float WaittingRate
	{
		get { return waittingTime / totalMoveTime; }
	}
	[SerializeField] private Road temRoad;
	[SerializeField] GameObject model;

	[Tooltip("The Speed of the Car move on the road (unit/second)")]
	[SerializeField] float moveSpeed = 1f;
	private float m_speed;
	private Vector3 direction;
	public Vector3 Speed { get { return m_speed * direction; } }
	public float MaxSpeed { get { return moveSpeed; } }
	public float SlowSpeed { get { return 0.05f * MaxSpeed; }}

	[Tooltip("The duration for the car to speed up from 0 to MoveSpeed")]
	[SerializeField] float accTime = 1f;
	public float AccelerationTime { get { return accTime ; } }
	public float Acceleration { get{ return MaxSpeed / AccelerationTime ; } }
	[SerializeField] float turnTime = 1f;
	[SerializeField] Vector3 size;
	[SerializeField] LayerMask carTestMask;
	[SerializeField] float additionalTestDistance = 0.1f;
	Location[] route = null;
	static int count = 0;

	BoxCollider m_collider;
	public float SafeDistance {
		get {
			return 0.5f * Speed.magnitude * AccelerationTime; 
		}
	}
	public float RawTestDistance {
		get { return 0.5f * MaxSpeed * AccelerationTime ; }
	}
	public float TestDistance
	{
		get { return SafeDistance + additionalTestDistance ; }
	}

	public enum State
	{
		None,
		MoveForward,
		Pass,
		Wait,
		Fade,
	}

	AStateMachine<State,LogicEvents> m_stateMachine;
	public State m_State;

	/// <summary>
	/// if the car is waiting in the location
	/// </summary>
	/// <value><c>true</c> if this instance is waitting; otherwise, <c>false</c>.</value>
	public bool IsWaitting {
		get { return m_stateMachine.State == State.Wait; }
	}

	/// <summary>
	/// If the car is in slow speed
	/// </summary>
	/// <value><c>true</c> if this instance is slow; otherwise, <c>false</c>.</value>
	public bool IsSlow{
		get { return Speed.magnitude <= SlowSpeed ; }
	}

	protected override void MAwake ()
	{
		base.MAwake ();
		InitStateMachine();

		if ( model != null )
			model.transform.localScale = size;

		m_collider = gameObject.AddComponent<BoxCollider>();
		m_collider.size = size;

		name = "Car" + count.ToString();
		CarID = count;
		count ++;
	}

	public void SetFromToLocation( Location _tem , Location _target )
	{
//		Debug.Log("Set From To" + _tem + " " + _target );
		// calculate the road
		temLocation = _tem;
		targetLocation = _target;
		CalculateNext();
		SetRoad();

		// set the init Position
		transform.position = temRoad.GetStartPosition();
		transform.forward = GetForwardDirection();

		m_stateMachine.State = State.MoveForward;
	}


	/// <summary>
	/// Update the next road to move, the following parameters are changed
	///  - next
	/// set to null if there is no
	/// </summary>
	virtual public void CalculateNext( )
	{
//		Debug.Log("Calculate Next");
		route = TrafficManager.Instance.GetRoute( temLocation , targetLocation , temRoad );
		if ( route != null && route.Length > 1 )
			nextLocation = route[1];
		else
			nextLocation = null;

//		nextLocation = TrafficManager.Instance.GetNextLocation( temLocation , targetLocation );
	}

	/// <summary>
	/// Set the temRoad according to tem Location and next Location
	/// </summary>
	public void SetRoad()
	{
		temRoad = temLocation.GetRoadToward( nextLocation );
	}

	void InitStateMachine()
	{
		m_stateMachine = new AStateMachine<State, LogicEvents>(State.None);

		m_stateMachine.AddUpdate(State.MoveForward , delegate {
			// Update Speed
			Car forwardCar = TestForward();
			if ( forwardCar != null && forwardCar.Speed.magnitude <= this.Speed.magnitude && ( forwardCar.temRoad == temRoad ) ) {
				m_speed = Mathf.Clamp( m_speed - Acceleration * Time.deltaTime , forwardCar.Speed.magnitude , MaxSpeed );
			}else if ( (transform.position - temRoad.GetEndPosition()).magnitude < SafeDistance )
			{
				m_speed = Mathf.Clamp( m_speed - Acceleration * Time.deltaTime , SlowSpeed , MaxSpeed );
			}
			else {
				m_speed = Mathf.Clamp( m_speed + Acceleration * Time.deltaTime , 0 , MaxSpeed );
			}

			if ( m_speed < SlowSpeed )
				waittingTime += Time.deltaTime;

			// Update Direction
			direction = GetForwardDirection().normalized;

			// Update Position
			transform.position += Speed * Time.deltaTime;

			// Test If Arrive the end position
			if ( ( transform.position - temRoad.GetEndPosition() ).magnitude < Speed.magnitude * Time.deltaTime * 1.1f ) {
				m_stateMachine.State = State.Wait;
			}
		});

		m_stateMachine.AddEnter(State.Wait , delegate {
			m_speed = 0;
	
			temLocation = nextLocation;
			CalculateNext();
			// arrive the tem Location
			if ( temLocation != null )
				temLocation.OnArrive( this );
		});

		m_stateMachine.AddUpdate( State.Wait , delegate {
			waittingTime += Time.deltaTime;
			OnWaitUpdate();
		});

		m_stateMachine.AddEnter(State.Pass, delegate {

//			CalculateNext();
			// move from last road to the next one
			if ( temRoad != null )
				temRoad.OnLeave( this );
			SetRoad();
			if ( temRoad != null )
				temRoad.OnArrive( this );
			
		});

		m_stateMachine.AddExit( State.Pass , delegate {
			transform.forward = GetForwardDirection();
			transform.position = temRoad.GetStartPosition();
			// leave the tem location and move to the next location
			temLocation.OnLeave( this );
		});

	}

	float waitUpdateTimer = 0.5f;
	protected virtual void OnWaitUpdate()
	{
		if ( waitUpdateTimer < 0 )
		{
			waitUpdateTimer = 0.5f;
			CalculateNext();
		}
		waitUpdateTimer -= Time.deltaTime;
	}

	public void Fade()
	{
		m_stateMachine.State = State.Fade;
	}

	public void WaitToPass()
	{
		if ( m_stateMachine.State == State.Wait )
			m_stateMachine.State = State.Pass;
	}

	public void PassToMoveForward()
	{
		if ( m_stateMachine.State == State.Pass )
			m_stateMachine.State = State.MoveForward;
	}

	public Road GetTemRoad()
	{
		return temRoad;
	}

	public Road GetNextRoad()
	{
		if ( temLocation != null )
			return temLocation.GetRoadToward(nextLocation) ;

		return null;
	}

	protected override void MUpdate ()
	{
		base.MUpdate ();
		m_stateMachine.Update();
		totalMoveTime += Time.deltaTime;
		m_State = m_stateMachine.State;
	}

	/// <summary>
	/// Test if there is any car in the front
	/// </summary>
	/// <returns> the car right before </returns>
	public Car TestForward ()
	{
		Car res = null;
		RaycastHit hitInfo;
		if ( Physics.Raycast (transform.position, transform.forward, out hitInfo, TestDistance + size.z /2f , carTestMask.value))
		{
			GameObject obj = hitInfo.collider.gameObject;
			res = obj.GetComponent<Car>();
		}

		return res;

	}
	/// <summary>
	/// Get the Forward Direction of the Car
	/// </summary>
	/// <returns>The forward direction.</returns>
	public Vector3 GetForwardDirection()
	{
		if ( temRoad == null )
			temRoad = temLocation.GetRoadToward( toward: nextLocation );
		if ( temRoad != null ) {
			return temRoad.GetDirection();
		}
		return transform.forward;
	}

	#region MoveInLocation


	public void CrossMoveTo( Vector3 toPosition , Location.EndPassHandler endHandler  )
	{
		Vector3 toward = toPosition - transform.position;
		if ( Vector3.Angle( transform.forward , toward ) < 1f ) // move forward
		{
//			Debug.Log("Move Straight");
			StartCoroutine( CrossForward ( toPosition , endHandler ) );
		}else if ( Mathf.RoundToInt( Vector3.Angle( transform.forward , toward ) ) == 90 ) // move back
		{
//			Debug.Log("Move Back");
			transform.position = toPosition;
			endHandler();
		}else // turn
		{
//			Debug.Log("Turn 90 degreeds");
			StartCoroutine( CrossTurn( toPosition , endHandler ));
		}
	}

	IEnumerator CrossTurn( Vector3 to , Location.EndPassHandler endHandler )
	{
		Vector3 fromPos = transform.position;
		Vector3 toward = to - fromPos;

		Vector3 coordinateY = transform.forward.normalized;
		Vector3 coordinateX = Vector3.Cross( coordinateY , Vector3.up ).normalized;
		if ( Vector3.Dot( toward , coordinateX ) < 0  ) // set the coordinate point of to position to be positive
			coordinateX = -coordinateX;

		float radiusX = Vector3.Dot( coordinateX , toward );
		float radiusY = Vector3.Dot( coordinateY , toward );
		float angle = 0; // angle of rotation in radian

		// make a turn 
		while( (transform.position - to).magnitude > Speed.magnitude * Time.deltaTime * 2f ) {
			m_speed = Mathf.Clamp( m_speed + Acceleration * Time.deltaTime , 0 , MaxSpeed );

			angle += m_speed * Time.deltaTime / radiusX ;
			Vector3 offsetFromTo = Vector3.zero;

			offsetFromTo -= coordinateX * radiusX * Mathf.Cos( angle );
			offsetFromTo -= coordinateY * radiusY * ( 1f - Mathf.Sin( angle ));

			transform.position = to + offsetFromTo;
			direction = Mathf.Cos(angle) * coordinateY + Mathf.Sin( angle ) * coordinateX;
			transform.forward = direction;
			yield return new WaitForEndOfFrame();
		}

		endHandler();

	}


	IEnumerator CrossForward( Vector3 to , Location.EndPassHandler endHandler )
	{
		Vector3 toward = to - transform.position;
		direction = toward;
		while ( (transform.position - to).magnitude > Speed.magnitude * Time.deltaTime ) {
			m_speed = Mathf.Clamp( m_speed + Acceleration * Time.deltaTime , 0 , MaxSpeed );
			transform.position += Speed * Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		endHandler();
	}

	#endregion

	void OnDrawGizmos ()
	{
		// draw the test ray
		Gizmos.DrawLine( transform.position + transform.forward * size.z /2f ,
			transform.position + transform.forward * ( TestDistance + size.z /2f));
	}

	void OnDrawGizmosSelected ()
	{
		// draw the car
		Gizmos.color = Color.Lerp (Color.red, Color.blue, 0.5f);
		Gizmos.DrawWireCube (transform.position, size);


		// draw the route to target
		Gizmos.color = Color.yellow;
		if ( route != null )
		{
			for( int i = 1 ; i < route.Length ; ++ i )
			{
				Vector3 start = ( i == 1 )? transform.position : route[i-1].transform.position;
				Vector3 end = route[i].transform.position;

				Gizmos.DrawLine( start , end );
				
			}
		}

		if ( temRoad != null )
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere( temRoad.GetEndPosition() , 0.2f );
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere( temRoad.Target.transform.position , 0.3f );
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere( GetNextRoad().Target.transform.position , 0.4f );
		}

	}
}

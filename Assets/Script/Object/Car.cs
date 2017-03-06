using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MBehavior
{
	int CarID;
	public Location targetLocation;
	public Location temLocation;
	public Location nextLocation;
	protected float totalMoveTime = 0.001f;
	protected float waittingTime;
	public float WaittingRate
	{
		get { return waittingTime / totalMoveTime; }
	}
	[SerializeField] protected Road temRoad;
	[SerializeField] protected GameObject model;

	[Tooltip("The Speed of the Car move on the road (unit/second)")]
	[SerializeField] float moveSpeed = 1f;
	protected float m_forwardSpeed;
	protected Vector3 forwardDirection;
	protected float m_sideSpeed;
	public Vector3 SideDirection{ get { return  Vector3.Cross( forwardDirection.normalized , -transform.up).normalized; } }
	public Vector3 Speed { get { return m_forwardSpeed * forwardDirection.normalized + m_sideSpeed * SideDirection; } }
	public float MaxSpeed { get { return moveSpeed; } }
	public float SlowSpeed { get { return 0.02f * MaxSpeed; }}

	[Tooltip("The duration for the car to speed up from 0 to MoveSpeed")]
	[SerializeField] float accTime = 1f;
	public float AccelerationTime { get { return accTime ; } }
	public float Acceleration { get{ return MaxSpeed / AccelerationTime ; } }
	[SerializeField] float turnTime = 1f;
	[SerializeField] Vector3 size;
	[SerializeField] LayerMask carTestMask;
	[SerializeField] float additionalTestDistance = 0.1f;
	public bool AffectedByFirstPriority = true;
	protected Location[] route = null;
	static int count = 0;
	Car firstPriorityCar = null;

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
	public float Length {
		get {
			return size.z;
		}
	}
	public float Width {
		get {
			return size.x;
		}
	}

	public enum State
	{
		None,
		/// <summary>
		/// Move in the straight line
		/// </summary>
		MoveForward,
		/// <summary>
		/// Go pass the cross
		/// </summary>
		Pass,
		/// <summary>
		/// Wait as the first car in the cross
		/// </summary>
		Wait,
		/// <summary>
		/// Waitting on the location
		/// </summary>
		WaitOnLocation,
		/// <summary>
		/// Fade away from the map
		/// </summary>
		Fade,
		/// <summary>
		/// Stop and wait until the first priority car pass
		/// </summary>
		StopForFirstPriority,
		/// <summary>
		/// Move Back to the road
		/// </summary>
		BackToRoad,

		StopForFirstPriorityWait,
		BackToRoadWait,
	}

	protected AStateMachine<State,LogicEvents> m_stateMachine;
	public State TemState{
		get{
			return m_stateMachine.State;
		}
	}
	public State m_State;

	/// <summary>
	/// if the car is waiting in the location
	/// </summary>
	/// <value><c>true</c> if this instance is waitting; otherwise, <c>false</c>.</value>
	public bool IsWaitting {
		get { return m_stateMachine.State == State.WaitOnLocation; }
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
		nextLocation = CalculateNext();

		SetRoad();

		// set the init Position
		transform.position = temRoad.GetStartPosition();
		transform.forward = GetForwardDirection();

		m_stateMachine.State = State.MoveForward;
	}


	/// <summary>
	/// calculate the next location
	/// return null if there is no
	/// </summary>
	virtual public Location CalculateNext( )
	{
		route = TrafficManager.Instance.GetRoute( temLocation , targetLocation , temRoad );
		if ( route != null && route.Length > 1 )
			return route[1];
		
		return null;
	}

	/// <summary>
	/// Set the temRoad according to tem Location and next Location
	/// </summary>
	virtual public void SetRoad()
	{
		if ( temRoad != null )
			temRoad.OnLeave( this );
		temRoad = temLocation.GetRoadToward( nextLocation );
		if ( temRoad != null )
			temRoad.OnArrive( this );
	}

	void InitStateMachine()
	{
		m_stateMachine = new AStateMachine<State, LogicEvents>(State.None);

		m_stateMachine.AddUpdate(State.MoveForward , OnMoveForwardUpdate );

		m_stateMachine.AddEnter(State.Wait , delegate {
			m_forwardSpeed = 0;
			temLocation = nextLocation;
			nextLocation = CalculateNext();
			// arrive the tem Location
			if ( temLocation != null )
				temLocation.OnArrive( this );

			m_stateMachine.State = State.WaitOnLocation;
		});

		m_stateMachine.AddUpdate( State.WaitOnLocation , delegate {
			waittingTime += Time.deltaTime;
			OnWaitUpdate();
		});

		m_stateMachine.AddEnter(State.Pass, OnEnterPass);

		m_stateMachine.AddExit( State.Pass , delegate {
			transform.forward = GetForwardDirection();
			transform.position = temRoad.GetStartPosition();
			// leave the tem location and move to the next location
			temLocation.OnLeave( this );
		});

		m_stateMachine.AddEnter( State.StopForFirstPriority , delegate {
			Debug.Log("Enter top ");
			m_sideSpeed = 0;
		});

		m_stateMachine.AddUpdate( State.StopForFirstPriority , delegate {
				
			float SideDistanceMax = Width + 0.1f;
			float sideDistance = Mathf.Clamp( temRoad.GetDistanceToRoad( transform.position ) , 0.001f , SideDistanceMax );
//			Debug.Log("Side Distance " + sideDistance );
			m_sideSpeed = Mathf.Sin( Mathf.Acos( Mathf.Clamp(  1f - 2 * sideDistance / SideDistanceMax  , -1f , 1f ) ) )* MaxSpeed + 0.001f ;

			// Update the forward Speed
			m_forwardSpeed = Mathf.Clamp( m_forwardSpeed - Acceleration / 2f  * Time.deltaTime , SlowSpeed , MaxSpeed );

			// Update Direction
			forwardDirection = GetForwardDirection().normalized;
			transform.forward = Speed.normalized;

			transform.position += Speed * Time.deltaTime;

			// Test If The Policd Car Walk Passed
			if ( (Vector3.Dot( ( firstPriorityCar.transform.position - transform.position ) , temRoad.GetDirection() ) > firstPriorityCar.Length + Length )
				|| temRoad != firstPriorityCar.temRoad ) {
				m_stateMachine.State = State.BackToRoad;
			}

		});

		m_stateMachine.AddExit( State.StopForFirstPriority , delegate {
			m_sideSpeed = 0;
			m_forwardSpeed = Mathf.Epsilon;
		});
			
		m_stateMachine.AddUpdate(State.BackToRoad , delegate() {

			float SideDistanceMax = Width + 0.1f;
			float sideDistance = Mathf.Clamp( SideDistanceMax - temRoad.GetDistanceToRoad( transform.position ) , 0.001f , SideDistanceMax);
				//			Debug.Log("Side Distance " + sideDistance );
			m_sideSpeed = - ( Mathf.Sin( Mathf.Acos( Mathf.Clamp( 1f - 2 * sideDistance / SideDistanceMax , -1f , 1f ) ) ) * MaxSpeed );

			// Update the forward Speed
			m_forwardSpeed = Mathf.Clamp( m_forwardSpeed + Acceleration * Time.deltaTime , SlowSpeed , MaxSpeed );
			transform.forward = Speed.normalized;
//			Debug.Log("Back Speed " + Speed + " " + Speed.normalized + " side " + m_sideSpeed + " forward " + m_forwardSpeed + " direction " + forwardDirection );

			transform.position += Speed * Time.deltaTime;

			if ( Vector3.Dot( ( transform.position - temRoad.GetStartPosition() ) , SideDirection ) < 0 )
				m_stateMachine.State = State.MoveForward;

		});

		m_stateMachine.AddExit( State.BackToRoad , delegate {
			if ( temRoad != null )	
				transform.position = temRoad.GetNearestPoint( transform.position );
			m_sideSpeed = 0;
		});



		m_stateMachine.AddUpdate( State.StopForFirstPriorityWait , delegate {

			float SideDistanceMax = Width + 0.1f;
			float sideDistance = Mathf.Clamp( temRoad.GetDistanceToRoad( transform.position ) , 0.001f , SideDistanceMax );
			//			Debug.Log("Side Distance " + sideDistance );
			m_sideSpeed = Mathf.Sin( Mathf.Acos( Mathf.Clamp(  1f - 2 * sideDistance / SideDistanceMax  , -1f , 1f ) ) )* MaxSpeed + 0.001f ;

			// Update Direction
			forwardDirection = GetForwardDirection().normalized;
			transform.forward = Speed.normalized;

			transform.position += Speed * Time.deltaTime;

			// Test If The Policd Car Walk Passed
			if ( (Vector3.Dot( ( firstPriorityCar.transform.position - transform.position ) , temRoad.GetDirection() ) > firstPriorityCar.Length + Length )
				|| temRoad != firstPriorityCar.temRoad ) {
				m_stateMachine.State = State.BackToRoadWait;
			}

		});

		m_stateMachine.AddExit( State.StopForFirstPriorityWait , delegate {
			m_sideSpeed = 0;
			m_forwardSpeed = Mathf.Epsilon;
		});

		m_stateMachine.AddUpdate(State.BackToRoadWait , delegate() {

			float SideDistanceMax = Width + 0.1f;
			float sideDistance = Mathf.Clamp( SideDistanceMax - temRoad.GetDistanceToRoad( transform.position ) , 0.001f , SideDistanceMax);
			//			Debug.Log("Side Distance " + sideDistance );
			m_sideSpeed = - ( Mathf.Sin( Mathf.Acos( Mathf.Clamp( 1f - 2 * sideDistance / SideDistanceMax , -1f , 1f ) ) ) * MaxSpeed );

			// Update the forward Speed
			transform.forward = Speed.normalized;
			Debug.Log("Back Speed " + Speed + " " + Speed.normalized + " side " + m_sideSpeed + " forward " + m_forwardSpeed + " direction " + forwardDirection );

			transform.position += Speed * Time.deltaTime;

			if ( Vector3.Dot( ( transform.position - temRoad.GetStartPosition() ) , SideDirection ) < 0 )
				m_stateMachine.State = State.WaitOnLocation;

		});

		m_stateMachine.AddExit( State.BackToRoadWait , delegate {
			if ( temRoad != null )	
				transform.position = temRoad.GetNearestPoint( transform.position );
			m_sideSpeed = 0;
		});
	}

	protected virtual void OnEnterPass()
	{
		SetRoad();
	}

	protected virtual void OnMoveForwardUpdate()
	{
		// Update Speed
		Car forwardCar = TestForward();
		if ( forwardCar != null && forwardCar.Speed.magnitude <= this.Speed.magnitude && ( forwardCar.temRoad == temRoad ) ) {
			m_forwardSpeed = Mathf.Clamp( m_forwardSpeed - Acceleration * Time.deltaTime , forwardCar.Speed.magnitude , MaxSpeed );
		}else if ( (transform.position - temRoad.GetEndPosition()).magnitude < SafeDistance )
		{
			m_forwardSpeed = Mathf.Clamp( m_forwardSpeed - Acceleration * Time.deltaTime , SlowSpeed , MaxSpeed );
		}
		else {
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
	}

	float waitUpdateTimer = 0.5f;
	protected virtual void OnWaitUpdate()
	{
		if ( waitUpdateTimer < 0 )
		{
			waitUpdateTimer = 0.5f;
			nextLocation =  CalculateNext();
		}
		waitUpdateTimer -= Time.deltaTime;
	}

	public void Fade()
	{
		m_stateMachine.State = State.Fade;
		gameObject.SetActive(false);
	}

	public void WaitToPass()
	{
		if ( m_stateMachine.State == State.WaitOnLocation )
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


	virtual public bool IsIgnoreTrafficLight()
	{
		return false;
	}

	public void StopByFirstPriority( Car _first )
	{
		// only affect the car in moveforward and wait
		if ( AffectedByFirstPriority && TemState == State.MoveForward  )
		{
			m_stateMachine.State = State.StopForFirstPriority;
			firstPriorityCar = _first;
		}
		else if ( AffectedByFirstPriority && TemState == State.Wait  )
		{
			m_stateMachine.State = State.StopForFirstPriorityWait;
			firstPriorityCar = _first;
		}
	}

	public void EndStop()
	{
		if ( m_stateMachine.State == State.StopForFirstPriority )
			m_stateMachine.State = State.MoveForward;
	}

	#region MoveInLocation

	public void CrossMoveTo( Vector3 toPosition , Location.EndPassHandler endHandler  )
	{
		Vector3 toward = toPosition - transform.position;
		if ( Vector3.Angle( transform.forward , toward ) < 1f ) // move forward
		{
			StartCoroutine( CrossForward ( toPosition , endHandler ) );
		}else if ( Mathf.RoundToInt( Vector3.Angle( transform.forward , toward ) ) == 90 ) // move back
		{
			// TODO : add the turn back animation
			transform.position = toPosition;
			endHandler();
		}else // turn
		{
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
//		while( (transform.position - to).magnitude > Speed.magnitude * Time.deltaTime * 2f ) {
		while( angle < Mathf.PI / 2f ) {
			m_forwardSpeed = Mathf.Clamp( m_forwardSpeed + Acceleration * Time.deltaTime , 0 , MaxSpeed );

			angle += m_forwardSpeed * Time.deltaTime / radiusX ;
			Vector3 offsetFromTo = Vector3.zero;

			offsetFromTo -= coordinateX * radiusX * Mathf.Cos( angle );
			offsetFromTo -= coordinateY * radiusY * ( 1f - Mathf.Sin( angle ));

			transform.position = to + offsetFromTo;
			forwardDirection = (Mathf.Cos(angle) * coordinateY + Mathf.Sin( angle ) * coordinateX).normalized;
			transform.forward = forwardDirection;
			yield return new WaitForEndOfFrame();
		}

		endHandler();

	}


	IEnumerator CrossForward( Vector3 to , Location.EndPassHandler endHandler )
	{
		
		Vector3 toward = to - transform.position;
		forwardDirection = toward.normalized;
//		while ( (transform.position - to).magnitude > Speed.magnitude * Time.deltaTime ) {
		while ( Vector3.Dot( ( transform.position - to) , toward ) < 0 ) {
			m_forwardSpeed = Mathf.Clamp( m_forwardSpeed + Acceleration * Time.deltaTime , 0 , MaxSpeed );
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

		if ( temRoad != null && temRoad.Original != null && temRoad.Target != null  )
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

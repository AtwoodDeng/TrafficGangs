using UnityEngine;
using System.Collections;

public class MBehavior : MonoBehaviour {

	[SerializeField] bool IsAffectedByPause = true;
	[SerializeField] bool IsPause = false;

	void Awake()
	{
		MAwake ();
	}

	void Start()
	{
		MStart ();
	}

	void Update()
	{
		if ( !IsPause )
			MUpdate ();
	}

	void OnEnable()
	{
		MOnEnable ();
		M_Event.RegisterEvent(LogicEvents.Pause, Pause);
		M_Event.RegisterEvent(LogicEvents.UnPause, UnPause);
	}

	void OnDisable()
	{
		MOnDisable ();
		M_Event.UnregisterEvent(LogicEvents.Pause, Pause);
		M_Event.UnregisterEvent(LogicEvents.UnPause, UnPause);
	}

	virtual protected void MAwake() {

	}

	// Use this for initialization
	virtual protected void MStart () {
		
	}
	
	// Update is called once per frame
	virtual protected void MUpdate () {
	
	}

	virtual protected void MOnEnable() {
		
	}

	virtual protected void MOnDisable() {
		
	}

	void Pause( LogicArg arg ) {
		if ( IsAffectedByPause )
			IsPause = true;

		Debug.Log("Pause");
	}

	void UnPause( LogicArg arg ) {
		if ( IsAffectedByPause ) 
			IsPause = false;
	}

}

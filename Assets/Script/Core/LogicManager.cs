using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MBehavior {

	static public LogicManager Instance{
		get { return m_Instance; }
		set { if ( m_Instance == null ) m_Instance = value ; }
	}
	static private LogicManager m_Instance;
	public LogicManager() {
		Instance = this;
	}

	public bool IsPause {
		get { return m_isPause; }
		set {
			if ( value != m_isPause ) {
				if ( value )
					M_Event.FireLogicEvent(LogicEvents.Pause,new LogicArg(this));
				else 
					M_Event.FireLogicEvent(LogicEvents.UnPause , new LogicArg(this));
				m_isPause = value;
			}
		}
	}
	bool m_isPause;

	[SerializeField] float TimeScale = 1f;
	[SerializeField] bool SetIsPause = false;

	protected override void MStart ()
	{
		base.MStart ();
	}

	protected override void MUpdate ()
	{
		base.MUpdate ();
		Time.timeScale = TimeScale;
		IsPause = SetIsPause;
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarOneWay : Car {

	public override void CalculateNext ()
	{
		nextLocation = temLocation.GetNeastestPassible ( GetTemRoad() ).Target;
	}

	protected override void OnWaitUpdate ()
	{
		CalculateNext();
	}
}

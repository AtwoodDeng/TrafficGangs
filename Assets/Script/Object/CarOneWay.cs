using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarOneWay : Car {

	public override Location CalculateNext ()
	{
		return temLocation.GetNeastestPassible ( GetTemRoad() ).Target;
	}

//	protected override void OnWaitUpdate ()
//	{
//		CalculateNext();
//	}
}

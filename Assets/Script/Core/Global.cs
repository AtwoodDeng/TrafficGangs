using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globa {
}

[System.Serializable]
public class MinMax
{
	public float min;
	public float max;
	public float Min{
		get { return Mathf.Min( min , max ); }
	}
	public float Max{
		get { return Mathf.Max( min , max ); }
	}
	public float rand
	{
		get { return Random.Range( Min , Max ); }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrafficLight : MonoBehaviour {
	[SerializeField] Button NorthLight;
	[SerializeField] Button NorthLeft;
	[SerializeField] Button NorthRight;
	[SerializeField] Button SouthLight;
	[SerializeField] Button SouthLeft;
	[SerializeField] Button SouthRight;
	[SerializeField] Button WestLight;
	[SerializeField] Button WestLeft;
	[SerializeField] Button WestRight;
	[SerializeField] Button EastLight;
	[SerializeField] Button EastLeft;
	[SerializeField] Button EastRight;
	[SerializeField] Cross crossScript;
	public bool isPassingNS;
	bool isLeftNorth = false;
	bool isRightNorth = false;
	bool isLeftSouth = false;
	bool isRightSouth = false;
	bool isLeftWest = false;
	bool isRightWest = false;
	bool isLeftEast = false;
	bool isRightEast = false;
//	enum Direction 
//	{   North = 1,
//		South = 2,
//		West = 3,
//		East = 4,
//	}

	// Use this for initialization
	void Start () {
		if(NorthLight.GetComponent<Image>().color == Color.green)
		{
			isPassingNS = true;
		}
		else
			isPassingNS = false;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void OnClickLight()
	{
		Debug.Log(isPassingNS);
		if(isPassingNS)
		{
			crossScript.AllowPassingWE();
			isPassingNS = false;
		}
		else
		{
			crossScript.AllowPassingNS();
			isPassingNS = true;
		}
	}

	public void OnClickLeftArrow(int direction)
	{
		switch(direction)
		{
		case 1://north
			if(isLeftNorth)
			{
				//disable turn left
				crossScript.SetNorthDirection(LimitedTurnDirection.Straight);
				//turn black
				NorthLeft.GetComponent<Image>().color = Color.black;
				isLeftNorth = false;
			}
			else
			{
				//set to left, disable right
				crossScript.SetNorthDirection(LimitedTurnDirection.Left);
				NorthLeft.GetComponent<Image>().color = Color.white;
				NorthRight.GetComponent<Image>().color = Color.black;
				isLeftNorth = true;
				isRightNorth = false;
			}
			break;
		case 2://south
			if(isLeftSouth)
			{
				//disable turn left
				crossScript.SetSouthDirection(LimitedTurnDirection.Straight);
				//turn black
				SouthLeft.GetComponent<Image>().color = Color.black;
				isLeftSouth = false;
			}
			else
			{
				//set to left, disable right
				crossScript.SetSouthDirection(LimitedTurnDirection.Left);
				SouthLeft.GetComponent<Image>().color = Color.white;
				SouthRight.GetComponent<Image>().color = Color.black;
				isLeftSouth = true;
				isRightSouth = false;
			}
			break;
		case 3://west
			if(isLeftWest)
			{
				//disable turn left
				crossScript.SetWestDirection(LimitedTurnDirection.Straight);
				//turn black
				WestLeft.GetComponent<Image>().color = Color.black;
				isLeftWest = false;
			}
			else
			{
				//set to left, disable right
				crossScript.SetWestDirection(LimitedTurnDirection.Left);
				WestLeft.GetComponent<Image>().color = Color.white;
				WestRight.GetComponent<Image>().color = Color.black;
				isLeftWest = true;
				isRightWest = false;
			}
			break;
		case 4: //east
			if(isLeftEast)
			{
				//disable turn left
				crossScript.SetEastDirection(LimitedTurnDirection.Straight);
				//turn black
				EastLeft.GetComponent<Image>().color = Color.black;
				isLeftEast = false;
			}
			else
			{
				//set to left, disable right
				crossScript.SetEastDirection(LimitedTurnDirection.Left);
				EastLeft.GetComponent<Image>().color = Color.white;
				EastRight.GetComponent<Image>().color = Color.black;
				isLeftEast = true;
				isRightEast = false;
			}
			break;
			
		}



		
	}

	public void OnClickRightArrow(int direction)
	{
		switch(direction)
		{
		case 1: //north
			if(isRightNorth)
			{
				//disable turn right
				crossScript.SetNorthDirection(LimitedTurnDirection.Straight);
				//turn black
				NorthRight.GetComponent<Image>().color = Color.black;
				isRightNorth = false;
			}
			else
			{
				//set to right, disable left
				crossScript.SetNorthDirection(LimitedTurnDirection.Right);
				NorthRight.GetComponent<Image>().color = Color.white;
				NorthLeft.GetComponent<Image>().color = Color.black;
				isRightNorth = true;
				isLeftNorth = false;
			}
			break;
		case 2: //south
			if(isRightSouth)
			{
				//disable turn right
				crossScript.SetSouthDirection(LimitedTurnDirection.Straight);
				//turn black
				SouthRight.GetComponent<Image>().color = Color.black;
				isRightSouth = false;
			}
			else
			{
				//set to right, disable left
				crossScript.SetSouthDirection(LimitedTurnDirection.Right);
				SouthRight.GetComponent<Image>().color = Color.white;
				SouthLeft.GetComponent<Image>().color = Color.black;
				isRightSouth = true;
				isLeftSouth = false;
			}
			break;
		case 3: //west
			if(isRightWest)
			{
				//disable turn right
				crossScript.SetWestDirection(LimitedTurnDirection.Straight);
				//turn black
				WestRight.GetComponent<Image>().color = Color.black;
				isRightWest = false;
			}
			else
			{
				//set to right, disable left
				crossScript.SetWestDirection(LimitedTurnDirection.Right);
				WestRight.GetComponent<Image>().color = Color.white;
				WestLeft.GetComponent<Image>().color = Color.black;
				isRightWest = true;
				isLeftWest= false;
			}
			break;
		case 4: //east
			if(isRightEast)
			{
				//disable turn right
				crossScript.SetEastDirection(LimitedTurnDirection.Straight);
				//turn black
				EastRight.GetComponent<Image>().color = Color.black;
				isRightEast = false;
			}
			else
			{
				//set to right, disable left
				crossScript.SetEastDirection(LimitedTurnDirection.Right);
				EastRight.GetComponent<Image>().color = Color.white;
				EastLeft.GetComponent<Image>().color = Color.black;
				isRightEast = true;
				isLeftEast = false;
			}
			break;


		}


		
	}



	public void DisplayPassingNS()
	{
		NorthLight.GetComponent<Image>().color = Color.green;
		SouthLight.GetComponent<Image>().color = Color.green;
		WestLight.GetComponent<Image>().color = Color.red;
		EastLight.GetComponent<Image>().color = Color.red;
	}

	public void DisplayPassingWE()
	{
		NorthLight.GetComponent<Image>().color = Color.red;
		SouthLight.GetComponent<Image>().color = Color.red;
		WestLight.GetComponent<Image>().color = Color.green;
		EastLight.GetComponent<Image>().color = Color.green;
	}


}

using UnityEngine;
using System.Collections;

public class FollowCurrentValueZY : MonoBehaviour {


	public int graphNumber;

	private float y;
	private float z;

	//this script must be run after the craft script
	void Update(){
		transform.localPosition.Set(0,y,z);
	}

	//called AFTER the phisics information is calculted 
	public void setYZ(float yVal,float zVal){
		y = yVal;
		z = zVal;
	}


}

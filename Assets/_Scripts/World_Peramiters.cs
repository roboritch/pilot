using UnityEngine;
using System.Collections;

public class World_Peramiters : MonoBehaviour {
	public float airDensity = 1;
	public Vector3 baseWindDirection = new Vector3(); 


	
	public void setAirDensity(float density){
		airDensity = density;
	}
	
	public void setWindDirection(Vector3 windDirection){
		baseWindDirection = windDirection;
	}


	public float getAirDensity(){
		return airDensity;
	}

	public Vector3 getWindDirection(){
		return baseWindDirection;
	}

}

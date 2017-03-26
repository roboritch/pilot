using UnityEngine;
using System.Collections;

public class Airbrake_Animator : MonoBehaviour {

	public string airbrakeSide;
	public float airbrakeMovmentMod = 3;
	private bool isLeft = false;

	private float targetRot;

	// Use this for initialization
	void Start () {
		if(airbrakeSide == "left"){
			isLeft = true;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
	}

	public void applyRotation(){

	}

	public void setTargetRotation(float currentBrakingPower, float wantedBrakingPower, float velocity){
		float currentRot = transform.eulerAngles.y;
		float targetRot = 0;

		currentBrakingPower /= velocity;
		wantedBrakingPower /= velocity;






	}



}

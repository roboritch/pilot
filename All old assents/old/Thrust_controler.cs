using UnityEngine;
using System.Collections;


public class Thrust_controler : MonoBehaviour {

	public static int numberOfThrusters = 8; 

//	overall thrust mod
	public float thrustModifyer = 30;

	//role mod 
	public float roleThrustModifyer = 0.5f;

	//hover compansation
	public float rearHoverMod = 0.5f;
	public float frontHoverMod = 0.21f;
	public float forwordHoverMod = 0.1f;

	//forword compensation
	public float forwordMod = 1f;
	public float backForwordMod = 0.75f;
	public float forwordBalenceMod = 0.1f;



	// Vareables for the raw input of controler axis
	private float TrigerR=0;
	private float TrigerL=0;
	private float LStickX=0;
	private float LStickY=0;
	private float RStickX=0;
	private float RStickY=0;

	public GameObject[] thrusters;
	 //in order forword thrusters left to right then down towords cocpit repeate.
								  //down thrusters next in the same order

	private float[] thrust = new float[numberOfThrusters]; // amount of thrust from each thruster
	private float[] thrusterGroundDistance = new float[numberOfThrusters]; // uses raycasts from thrusters to show affect thrust
	private Vector3[] thrustVector = new Vector3[numberOfThrusters]; // amount of thrust in a vector 3 


	void Awake(){																
		thrusters = new GameObject[] {GameObject.Find("LF_down_thruster"),GameObject.Find("RF_down_thruster"),
			GameObject.Find("LB_down_thruster"),GameObject.Find("RB_down_thruster"),
			GameObject.Find("LF_forward_thruster"),GameObject.Find("RF_forward_thruster"),
			GameObject.Find("LB_forward_thruster"),GameObject.Find("RB_forward_thruster"),
		};

	}


	 //updates every phisics calculation
	void FixedUpdate(){
		for (int x = 0; x < thrusters.Length; x++){ 
			Debug.DrawRay (thrusters [x].transform.position, thrusters [x].transform.TransformDirection (Vector3.down), Color.red);
			Debug.DrawRay (thrusters [x].transform.position, -thrustVector [x]);
		}

		controlerAccessor();

		//raycasting
		distanceToSolid();

		calculateThrust();

		applyThrust();
	}

	
	private void controlerAccessor(){
		// All Xbox controler axis inputs: TODO Multy controler support
		// TODO Controler script for this infromantion to be imported from
		TrigerR = Input.GetAxisRaw("TriggersR_1");
		TrigerL = Input.GetAxisRaw("TriggersL_1");
		
		LStickX = Input.GetAxisRaw("L_XAxis_1");
		LStickY = Input.GetAxisRaw("L_YAxis_1");
		RStickX = Input.GetAxisRaw("R_XAxis_1");
		RStickY = Input.GetAxisRaw("R_YAxis_1");

	}

	/**
	 * Array thrust is organized in a grid system grouped acording to direction down, back
	 */
	private void calculateThrust(){
		for (int x = 0; x < thrust.Length; x++) {
			thrust[x] = 0;
		}

		//stick direction is negative and right is positive
		//down is positive up is negative
		//TODO add stick invert options
		if(LStickX > 0){
			thrust[0] += LStickX*roleThrustModifyer; //invert stick value to get proper thrust
			thrust[2] += LStickX*roleThrustModifyer * 0.5f; // back thrusters are smaller
		}else{
			thrust[1] += -LStickX*roleThrustModifyer;
			thrust[3] += -LStickX*roleThrustModifyer * 0.5f;
		}

		if(LStickY > 0){
			thrust[0] += LStickY;
			thrust[1] += LStickY;
		}else{
			thrust[2] += -LStickY * 0.5f;
			thrust[3] += -LStickY * 0.5f;
		}

		if(RStickX < 0){
			
		}else{
			
		}

		if(RStickY < 0){
			
		}else{
			
		}

		//forword thrust machanic
		thrust[4] += TrigerR * forwordMod;
		thrust[5] += TrigerR * forwordMod;
		thrust[6] += TrigerR * backForwordMod;
		thrust[7] += TrigerR * backForwordMod;

		thrust[2] += TrigerR * forwordBalenceMod;
		thrust[3] += TrigerR * forwordBalenceMod;



		//hover mechanics
		if(thrusterGroundDistance[0] <= 3.5 && thrusterGroundDistance[0] >=0.1){
			thrust[0] += 2/thrusterGroundDistance[0]*frontHoverMod;
		}
		
		if(thrusterGroundDistance[1] <= 3.5 && thrusterGroundDistance[1] >=0.1){
			thrust[1] += 2/thrusterGroundDistance[1]*frontHoverMod;
		}
		
		if(thrusterGroundDistance[2] <= 5.5 && thrusterGroundDistance[2] >=0.1){
			thrust[2] += 2/thrusterGroundDistance[2]*rearHoverMod;
		}
		
		if(thrusterGroundDistance[3] <= 5.5 && thrusterGroundDistance[3] >=0.1){
			thrust[3] += 2/thrusterGroundDistance[3]*rearHoverMod;
		}

		
		// forword thruster compensation for hover
		float racerReverseVolosity = GameObject.Find("/Racer_Arrow").transform.rigidbody.angularVelocity.y;
		if(TrigerR == 0 && racerReverseVolosity < 5){
			thrust[4] += 1/racerReverseVolosity * forwordHoverMod;
			thrust[5] += 1/racerReverseVolosity * forwordHoverMod;
		}




		for (int x = 0; x < thrustVector.Length; x++) {
			thrustVector[x] = new Vector3(0.0f, thrust[x] * thrustModifyer, 0.0f); // gets thrust from specifyed key input. The thrust is in the objects -y direction.
			thrustVector[x] = thrusters[x].transform.TransformDirection(thrustVector[x]); // Transforms thrust direction from local space to world space. otherwise thrust is always up
		} 
	}

	private void applyThrust(){
		for (int x = 0; x < thrust.Length; x++) {
			rigidbody.AddForceAtPosition(thrustVector[x],thrusters[x].transform.position);

		} 
	}
	
	/**
	 * creates a ray and RaycastHit object and gets the distance from the ground for each
	 * individual thruster
	 */
	private void distanceToSolid(){
		Ray caster = new Ray();
		RaycastHit hitDistance = new RaycastHit();
		
		//gets distance from ground for all thrusters
		for (int x = 0; x < thrusterGroundDistance.Length; x++) {
			caster.direction = thrusters[x].transform.TransformDirection(Vector3.down); // sets caster direction relitive to current thruster position in the -y direction
			caster.origin = thrusters[x].transform.position;
			Physics.Raycast(caster, out hitDistance,10f);

			thrusterGroundDistance[x] = hitDistance.distance;
		}
	}

	public float[] getContolerOutput(){
		float[] cont = {TrigerR,TrigerL,LStickX,LStickY,RStickX,RStickY};
		return cont;
	}
	 
	public float[] getThrust(){
		return thrust;
	}

	public float[] getGroundDistance(){
		return thrusterGroundDistance;
	}
	


}
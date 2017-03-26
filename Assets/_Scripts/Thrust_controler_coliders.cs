using UnityEngine;
using System.Collections;

/// <summary>
/// This class is the central controler for the racecraft, it is mostly self containd
/// due most thrust calculations being simple. Both drag and angular drag are implemented
/// inside there own classes and called here to effect the rigedbody.
/// </summary>
public class Thrust_controler_coliders : MonoBehaviour {
	

	#region Variables
	private static int numberOfThrusters = 8; 
	
	//total thrust
	public float thrustModifyer;
	public float blowbackMod;

	//role modifyers
	public float roleThrustModifyer;
	public float maxAngular = 10.0f;
	public float directRoleMod;

	//forword role modifyers
	public float frontMod;
	public float rearMod;

	// hover modifyers
	public float frontHoverMod;
	public float rearHoverMod;
	public float forwordHoverMod;

	private float forwordHoverSmoothing = 0;
	private bool isHovering = true;
	private bool hoverToggle = true;

	//forword modifyers
	public float forwordMod_Main;
	public float forwordMod_Aux;
	public float forwordMod_FrontBalence;
	public float forwordMod_RearBalence;

	//up modifyers
	public float upwordMod_FrontBalence;
	public float upwordMod_RearBalence;

	//turning modifyers
	public float rightTurnBot;
	public float rightTurnTop;
	public float leftTurnBot;
	public float leftTurnTop;

	#region Airbrake information AND modifyers
	public float airbrakeMod; // left and right
	public float brakeMod;// both at the same time
	public float airbrakeMovmentSpeed; // how fast the airbrakes open and close
									  //modifyers based on speed used as well

	private float airbrakePower_L = 0;
	private float airbrakePower_R = 0;
	private float  airbrakeDisired_L = 0;
	private float  airbrakeDisired_R = 0;
	private Vector3 airbrakeVector_L = new Vector3(0f,0f,0f);
	private Vector3 airbrakeVector_R = new Vector3(0f,0f,0f);
	//private Equation_Creator AirbrakeGraph = new Equation_Creator();	
	#endregion

	//volocity constraints
	public float maxYvolocity;
	public float volocitySmothing;

	//this must be changed for rebinding
	#region Vareables for the raw input of controler axis
	private float TrigerR=0;
	private float TrigerL=0;
	private float LStickX=0;
	private float LStickY=0;
	private float RStickX=0;
	private float RStickY=0;
	private float RBumper=0;
	private float LBumper=0;
	private float dPadX=0;
	private float dPadY=0;
	private float yButton = 0;
	private float bButton = 0;

	private bool aButton=false;
	#endregion

	//world craft distance from ground
	private RaycastHit groundInformation; 
	 
	private GameObject[] thrusters = new GameObject[8]; //in order forword thrusters left to right then down towords cocpit repeate.
								  //down thrusters next in the same order

	private Equation_Creator[] equations;

	private float[] thrustBildup = new float[numberOfThrusters]; //thrust given by input is spread out reaching full aplication after a few frames
									//this vareable always contains the final desired thrust		
	private float[] thrust = new float[numberOfThrusters]; // amount of thrust currently provided by each thruster
	private float[] thrusterGroundDistance = new float[numberOfThrusters]; // uses raycasts from thrusters to show affect thrust
	private Vector3[] thrustVector = new Vector3[numberOfThrusters]; // amount of thrust in a vector 3 
	private Vector3 gyroscopeTorque = new Vector3();

	#region Object and component refrences

	//airbrakes
	private Transform airbrakeForceTransform_L;
	private Transform airbrakeForceTransform_R;
	private Airbrake_Animator airbrakeAnim_L;
	private Airbrake_Animator airbrakeAnim_R;


	#endregion

	#endregion 

	#region Program start | F-Awake
	void Awake(){																
		thrusters = new GameObject[] {
			GameObject.Find("LF_down_thruster"),
			GameObject.Find("RF_down_thruster"),
			GameObject.Find("LB_down_thruster"),
			GameObject.Find("RB_down_thruster"),
			GameObject.Find("LF_forward_thruster"),
			GameObject.Find("RF_forward_thruster"),
			GameObject.Find("LB_forward_thruster"),
			GameObject.Find("RB_forward_thruster"),
		};
		gameObject.GetComponent<Rigidbody>().maxAngularVelocity = maxAngular;
		equations = GetComponents<Equation_Creator>();
		GetComponent<Rigidbody>().SetDensity(0.04f);

		airbrakeForceTransform_L = transform.FindChild("Airbrake_Force.L").transform;
		airbrakeForceTransform_R = transform.FindChild("Airbrake_Force.R").transform;
		airbrakeAnim_L = transform.FindChild("colliders").FindChild("Airbrake_L").GetComponent<Airbrake_Animator>();
		airbrakeAnim_R = transform.FindChild("colliders").FindChild("Airbrake_R").GetComponent<Airbrake_Animator>();
	
	}
	#endregion

	#region Phisics | F-FixedUpdate
	//updates every phisics calculation
	//contains all main methods 
	void FixedUpdate(){

		controlerAccessor();

		//raycasting
		thrusterDistanceFromSolid();
		groundInformationDetect();

		calculateThrust();

		applyThrust();

		applyGyroscope();

		constrainGlobalYaxisMovement();

		debugRays(); 

		debugThrust();
	} 
	#endregion

	#region Anamation | F-Update
	void Update(){
		//all non phisics animation for the ship is called in this function
		//all single button presses 
		aButton = Input.GetButtonDown("A_1");
		hoverCompToggle();
	}

	#endregion

	#region Player inputs | F-controlerAccessor
	private void controlerAccessor(){
		// All Xbox controler axis inputs: TODO Multy controler support
		// TODO Controler script for this infromantion to be imported from
		// called in fixedUpdate for better resposivness
		TrigerR = Input.GetAxisRaw("TriggersR_1");
		TrigerL = Input.GetAxisRaw("TriggersL_1");
		
		LStickX = Input.GetAxisRaw("L_XAxis_1");
		LStickY = Input.GetAxisRaw("L_YAxis_1");
		RStickX = Input.GetAxisRaw("R_XAxis_1");
		RStickY = Input.GetAxisRaw("R_YAxis_1");
		RBumper = Input.GetAxisRaw("RB_1");
		LBumper = Input.GetAxisRaw("LB_1");
		dPadX = Input.GetAxisRaw("DPad_XAxis_1");
		dPadY = Input.GetAxisRaw("DPad_YAxis_1");
		yButton = Input.GetAxisRaw("Y_1");
		bButton = Input.GetAxisRaw("B_1");

		if(TrigerR + TrigerL + LStickX + LStickY + RStickX + RStickY + LBumper + RBumper == 0){
		//kebord equivelents 
			TrigerR += Input.GetAxisRaw("Forword");
			LStickX += Input.GetAxisRaw("Horizontal");
			LStickY += Input.GetAxisRaw("Vertical");
			TrigerL += Input.GetAxisRaw("Up");

			RBumper = Input.GetAxisRaw("BrakeL");
			LBumper = Input.GetAxisRaw("BrakeR");
		}
	}
	#endregion

	#region Thrust calculation consolidation | F-calculateThrust
	// Array thrust is organized in a grid system grouped acording to direction down â†“ , back â†’
	private void calculateThrust(){

		//bildup is reset every calculation to get new controler inputs 
		for (int x = 0; x < thrust.Length; x++) {
			thrustBildup[x] = 0; 
		}

		#region Thrust calculation method activation
		rollThrust();

		somersaultThrust();

		//turningThrust();  // testing without this using airbrakes

		forwordThrust();

		upwordThrust();

		hoverThrust();

		airBrakes();

		calculateBlowbackThrust(); //changes based on isHovering
		#endregion

		//transition to final thrust amount
		for(int x = 0; x < thrust.Length; x++){ //since airbrakes are smoothed in there own method since their smoothing is more complicated
			thrust[x] = Mathf.MoveTowards(thrust[x], thrustBildup[x], (thrustBildup[x]/20) + 1f);
		}

		//all thrust converted to vector form
		for (int x = 0; x < thrustVector.Length; x++) {
			thrustVector[x] = new Vector3(0.0f, thrust[x] * thrustModifyer, 0.0f); // gets thrust from controler inputs. The thrust is in the objects -y direction.
			thrustVector[x] = thrusters[x].transform.TransformDirection(thrustVector[x]); // Transforms thrust direction from world space to local space. otherwise thrust is always up
		} 
		//airbrakes, power applyed on z axis due to the way airbrake volocity is calculated
		airbrakeVector_L = airbrakeForceTransform_L.TransformDirection(new Vector3(0.0f,0.0f,-airbrakePower_L)); 
		airbrakeVector_R = airbrakeForceTransform_R.TransformDirection(new Vector3(0.0f,0.0f,-airbrakePower_R));
	}
	#endregion

	#region Thrust application to craft object | F-applyThrust
	//applyes the forces to the rigedbody according to the thruster empties 
	private void applyThrust(){
		// all main thrusters
		for (int x = 0; x < thrust.Length; x++) {
			GetComponent<Rigidbody>().AddForceAtPosition(thrustVector[x],thrusters[x].transform.position);
		} 
		//airbrakes
		GetComponent<Rigidbody>().AddForceAtPosition(airbrakeVector_L,airbrakeForceTransform_L.position);
		GetComponent<Rigidbody>().AddForceAtPosition(airbrakeVector_R,airbrakeForceTransform_R.position);
	}
	#endregion

	#region Other user controled inputs
	public void hoverCompToggle(){
		if(aButton){
			hoverToggle = !hoverToggle;
		}
	}


	#endregion

	#region Thrust mechanics, User
	private void forwordThrust(){
		thrustBildup[4] += TrigerR * forwordMod_Main;
		thrustBildup[5] += TrigerR * forwordMod_Main;
		thrustBildup[6] += TrigerR * forwordMod_Aux;
		thrustBildup[7] += TrigerR * forwordMod_Aux;
		
		thrustBildup[0] += TrigerR * forwordMod_FrontBalence;
		thrustBildup[1] += TrigerR * forwordMod_FrontBalence;
		thrustBildup[2] += TrigerR * forwordMod_RearBalence;
		thrustBildup[3] += TrigerR * forwordMod_RearBalence;
	}

	private void turningThrust(){
		if(RStickX < 0){	//turning thrust
			thrustBildup[5] += -RStickX * rightTurnBot;
			thrustBildup[7] += -RStickX * rightTurnTop;
		}else{
			thrustBildup[4] += RStickX * leftTurnBot;
			thrustBildup[6] += RStickX * leftTurnTop;
		}
	}

	private void rollThrust(){
		if(LStickX > 0){
			thrustBildup[0] += LStickX*roleThrustModifyer; //invert stick value to get proper thrust
			thrustBildup[2] += LStickX*roleThrustModifyer * 0.5f; // back thrusters are smaller
		}else{
			thrustBildup[1] += -LStickX*roleThrustModifyer;
			thrustBildup[3] += -LStickX*roleThrustModifyer * 0.5f;
		}

	}

	private void somersaultThrust(){
		if(LStickY > 0){	//semarsalt thrust
			thrustBildup[0] += LStickY * frontMod;
			thrustBildup[1] += LStickY * frontMod;
		}else{
			thrustBildup[2] += -LStickY * rearMod;
			thrustBildup[3] += -LStickY * rearMod;
		}
	}

	private void upwordThrust(){
		//upword thrust machanic 
		thrustBildup[0] += TrigerL * upwordMod_FrontBalence;
		thrustBildup[1] += TrigerL * upwordMod_FrontBalence;
		thrustBildup[2] += TrigerL * upwordMod_RearBalence;
		thrustBildup[3] += TrigerL * upwordMod_RearBalence;

	}

	private void airBrakes(){
		//must be reset due to the second input Parameter
		airbrakeDisired_L = 0;
		airbrakeDisired_R = 0;
		airbrakePower_L = 0;
		airbrakePower_R = 0;
		//reset at start due to information being neaded by GUI

		// velocity must be taken from airbrakes for correct turning circles
		// this formula takes the volocity relative to the point were force is applyed
		Vector3 airbrakeForceLocationtL = airbrakeForceTransform_L.position;
		Vector3 airbrakeForceLocationtR = airbrakeForceTransform_R.transform.position;

		Vector3 airbrakeForceL = GetComponent<Rigidbody>().GetRelativePointVelocity(airbrakeForceLocationtL);
		Vector3 airbrakeForceR = GetComponent<Rigidbody>().GetRelativePointVelocity(airbrakeForceLocationtR);

		airbrakeForceL = airbrakeForceTransform_L.InverseTransformDirection(airbrakeForceL); //gets volocity of airbrakes in world
		airbrakeForceR = airbrakeForceTransform_R.InverseTransformDirection(airbrakeForceR);

		Debug.DrawRay(airbrakeForceLocationtL,airbrakeForceL,Color.green);
		Debug.DrawRay(airbrakeForceLocationtR,airbrakeForceR,Color.green);

		//reversesed for application at the end
		float forwordVolocity_L = airbrakeForceL.z;
		float forwordVolocity_R = airbrakeForceR.z;

		//old formula 100/(1+Mathf.Exp((-forwordVolocity_L*0.037f+6)))

		float forwordVolocityModifyed_L = 100/(1+Mathf.Exp((-forwordVolocity_L*0.037f+6)));
		float forwordVolocityModifyed_R = 100/(1+Mathf.Exp((-forwordVolocity_R*0.037f+6)));

		if(RStickX < 0){
			airbrakePower_L = forwordVolocityModifyed_L * -RStickX * airbrakeMod;
		}else if(RStickX > 0){
			airbrakePower_R = forwordVolocityModifyed_R *  RStickX * airbrakeMod;
		}

		if(RStickY > 0){
			airbrakePower_L += RStickY * forwordVolocityModifyed_L * brakeMod;
			airbrakePower_R += RStickY * forwordVolocityModifyed_R * brakeMod;
		}

		//TODO add code that stops the airbrakes from applying a force that would do more than stop the arirake point on the xz plain

		//this stops the airbrakes from pulling the craft backwords or forwards
		if(airbrakePower_L > 0 && forwordVolocity_L < 0 || airbrakePower_L < 0 && forwordVolocity_L > 0 ){
			airbrakePower_L = 0;
		}

		if(airbrakePower_R > 0 && forwordVolocity_R < 0 || airbrakePower_R < 0 && forwordVolocity_R > 0){
			airbrakePower_R = 0;
		}

		//give info to airbrake animator
		airbrakeAnim_L.setTargetRotation(airbrakePower_L,airbrakeDisired_L,forwordVolocity_L);
		airbrakeAnim_R.setTargetRotation(airbrakePower_R,airbrakeDisired_R,forwordVolocity_R);


	}

	
	//TODO add light output rcs thrusters to the front of the craft

	#endregion

	#region Thrust mechanics & Gyroscopic motion, computational 

	#region Hover calculations
	public void hoverThrust(){
		isHovering = false;
		if(TrigerL == 0){
			//hovering code 
			if(thrusterGroundDistance[0] <= 10f && thrusterGroundDistance[0] >=0.1){
				thrustBildup[0] += frontHoverMod * equations[0].findYValue(thrusterGroundDistance[0]).y;
				isHovering = true;
			}
			
			if(thrusterGroundDistance[1] <= 10f && thrusterGroundDistance[1] >=0.1){
				thrustBildup[1] += frontHoverMod * equations[0].findYValue(thrusterGroundDistance[1]).y;
				isHovering = true;
			}
			
			if(thrusterGroundDistance[2] <= 10f && thrusterGroundDistance[2] >=0.1){
				thrustBildup[2] += rearHoverMod * equations[1].findYValue(thrusterGroundDistance[2]).y;
				isHovering = true;
			}
			
			if(thrusterGroundDistance[3] <= 10f && thrusterGroundDistance[3] >=0.1){
				thrustBildup[3] += rearHoverMod * equations[1].findYValue(thrusterGroundDistance[3]).y;
				isHovering = true;
			}
		}
		
		//code for forword thrusters during hover mode 
		Vector3 rVvector = GetComponent<Rigidbody>().velocity;
		rVvector = transform.InverseTransformDirection(rVvector);

		forwordHoverSmoothing = Mathf.MoveTowards(forwordHoverSmoothing, rVvector.z,1f);

		//this stops the craft from freaking out
		if(forwordHoverSmoothing < -0.07f){
			forwordHoverSmoothing = -0.07f; // 
		}

		//old code to stop !(250 < transform.rotation.eulerAngles.x && transform.rotation.eulerAngles.x <  310)&&
		//this part of the code will not activate if, lots of things
		if(TrigerL==0 && isHovering && -rVvector.z > 0 &&  hoverToggle){
			thrustBildup[4] += -forwordHoverSmoothing * forwordHoverMod;
			thrustBildup[5] += -forwordHoverSmoothing * forwordHoverMod;
		}

	}
	#endregion

	//TODO add wind resistance

	//TODO add air brakes

	// more force is aplyed of thrusters are closer to the ground
	private void calculateBlowbackThrust(){
		int downSkip = 0;
		// if hovering skip blowback calculation (it is pre-calculated)
		if(isHovering){
			downSkip = 4;
		}

		for (int x = downSkip; x < thrusterGroundDistance.Length; x++) {
			if(thrusterGroundDistance[x] > 0 && thrusterGroundDistance[x] < 1 && thrustBildup[x] < 0.1){
				thrustBildup[x] +=  (thrustBildup[x]*blowbackMod)/thrusterGroundDistance[x];
			}
		}
	}

	// this script creats gyroscopic motion helping control the crafts rotation 
	private void applyGyroscope(){
		Vector3 craftRotation = new Vector3();
		craftRotation.Set(GetComponent<Rigidbody>().angularVelocity.x,GetComponent<Rigidbody>().angularVelocity.y,GetComponent<Rigidbody>().angularVelocity.z);
		
		// tryes to slowly make all anguler motion 0 dampning the rotational effect of thrusters and collisons 
//		craftRotation.x = Mathf.MoveTowards(craftRotation.x, 0f , craftRotation.x * 0.1f);
//		craftRotation.y = Mathf.MoveTowards(craftRotation.y, 0f , craftRotation.y * 0.1f);
//		craftRotation.z = Mathf.MoveTowards(craftRotation.z, 0f , craftRotation.z * 0.1f);
//		
		
		GetComponent<Rigidbody>().angularVelocity = craftRotation;
	}

	/// <summary>
	/// This method directly changes the inerta of the craft 
	/// </summary>
	private void directInertiaModification(){

	}
	

	/// /// <summary>
	///slowes upword momentem of the craft to prevent suborbatal trajectories
	/// </summary>
	private void constrainGlobalYaxisMovement(){
		//only works when the craft is farther from the ground 
		if(GetComponent<Rigidbody>().velocity.y > maxYvolocity){
			GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, Mathf.MoveTowards(GetComponent<Rigidbody>().velocity.y, maxYvolocity, (GetComponent<Rigidbody>().velocity.y - maxYvolocity) * volocitySmothing ),GetComponent<Rigidbody>().velocity.z);
		}
	}



	#endregion

	#region Animator components


	
	#endregion
	
	#region Enviromental inputs

	/**
	 * creates a ray and RaycastHit object and gets the distance from the ground for each
	 * individual thruster
	 */
	private void thrusterDistanceFromSolid(){
		Ray caster = new Ray();
		RaycastHit hitDistance = new RaycastHit();
		
		//gets distance from ground for all thrusters
		for (int x = 0; x < thrusterGroundDistance.Length; x++) {
			caster.direction = thrusters[x].transform.TransformDirection(Vector3.down); // sets caster direction relitive to current thruster position in the -y direction
			caster.origin = thrusters[x].transform.position;
			Physics.Raycast(caster, out hitDistance, 10f); // max length is 10

			thrusterGroundDistance[x] = hitDistance.distance;
		}
	}

	private void groundInformationDetect(){
		Ray caster = new Ray();
		RaycastHit hit = new RaycastHit();

		caster.direction = Vector3.down;
		caster.origin = GetComponent<Rigidbody>().transform.position;
		Physics.Raycast(caster,out hit, 20f);

		groundInformation = hit;
	}




	#endregion

	#region Debug methods | F-debugRays F-debugThrust
	private void debugRays(){
		for (int x = 0; x < thrusters.Length; x++) {
			Debug.DrawRay (thrusters[x].transform.position, thrusters[x].transform.TransformDirection (Vector3.down), Color.red);
			Debug.DrawRay (thrusters[x].transform.position, -thrustVector[x]);
			Debug.DrawRay (airbrakeForceTransform_L.position,-airbrakeVector_L,Color.yellow);
			Debug.DrawRay (airbrakeForceTransform_R.position,-airbrakeVector_R,Color.yellow);
		}
		Debug.DrawRay(GetComponent<Rigidbody>().position,GetComponent<Rigidbody>().velocity,Color.blue); // total velocity of craft in vector form
	}

	
	public void debugThrust(){
		if(dPadY > 0f){
			GetComponent<Rigidbody>().AddForce(Vector3.forward*3f);
		}else if(dPadY < 0f){
			GetComponent<Rigidbody>().AddForce(Vector3.back*3f);
		}
		
		if(dPadX > 0f){
			GetComponent<Rigidbody>().AddForce(Vector3.right*3f);
		}else if(dPadX < 0f){
			GetComponent<Rigidbody>().AddForce(Vector3.left*3f);
		}

		if(yButton > 0f){
			GetComponent<Rigidbody>().AddForce(Vector3.up*3f);

		}

		if(bButton > 0f){
			GetComponent<Rigidbody>().AddForce(Vector3.down*3f);
		}
	}

	private void debugNegativeThrustCheck(){
		
	}

	#endregion

	#region Get Methods 
	public float[] getBrakingPower(){
		return new float[] {airbrakePower_L,airbrakePower_R,airbrakeDisired_L,airbrakeDisired_R};
	}

	public float[] getGlobalVolocities(){
		return new float[] {GetComponent<Rigidbody>().velocity.x,GetComponent<Rigidbody>().velocity.y,GetComponent<Rigidbody>().velocity.z};
	}


	public float[] getCurrentRotation(){ 
		return new float[] {GetComponent<Rigidbody>().angularVelocity.x,GetComponent<Rigidbody>().angularVelocity.y,GetComponent<Rigidbody>().angularVelocity.z,GetComponent<Rigidbody>().maxAngularVelocity};
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
	#endregion
	}
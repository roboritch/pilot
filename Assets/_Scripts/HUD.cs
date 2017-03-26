using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour{

	#region box offsets
	public float letterOffset = 6.5f;
	public float buttonOffset = 20f;

	public float boxWidthOffset = 5.5f;
	public float boxHeightOffset = 30.5f;
	#endregion


	#region desplayed information
	private float[] controlerAxis;
	private float[] groundDistance;
	private float[] angularVelocity;
	private float[] globalVelocity;
	private float[] breakingPower;
	private float[] scs;

	private Thrust_controler_coliders infoLocation; 
	private Drag_Calculations dragInfo; 
#endregion

	#region UI displayinfromation

	//the first array is space in ui the second is active or not
	public int[,] currentPositions; //max number of ui per space must be changed here
													

	private int[] currentUI;
	private int[] visibleUI;
	private float[] currentButtonsLength;
	#endregion


	void Awake(){
		currentUI = new int[4];
		visibleUI = new int[4];
		currentButtonsLength = new float[4];
		scs = new float[1];
		infoLocation = transform.Find("/Racer_Arrow").GetComponent<Thrust_controler_coliders>();
		dragInfo = transform.Find("/Racer_Arrow").GetComponent<Drag_Calculations>();
	}

	public void resetInfo(){
		for (int x = 0; x < currentUI.Length; x++) {
			currentUI[x]= 0;
			currentButtonsLength[x]= 0;
		}
	}

	public int positionControler(){

		return 3;
	}


	void OnGUI () {
		resetInfo();

		grabInfo();
		createInfoBox(0,"Global Velocity", new string[] {"x axis Velocity: ","y axis Velocity: ","z axis Velocity: "}, globalVelocity,false);
		createInfoBox(3,"Controler Levels", new string[] {"TrigerR and L: ","L Stick x and y: ","R Stick x and y: "},controlerAxis,true); 
		createInfoBox(0,"Angular Velocity", new string[] {"x axis rotation: ","y axis rotation: ","z axis rotation: ","Max rotation for all axis: "}, angularVelocity,false);

		createInfoBox(2,"Thruster ground distance", new string[] {"Distance front L - R: " ,"Distance back  L - R: "},groundDistance,true);
		createInfoBox(1,"Brake information",new string[] {"Current power L-R: ", "Wanted power L-R: "},breakingPower,true);
		createInfoBox(1,"pherical velocity coordinates",new string[]{"spherical velocity coordinates: "},scs,true);
	}


		#region create information display
	public void createButtons(string informationName,int position){
		float rectWidth = informationName.Length * letterOffset;
		currentButtonsLength[position] += rectWidth;


		if(position==0){
			if(GUI.Button(new Rect(currentButtonsLength[position]-rectWidth,0,rectWidth,buttonOffset),informationName)){visibleUI[position] = currentUI[position]; }
		}else if(position==1){
			if(GUI.Button(new Rect(Screen.width-currentButtonsLength[position],0,rectWidth,buttonOffset),informationName)){visibleUI[position] = currentUI[position]; }
		}else if(position==2){ 
			if(GUI.Button(new Rect(currentButtonsLength[position]-rectWidth,Screen.height-buttonOffset,rectWidth,buttonOffset),informationName)){visibleUI[position] = currentUI[position];}
		}else if(position==3){
			if(GUI.Button(new Rect(Screen.width-currentButtonsLength[position],Screen.height-buttonOffset,rectWidth,buttonOffset),informationName)){visibleUI[position] = currentUI[position];}
		 }
	}

	public void createInfoBox(int position, string boxName,string[] dataNames, float[] data, bool dualData){

		createButtons(boxName,position);
	
		//if the current ui is not active don't render it
		if(visibleUI[position] != currentUI[position]){
			currentUI[position]++;
			return;
		}

		float groupWidth = 0;
		float groupHeight = 0;

		// max of string + max float size * 2 for dual data 
		groupWidth = (float)boxName.Length;

		for (int x = 0; x < dataNames.Length; x++) {
			groupWidth = Mathf.Max((float)dataNames[x].Length, groupWidth);	
		}

		groupWidth *= 4; // multuply my average character pixel length

		//add length when 2 data sets per line
		if(dualData){
			groupWidth += 2f * 18f;
		}else{
			groupWidth += 18f;
		}


		groupHeight = (float)dataNames.Length;

		//apply offsets
		groupWidth *= boxWidthOffset;
		groupHeight *= boxHeightOffset;

		//0 1 ui int to screen position
		//2 3
		//creates a GUI group 10 units away from the top or bottem edge of the screen
		if(position==0){
			GUI.BeginGroup(new Rect(0,buttonOffset,groupWidth,groupHeight));
		}else if(position==1){
			GUI.BeginGroup(new Rect(Screen.width-groupWidth,buttonOffset,groupWidth,groupHeight));
		}else if(position==2){
			GUI.BeginGroup(new Rect(0,Screen.height-groupHeight-buttonOffset,groupWidth,groupHeight));
		}else if(position==3){
			GUI.BeginGroup(new Rect(Screen.width-groupWidth,Screen.height-groupHeight-buttonOffset,groupWidth,groupHeight));
		}
		GUI.Box(new Rect (0,0,groupWidth,groupHeight),boxName);
		//creats a space for auto fromating to fill
		//all objects are move 5px from edge
		GUILayout.BeginArea(new Rect(5f,0,groupWidth,groupHeight));
		GUILayout.Space(23f); // adds space between box name and lables

		//creats a line of text for all the data 
		if(!dualData){
			for (int x = 0; x < dataNames.Length; x++) {
				GUILayout.Label(dataNames[x] + " " + data[x]);
			}
		}else{ //this is used when  there are 2 data sets per lable
			int y = 0;
			for (int x = 0; x < dataNames.Length; x++) {
				GUILayout.Label(dataNames[x] + " " + data[y] + " , " + data[y+1]);
				y +=2;
		    }
		}
		GUILayout.EndArea();
		GUI.EndGroup();

		//the next ui at this position will be one more than this
		currentUI[position]++;
	}
	#endregion

		
	public void grabInfo(){
		controlerAxis = infoLocation.getContolerOutput();
		groundDistance = infoLocation.getGroundDistance();
		angularVelocity = infoLocation.getCurrentRotation();
		globalVelocity = infoLocation.getGlobalVolocities();
		breakingPower = infoLocation.getBrakingPower();
		scs = dragInfo.getSphericalCoord();
	}
	
}

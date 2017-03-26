using UnityEngine;
using System.Collections;

/// <summary>
/// This class handles the drag for a racecraft, the inertial drag at any 
/// given time is affected by craft orentation, speed, air density,
/// and deployment values for air resistant armatures (air brakes)
/// 
/// </summary>
public class Drag_Calculations : MonoBehaviour{
	private float airDensity;
	private float dragCoefficient;
	//http://en.wikipedia.org/wiki/Drag_coefficient#mediaviewer/File:14ilf1l.svg
	private Vector3 finalDragCalculation;
	private float dragForce;
	//
	private Vector3 airSpeed;
	// a combonation of airflow (caves, updrafts, ext) and world air speed
	private Vector2 sphericalCoord;
	// the directrion of the volocity reltive to the front of the craft
	private float[,] dragCoefGraph;


	private World_Peramiters worldPram;

	
	#region initialization

	void Awake(){
		worldPram = GameObject.FindGameObjectWithTag("WorldPram").GetComponent<World_Peramiters>();
		finalDragCalculation = new Vector3();
		sphericalCoord = new Vector2();
		dragForce = 1f;
	
		airDensity = worldPram.getAirDensity();
		dragCoefficient = 0f;


		dragCoefGraph = new float[180, 360];
		BitStream dragStream = new BitStream();
	



		for(int x = 0; x < dragCoefGraph.GetLength(0); x++){
			for(int y = 0; y < dragCoefGraph.GetLength(1); y++){
				dragCoefGraph[x, y] = 1f;				
			}
		}


	}

	void Start(){
		InvokeRepeating("coordDebugLog", 0f, 1f);
	}

	#endregion

	void FixedUpdate(){
		//calculateDrag();
	}

	private void coordDebugLog(){
		Debug.Log("x and y coordinate is " + sphericalCoord);
	}

	
	private void calculateDrag(){
		Vector3 objectVelcoity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
		sphericalCoord = CartesianToPolar(objectVelcoity);
		sphericalCoord.x *= -1f;
		airDensity = 1.275f; //the advrige density of air on earth in kg/m^3


		int xCeil = Mathf.CeilToInt(sphericalCoord.x + 90f);
		int xFloor = Mathf.FloorToInt(sphericalCoord.x + 90f);
		int yCeil = Mathf.CeilToInt(sphericalCoord.y + 180f);
		int yFloor = Mathf.FloorToInt(sphericalCoord.y + 180f);

		//place all the points on a 2 by to grind
		//1 2
		//3 4
		float point2 = dragCoefGraph[xCeil, yCeil];
		float point3 = dragCoefGraph[xFloor, yFloor];
		float point1 = dragCoefGraph[xCeil, yFloor];
		float point4 = dragCoefGraph[xFloor, yCeil];
	
		//get each points distence from the crafts current orentation 
		float xPointOffset = (float)xCeil - sphericalCoord.x;
		float revXPointOffset = 1f - xPointOffset;

		float yPointOffset = (float)yCeil - sphericalCoord.y;
		float revYPointOffset = 1f - yPointOffset; 
	
		//get percentage of point 1 and 2 as well as point 3 and 4
		float xPointMag12 = point1 * revXPointOffset + point2 * xPointOffset;
		float xPointMag34 = point3 * revXPointOffset + point4 * xPointOffset;

		//consilodates all the points in the final value
		dragCoefficient = xPointMag34 * yPointOffset + xPointMag12 * revYPointOffset;


		//http://en.wikipedia.org/wiki/Drag_coefficient
		//sqrMagnitude is faster than getting magnitude and squaring it
		dragForce = 0.5f * airDensity * objectVelcoity.sqrMagnitude * dragCoefficient;

	}

	#region get sperical coordnits

	private Vector2 CartesianToPolar(Vector3 volocity){
		Vector2 polar;
		
		//calc longitude
		polar.y = Mathf.Atan2(volocity.x, volocity.z);
		
		//this is easier to write and read than sqrt(pow(x,2), pow(y,2))!
		var xzLen = new Vector2(volocity.x, volocity.z).magnitude; 
		//atan2 does the magic
		polar.x = Mathf.Atan2(-volocity.y, xzLen);
		
		//convert to deg
		polar *= Mathf.Rad2Deg;
		
		return polar;
	}

	#endregion

	#region get and set

	public float[] getSphericalCoord(){
		return new float[] { sphericalCoord.x, sphericalCoord.y };
	}

	public Vector3 getfinalDragCalculation(){
		return finalDragCalculation;
	}

	#endregion
}
using UnityEngine;
using System.Collections;
using UnityEditor;




class DrawBezierHandle : Editor {
	private Vector3[] pointOne;
	private Vector3[] pointTwo;
	private Vector3[] handleOne;
	private Vector3[] handleTwo;
	private Color W;
	private int numberOfGraphs;

	void Awake(){
		numberOfGraphs = 2;

		pointOne = new Vector3[numberOfGraphs];
		pointTwo = new Vector3[numberOfGraphs];
		handleOne = new Vector3[numberOfGraphs];
		handleTwo = new Vector3[numberOfGraphs];


		W = Color.white; 
	}

	 void OnSceneGUI() {

		GameObject graphHolder;

		for (int x = 0; x < numberOfGraphs; x++) {
			graphHolder = GameObject.Find("Phisics curves"+x);
			
			pointOne[x] = graphHolder.transform.FindChild("point1").transform.position;
			pointTwo[x] = graphHolder.transform.FindChild("point2").transform.position;
			handleOne[x] = graphHolder.transform.FindChild("handle1").transform.position;
			handleTwo[x] = graphHolder.transform.FindChild("handle2").transform.position;
		}


		float width = 2f+HandleUtility.GetHandleSize(Vector3.zero) * 0.01f;
		for(int x = 0; x < pointOne.Length;x++){
			Handles.DrawBezier(pointOne[x],pointTwo[x],handleOne[x],handleTwo[x],W,null,width);
		}
	}



}

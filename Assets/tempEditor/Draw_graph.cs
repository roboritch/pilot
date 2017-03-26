using UnityEngine;
using System.Collections;
using UnityEditor;
/*
 * This is an editor program the draws graphs sent from the asoceated Equation creater
 * 
 */



//the graph is always displayed
[CustomEditor(typeof(GameObject))]
public class Draw_graph : Editor {

	private Vector3 origin; // where the graph is in world space
	
	private int numberOfPoints; // the number of lines to make for the graph
	private GUIStyle TextStyle;
	private float fontSize;
	private int pointLocation;
	private Vector3[] points;
	private float labelSize;
	private int lableRoundDP;

	private Equation_Creator[] equations;

	private GameObject[] origins;

	void Awake(){
		TextStyle = new GUIStyle();

	}

	void OnSceneGUI(){
		equations = GameObject.Find("GraphDisplay").transform.GetComponent<Graph_display>().getScriptLocation().GetComponents<Equation_Creator>();
		origins = GameObject.FindGameObjectsWithTag("Graph"); 
		for (int i = 0; i < equations.Length; i++){
			origin = origins[i].transform.position; 

			points = equations[i].getPoints();
			labelSize = equations[i].getLableSize();
			if(points.Length <=0 || points == null){
				return;
			}

			drawName(i);

			drawGraph();
			
			//drawLables();
			
			drawGridLines();
		}



	}
	
	/// <summary>
	/// Draws the graph using points gotten from Equation_Creator
	/// the number of points is specifyed by the user
	/// </summary>
	private void drawGraph(){
		//points.Length must be sutracted by 2 due to a rounding error
		Handles.color = Color.red;
		for(int x = 0; x < points.Length-2;x++){
			Handles.DrawLine(origin + points[x],origin + points[x+1]);
		}
	}

	private void drawName(int equationNumber){
		TextStyle.normal.textColor = Color.white;
		TextStyle.fontStyle = FontStyle.Bold;
		Handles.Label(origin + new Vector3(0,points[0].y + 3f),equations[equationNumber].getEquationName(),TextStyle);
	}


	/// <summary>
	/// gives the user information on 4 spaced points on the graph
	/// </summary>
	private void drawLables(){
				
		TextStyle.normal.textColor = Color.white;
		TextStyle.fontStyle = FontStyle.Bold;


		pointLocation = 0;
		lableRoundDP = (int)Mathf.Pow(10f,3f);
		TextStyle.fontSize =  Mathf.FloorToInt(labelSize);
		Handles.Label(origin + points[pointLocation],(Mathf.Round(points[pointLocation].y * lableRoundDP)/lableRoundDP) + " y & " + (Mathf.Round(points[pointLocation].x * lableRoundDP)/lableRoundDP) + " x",TextStyle);

		pointLocation = points.Length/4;
		TextStyle.fontSize =  Mathf.FloorToInt(labelSize);
		Handles.Label(origin + points[pointLocation],(Mathf.Round(points[pointLocation].y * lableRoundDP)/lableRoundDP) + " y & " + (Mathf.Round(points[pointLocation].x * lableRoundDP)/lableRoundDP) + " x",TextStyle);

		pointLocation = 3*points.Length/4;
		TextStyle.fontSize =  Mathf.FloorToInt(labelSize);
		Handles.Label(origin + points[pointLocation],(Mathf.Round(points[pointLocation].y * lableRoundDP)/lableRoundDP) + " y & " + (Mathf.Round(points[pointLocation].x * lableRoundDP)/lableRoundDP) + " x",TextStyle);

		pointLocation = points.Length - 2;
		TextStyle.fontSize =  Mathf.FloorToInt(labelSize);
		Handles.Label(origin + points[pointLocation],(Mathf.Round(points[pointLocation].y * lableRoundDP)/lableRoundDP) + " y & " + (Mathf.Round(points[pointLocation].x * lableRoundDP)/lableRoundDP) + " x",TextStyle);

	}


	/// <summary>
	/// Draws the grid lines encompacing the graph
	/// Slow with large graphs
	/// </summary>
	private void drawGridLines(){
		Handles.color = Color.white;
		
		//finds the largest and smallest values
		float smallestX = points[0].x;
		float smallestY = points[0].y;
		float largestX = points[0].x;
		float largestY = points[0].y;
		for(int i=0;i<points.Length;i++){
			if (points[i].x < smallestX){
				smallestX = points[i].x;
			}
			if (points[i].y < smallestY){
				smallestY = points[i].y;
			}
			if (points[i].x > largestX){
				largestX = points[i].x;
			}
			if (points[i].y > largestY){
				largestY = points[i].y;
			}
		}
		
		smallestX =  Mathf.Round(smallestX-1f);
		smallestY =  Mathf.Round(smallestY-1f);
		largestX = Mathf.Round(largestX+1f);
		largestY = Mathf.Round(largestY+1f);
		
		for (int i = (int)smallestX; i < (int)largestX+1; i+=2) {
			Handles.DrawLine(origin + new Vector3(i,largestY),origin + new Vector3(i,smallestY));
		}
		
		for (int i = (int)smallestY; i < (int)largestY+1; i+=2) {
			Handles.DrawLine(origin + new Vector3(smallestX,i),origin + new Vector3(largestX,i));
		}
		
		
		Handles.color = Color.blue;
		
		Handles.DrawLine(origin + new Vector3(smallestX,0),origin + new Vector3(largestX,0));
		Handles.DrawLine(origin + new Vector3(0,smallestY),origin + new Vector3(0,largestY));
	}




}

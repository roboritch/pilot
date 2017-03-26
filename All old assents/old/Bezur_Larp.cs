using UnityEngine;
using System.Collections;

public class Bezur_Larp : MonoBehaviour {

	private Vector2[] points;

	 Bezur_Larp(GameObject graphToUse, int numberOfPoints){
		points = new Vector2[numberOfPoints];
	
		Debug.Log("begining construction of larp at" + Time.time + "seconds");
		for (int x = 0; x < points.Length; x++) {
			transform.GetComponent<bezir_curves>().CalculateBezierPoint(x/points.Length,graphToUse);
		}
			
	}



}
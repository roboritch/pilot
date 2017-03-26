using UnityEngine;
using System.Collections;
 


//base formula and code adapted from http://devmag.org.za/2011/04/05/bzier-curves-a-tutorial/
//this code is capable of creating a basic Bezier curve from 4 objects in 3D space
//the code will display the curve for editing, but its purpose is to take an time input and provide a point output in a 2D plane 
//the intention is to use Bezier curves in place of complex formulas to calculate values
public class bezir_curves : MonoBehaviour {

	// this code largly copied from http://devmag.org.za/2011/04/05/bzier-curves-a-tutorial/
	public Vector3 CalculateBezierPoint(float time,GameObject graphObject){
		float u = 1f - time;
		float tt = time*time;
		float uu = u*u;
		float uuu = uu * u;
		float ttt = tt * time;
		
		Vector3 point = uuu * graphObject.transform.FindChild("point1").transform.localPosition; //first term
		point += 3f * uu * time * graphObject.transform.FindChild("point2").transform.localPosition; //second term
		point += 3f * u * tt * graphObject.transform.FindChild("handle1").transform.localPosition; //third term
		point += ttt * graphObject.transform.FindChild("handle2").transform.localPosition; //fourth term
		
		return point;
	}
}

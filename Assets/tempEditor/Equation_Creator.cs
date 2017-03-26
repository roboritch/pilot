using UnityEngine;
using System.Collections;



public enum currentEquation{
	linear,xSquared,sqrtX,xCubed,exponental
}

public enum currentRotation{
	xFliped,normal
}

/// <summary>
/// This class formes an equation based on a number of factors 
/// </summary>
[System.Serializable]
public class Equation_Creator:MonoBehaviour{
	public string equationName;

	public currentEquation equation = currentEquation.linear; 
	public currentRotation equationRotation = currentRotation.normal;
	
	[Range(0.001f, 5f)]
	public float yScail;
	[Range(-0.05f,0.05f)]
	public float yScailAdjust;
	
	public Vector2 zeroPoint;

	public float lowestXCutoff;
	public float highestXCutoff;
	public float exponetBase = 2f;

	//does not affect equation 
	public float lowestX;
	public float highestX;



	[Range(2,500)]
	public int numberOfPoints;
	public float lableSize = 1f;

	private float currentYScail = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="Equation_Creator"/> class.
	/// </summary>
	/// <param name="ySca">yScail</param>
	/// <param name="zeroP">zeroPoint</param>
	/// <param name="equ">Equ.</param>
	/// <param name="rot">Rot.</param>
	public Equation_Creator(float ySca, Vector2 zeroP, int equ, int rot, float exBase){
		yScail = ySca;
		zeroPoint = zeroP;
		equation = (currentEquation)equ;
		equationRotation = (currentRotation)rot;
		exponetBase = exBase;
	}



	/// <summary>
	/// Finds the Y value based on information given.
	/// </summary>
	/// <returns>The Y value.</returns>
	/// <param name="x">The x coordinate.</param>
	public Vector3 findYValue(float x){
		float currentY = x;
		currentYScail = (yScail + yScailAdjust);


		if(x<lowestXCutoff){
			currentY = lowestXCutoff;
		}else if(x>highestXCutoff){
			currentY = highestXCutoff;
		}

		switch(equation){ //detects what equation
		case currentEquation.linear:
			currentY *= currentYScail; // these do opposit but simler things
			currentY += -1f*zeroPoint.x * (currentYScail); // moves x intercept
			if(equationRotation == currentRotation.xFliped){
				currentY = -currentY;
				currentY += -zeroPoint.y; // moves y intercept 
			}else{
				currentY += zeroPoint.y; // moves y intercept
			}
			break;
		case currentEquation.xSquared:
			currentY -= zeroPoint.x; // move the graph left and right 
			currentY *= currentY; // square the number
			currentY *= currentYScail;

			if(equationRotation == currentRotation.xFliped){
				currentY *= -1f;
			}
			currentY += zeroPoint.y; // move the graph up and down
			break;
		case currentEquation.sqrtX:
			currentY -= zeroPoint.x;
			currentY = Mathf.Sqrt(Mathf.Abs(currentY)); // will allways use abs value of y
			currentY *= currentYScail;
			if(equationRotation == currentRotation.xFliped){
				currentY *= -1f;
			}
			currentY += zeroPoint.y;
			break;
		case currentEquation.exponental:

			currentY -= zeroPoint.x;
			if(exponetBase  <= 0.0001f){
				exponetBase = 0.0001f;
			}

			currentY = Mathf.Pow(exponetBase,currentY);
			currentY *= currentYScail;
			if(equationRotation == currentRotation.xFliped){
				currentY *= -1f;
			}
			currentY += zeroPoint.y;

			break;
		}


		return new Vector3(x,currentY);
	}


	public Vector3[] getPoints(){
		Vector3[] points = new Vector3[numberOfPoints+1]; 
		int index = 0;
		float currentPoint = lowestX;

		for (float x = -(float)numberOfPoints/2; x < (float)numberOfPoints/2; x++) {
			points[index] = findYValue(currentPoint);
			currentPoint += ((-1*lowestX)+highestX)/numberOfPoints;
			index++;
		}

		return points;
	}

	public float getLableSize(){
		return lableSize;
	}

	public string getEquationName(){
		return equationName;
	}

	public string equationOutput(){
		string equationString = "";

		equationString += currentYScail + "f ,";
		equationString += "new Vector2(" + zeroPoint.x +"f , " + zeroPoint.y + "f) ,";
		equationString += (int)equation + ",";
		equationString += (int)equationRotation + ",";
		equationString += exponetBase + "f";

		return equationString;
	}
}

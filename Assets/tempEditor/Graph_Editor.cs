using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Equation_Creator))]
public class Graph_Editor : Editor {
	Equation_Creator equationScript;

	void Awake() {
	

	}

	[SerializeField]
	public override void OnInspectorGUI(){
		DrawDefaultInspector();
		equationScript = (Equation_Creator)target;
		// float ySca, Vector2 zeroP, currentEquation equ, currentRotation rot
		if(GUILayout.Button("copy equation to clipboard")){
			EditorGUIUtility.systemCopyBuffer = equationScript.equationOutput();
		}


		//scriptTarget.set

	}

}

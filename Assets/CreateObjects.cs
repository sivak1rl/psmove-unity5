using UnityEngine;
using System.Collections;

public class CreateObjects : MonoBehaviour {
    private VRCursorController controller;
    public GameObject littleSquare;

	// Use this for initialization
	void Start () {
        controller = new VRCursorController();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	    if(controller.m_bIsPressed)
        {
            GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
	}
}

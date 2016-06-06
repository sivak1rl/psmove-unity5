using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIEventHandler : MonoBehaviour {

    public Text OutputWindow;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnAddTextClicked()
    {
        OutputWindow.text += "\nClicked!";
    }

    public void OnClearTextClicked()
    {
        OutputWindow.text = "";
    }
}
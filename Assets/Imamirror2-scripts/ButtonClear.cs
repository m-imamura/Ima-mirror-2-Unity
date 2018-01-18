using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClear : MonoBehaviour {

    public GameObject root_obj;
    private Root root_scr;

	// Use this for initialization
	void Start () {
        root_scr = root_obj.GetComponent<Root>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void all_clear() {
        root_scr.clear_all_shape_actor();
        return;
    }
}

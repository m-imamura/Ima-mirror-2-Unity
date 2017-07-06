using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour {

    // Rootオブジェクト
    public GameObject _root;
    private Root _root_script;

	// Use this for initialization
	void Start () {
        _root_script = _root.GetComponent<Root>();
        if (_root_script == null) {
            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void exchange() {
        if (_root_script == null)
        {
            return;
        }
        _root_script.get_init_data();
    }
}

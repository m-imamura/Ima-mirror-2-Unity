using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBackgroundScript : MonoBehaviour {

    // Rootオブジェクト
    public GameObject _background;
    private Background _back_script;

    // Use this for initialization
    void Start () {
        _back_script = _background.GetComponent<Background>();
        if (_back_script == null)
        {
            return;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void get_background() {
        if (_back_script == null) {
            return;
        }
        _back_script.get_background_data();
    }

    public void set_on_background() {
        _back_script.on_background();
    }

    public void set_off_background()
    {
        _back_script.off_background();
    }
}

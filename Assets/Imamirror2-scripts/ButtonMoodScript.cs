using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMoodScript : MonoBehaviour {

    public GameObject _root_object;
    private Root _root_script;
    private HighTouch _hightouch_script;

    private Text mode_text;

    private bool pre_mode = true;

	// Use this for initialization
	void Start () {

        _root_script = _root_object.GetComponent<Root>();
        _hightouch_script = _root_object.GetComponent<HighTouch>();

        mode_text = GetComponentInChildren<Text>();

        // 最初はプレモード
        _root_script.pre_body_mode = true;
        _hightouch_script.ready = false;
        pre_mode = true;
        mode_text.text = "Mode:PreBody";

        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void change_mode() {
        if (pre_mode)
        {
            _root_script.pre_body_mode = false;
            _hightouch_script.ready = true;
            pre_mode = false;
            mode_text.text = "Mode:HighTouch";
        }
        else
        {
            _root_script.pre_body_mode = true;
            _hightouch_script.ready = false;
            pre_mode = true;
            mode_text.text = "Mode:PreBody";
        }
    }
}

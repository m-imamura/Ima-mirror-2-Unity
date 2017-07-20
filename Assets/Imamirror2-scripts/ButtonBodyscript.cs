using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBodyscript : MonoBehaviour {

    // 外部オブジェクト
    public GameObject _root_object;
    private Root _root_script;

    // pre_body番号
    public int pre_body = -1;

    // define
    private int BODY_NUM = 6;

	// Use this for initialization
	void Start () {
        _root_script = _root_object.GetComponent<Root>();
        if (_root_script == null)
            return;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void set_ready() {
        
        if (_root_script == null)
            return;

        for(int i = 0; i < BODY_NUM; i++)
        {
            if (_root_script.set_pre_body_actor(pre_body, i) == true)
                break;
        }

        return;
    }
}

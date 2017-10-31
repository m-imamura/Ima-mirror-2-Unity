using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 背景関係のボタンに共通でつけるスクリプト．
// Root_System の Background.cs に対して，背景の取得，表示，非表示の命令を送る．

public class ButtonBackgroundScript : MonoBehaviour {

    // Rootオブジェクト
    public GameObject _background;
    private Background _back_script;

    // Use this for initialization
    void Start () {
        _back_script = _background.GetComponent<Background>();
        if (_back_script == null)
            return;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void get_background() // 背景を取得
    {
        if (_back_script == null) 
            return;
        
        _back_script.get_background_data();
    }

    public void set_on_background() // 背景を表示
    {
        if (_back_script == null)
            return;
        _back_script.view_background();
    }

    public void set_off_background() // 背景を非表示
    {
        if (_back_script == null)
            return;
        _back_script.off_background();
    }
}

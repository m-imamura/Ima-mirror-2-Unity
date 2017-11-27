using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capture : MonoBehaviour {

    //public bool grab;
    //public Renderer display;

    private bool now_capture = false;

    private int screenshot_count = 0;

    public GameObject _audio_obj;
    private AudioSource _audio_source;

    // Use this for initialization
    void Start() {
        _audio_source = _audio_obj.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {
        if (now_capture) {
            string date_time = 
                System.DateTime.Now.Year.ToString() + "-" +
                System.DateTime.Now.Month.ToString() + "-" +
                System.DateTime.Now.Day.ToString() + "-" +
                System.DateTime.Now.Hour.ToString() + "-" +
                System.DateTime.Now.Minute.ToString() + "-" +
                System.DateTime.Now.Second.ToString();
            ScreenCapture.CaptureScreenshot("Screenshot_" + date_time + ".png");
            Debug.Log("スクリーンショット " + "Screenshot_" + date_time + ".png"  +  " を撮影しました");
            _audio_source.Play();
            now_capture = false;
            screenshot_count++;
        }
    }

    public void capture_button(){
        now_capture = true;
        return;
    }
}

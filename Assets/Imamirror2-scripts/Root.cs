﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class Root : MonoBehaviour {

    // define
    private int BODY_MAX = 6;

    // Kinectセンサー
    private KinectSensor _Sensor;
    

    // Multi
    public GameObject MultiSourceManager; // MaltiSourceMagagerがアタッチされているオブジェクト
    private MultiSourceManager _MultiManager; // ↑のスクリプトを格納

    // Body
    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;
    Windows.Kinect.Body[] body_data;

    // Human
    public GameObject HumanOject_prefab;
    private GameObject[] HumanObject;
    public Human[] human_script;

    // Use this for initialization
    void Start () {

        // センサーを取得
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor == null)
        {
            return;
        }

        // MultiSourceManagerのスクリプト取得
        _MultiManager = MultiSourceManager.GetComponent<MultiSourceManager>();
        if (_MultiManager == null)
        {
            return;
        }

        // BodySourceManagerのスクリプト取得
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        // Humanオブジェクトのインスタンス
        HumanObject = new GameObject[BODY_MAX];
        human_script = new Human[BODY_MAX];
        for (int body = 0; body < BODY_MAX; body++) {
            HumanObject[body] = Instantiate(HumanOject_prefab) as GameObject; // オブジェクトを生成
            human_script[body] = HumanObject[body].GetComponent<Human>(); // スクリプトを取得
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Bodyデータ取得
        if (_BodyManager == null)
        {
            return;
        }
        body_data = _BodyManager.GetData(); // ここでbody_dataを取得
        if (body_data == null)
        {
            return;
        }
        // 追跡できなくなったデータをクリア
        for (int i = 0; i < 6; i++)
        {
            // トラッキングできなくなったらデータをクリアする
            int act = human_script[i].actor_num;　
            if (act == -1)
                continue;

            if (body_data[act].IsTracked == false && human_script[i].ready == true)
                human_script[i].clear_data();
        }
    }

    // 初期データを取る
    public void get_init_data() {
        // Multiデータ取得
        if (_MultiManager == null)
        {
            return;
        }

        // Bodyデータ取得
        if (_BodyManager == null)
        {
            return;
        }
        Body[] body_data = _BodyManager.GetData();
        if (body_data == null)
        {
            return;
        }

        // めも　bodyを基準に構築する．認識範囲を狭める

        // データのクリア?


        // すべてのbodyについて繰り返し
        for (int body = 0; body < BODY_MAX; body++) {
            // bodyがあり，追跡できており，
            if (body_data[body] != null && body_data[body].IsTracked == true)
            {
                if (true)
                { // 深度やポーズの条件
                    Debug.Log("body " + body+ " exist");
                    HumanObject[body].GetComponent<Human>().shape_num = body;
                    HumanObject[body].GetComponent<Human>().actor_num = body;
                    HumanObject[body].GetComponent<Human>().set_init_data(body,body);
                }
            }
            else {
                // humanデータをクリア?
                Debug.Log("ないよ-");
            }
        }
    }

    public void hightouch_exchange(int shape, int actor) {
        //Debug.Log("exchange " + shape + " -> " + actor);
        human_script[shape].shape_num = shape;
        human_script[shape].actor_num = actor;
        human_script[shape].set_init_data(shape, actor);
        return;
    } 
}

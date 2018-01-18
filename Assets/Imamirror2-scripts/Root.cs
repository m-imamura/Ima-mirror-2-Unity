using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class Root : MonoBehaviour {

    // 実験で使用した「あらかじめ用意された身体を使うモード」
    public bool pre_body_mode = true;

    // define
    public int PRE_BODY_NUM = 0;
    private int BODY_MAX = 6;

    // Kinectセンサー
    private KinectSensor _Sensor;
    
    // Body
    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;
    Windows.Kinect.Body[] body_data;

    // Human
    public GameObject HumanOject_prefab;
    private GameObject[] HumanObject;
    public Human[] human_script;
    public Human[] human_script_body;

    // ポーズ認識用
    public GameObject _hightouch_object;
    private HighTouch _hightouch_script;

    // Use this for initialization
    void Start () {

        // センサーを取得
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor == null)
            return;
        
        // BodySourceManagerのスクリプト取得
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
            return;

        _hightouch_script = _hightouch_object.GetComponent<HighTouch>();
        if (_hightouch_script == null)
            return;
        
        // プレ人体モードの初期化
        human_script_body = new Human[PRE_BODY_NUM];
        for (int body = 0; body < PRE_BODY_NUM; body++)
        {
            GameObject body_shape = GameObject.Find("Body_" + body);
            human_script_body[body] = body_shape.GetComponent<Human>();
        }
        
        // ハイタッチモードの初期化
        // Humanオブジェクトのインスタンス
        HumanObject = new GameObject[BODY_MAX];
        human_script = new Human[BODY_MAX];
        for (int body = 0; body < BODY_MAX; body++)
        {
            HumanObject[body] = Instantiate(HumanOject_prefab) as GameObject; // オブジェクトを生成
            human_script[body] = HumanObject[body].GetComponent<Human>(); // スクリプトを取得
        }
        
    }

    // Update is called once per frame
    void Update()
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

        if (pre_body_mode) // プレボディモード
        {
            for (int i = 0; i < PRE_BODY_NUM; i++) // actorを追跡できなくなったデータをクリア
            {
                int act = human_script_body[i].actor_num;
                if (human_script_body[i].ready == true && body_data[act].IsTracked == false)
                    human_script_body[i].clear_data_pre();
            }
        }
        else // ハイタッチモード
        {
            // 追跡できなくなったデータをクリア
            for (int i = 0; i < 6; i++)
            {
                int act = human_script[i].actor_num;
                if (act == -1)
                    continue;

                if (body_data[act].IsTracked == false && human_script[i].ready == true)
                    human_script[i].clear_data();
            }
        }
    }
    
    // "Start"ボタンで呼び出される．自分と入れ替え．
    public void get_init_data() 
    {
        if (_BodyManager == null)
            return;

        Body[] body_data = _BodyManager.GetData();
        if (body_data == null)
            return;
        
        // すべてのbodyについて繰り返し
        for (int body = 0; body < BODY_MAX; body++)
        {
            // bodyがあり，追跡できている．
            if (body_data[body] != null && body_data[body].IsTracked == true)
            {
                int pose_num = _hightouch_script.pose_decision(body_data[body]);
                HumanObject[body].GetComponent<Human>().shape_num = body; // ←shapeとactorが同じなので
                HumanObject[body].GetComponent<Human>().actor_num = body; // ←入れ替えなし
                HumanObject[body].GetComponent<Human>().set_init_data(body, body, pose_num); 
            }
        }
        return;
    }

    // ハイタッチで入れ替わる
    public void hightouch_exchange(int shape, int actor, int pose) {

        // 初回はshapeとactorの番号を入れる
        if (human_script[shape].shape_num != shape || human_script[shape].actor_num != actor) {
            human_script[shape].shape_num = shape;
            human_script[shape].actor_num = actor;
        }

        // 二回目以降はこれだけ実行される
        human_script[shape].set_init_data(shape, actor, pose);

        return;
    }

    // ボタンでプレボディを指定
    public bool set_pre_body_actor(int pre_body, int actor) {

        if (PRE_BODY_NUM <= pre_body) // 無効な引数
            return false;

        body_data = _BodyManager.GetData(); // body_dataを取得
        if (body_data == null)
            return false;

        if (body_data[actor].IsTracked == false) // 無効なactor
            return false;

        // actorに割り当てられているプレボディがあったら関係をクリア
        for (int i = 0; i<PRE_BODY_NUM; i++) {
            if(human_script_body[i].actor_num == actor)
                human_script_body[i].clear_data_pre();
        }

        // 指定されたプレボディにactorを割り当てる
        human_script_body[pre_body].actor_num = actor;
        human_script_body[pre_body].set_init_data(-1, actor, 0);
        Debug.Log("set_pre_body_actor " + pre_body + " -> " + actor);
        
        return true;
    }

    public void clear_all_shape_actor() {
        if (!pre_body_mode)
        {
            for (int i = 0; i < BODY_MAX; i++)
                human_script[i].clear_data();
        }
        else {
            for (int i = 0; i < BODY_MAX; i++)
                human_script_body[i].clear_data_pre();
        }
        return;
    }
}

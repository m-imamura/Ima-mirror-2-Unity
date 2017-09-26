using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class Root : MonoBehaviour {

    public bool pre_body_mode = true;

    // define
    private int BODY_MAX = 6;

    public int PRE_BODY_NUM = 0;

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
    public Human[] human_script_body;

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
            // プレボディにactorをあてがう
            //for (int i = 0; i < BODY_MAX; i++) // Kinectが認識できる人体の数だけ
            //{
            //    if (body_data[i].IsTracked) // 人体がトラッキングできていたら
            //    {
            //        //Debug.Log("body_"+i+" is Tracking");
            //        for (int j = 0; j < PRE_BODY_NUM; j++) // プレ身体の数だけ
            //        {
            //            if (human_script_body[j].ready == false)
            //            {
            //                human_script_body[j].actor_num = i;
            //                human_script_body[j].set_init_data(-1, i);
            //                Debug.Log("human_script_body[ " + j + " ].set_init_data(-2, " + i + ")");
            //            }
            //        }
            //    }
            //}
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

    // 初期データを取る
    public void get_init_data()
    {
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
        for (int body = 0; body < BODY_MAX; body++)
        {
            // bodyがあり，追跡できており，
            if (body_data[body] != null && body_data[body].IsTracked == true)
            {
                if (true)
                { // 深度やポーズの条件
                    Debug.Log("body " + body + " exist");
                    HumanObject[body].GetComponent<Human>().shape_num = body;
                    HumanObject[body].GetComponent<Human>().actor_num = body;
                    HumanObject[body].GetComponent<Human>().set_init_data(body, body, 0);
                }
            }
            else
            {
                // humanデータをクリア?
                Debug.Log("ないよ-");
            }
        }
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
}

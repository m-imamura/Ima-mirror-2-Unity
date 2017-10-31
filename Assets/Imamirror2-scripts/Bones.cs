using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;


public class Bones : MonoBehaviour
{

    // mood
    public bool pre_body_mode;

    // Manager
    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;


    // define
    private int BONES = 24;
    private int JOINTS = 25;


    // ボーンの情報
    public Vector4[] top_init;  //初期位置
    public Vector4[] bottom_init;//初期位置
    public Vector4[] vector_init;//初期方向
    public float[] length;              //長さ (vector_initの)

    public Vector4[] top;       //逐次位置
    public Vector4[] bottom;        //逐次位置
    public Vector4[] vector;        //逐次方向 (bottom -> top)


    // 接続関係
    public Kinect.JointType[] connect_top;    //先端のジョイント番号
    public Kinect.JointType[] connect_bottom; //根元のジョイント番号
    public int[] connect_parent; //親ボーン
    public float[] connect_impactrange; //ボーンの影響範囲


    // joint_position[]のエラー値
    private Vector4[] joint_error_values = new Vector4[25];// 25=JOINTS



    // Use this for initialization
    void Start()
    {
        if (!pre_body_mode) // ハイタッチモードだから初期化が必要
        {
            // 動的割り当て

            // ボーンの情報
            top_init = new Vector4[BONES];  //初期位置
            bottom_init = new Vector4[BONES];//初期位置
            vector_init = new Vector4[BONES];//初期方向

            top = new Vector4[BONES];
            bottom = new Vector4[BONES];        //逐次位置
            vector = new Vector4[BONES];        //逐次方向 (bottom -> top)

            // parent = new int[BONES];            //親ボーン
            length = new float[BONES];          //長さ (vector_initの)

            // ボーンの接続関係
            connect_top = new Kinect.JointType[BONES];
            connect_bottom = new Kinect.JointType[BONES];
            connect_parent = new int[BONES];
            connect_impactrange = new float[BONES];

            define_bone_connect(); // 接続関係を定義

            // joint_positionのエラー用の値をセットする
            joint_error_values = set_joint_error_value();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void set_bones_init_data(int body_num)
    {
        // body_num番の身体をもらってくる．
        Vector4[] joint_position = new Vector4[JOINTS];
        joint_position = get_body_1(body_num);
        if (joint_position == joint_error_values)
        {
            Debug.Log("set_bones_init_data　エラー");
            return;
        }

        // Bodyがあればすべてのボーンについて繰り返し処理
        for (int i = 0; i < BONES; i++)
        {

            // i番目のボーンのtop，bottomがどの関節かを求めて，関節の位置を入れる
            bottom_init[i] = joint_position[(int)connect_bottom[i]];
            top_init[i] = joint_position[(int)connect_top[i]];

            // i番目のボーンのvector(ボーンの向き)をtop-bottomから求める
            vector_init[i] = top_init[i] - bottom_init[i];
            vector_init[i].w = 0.0f; // 正規化や長さを求めるためにいったん0を入れる．

            length[i] = vector_init[i].magnitude; // 長さを求める．
            vector_init[i].Normalize(); // 正規化
            vector_init[i].w = 1.0f;
            
        }

        // C#ではnewに対するdeleteがないらしい（ガベージコレクション）
    }

    // ボーン情報の更新
    public int set_bones_data(int body_num)
    {
        Vector4[] joint_position = new Vector4[JOINTS];
        joint_position = get_body_1(body_num);
        if (joint_position == joint_error_values)
        {
            Debug.Log("set_bones_data　エラー");
            return -1;
        }

        //すべてのボーンについて繰り返し処理
        for (int i = 0; i < BONES; i++)
        {

            // i番目のボーンのtop，bottomがどの関節かを求めて，関節の位置を入れる
            bottom[i] = joint_position[(int)connect_bottom[i]];
            top[i] = joint_position[(int)connect_top[i]];

            // i番目のボーンのvector(ボーンの向き)をtop-bottomから求める
            vector[i] = top[i] - bottom[i];
            vector[i].w = 0.0f;
            vector[i].Normalize();
            vector[i].w = 1.0f;

        }

        /*ポーズ判定*/
        if (vector[5].y > 0 && vector[11].y > 0) // 肩のベクトルで判断
            return 1; // ポーズ
        else if (false) // 他の条件があればここで． 
            return 2;
        else
            return 0;
    }

    private Vector4[] set_joint_error_value()
    {

        Vector4 error_value = new Vector4(0, 0, 0, 0);

        for (int i = 0; i < JOINTS; i++)
            joint_error_values[i] = error_value;

        return joint_error_values;
    }

    private Vector4[] get_body_1(int body_num)
    {

        // Kinect.Joints.Position型では扱えないので，Vector4型のjoint_positon[]に入れなおす．
        Vector4[] joint_position = new Vector4[JOINTS];


        // Managerからデータをとる
        if (BodySourceManager == null)
        {
            return joint_error_values;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return joint_error_values;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return joint_error_values;
        }

        // data[]からbody_num番の身体だけ取り出す
        Kinect.Body my_body = data[body_num];


        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            joint_position[(int)jt].x = my_body.Joints[jt].Position.X;
            joint_position[(int)jt].y = my_body.Joints[jt].Position.Y;
            joint_position[(int)jt].z = my_body.Joints[jt].Position.Z;
            joint_position[(int)jt].w = 1.0f;
        }

        return joint_position;
    }


    // ボーンの接続関係の定義
    private void define_bone_connect()
    {

        for (int i = 0; i < BONES; i++)
        {
            connect_impactrange[i] = 0.5f;
        }

        //体幹
        connect_bottom[0] = Kinect.JointType.SpineBase;//根元のジョイントを設定
        connect_top[0] = Kinect.JointType.SpineMid;//先端のジョイントを設定
        connect_parent[0] = -1;//親のボーンを設定（相対位置の基準）
        connect_impactrange[0] = 0.8f;

        connect_bottom[1] = Kinect.JointType.SpineMid;
        connect_top[1] = Kinect.JointType.SpineShoulder;
        connect_parent[1] = 0;
        connect_impactrange[1] = 0.8f;

        connect_bottom[2] = Kinect.JointType.SpineShoulder;
        connect_top[2] = Kinect.JointType.Neck;
        connect_parent[2] = 1;
        connect_impactrange[2] = 0.4f;

        connect_bottom[3] = Kinect.JointType.Neck;
        connect_top[3] = Kinect.JointType.Head;
        connect_parent[3] = 2;
        connect_impactrange[3] = 1.0f;

        //左腕
        connect_bottom[4] = Kinect.JointType.SpineShoulder;
        connect_top[4] = Kinect.JointType.ShoulderLeft;
        connect_parent[4] = 1;//枝分かれ bone2（首）と同じ立場

        connect_bottom[5] = Kinect.JointType.ShoulderLeft;
        connect_top[5] = Kinect.JointType.ElbowLeft;
        connect_parent[5] = 4;

        connect_bottom[6] = Kinect.JointType.ElbowLeft;
        connect_top[6] = Kinect.JointType.WristLeft;
        connect_parent[6] = 5;

        connect_bottom[7] = Kinect.JointType.WristLeft;
        connect_top[7] = Kinect.JointType.HandLeft;
        connect_parent[7] = 6;

        connect_bottom[8] = Kinect.JointType.HandLeft;
        connect_top[8] = Kinect.JointType.HandTipLeft;
        connect_parent[8] = 7;

        connect_bottom[9] = Kinect.JointType.HandLeft;
        connect_top[9] = Kinect.JointType.ThumbLeft;
        connect_parent[9] = 7;

        //右腕
        connect_bottom[10] = Kinect.JointType.SpineShoulder;
        connect_top[10] = Kinect.JointType.ShoulderRight;
        connect_parent[10] = 1;//枝分かれ bone2（首）と同じ立場

        connect_bottom[11] = Kinect.JointType.ShoulderRight;
        connect_top[11] = Kinect.JointType.ElbowRight;
        connect_parent[11] = 10;

        connect_bottom[12] = Kinect.JointType.ElbowRight;
        connect_top[12] = Kinect.JointType.WristRight;
        connect_parent[12] = 11;

        connect_bottom[13] = Kinect.JointType.WristRight;
        connect_top[13] = Kinect.JointType.HandRight;
        connect_parent[13] = 12;

        connect_bottom[14] = Kinect.JointType.HandRight;
        connect_top[14] = Kinect.JointType.HandTipRight;
        connect_parent[14] = 13;

        connect_bottom[15] = Kinect.JointType.HandRight;
        connect_top[15] = Kinect.JointType.ThumbRight;
        connect_parent[15] = 13;

        //左脚
        connect_bottom[16] = Kinect.JointType.SpineBase;
        connect_top[16] = Kinect.JointType.HipLeft;
        connect_parent[16] = -1;//左脚を独立と考える．親はなし

        connect_bottom[17] = Kinect.JointType.HipLeft;
        connect_top[17] = Kinect.JointType.KneeLeft;
        connect_parent[17] = 16;

        connect_bottom[18] = Kinect.JointType.KneeLeft;
        connect_top[18] = Kinect.JointType.AnkleLeft;
        connect_parent[18] = 17;

        connect_bottom[19] = Kinect.JointType.AnkleLeft;
        connect_top[19] = Kinect.JointType.FootLeft;
        connect_parent[19] = 18;

        //右脚
        connect_bottom[20] = Kinect.JointType.SpineBase;
        connect_top[20] = Kinect.JointType.HipRight;
        connect_parent[20] = -1;//右脚を独立と考える．親はなし

        connect_bottom[21] = Kinect.JointType.HipRight;
        connect_top[21] = Kinect.JointType.KneeRight;
        connect_parent[21] = 20;

        connect_bottom[22] = Kinect.JointType.KneeRight;
        connect_top[22] = Kinect.JointType.AnkleRight;
        connect_parent[22] = 21;

        connect_bottom[23] = Kinect.JointType.AnkleRight;
        connect_top[23] = Kinect.JointType.FootRight;
        connect_parent[23] = 22;


        // テスト用出力
        for (int i = 0; i < BONES; i++)
        {
            //Debug.Log("define_bone_connect(): ボーン "+i+ ": ボトム "+connect_bottom[i]+"トップ "+connect_top[i]);
        }
    }

    public void clear_bones() {
        for (int b = 0; b < BONES; b++ ) {
            top_init[b] = new Vector4(0, 0, 0, 0);
            bottom_init[b] = new Vector4(0, 0, 0, 0);
            vector_init[b] = new Vector4(0, 0, 0, 0);
            length[b] = 0;     

            top[b] = new Vector4(0, 0, 0, 0);
            bottom[b] = new Vector4(0, 0, 0, 0);    
            vector[b] = new Vector4(0, 0, 0, 0);
        }
        Debug.Log("clear bones");
        return;
    }

}
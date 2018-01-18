using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class HighTouch : MonoBehaviour {

    // 機能全体の有効無効
    public bool ready = false;

    // Kinectセンサー
    private KinectSensor _Sensor;

    // Manager
    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;

    // Bone
    private CameraSpacePoint[] BonePOINTS;
    private int BONES = 24;
    private int JOINTS = 25;

    // メインのシステムを取得
    public GameObject _root_object;
    private Root _root_script;

    public float hand_to_hand_distance = 0.15f; // 単位はm

    // 1人用か2人用か
    // 一人でデバッグするときのため
    public bool can_1_person = false;

    // 最後にハイタッチした時間
    private System.DateTime[][] last_hightouch_time;
    
    // Use this for initialization
    void Start () {
		
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
            return;

        _root_script = _root_object.GetComponent<Root>();
        if (_root_script == null)
            return;

        // ハイタッチした時間を記録する
        last_hightouch_time = new System.DateTime[6][];
        for (int i = 0; i < 6; i++)
        {
            last_hightouch_time[i] = new System.DateTime[6];
            for (int j = 0; j < 6; j++)
                last_hightouch_time[i][j] = new System.DateTime(2017, 11, 1, 0, 0, 0); // 初期値
        }
    }

    // Update is called once per frame
    void Update() {
        if (ready)
        {

            if (_BodyManager == null)
                return;

            // data[]に全部の骨格情報を取得する
            Windows.Kinect.Body[] data = _BodyManager.GetData();
            if (data == null)
                return;

            int hand_num = 6 * 2; // 人数×手の数
            UnityEngine.Vector3[] hand_position = new UnityEngine.Vector3[hand_num];
            for (int body_num = 0; body_num < 6; body_num++)
            {
                // 右手と左手の位置を hand_position[]に格納
                UnityEngine.Vector3 joint_position_left_hand = new UnityEngine.Vector3();
                joint_position_left_hand.x = data[body_num].Joints[JointType.HandLeft].Position.X;
                joint_position_left_hand.y = data[body_num].Joints[JointType.HandLeft].Position.Y;
                joint_position_left_hand.z = data[body_num].Joints[JointType.HandLeft].Position.Z;
                UnityEngine.Vector3 joint_position_right_hand = new UnityEngine.Vector3();
                joint_position_right_hand.x = data[body_num].Joints[JointType.HandRight].Position.X;
                joint_position_right_hand.y = data[body_num].Joints[JointType.HandRight].Position.Y;
                joint_position_right_hand.z = data[body_num].Joints[JointType.HandRight].Position.Z;
                hand_position[body_num * 2] = joint_position_left_hand;
                hand_position[body_num * 2 + 1] = joint_position_right_hand;
            }

            for (int i = 0; i < hand_num; i++)
            { // 全手の位置について
                if (hand_position[i] == new UnityEngine.Vector3(0, 0, 0)) // なかったらやめ．
                    continue;

                for (int j = 0; j < hand_num; j++)
                { // 全手の位置を見る

                    if (hand_position[j] == new UnityEngine.Vector3(0, 0, 0)) // なかったらやめ．
                        continue;

                    if (i == j) // 同じ手だったらやめ
                        continue;

                    if (!can_1_person) // 一人でバッグモードでない場合
                        if (i / 2 == j / 2) // 自分の手だったらやめ
                            continue;

                    if ((hand_position[i] - hand_position[j]).magnitude < hand_to_hand_distance)
                    {
                        int body1 = i / 2;
                        int body2 = j / 2;

                        // ここで元に戻る（交換関係を解除する）機能を付ける
                        Debug.Log(System.DateTime.Now - last_hightouch_time[body1][body2]);
                        //if(System.DateTime.Now - last_hightouch_time[body1][body2] > )
                        last_hightouch_time[body1][body2] = System.DateTime.Now;
                        
                        // ポーズ判定メソッドにかける
                        int pose1 = pose_decision(data[body1]);
                        _root_script.hightouch_exchange(body1, body2, pose1);
                        
                        int pose2 = pose_decision(data[body2]);
                        _root_script.hightouch_exchange(body2, body1, pose2);
                        
                    }
                }
            }
        }
        
    }

    // ポーズ判定
    public int pose_decision(Body _data) {
        int pose_num = 0;
        
        // 両腕が上を向いてるという判定(肩回りの改善)
        float LeftShoul_vec1 = _data.Joints[JointType.ElbowLeft].Position.Y - _data.Joints[JointType.ShoulderLeft].Position.Y;
        float RightShoul_vec1 = _data.Joints[JointType.ElbowRight].Position.Y - _data.Joints[JointType.ShoulderRight].Position.Y;
        if (LeftShoul_vec1 > 0 && RightShoul_vec1 > 0)
            pose_num = 1;

        // 他の判定が欲しければここでつくる．
        if (false) 
            pose_num = 2;

        //Debug.Log("ポーズ " + pose_num + "と判定");

        return pose_num;
    }
}

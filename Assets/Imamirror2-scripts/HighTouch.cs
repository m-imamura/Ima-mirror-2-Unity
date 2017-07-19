using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class HighTouch : MonoBehaviour {

    // Kinectセンサー
    private KinectSensor _Sensor;

    // Manager
    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;

    // Bone
    private CameraSpacePoint[] BonePOINTS;
    private int BONES = 24;
    private int JOINTS = 25;

    // 
    private Root _root_script;

    public float hand_to_hand_distance = 0.15f; // m

    // 1人用か2人用か
    public bool can_1_person = false;


    // Particle system
    public  GameObject particle_object;
    private ParticleSystem particle_system;


    // Use this for initialization
    void Start () {
		
        // Managerからデータをとる
        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        _root_script = GetComponent<Root>();


        // パーティクルを作る
        particle_system = particle_object.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update() {

        if (_BodyManager == null)
        {
            return;
        }

        Windows.Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        int hand_num = 6 * 2;
        UnityEngine.Vector3[] hand_position = new UnityEngine.Vector3[hand_num];
        for (int body_num = 0; body_num < 6; body_num++) {
            
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

        for (int i = 0; i < hand_num; i++) { // 全手の位置について
            if (hand_position[i] == new UnityEngine.Vector3(0,0,0))  // なかったらやめ．
                continue;
            
            for (int j = 0; j < hand_num; j++) { // 全手の位置を見る
                if (hand_position[j] == new UnityEngine.Vector3(0, 0, 0))// なかったらやめ．
                    continue;

                if (can_1_person) {
                    if (i == j) // 1人用
                        continue;
                }
                else {
                    if (i / 2 == j / 2) // 自分の手だったらやめ
                        continue;
                }

                if ((hand_position[i] - hand_position[j]).magnitude < hand_to_hand_distance) {
                    //Debug.Log("★タッチ確認 " + (hand_position[i] - hand_position[j]).magnitude);
                    int body1 = i / 2;
                    int body2 = j / 2;
                    _root_script.hightouch_exchange(body1, body2);
                    _root_script.hightouch_exchange(body2, body1);

                    particle_object.transform.position = hand_position[i];
                    particle_system.Play();
                }
            }
        }
        
    }
}

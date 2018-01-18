using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class HighTouchSign : MonoBehaviour
{
    // Kinectセンサー
    private KinectSensor _Sensor;
    
    // Body
    public GameObject BodySourceManager;
    private BodySourceManager _BodyManager;
    
    // Root
    public GameObject RootObject;
    private Root _root;

    // Camera
    private CameraSpacePoint[] CameraSpacePOINTS;

    // mapper
    private CoordinateMapper mapper;

    // Particles
    private ParticleSystem.Particle[] particles;
    public int particle_Max = 6*2;
    public float particle_Size = 5f;

    // 手のポジション
    private Vector3[] hand_position;

    // 手のポジションにつける色
    private Color[] _color;

    void Start()
    {
        // センサーを取得
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor == null)
        {
            return;
        }

        // BodyIndexのスクリプトを取得
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        
        // Root
        _root = RootObject.GetComponent<Root>();
        
        // mapper
        mapper = KinectSensor.GetDefault().CoordinateMapper;

        // パーティクルを作る
        particles = new ParticleSystem.Particle[particle_Max];
        for (int i = 0; i < particle_Max; i++)
        {
            particles[i].position = new Vector3(0, 0, 0);
            particles[i].startSize = particle_Size;
            particles[i].startColor = Color.clear;
        }

        // 手のポジション
        hand_position = new Vector3[6 * 2];
        
    }

    void Update()
    {

        // body関係
        if (BodySourceManager == null)
        {
            return;
        }
        
        // root
        bool[] actor_exixt = new bool[6];
        for (int i = 0; i < 6; i++)
            actor_exixt[i] = false;


        // パーティクルをクリア
        for (int p = 0; p < particle_Max; p++)
            particles[p].position = new Vector3(0, 0, 0);

        // data[]に全部の骨格情報を取得する
        Windows.Kinect.Body[] data = _BodyManager.GetData();

        Vector3[] head_position = new Vector3[6];
        // 無効な値をセット
        for (int i = 0; i < 6; i++) {
            head_position[i] = new Vector3(100, 100, 100);
            hand_position[i * 2 + 0] = new Vector3(100, 100, 100);
            hand_position[i * 2 + 1] = new Vector3(100, 100, 100);
        }

        // 全員の位置を取得する
        for (int body = 0; body < 6; body++)
        {
            if (data[body].IsTracked)
            {
                // 頭
                if (data[body].Joints[JointType.Head].TrackingState == TrackingState.Tracked && _root.human_script[body].ready == false)
                {
                    head_position[body].x = data[body].Joints[JointType.Head].Position.X;
                    head_position[body].y = data[body].Joints[JointType.Head].Position.Y;
                    head_position[body].z = data[body].Joints[JointType.Head].Position.Z;
                }
                // 左手
                if (data[body].Joints[JointType.HandLeft].TrackingState == TrackingState.Tracked && _root.human_script[body].ready == false)
                {
                  hand_position[body * 2 + 0].x = data[body].Joints[JointType.HandLeft].Position.X;
                  hand_position[body * 2 + 0].y = data[body].Joints[JointType.HandLeft].Position.Y;
                  hand_position[body * 2 + 0].z = data[body].Joints[JointType.HandLeft].Position.Z;
                }
                // 右手
                if (data[body].Joints[JointType.HandRight].TrackingState == TrackingState.Tracked && _root.human_script[body].ready == false)
                {
                  hand_position[body * 2 + 1].x = data[body].Joints[JointType.HandRight].Position.X;
                  hand_position[body * 2 + 1].y = data[body].Joints[JointType.HandRight].Position.Y;
                  hand_position[body * 2 + 1].z = data[body].Joints[JointType.HandRight].Position.Z;
                }
            }
        }

        // 頭を基準にソート
        int[] human_soat = new int[6];
        for (int i = 0; i < 6; i++) {
            human_soat[i] = i;
        }
        for (int i = 0; i < 6; i++) {
            int smaller_index = i;
            for(int j = i; j < 6; j++)
            {
                if (head_position[j].x < head_position[smaller_index].x) {
                    Vector3 tmp_vector = head_position[j];
                    head_position[j] = head_position[smaller_index];
                    head_position[smaller_index] = tmp_vector;

                    int tmp_int = human_soat[j];
                    human_soat[j] = human_soat[smaller_index];
                    human_soat[smaller_index] = tmp_int;
                }
            }
        }

        //Debug.Log(human_soat[0] +" "+ human_soat[1] + " " + human_soat[2] + " " + human_soat[3] + " " + human_soat[4] + " " + human_soat[5]);

        // 隣り合う人の手に色付け
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                if (hand_position[human_soat[i] * 2 + j] != new Vector3(100, 100, 100))
                {
                    particles[i * 2 + j].position = hand_position[human_soat[i] * 2 + j] * 10.0f;

                    switch (i * 2 + j)
                    {
                        case 0:
                            break;
                        case 1:
                        case 2:
                            particles[i * 2 + j].startColor = Color.yellow;
                            break;
                        case 3:
                        case 4:
                            particles[i * 2 + j].startColor = Color.cyan;
                            break;
                        case 5:
                        case 6:
                            particles[i * 2 + j].startColor = Color.magenta;
                            break;
                        case 7:
                        case 8:
                            particles[i * 2 + j].startColor = Color.red;
                            break;
                        case 9:
                        case 10:
                            particles[i * 2 + j].startColor = Color.green;
                            break;
                        case 11:
                            break;
                        default:
                            particles[i * 2 + j].startColor = Color.clear;
                            break;
                    }
                }
            }
        }
        
        GetComponent<ParticleSystem>().SetParticles(particles, particles.Length);
    }
 
}

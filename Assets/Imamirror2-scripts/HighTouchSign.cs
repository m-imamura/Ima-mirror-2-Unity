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

        // 無効な値をセット
        for (int i = 0; i < 6 * 2; i++)
            hand_position[i] = new Vector3(100, 100, 100);

        // 全員の手の位置を取得する
        for (int body = 0; body < 6; body++)
        {
            if (data[body].IsTracked)
            {
                if (data[body].Joints[JointType.HandLeft].TrackingState == TrackingState.Tracked)
                {
                  hand_position[body * 2 + 0].x = data[body].Joints[JointType.HandLeft].Position.X;
                  hand_position[body * 2 + 0].y = data[body].Joints[JointType.HandLeft].Position.Y;
                  hand_position[body * 2 + 0].z = data[body].Joints[JointType.HandLeft].Position.Z;
                }
                if (data[body].Joints[JointType.HandRight].TrackingState == TrackingState.Tracked)
                {
                  hand_position[body * 2 + 1].x = data[body].Joints[JointType.HandRight].Position.X;
                  hand_position[body * 2 + 1].y = data[body].Joints[JointType.HandRight].Position.Y;
                  hand_position[body * 2 + 1].z = data[body].Joints[JointType.HandRight].Position.Z;
                }
            }
        }

        // xでソートして色を付ける
        int smaller_index = 0;
        int particle_count = 0;
        for (int i = 0; i < 6 * 2; i++)
        {
            for (int j = 0; j < 6 * 2; j++)
            {
                if (hand_position[j].x < hand_position[smaller_index].x)
                {
                    smaller_index = j;
                }
            }

            if (hand_position[smaller_index] != new Vector3(100, 100, 100))
            {

                particles[particle_count].position = hand_position[smaller_index] * 10.0f;

                switch (particle_count)
                {
                    case 0:
                        break;
                    case 1:
                    case 2:
                        particles[particle_count].startColor = Color.yellow;
                        break;
                    case 3:
                    case 4:
                        particles[particle_count].startColor = Color.cyan;
                        break;
                    case 5:
                    case 6:
                        particles[particle_count].startColor = Color.magenta;
                        break;
                    case 7:
                    case 8:
                        particles[particle_count].startColor = Color.red;
                        break;
                    case 9:
                    case 10:
                        particles[particle_count].startColor = Color.green;
                        break;
                    case 11:
                        break;
                    default:
                        particles[particle_count].startColor = Color.clear;
                        break;
                }
            }
            
            // パーティクル位置に保存したhand_positionに無効な値をセット
            hand_position[smaller_index] = new Vector3(100, 100, 100);
            particle_count++;
        }

        GetComponent<ParticleSystem>().SetParticles(particles, particles.Length);
    }
 
}

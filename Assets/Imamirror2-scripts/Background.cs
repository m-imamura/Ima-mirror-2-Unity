using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class Background : MonoBehaviour
{
    // 背景の情報
    public UnityEngine.Vector4[] points;
    public Color32[] points_color;
    public int points_num = 0;

    // Kinectセンサー
    private KinectSensor _Sensor;

    // Multi
    public GameObject MultiSourceManager; // MaltiSourceMagagerがアタッチされているオブジェクト
    private MultiSourceManager _MultiManager; // ↑のスクリプトを格納

    // Color
    private Texture2D ColorDATA; // 色を取るためのテクスチャ
    private ColorSpacePoint[] ColorSpacePOINTS; // cameraやdepthとマッピングするため
    private int color_height;
    private int color_width;

    // Depth
    private ushort[] DepthDATA;
    private int depth_width;
    private int depth_height;

    // Camera
    private CameraSpacePoint[] CameraSpacePOINTS;

    // mapper
    private CoordinateMapper mapper;

    // Particles
    public ParticleSystem.Particle[] particles;
    public int particle_Max = 10000; // パーティクルの最大個数
    public float particle_Size = 2f; // パーティクルのサイズ
    public int particle_density = 4; // パーティクルの密度．1が全く間引かない

    private ParticleSystem.Particle[] particles_no; // 非表示用パーティクル

    private bool background_switch = true;


    // Use this for initialization
    void Start()
    {
        points = new UnityEngine.Vector4[particle_Max];
        points_color = new Color32[particle_Max];

        // センサーを取得
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor == null)
        {
            return;
        }

        _MultiManager = MultiSourceManager.GetComponent<MultiSourceManager>();
        if (_MultiManager == null)
        {
            return;
        }

        // DepthDATA関係
        depth_width = (int)_Sensor.DepthFrameSource.FrameDescription.Width;
        depth_height = (int)_Sensor.DepthFrameSource.FrameDescription.Height;
        DepthDATA = new ushort[depth_width * depth_height];

        // ColorDATA関係
        color_width = _MultiManager.ColorWidth;
        color_height = _MultiManager.ColorHeight;
        ColorDATA = new Texture2D(color_width, color_height, TextureFormat.RGBA32, false);
        ColorSpacePOINTS = new ColorSpacePoint[depth_width * depth_height];

        // Camera関係
        CameraSpacePOINTS = new CameraSpacePoint[depth_width * depth_height];

        // mapper
        mapper = KinectSensor.GetDefault().CoordinateMapper;

        // パーティクルを作る
        particles = new ParticleSystem.Particle[particle_Max];
        for (int i = 0; i < particle_Max; i++)
        {
            particles[i].position = new Vector3(0, 0, 0);
            particles[i].startSize = 0;
            particles[i].startColor = Color.black;
        }
        // 非表示用パーティクル
        particles_no = new ParticleSystem.Particle[1];
        particles_no[0].position = new Vector3(0, 0, 0);
        particles_no[0].startSize = 0;
        particles_no[0].startColor = Color.clear;

    }

    // Update is called once per frame
    void Update()
    {

    }

    // 背景を取得する
    public void get_background_data()
    {
        if (_MultiManager == null)
            return;

        // 各種データを取得
        ColorDATA = _MultiManager.GetColorTexture();
        DepthDATA = _MultiManager.GetDepthData();

        // mapper
        mapper.MapDepthFrameToCameraSpace(DepthDATA, CameraSpacePOINTS);
        mapper.MapDepthFrameToColorSpace(DepthDATA, ColorSpacePOINTS);

        // Depthデータを基準にパーティクルを表示する
        int particle_count = 0;
        for (int y = 0; y < depth_height; y += particle_density)
        {
            for (int x = 0; x < depth_width; x += particle_density)
            {
                int index = y * depth_width + x;

                if (particle_count < particle_Max)
                {
                    // 座標取得
                    float p_x = CameraSpacePOINTS[index].X;
                    float p_y = CameraSpacePOINTS[index].Y;
                    float p_z = CameraSpacePOINTS[index].Z;

                    // 色取得
                    int color_x = (int)ColorSpacePOINTS[index].X;
                    int color_y = (int)ColorSpacePOINTS[index].Y;
                    Color32 color = ColorDATA.GetPixel(color_x, color_y);
                    
                    // 初期点データに代入
                    points[particle_count].x = p_x;
                    points[particle_count].y = p_y;
                    points[particle_count].z = p_z;
                    points[particle_count].w = 1.0f;
                    points_color[particle_count] = color;

                    particle_count++;
                }
            }
        }
        points_num = particle_count;
        return;
    }

    // 背景を表示する
    public void view_background()
    {
        for (int p = 0; p < points_num; p++)
        {
            particles[p].position = new Vector3(points[p].x * 10f, points[p].y * 10f, points[p].z * 10f);
            particles[p].startSize = particle_Size;
            particles[p].startColor = points_color[p];
        }
        GetComponent<ParticleSystem>().SetParticles(particles, particles.Length);
        return;
    }

    // 背景を非表示にする
    public void off_background()
    {
        GetComponent<ParticleSystem>().SetParticles(particles_no, particles_no.Length);
        return;
    }
}

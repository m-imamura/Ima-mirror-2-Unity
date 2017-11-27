using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class BodyIndexView : MonoBehaviour
{
    // Kinectセンサー
    private KinectSensor _Sensor;

    // Multi
    public GameObject MultiSourceManager; // MaltiSourceMagagerがアタッチされているオブジェクト
    private MultiSourceManager _MultiManager; // ↑のスクリプトを格納
    
    // BodyIndex
    public GameObject BodyIndexSourceManager;
    private BodyIndexSourceManager _BodyIndexManager;
    private byte[] IndexDATA;
    private int index_height;
    private int index_width;
    
    // Color
    private Texture2D ColorDATA; // 色を取るためのテクスチャ
    private ColorSpacePoint[] ColorSpacePOINTS; // cameraやdepthとマッピングするため
    private int color_height;
    private int color_width;

    // Depth
    private ushort[] DepthDATA;
    private int depth_width;
    private int depth_height;

    // Root
    public GameObject RootObject;
    private Root _root;
    
    // Camera
    private CameraSpacePoint[] CameraSpacePOINTS;

    // mapper
    private CoordinateMapper mapper;

    // Particles
    private ParticleSystem.Particle[] particles;
    public int particle_Max = 1000;
    public float particle_Size = 1f;
    public int particle_density = 4; // パーティクル密度．何個間引くか．1以上整数

    void Start()
    {
        // センサーを取得
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor == null) {
            return;
        }

        // BodyIndexのスクリプトを取得
        _BodyIndexManager = BodyIndexSourceManager.GetComponent<BodyIndexSourceManager>();
        if (_BodyIndexManager == null)
        {
            return;
        }

        // MultiSourceManagerのスクリプト取得
        _MultiManager = MultiSourceManager.GetComponent<MultiSourceManager>();
        if (_MultiManager == null) {
            return;
        }
        
        // BodyIndexDATA関係
        index_width = _Sensor.BodyIndexFrameSource.FrameDescription.Width;
        index_height = _Sensor.BodyIndexFrameSource.FrameDescription.Height;
        IndexDATA = new byte[index_width * index_height];
        
        // DepthDATA関係
        depth_width = (int)_Sensor.DepthFrameSource.FrameDescription.Width;
        depth_height = (int)_Sensor.DepthFrameSource.FrameDescription.Height;
        DepthDATA = new ushort[depth_width * depth_height];

        // ColorDATA関係
        color_width = _MultiManager.ColorWidth;
        color_height = _MultiManager.ColorHeight;
        ColorDATA = new Texture2D(color_width, color_height, TextureFormat.RGBA32, false);
        ColorSpacePOINTS = new ColorSpacePoint[depth_width * depth_height];

        // Root
        _root = RootObject.GetComponent<Root>();
        
        // Camera関係
        CameraSpacePOINTS = new CameraSpacePoint[depth_width * depth_height];

        // mapper
        mapper = KinectSensor.GetDefault().CoordinateMapper;

        // パーティクルを作る
        particles = new ParticleSystem.Particle[particle_Max];
        for (int i = 0; i < particle_Max; i++) {
            particles[i].position = new Vector3(0, 0, 0);
            particles[i].startSize = 0;
            particles[i].startColor = Color.black;
        }
        
    }

    void Update()
    {

        // index関係
        if (BodyIndexSourceManager == null)
        {
            return;
        }
        // multi関係
        if (MultiSourceManager == null) {
            return;
        }

        // 各種データを取得
        IndexDATA = _BodyIndexManager.GetData();
        ColorDATA = _MultiManager.GetColorTexture();
        DepthDATA = _MultiManager.GetDepthData();

        // mapper
        mapper.MapDepthFrameToCameraSpace(DepthDATA, CameraSpacePOINTS);
        mapper.MapDepthFrameToColorSpace(DepthDATA, ColorSpacePOINTS);

        // root
        bool[] actor_exixt = new bool[6];
        for (int i = 0; i < 6; i++)
            actor_exixt[i] = false;
        

        // パーティクルをクリア
        for (int p = 0; p < particle_Max; p++)
            particles[p].position = new Vector3(0, 0, 0);

        // Depthデータを基準にパーティクルを表示する
        int particle_count = 0;
        for (int y=0;y<depth_height; y+=particle_density) { 
            for (int x =0;x<depth_width; x+=particle_density) {

                int index = y * index_width + x;

                if (particle_count < particle_Max)
                {
                    if (IndexDATA[index] != 255)
                    {
                        int j = IndexDATA[index];
                        if (_root.human_script[j].actor_num == -1) {
                            // Debug.Log("+ " + IndexDATA[index]);
                            // 座標取得
                            float p_x = CameraSpacePOINTS[index].X * 10;
                            float p_y = CameraSpacePOINTS[index].Y * 10;
                            float p_z = CameraSpacePOINTS[index].Z * 10;

                            // 色取得
                            int color_x = (int)ColorSpacePOINTS[index].X;
                            int color_y = (int)ColorSpacePOINTS[index].Y;
                            Color32 color = ColorDATA.GetPixel(color_x, color_y);

                            particles[particle_count].position = new Vector3(p_x, p_y, p_z);
                            particles[particle_count].startSize = particle_Size;
                            particles[particle_count].startColor = color;
                            particle_count++;
                        }
                    }
                }
            }
        }
        GetComponent<ParticleSystem>().SetParticles(particles, particles.Length);
        
    }
}

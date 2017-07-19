using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class Points : MonoBehaviour {
    
    // 標本点の情報
    public UnityEngine.Vector4[] points_init;   //初期位置の点(Camera座標) 最大POINTS_XYZ_NUM個
    public UnityEngine.Vector4[] points;        //変換後の点(Camera座標) 最大POINTS_XYZ_NUM個
    public Color32[] points_color;         //色
    public int points_num;          //点の総数
    public int body_num;            //身体番号
    
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

    // Bone
    private CameraSpacePoint[] BonePOINTS;
    private int BONES = 24;
    private int JOINTS = 25;

    // Camera
    private CameraSpacePoint[] CameraSpacePOINTS;

    // mapper
    private CoordinateMapper mapper;

    // Particles
    public ParticleSystem.Particle[] particles;
    public int particle_Max = 100;
    public float particle_Size = 1f;
    public int particle_density = 4; // パーティクル密度．何個間引くか．1以上整数


    // Use this for initialization
    void Start () {
        points_init = new UnityEngine.Vector4[particle_Max];
        points = new UnityEngine.Vector4[particle_Max];
        points_color = new Color32[particle_Max];


        // センサーを取得
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor == null)
        {
            return;
        }

        _BodyIndexManager = BodyIndexSourceManager.GetComponent<BodyIndexSourceManager>();
        if (_BodyIndexManager == null)
        {
            return;
        }

        _MultiManager = MultiSourceManager.GetComponent<MultiSourceManager>();
        if (_MultiManager == null)
        {
            return;
        }

        // BodyIndexDATA関係
        index_width = _Sensor.BodyIndexFrameSource.FrameDescription.Width;
        index_height = _Sensor.BodyIndexFrameSource.FrameDescription.Height;
        IndexDATA = new byte[index_width * index_height];

        // DepthDATA関係
        depth_width = _Sensor.DepthFrameSource.FrameDescription.Width;
        depth_height = _Sensor.DepthFrameSource.FrameDescription.Height;
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
	}
	
	// Update is called once per frame
	void Update () {

    }

    // getinitしたときのデータをもらって保存しておく関数
    public void set_points_data(int person)
    {
        body_num = person;
        if (_MultiManager == null) {
            return;
        }
        if (_BodyIndexManager == null) {
            return;
        }

        // 各種データを取得
        IndexDATA = _BodyIndexManager.GetData();
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
                int index = y * index_width + x;

                if (particle_count < particle_Max)
                {
                    if (IndexDATA[index] == body_num)
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
                        points_init[particle_count].x = p_x;
                        points_init[particle_count].y = p_y;
                        points_init[particle_count].z = p_z;
                        points_init[particle_count].w = 1.0f;
                        points_color[particle_count] = color;

                        particles[particle_count].startSize = particle_Size;
                        particles[particle_count].startColor = points_color[particle_count];
                        
                        particle_count++;
                    }
                }
            }
        }
        points_num = particle_count;
        
        return;
    }

    public void view_trans_points() {

        for (int p =0; p<points_num; p++) {
            particles[p].position = new Vector3(points[p].x * 10f, points[p].y * 10f, points[p].z * 10f);
            particles[p].startSize = particle_Size;
            particles[p].startColor = points_color[p];
        }
        GetComponent<ParticleSystem>().SetParticles(particles, particles.Length);
    }

    public void clear_points() {
        for (int p = 0; p < particle_Max; p++) // パーティクルを全部クリアするとうまくいく．
        {
            points[p] = points_init[p] = new UnityEngine.Vector4(0, 0, 0, 0);
            particles[p].position = new Vector3(0, 0, 0);
        }
        GetComponent<ParticleSystem>().SetParticles(particles, particles.Length);
        Debug.Log("clear points body " + body_num);
        return;
    }
}

// DepthParticlize.cs

using UnityEngine;
using Windows.Kinect;
using System.Collections;
using System;
public class Particle1 : MonoBehaviour
{

    // FROM KINECT v2
    public GameObject multiSourceManager;
    MultiSourceManager multiSourceManagerScript;
    FrameDescription depthFrameDesc;
    CameraSpacePoint[] cameraSpacePoints;
    CoordinateMapper mapper;

    private int depthWidth;
    private int depthHeight;

    // PARTICLE SYSTEM
    private ParticleSystem.Particle[] particles2;


    // DRAW CONTROL
    public Color color = Color.white;
    public Color color2 = Color.white;
    float size = 1.0f;
    public float scale = 10f;

    public Vector3 portal1;
    public Vector3 portal2;

    public Vector3 portal3;
    public Vector3 portal4;

    ushort[] rawdata2;
    Texture2D colorTexture2;
    byte[] colordata2;


    float depth = 0;
    float depthcount = 0;
    float depthvalue = 0;

    public float depth2 = 0;
    public float depthcount2 = 0;
    public float depthvalue2 = 0;


    float Ydepth = 0;
    float Ydepthcount = 0;
    float Ydepthvalue = 0;

    public float Ydepth2 = 0;
    public float Ydepthcount2 = 0;
    public float Ydepthvalue2 = 0;


    Vector3 red;
    int redcount = 0;

    public Vector3 red2;
    public int redcount2 = 0;

    Vector3 yelow;
    int yelowcount = 0;

    public Vector3 yelow2;
    public int yelowcount2 = 0;

    public Vector3 move;

    float H;
    float S;
    float V;

    int j;

    int colorX;
    int colorY;

    float X;
    float Y;
    float Z;

    int mabiki = 4;
    double mabikiD;

    double haba = 3.0;

    Color32 COLOR;

    int c;
    public int C1;

    void Start()
    {

        Application.targetFrameRate = 60;

        // Get the description of the depth frames.
        depthFrameDesc = KinectSensor.GetDefault().DepthFrameSource.FrameDescription;
        depthWidth = depthFrameDesc.Width;
        depthHeight = depthFrameDesc.Height;


        // buffer for points mapped to camera space coordinate.
        cameraSpacePoints = new CameraSpacePoint[depthWidth * depthHeight];
        mapper = KinectSensor.GetDefault().CoordinateMapper;

        // get reference to DepthSourceManager (which is included in the distributed 'Kinect for Windows v2 Unity Plugin zip')
        multiSourceManagerScript = multiSourceManager.GetComponent<MultiSourceManager>();

        // particles to be drawn
        particles2 = new ParticleSystem.Particle[depthWidth * depthHeight/10];
        portal1 = new Vector3(0, 0, 0);
        portal2 = new Vector3(10, 10, 10);

        // ParticleSystem psystem = GetComponent<ParticleSystem>();
        mabikiD = (double)mabiki;
        haba = 4;
        scale = 10;
        c = depthWidth * depthHeight/10;

    }

    void Update()
    {


        // get new depth data from DepthSourceManager.
        ushort[] rawdata = multiSourceManagerScript.GetDepthData();
        Texture2D colorTexture = multiSourceManagerScript.GetColorTexture();





        // map to camera space coordinate
        mapper.MapDepthFrameToCameraSpace(rawdata, cameraSpacePoints);



        var colorSpace = new ColorSpacePoint[depthFrameDesc.LengthInPixels];
        mapper.MapDepthFrameToColorSpace(rawdata, colorSpace);

        depth = 0;//どあ距離初期化
        depthcount = 0;
        depthvalue = 0;


        Ydepth = 0;//どあ距離初期化
        Ydepthcount = 0;
        Ydepthvalue = 0;

        red.x = 0;
        red.y = 0;
        red.z = 0;

        redcount = 0;

        yelow.x = 0;
        yelow.y = 0;
        yelow.z = 0;

        yelowcount = 0;


        move.x = (red2.x - yelow2.x);
        move.y = (red2.y - yelow2.y);
        move.z = (red2.z - yelow2.z);

        int C = 0;



        for (int i = 0; i < cameraSpacePoints.Length; i=i+10)
        {



            COLOR = colorTexture.GetPixel(colorX, colorY);

            if (C < c)
            {

                if (rawdata[i] != 0)
                {

                    colorX = (int)colorSpace[i].X;
                    colorY = (int)colorSpace[i].Y;

                    X = cameraSpacePoints[i].X * scale;
                    Y = cameraSpacePoints[i].Y * scale;
                    Z = cameraSpacePoints[i].Z * scale;

                    Color.RGBToHSV(colorTexture.GetPixel(colorX, colorY), out H, out S, out V);




                    if ((Z > 0) && (Z < 50))
                    {
                        particles2[C].position = new Vector3(X, Y, Z);
                        particles2[C].startColor = COLOR;
                        particles2[C].startSize = size;
                        C++;

                      
                    }
                    else {
                        particles2[C].startSize = 0;
                    }




                }
                if (rawdata[i] == 0)
                {

                }


            }



        }

        C1 = C;

        depth2 = depth;
        depthcount2 = depthcount;
        depthvalue = depth / (float)depthcount;
        depthvalue2 = depthvalue;



        red2.x = red.x / redcount;
        red2.y = red.y / redcount;
        red2.z = red.z / redcount;
        redcount2 = redcount;



        Ydepth2 = Ydepth;
        Ydepthcount2 = Ydepthcount;
        Ydepthvalue = Ydepth / (float)Ydepthcount;
        Ydepthvalue2 = Ydepthvalue;

        yelow2.x = yelow.x / yelowcount;
        yelow2.y = yelow.y / yelowcount;
        yelow2.z = yelow.z / yelowcount;
        yelowcount2 = yelowcount;
        //Debug.Log(" H=" + H + " S=" + S+" V="+V);

        // update particle system
        // particleSystem.SetParticles(particles, particles.Length);
        //Debug.Log(" X=" + cameraSpacePoints[5000].X + " Y=" + cameraSpacePoints[5000].Y + " Z=" + cameraSpacePoints[5000].Z+" scale="+scale);
        //Debug.Log(" X=" + cameraSpacePoints[5000].X + " Y=" + cameraSpacePoints[4500].X);
        //Debug.Log("cameraSpacePoints.Length=" + cameraSpacePoints.Length);
        GetComponent<ParticleSystem>().SetParticles(particles2, particles2.Length);

    }
}
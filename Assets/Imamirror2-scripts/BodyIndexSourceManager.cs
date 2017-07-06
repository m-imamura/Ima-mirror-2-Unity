using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class BodyIndexSourceManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private BodyIndexFrameReader _Reader;
    private byte[] _Data;


    private Texture2D _Texture;

    public byte[] GetData()
    {
        return _Data;
    }

    public Texture2D GetBodyIndexTexture() {
        return _Texture;
    }
    
    // Use this for initialization
    void Start () {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyIndexFrameSource.OpenReader();
            _Data = new byte[_Sensor.BodyIndexFrameSource.FrameDescription.LengthInPixels];
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                frame.CopyFrameDataToArray(_Data);
                frame.Dispose();
                frame = null;
            }
        }
    }


    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}

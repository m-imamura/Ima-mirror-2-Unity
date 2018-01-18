using UnityEngine;
using System.Collections;
using Windows.Kinect;
using UnityEngine.UI;

public class ColorViewUI : MonoBehaviour
{
    public GameObject ColorSourceManager;
    private ColorSourceManager _ColorManager;

    void Start()
    {
        gameObject.GetComponent<Image>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }

    void Update()
    {
        if (ColorSourceManager == null)
        {
            return;
        }

        _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
        if (_ColorManager == null)
        {
            return;
        }

        //gameObject.GetComponent<SpriteRenderer>().material.mainTexture = _ColorManager.GetColorTexture();
        gameObject.GetComponent<Image>().material.mainTexture = _ColorManager.GetColorTexture();
    }
}

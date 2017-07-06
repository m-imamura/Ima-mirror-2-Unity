using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transformation : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Matrix4x4 transrate(Vector4 t){

        Matrix4x4 mat;

        mat = Matrix4x4.identity;
        mat.SetColumn(3, new Vector4(t.x, t.y, t.z, t.w));

	    return mat;
    }
    
    // 回転移動行列を計算 (計算される行列，回転元のベクトル，回転後のベクトル)
    public Matrix4x4 rotate(Vector4 u, Vector4 v)
    {
        Matrix4x4 mat = Matrix4x4.identity;

        // Vector4 の u, v の x，y，z 要素を切り出す
        Vector3 u_v3, v_v3;
        u_v3 = new Vector3(u.x, u.y, u.z);
        v_v3 = new Vector3(v.x, v.y, v.z);
        
        Vector3 n = Vector3.Cross(u_v3, v_v3).normalized;
        Vector3 l = Vector3.Cross(u_v3, n).normalized;
        Vector3 m = Vector3.Cross(v_v3, n).normalized;
        
        
        Matrix4x4 mv = Matrix4x4.identity;
        mv.SetColumn(0, new Vector4(v_v3.x, v_v3.y, v_v3.z, 0));
        mv.SetColumn(1, new Vector4(n.x, n.y, n.z, 0));
        mv.SetColumn(2, new Vector4(m.x, m.y, m.z, 0));

        
        Matrix4x4 mu = Matrix4x4.identity;
        mu.SetColumn(0, new Vector4(u_v3.x, u_v3.y, u_v3.z, 0));
        mu.SetColumn(1, new Vector4(n.x, n.y, n.z, 0));
        mu.SetColumn(2, new Vector4(l.x, l.y, l.z, 0));

        
        Matrix4x4 mu_t = mu.transpose;
        mat = mv * mu_t;
        
        return mat;
    }
    
}

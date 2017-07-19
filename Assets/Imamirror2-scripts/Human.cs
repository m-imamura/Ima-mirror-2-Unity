using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class Human : MonoBehaviour {
    
    // define
    private int BONES = 24;
    private int JOINTS = 25;

    // Bones・Points情報
    private Bones shape_bones;// shape（固定）
    private Bones actor_bones;// actor（更新）
    private Points shape_points;// points情報（shapeのもの，固定）

    // 変換元と変換先の番号
    public int shape_num = -1;
    public int actor_num = -1;

    // 交換済みフラグ
    public bool ready = false;

    // 行列変換スクリプト
    private Transformation trans;

    // ボーン用particle
    public ParticleSystem.Particle[] particles;


    // mapper
    private Kinect.CoordinateMapper mapper;

    // Use this for initialization
    void Start () {
        shape_bones = new Bones();
        actor_bones = new Bones();
        shape_points = new Points();
        trans = new Transformation();

        GameObject shape_bones_obj = transform.Find("Shape_Bones").gameObject;
        shape_bones = shape_bones_obj.GetComponent<Bones>();

        GameObject actor_bones_obj = transform.Find("Actor_Bones").gameObject;
        actor_bones = actor_bones_obj.GetComponent<Bones>();
        
        GameObject shape_points_obj = transform.Find("Points").gameObject;
        shape_points = shape_points_obj.GetComponent<Points>();

        GameObject trans_obj = transform.Find("Transformation").gameObject;
        trans = trans_obj.GetComponent<Transformation>();

        particles = new ParticleSystem.Particle[BONES];
        for (int i = 0; i < BONES; i++)
        {
            particles[i].position = new Vector3(0, 0, 0);
            particles[i].startSize = 1f;
            particles[i].startColor = Color.white;
        }

        // mapper
        mapper = Kinect.KinectSensor.GetDefault().CoordinateMapper;

    }
	
	// Update is called once per frame
	void Update () {
        if (ready)
        {
            actor_bones.set_bones_data(actor_num);// actorのボーン情報を取得
            get_translate_body();                 // get_translate_body
            shape_points.view_trans_points();
        }
    }

    public void set_init_data(int shape, int actor) { // 実質Start()
         // shape_num = shape;
         // actor_num = actor;

        shape_bones.set_bones_init_data(shape_num);
        actor_bones.set_bones_init_data(actor_num);
        shape_points.set_points_data(shape_num);

        actor_bones.set_bones_data(actor_num); //?

        ready = true;

        Debug.Log("set_init_data(" + shape_num + ", " + actor_num + ")");

    }

    private void get_translate_body() {
        
        // shapeの身体でactorの姿勢のときのbottomを計算
        Vector4[] new_bottom = new Vector4[BONES];
        for (int b = 0; b < BONES; b++)
        {
            new_bottom[b] = new Vector4( 0.0f, 0.0f, 0.0f, 1.0f);
            int parent = shape_bones.connect_parent[b]; // 接続関係なのでactor_boneでもOK

            if (parent == -1)
            {
                float y_diff = actor_bones.bottom_init[b].y - shape_bones.bottom_init[b].y; // 腰の高さを合わせる
                new_bottom[b] = actor_bones.bottom[b];
                new_bottom[b].y -= y_diff;
            }
            else
            {
                new_bottom[b] = new_bottom[parent] + shape_bones.length[parent] * actor_bones.vector[parent];
                new_bottom[b].w = 1.0f; // w値は直す
            }

            // テスト
            //particles[b].position = new Vector3(new_bottom[b].x *10f, new_bottom[b].y*10f, new_bottom[b].z*10f);
        }
        // new_bottomをパーティクルで表示
        //GetComponent<ParticleSystem>().SetParticles(particles, particles.Length);

        // 点群の変換
        for (int p = 0; p < shape_points.points_num; p++)
        {
            // 点の位置を初期化
            shape_points.points[p] = new Vector4( 0.0f, 0.0f, 0.0f, 0.0f);

            for (int b = 0; b < BONES; b++)
            {
                Vector4 v1 = shape_bones.top_init[b] - shape_bones.bottom_init[b];// v1 : bottom から top
                Vector4 v2 = shape_points.points_init[p] - shape_bones.bottom_init[b];// v2 : Bottomから点
                float l = Vector4.Dot(v1, v1);
                if (l > 0.0) //0-1にクランプ
                    v2 -= v1 * Mathf.Clamp(Vector4.Dot(v1, v2) / l, 0.0f, 1.0f);

                float d = v2.magnitude;//(v2 - v1 * t).magnitude; // ボーンと標本点の距離

                // ↓ここで，ボーンごとに許容範囲を変えるとうまくいきそう？
                if (d < v1.magnitude * shape_bones.connect_impactrange[b])
                {
                    // ボーンの影響（重み）
                    float w = Mathf.Pow(d + 1.0f, -16);
                    Matrix4x4 w_matrix;
                    w_matrix = Matrix4x4.identity;
                    w_matrix.m00 = w_matrix.m11 = w_matrix.m22 = w_matrix.m33 = w;

                    // 変換行列の全体
                    Matrix4x4 M_matrix; // M_matrixを最初に求めておくと計算回数が少なくなるはず．
                    Matrix4x4 M_inverse;
                    Matrix4x4 B_matrix;

                    // 変換行列の部分
                    Matrix4x4 M_transrate;
                    Matrix4x4 M_rotate;
                    Matrix4x4 B_transrate;
                    Matrix4x4 B_rotate;

                    // 基本のベクトル
                    Vector4 unit_vector = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                    // 行列 M の部分を求める
                    M_transrate = trans.transrate(shape_bones.bottom_init[b]);
                    M_rotate = trans.rotate(unit_vector, shape_bones.vector_init[b]);

                    // 初期ボーンのノルムを求める
                    float vi_norm = shape_bones.length[b];
                    //float vi_norm = shape_bones.length[b];
                    Matrix4x4 vi_norm_mat = Matrix4x4.identity;
                    vi_norm_mat.m00 = vi_norm_mat.m11 = vi_norm_mat.m22 = vi_norm_mat.m33 = vi_norm;
                    
                    // 行列 M の全体を求める
                    M_matrix = vi_norm_mat * M_transrate * M_rotate;
                    M_inverse = M_matrix.inverse;

                    // 行列 B の部分を求める
                    B_rotate = trans.rotate(unit_vector, actor_bones.vector[b]);
                    B_transrate = trans.transrate(new_bottom[b]);
                    
                    // 変換後ボーンのノルムを求める
                    float v_norm = actor_bones.length[b];
                    Matrix4x4 v_norm_mat = Matrix4x4.identity;
                    v_norm_mat.m00 = v_norm_mat.m11 = v_norm_mat.m22 = v_norm_mat.m33 = v_norm;

                    // 行列 B の全体を求める
                    B_matrix = v_norm_mat * B_transrate * B_rotate;
                    
                    // 変換後の点に b 番ボーンの影響を足し合わせる
                    shape_points.points[p] += w_matrix * B_matrix * M_inverse * shape_points.points_init[p];
                    
                }
            }
            shape_points.points[p].x /= shape_points.points[p].w;
            shape_points.points[p].y /= shape_points.points[p].w;
            shape_points.points[p].z /= shape_points.points[p].w;
            shape_points.points[p].w /= shape_points.points[p].w;
        }
        
    }

    public void clear_data()
    {
        Debug.Log("clear" + shape_num);
        shape_points.clear_points();
        shape_bones.clear_bones();
        actor_bones.clear_bones();
        actor_num = -1;
        shape_num = -1;
        ready = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;
using System; // try-catch

public class Human : MonoBehaviour {

    // 予め用意した身体形状を使用する場合
    public bool pre_body_mode = false;

    // define
    private int BONES = 24;
    private int JOINTS = 25;

    // Bones・Points情報
    private Bones actor_bones; // 動作ユーザの骨格情報（毎フレーム更新）
    private Bones[] shape_bones; // 形状ユーザの骨格情報（固定，複数）
    private Points[] shape_points; // 形状ユーザの点群情報（固定，複数）
    private int SHAPE_BODY_MAX = 2; // 形状ユーザの骨格と点群の組の数

    // Matrix情報
    private Matrix4x4[][] M_inverse;

    // 変換元と変換先の番号
    public int shape_num = -1;
    public int actor_num = -1;

    // 交換済みフラグ．これがtrueなら毎フレーム入れ替え映像をつくる．
    public bool ready = false;

    // 行列変換スクリプト
    private Transformation trans;

    // ボーン用particle
    public ParticleSystem.Particle[] particles;

    // mapper
    private Kinect.CoordinateMapper mapper;

    // eye_level
    public Vector3 eye_level;

    // Use this for initialization
    void Start() {
        shape_bones = new Bones[SHAPE_BODY_MAX];
        shape_points = new Points[SHAPE_BODY_MAX];
        M_inverse = new Matrix4x4[SHAPE_BODY_MAX][];
        for (int i=0; i< SHAPE_BODY_MAX; i++) {
            shape_bones[i] = new Bones();
            shape_points[i] = new Points();
            M_inverse[i] = new Matrix4x4[BONES];
            for (int j = 0; j < BONES; j++) {
                M_inverse[i][j] = new Matrix4x4();
            }
        }
        actor_bones = new Bones();
        trans = new Transformation();

        // Shape側の骨格情報と点群情報の組
        // Unity側ではHumanオブジェクトの下に欲しい数だけオブジェクトの組を追加していく．
        GameObject shape_bones_obj = transform.Find("Shape_Bones").gameObject;
        shape_bones[0] = shape_bones_obj.GetComponent<Bones>();

        GameObject shape_points_obj = transform.Find("Points").gameObject;
        shape_points[0] = shape_points_obj.GetComponent<Points>();

        for (int i = 1; i < SHAPE_BODY_MAX; i++) {

            GameObject shape_bones_obj_i;
            try {
                shape_bones_obj_i = transform.Find("Shape_Bones_" + i).gameObject;
                shape_bones[i] = shape_bones_obj_i.GetComponent<Bones>();
            } catch {
                // オブジェクトがない場合（pre_bodyなど）
                shape_bones[i] = shape_bones[0];
                Debug.Log("Shape_Bones_" + i + "オブジェクトがないので他のshape_bonesで代用しました．");
            }
            
            GameObject shape_points_obj_i;
            try {
                shape_points_obj_i = transform.Find("Points_" + i).gameObject;
                shape_points[i] = shape_points_obj_i.GetComponent<Points>();
            }catch {
                shape_points[i] = shape_points[0];
                Debug.Log("Shape_Points_" + i + "オブジェクトがないので他のshape_pointsで代用しました．");
            }
        }

        // Actor側の骨格情報
        GameObject actor_bones_obj = transform.Find("Actor_Bones").gameObject;
        actor_bones = actor_bones_obj.GetComponent<Bones>();

        // 演算用
        GameObject trans_obj = transform.Find("Transformation").gameObject;
        trans = trans_obj.GetComponent<Transformation>();
        
        // Bone表示用パーティクルを生成
        particles = new ParticleSystem.Particle[BONES];
        for (int i = 0; i < BONES; i++)
        {
            particles[i].position = new Vector3(0, 0, 0);
            particles[i].startSize = 1f;
            particles[i].startColor = Color.white;
        }

        // mapper
        mapper = Kinect.KinectSensor.GetDefault().CoordinateMapper;
        Debug.Log("start bone");
    }
	
	// Update is called once per frame
	void Update () {
        if (ready)
        {
            // actorの姿勢から，shapeはどのセットを利用するか決定する．
            int pose_num = actor_bones.set_bones_data(actor_num); 
            
            // 全ポーズの点群を隠す
            for (int i=0; i< SHAPE_BODY_MAX; i++)
                shape_points[i].hide_trans_points();
            
            get_translate_body(pose_num);
            shape_points[pose_num].view_trans_points();
            
        }
    }

    public void set_init_data(int shape, int actor, int pose) {

        if (!pre_body_mode) // ハイタッチモードでは身体形状の骨格情報と点群情報も取得する
        {
            Debug.Log("ハイタッチモード");
            shape_bones[pose].set_bones_init_data(shape_num);
            shape_points[pose].set_points_data(shape_num);
            set_Matrix_M(pose); // ここでMatrixM-1の計算と保存をする．

            if (!ready) { // 初回に限り全部の骨格情報と点群情報の組に現在取得したデータを入れる
                for (int i = 0; i < SHAPE_BODY_MAX; i++) { 
                    shape_bones[i].set_bones_init_data(shape_num);
                    shape_points[i].set_points_data(shape_num);
                    set_Matrix_M(i); // ここでMatrixM-1の計算と保存をする．
                }
            }
        }
        actor_bones.set_bones_init_data(actor_num);
        //actor_bones.set_bones_data(actor_num); // 要らないかも？
        
        Debug.Log("set_init_data(" + shape_num + ", " + actor_num + ")");
        ready = true;
        return;
    }

    private void set_Matrix_M(int pose_num) {

        for (int b = 0; b < BONES; b++)
        {
            // 変換行列の全体
            Matrix4x4 M_matrix; // M_matrixを最初に求めておくと計算回数が少なくなるはず．
            
            // 変換行列の部分
            Matrix4x4 M_transrate;
            Matrix4x4 M_rotate;

            // 基本のベクトル
            Vector4 unit_vector = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

            // 行列 M の部分を求める
            M_transrate = trans.transrate(shape_bones[pose_num].bottom_init[b]);
            M_rotate = trans.rotate(unit_vector, shape_bones[pose_num].vector_init[b]);

            // 初期ボーンのノルムを求める
            float vi_norm = shape_bones[pose_num].length[b];
            //float vi_norm = shape_bones.length[b];
            Matrix4x4 vi_norm_mat = Matrix4x4.identity;
            vi_norm_mat.m00 = vi_norm_mat.m11 = vi_norm_mat.m22 = vi_norm_mat.m33 = vi_norm;

            // 行列 M の全体を求める
            M_matrix = vi_norm_mat * M_transrate * M_rotate;
            M_inverse[pose_num][b] = M_matrix.inverse;
        }
        return;
    }

    private void get_translate_body(int pose_num) {
        
        // shapeの身体でactorの姿勢のときのbottomを計算
        Vector4[] new_bottom = new Vector4[BONES];
        for (int b = 0; b < BONES; b++)
        {
            new_bottom[b] = new Vector4( 0.0f, 0.0f, 0.0f, 1.0f);
            int parent = shape_bones[0].connect_parent[b]; // ボーンの親子（接続）関係なのでactor_boneでもOK

            if (parent == -1)
            {
                float y_diff = actor_bones.bottom_init[b].y - shape_bones[pose_num].bottom_init[b].y; // 腰の高さを合わせる
                new_bottom[b] = actor_bones.bottom[b];
                new_bottom[b].y -= y_diff;
            }
            else
            {
                new_bottom[b] = new_bottom[parent] + shape_bones[pose_num].length[parent] * actor_bones.vector[parent];
                new_bottom[b].w = 1.0f; // w値は直す
            }
            
        }

        // 目線の位置 // HMDで使えるか？ただし目線方向はない
        eye_level = new_bottom[3] + shape_bones[pose_num].length[3] * actor_bones.vector[3];
        eye_level.z *= -1;


        // new_bottomをパーティクルで表示
        //GetComponent<ParticleSystem>().SetParticles(particles, particles.Length);

        // 点群の変換
        for (int p = 0; p < shape_points[pose_num].points_num; p++)
        {
            // 点の位置を初期化
            shape_points[pose_num].points[p] = new Vector4( 0.0f, 0.0f, 0.0f, 0.0f);

            for (int b = 0; b < BONES; b++)
            {
                Vector4 v1 = shape_bones[pose_num].top_init[b] - shape_bones[pose_num].bottom_init[b];// v1 : bottom から top
                Vector4 v2 = shape_points[pose_num].points_init[p] - shape_bones[pose_num].bottom_init[b];// v2 : Bottomから点
                float l = Vector4.Dot(v1, v1);
                if (l > 0.0) //0-1にクランプ
                    v2 -= v1 * Mathf.Clamp(Vector4.Dot(v1, v2) / l, 0.0f, 1.0f);

                float d = v2.magnitude;//(v2 - v1 * t).magnitude; // ボーンと標本点の距離

                // ↓ここで，ボーンごとに許容範囲を変えるとうまくいきそう？
                if (d < v1.magnitude * shape_bones[0].connect_impactrange[b])
                {
                    // ボーンの影響（重み）
                    float w = Mathf.Pow(d + 1.0f, -16);
                    Matrix4x4 w_matrix;
                    w_matrix = Matrix4x4.identity;
                    w_matrix.m00 = w_matrix.m11 = w_matrix.m22 = w_matrix.m33 = w;

                    // 変換行列の全体
                    //Matrix4x4 M_matrix; // M_matrixを最初に求めておくと計算回数が少なくなるはず．
                    //Matrix4x4 M_inverse;
                    Matrix4x4 B_matrix;

                    // 変換行列の部分
                    //Matrix4x4 M_transrate;
                    //Matrix4x4 M_rotate;
                    Matrix4x4 B_transrate;
                    Matrix4x4 B_rotate;

                    // 基本のベクトル
                    Vector4 unit_vector = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);

                    // 行列 M の部分を求める
                    //M_transrate = trans.transrate(shape_bones[pose_num].bottom_init[b]);
                    //M_rotate = trans.rotate(unit_vector, shape_bones[pose_num].vector_init[b]);

                    // 初期ボーンのノルムを求める
                    //float vi_norm = shape_bones[pose_num].length[b];
                    //float vi_norm = shape_bones.length[b];
                    //Matrix4x4 vi_norm_mat = Matrix4x4.identity;
                    //vi_norm_mat.m00 = vi_norm_mat.m11 = vi_norm_mat.m22 = vi_norm_mat.m33 = vi_norm;
                    
                    // 行列 M の全体を求める
                    //M_matrix = vi_norm_mat * M_transrate * M_rotate;
                    //M_inverse = M_matrix.inverse;

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
                    shape_points[pose_num].points[p] += w_matrix * B_matrix * M_inverse[pose_num][b] * shape_points[pose_num].points_init[p];
                    
                }
            }
            shape_points[pose_num].points[p].x /= shape_points[pose_num].points[p].w;
            shape_points[pose_num].points[p].y /= shape_points[pose_num].points[p].w;
            shape_points[pose_num].points[p].z /= shape_points[pose_num].points[p].w;
            shape_points[pose_num].points[p].w /= shape_points[pose_num].points[p].w;
        }

        return;
    }

    public void clear_data()
    {
        Debug.Log("clear" + shape_num);
        for (int i =0;i<SHAPE_BODY_MAX; i++) {
            shape_points[i].clear_points();
            shape_bones[i].clear_bones();
        }
        actor_bones.clear_bones();
        actor_num = -1;
        shape_num = -1;
        ready = false;

        return;
    }

    public void clear_data_pre() {
        Debug.Log("clear_data_pre");

        for (int i = 0; i < SHAPE_BODY_MAX; i++)
            shape_points[i].clear_points_pre();

        actor_num = -1;
        ready = false;

        return;
    }
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
public class BlendShape : MonoBehaviour {
    //表情基的Mesh网格
    public Mesh[] meshs;
    //每个表情基的权重值数组
    public float[] weights;

    bool isChange = false;

    void Start() {
        //初始化权重值数组
        weights = new float[meshs.Length];
        System.Array.Clear(weights, 0, weights.Length);
        //将当前网格置为默认的模型,即第一个表情基模型
        GetComponent<MeshFilter>().mesh = meshs[0];
    }

    //设置表情基的权重值
    public void SetWeight(int argIndex, float argWeight) {
        weights[argIndex] = (argWeight > 1 ? 1 : argWeight);
        isChange = true;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.W)) {
            SetWeight(1, weights[1] + 0.1f);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            SetWeight(1, weights[1] - 0.1f);
        }
        UpdateMesh();
    }

    //更新网格
    void UpdateMesh() {
#if UNITY_EDITOR
#else
        if (!isChange) return;
#endif
        //获取模型的网格信息
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        //初始化模型的顶点数组
        Vector3[] vertices = new Vector3[meshs[0].vertices.Length];
        System.Array.Copy(meshs[0].vertices, vertices, vertices.Length);

        //根据权重值与表情基计算新的顶点位置
        for (int w = 1; w < weights.Length; w++) {
            if (weights[w] <= 0) continue;

            //获取表情基顶点
            Vector3[] verticesT = meshs[w].vertices;

            int i = 0;
            while (i < vertices.Length) {
                vertices[i] = vertices[i] + (verticesT[i] - meshs[0].vertices[i]) * weights[w];
                i++;
            }
        }
        //改变顶点位置坐标
        mesh.vertices = vertices;
        isChange = false;
    }
}

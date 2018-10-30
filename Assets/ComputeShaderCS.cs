using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class ComputeShaderCS : MonoBehaviour {
    //表情基的Mesh网格 0为natural模型
    public Mesh[] meshs;
    //每个表情基的权重值数组(限制在0-1)
    [Range(0,1)]
    public float[] weights;
    //计算着色器
    public ComputeShader shader;

    ComputeBuffer vertBuffer;       //顶点缓冲
    ComputeBuffer blendshapeBuffer; //表情基模型缓冲
    ComputeBuffer naturalBuffer;    //natural模型缓冲

    //模型网格信息
    Mesh mesh;

    private void Start() {
        //获取模型的网格信息
        mesh = GetComponent<MeshFilter>().mesh;

        //分配缓冲空间(每个float占4个字节，一个Vector3包括3个float,所占大小为4*3)
        vertBuffer = new ComputeBuffer(meshs[0].vertices.Length, 4 * 3);
        blendshapeBuffer = new ComputeBuffer(meshs[0].vertices.Length, 4 * 3);
        naturalBuffer = new ComputeBuffer(meshs[0].vertices.Length, 4 * 3);
    }

    private void Update() {
        //不必每一帧执行，此处为便于测试
        RunShader();
    }

    //执行计算着色器
    void RunShader() {

        int kernel = shader.FindKernel("Cul");

        Vector3[] vertices = new Vector3[meshs[0].vertices.Length];

        //初始化缓冲
        vertBuffer.SetData(meshs[0].vertices);
        naturalBuffer.SetData(meshs[0].vertices);

        //填充缓冲
        shader.SetBuffer(kernel, "vertBuffer", vertBuffer);
        shader.SetBuffer(kernel, "naturalBuffer", naturalBuffer);

        //根据权重值与表情基计算新的顶点位置
        for (int w = 1; w < weights.Length; w++) {

            if (weights[w] <= 0) continue;

            //初始化缓冲
            blendshapeBuffer.SetData(meshs[w].vertices);

            //填充缓冲
            shader.SetBuffer(kernel, "blendshapeBuffer", blendshapeBuffer);
            
            //设置权重
            shader.SetFloat("weight", weights[w]);

            //执行Compute Shader
            shader.Dispatch(kernel, 4, 4, 1);

        }

        //获取计算结果
        vertBuffer.GetData(vertices);

        //更改网格结构
        mesh.vertices = vertices;
    }

    void OnDestroy() {
        if (vertBuffer != null) vertBuffer.Release();
        if (blendshapeBuffer != null) blendshapeBuffer.Release();
        if (naturalBuffer != null) naturalBuffer.Release();
    }
}
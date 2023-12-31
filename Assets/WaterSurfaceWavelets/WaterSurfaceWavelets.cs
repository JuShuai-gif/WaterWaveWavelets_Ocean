﻿using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class WaterSurfaceWavelets : MonoBehaviour
{
    // 真实世界缩放
    private const int realWorldScale = 10;
    // 节点个数
    private const int numNodes = 100;
    // 方向数
    private const int numDirections = 16;
    // 贴图大小
    private const int textureSize = 128;
    // 
    private const int profileBufferSize = 1024;
    // 风速
    private const float windSpeed = 10.0f;
    // 最小频率
    private readonly static float spectrumMin = Mathf.Log(0.03f, 2.0f);
    // 最大频率
    private readonly static float spectrumMax = Mathf.Log(10.0f, 2.0f);
    // 时期
    private readonly static float period = realWorldScale * Mathf.Pow(2.0f, spectrumMax);
    // 组 速度
    private readonly static float groupSpeed = ComputeGroupSpeed();

    // compute shaders
    private ComputeShader csAdvect;
    private int csAdvectMain;

    private ComputeShader csDiffuse;
    private int csDiffuseMain;

    private ComputeShader csInject;
    private int csInjectPointMain;

    private ComputeShader csProfileBuffer;
    private int csProfileBufferMain;

    private ComputeShader csNormals;
    private int csNormalsMain;

    private ComputeShader csHeight;
    private int csHeightMain;

    // buffers
    private RenderTexture[] bufAmplitude;
    private int bufAmplitudeCurrent = 0;

    private RenderTexture bufProfileBuffer;
    private RenderTexture bufNormals;
    private RenderTexture bufHeight;

    public RenderTexture normalsOutput;
    public RenderTexture heightsOutput;

    // 计算频谱
    static float Spectrum(float zeta)
    {
        // maxZeta = log2(10),minZeta = log2(0.03)
        float A = Mathf.Pow(2.0f, 1.5f * zeta);
        float B = Mathf.Exp(-1.8038897788076411f * Mathf.Pow(4.0f, zeta) / Mathf.Pow(windSpeed, 4.0f));
        return 0.139098f * Mathf.Sqrt(A * B);
    }

    private static Vector2 Integrate(int count, float min, float max, Func<float, Vector2> f)
    {
        float dx = (max - min) / count;
        float x = min + 0.5f * dx;

        var result = dx * f(x);
        for (int i = 1; i < count; i++)
        {
            x += dx;
            result += dx * f(x);
        }
        return result;
    }

    static float ComputeGroupSpeed()
    {
        var result = Integrate(numNodes, spectrumMin, spectrumMax, zeta =>
        {
            float waveLength = Mathf.Pow(2, zeta);
            float waveNumber = Mathf.PI * 2.0f / waveLength;
            float cg = 0.5f * Mathf.Sqrt(9.81f / waveNumber);
            float density = Spectrum(zeta);
            return new Vector2(cg * density, density);
        });
        float groupSpeed = result.x / result.y;
        return realWorldScale * groupSpeed;
    }

    void Awake()
    {
        Debug.LogFormat("Group speed: {0}", groupSpeed);

        csAdvect = Resources.Load<ComputeShader>("WaterSurfaceWaveletsAdvect");
        csAdvectMain = csAdvect.FindKernel("AdvectMainCS");

        csDiffuse = Resources.Load<ComputeShader>("WaterSurfaceWaveletsDiffuse");
        csDiffuseMain = csDiffuse.FindKernel("DiffuseMainCS");

        csInject = Resources.Load<ComputeShader>("WaterSurfaceWaveletsInject");
        csInjectPointMain = csInject.FindKernel("InjectPointMainCS");

        csProfileBuffer = Resources.Load<ComputeShader>("WaterSurfaceWaveletsProfileBuffer");
        csProfileBufferMain = csProfileBuffer.FindKernel("ProfileBufferMainCS");

        csNormals = Resources.Load<ComputeShader>("WaterSurfaceWaveletsNormals");
        csNormalsMain = csNormals.FindKernel("NormalsMainCS");

        csHeight = Resources.Load<ComputeShader>("WaterSurfaceWaveletsHeight");
        csHeightMain = csHeight.FindKernel("HeightMainCS");

        bufAmplitude = new RenderTexture[] {
            new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.RFloat),
            new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.RFloat)
        };
        foreach (var buf in bufAmplitude)
        {
            buf.dimension = TextureDimension.Tex3D;
            buf.volumeDepth = numDirections;
            buf.enableRandomWrite = true;
            buf.Create();
        }

        bufProfileBuffer = new RenderTexture(profileBufferSize, 1, 0, RenderTextureFormat.ARGBFloat);
        bufProfileBuffer.enableRandomWrite = true;
        bufProfileBuffer.Create();

        if (normalsOutput != null)
        {
            Debug.LogFormat("Normals {0} x {1}", normalsOutput.width, normalsOutput.height);
            bufNormals = new RenderTexture(normalsOutput);
            bufNormals.enableRandomWrite = true;
            bufNormals.Create();
        }
        if (heightsOutput != null)
        {
            Debug.LogFormat("Height {0} x {1}", heightsOutput.width, heightsOutput.height);
            bufHeight = new RenderTexture(heightsOutput);
            bufHeight.enableRandomWrite = true;
            bufHeight.Create();
        }




    }



    void OnDisable()
    {
    }

    void StepAdvect(float dt)
    {
        csAdvect.SetVector("origin", new Vector3(0.0f, 0.0f, 0.0f));
        csAdvect.SetVector("dims", new Vector3(1.0f / textureSize, 1.0f / textureSize, 1.0f / numDirections));
        csAdvect.SetFloat("dt", dt);
        csAdvect.SetFloat("groupSpeed", groupSpeed);
        csAdvect.SetTexture(csAdvectMain, "input", bufAmplitude[bufAmplitudeCurrent]);
        csAdvect.SetTexture(csAdvectMain, "result", bufAmplitude[(bufAmplitudeCurrent + 1) % bufAmplitude.Length]);
        bufAmplitudeCurrent = (bufAmplitudeCurrent + 1) % bufAmplitude.Length;
        csAdvect.Dispatch(csAdvectMain, textureSize, textureSize, numDirections);
    }
    // 
    void StepDiffuse(float dt)
    {
        csDiffuse.SetFloat("dt", dt);
        csDiffuse.SetFloat("groupSpeed", groupSpeed);
        csDiffuse.SetTexture(csDiffuseMain, "input", bufAmplitude[bufAmplitudeCurrent]);
        csDiffuse.SetTexture(csDiffuseMain, "result", bufAmplitude[(bufAmplitudeCurrent + 1) % bufAmplitude.Length]);
        bufAmplitudeCurrent = (bufAmplitudeCurrent + 1) % bufAmplitude.Length;
        csDiffuse.Dispatch(csDiffuseMain, textureSize, textureSize, numDirections);
    }

    void StepInjectPoint(int x, int y)
    {
        csInject.SetInt("x", x);
        csInject.SetInt("y", y);
        csInject.SetTexture(csInjectPointMain, "result", bufAmplitude[bufAmplitudeCurrent]);
        csInject.Dispatch(csInjectPointMain, 1, 1, numDirections);
    }

    // 步进积分
    void StepProfileBuffer()
    {
        csProfileBuffer.SetInt("size", profileBufferSize);
        csProfileBuffer.SetFloat("time", Time.time);
        csProfileBuffer.SetFloat("period", period);
        csProfileBuffer.SetFloat("windSpeed", windSpeed);
        csProfileBuffer.SetFloat("spectrumMin", spectrumMin);
        csProfileBuffer.SetFloat("spectrumMax", spectrumMax);
        csProfileBuffer.SetTexture(csProfileBufferMain, "result", bufProfileBuffer);
        csProfileBuffer.Dispatch(csProfileBufferMain, profileBufferSize, 1, 1);
    }

    void StepNormals()
    {
        if (bufNormals != null)
        {
            csNormals.SetInts("size", bufNormals.width, bufNormals.height);
            csNormals.SetFloat("period", period);
            csNormals.SetTexture(csNormalsMain, "ampl", bufAmplitude[bufAmplitudeCurrent]);
            csNormals.SetTexture(csNormalsMain, "profile", bufProfileBuffer);
            csNormals.SetTexture(csNormalsMain, "result", bufNormals);
            csNormals.Dispatch(csNormalsMain, bufNormals.width, bufNormals.height, 1);
            Graphics.CopyTexture(bufNormals, normalsOutput);
        }
    }

    void StepHeight()
    {
        if (bufHeight != null)
        {
            csHeight.SetInts("size", bufHeight.width, bufHeight.height);
            csHeight.SetFloat("period", period);
            csHeight.SetTexture(csHeightMain, "ampl", bufAmplitude[bufAmplitudeCurrent]);
            csHeight.SetTexture(csHeightMain, "profile", bufProfileBuffer);
            csHeight.SetTexture(csHeightMain, "result", bufHeight);
            csHeight.Dispatch(csHeightMain, bufHeight.width, bufHeight.height, 1);
            Graphics.CopyTexture(bufHeight, heightsOutput);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        //Debug.Log("时间" + dt);
        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.collider == GetComponent<Collider>())
            {
                int x = Mathf.FloorToInt(hit.textureCoord.x * textureSize);
                int y = Mathf.FloorToInt(hit.textureCoord.y * textureSize);
                StepInjectPoint(x, y);
            }
        }

        StepAdvect(dt);
        StepDiffuse(dt);
        StepProfileBuffer();
        StepNormals();
        StepHeight();
    }

    
}

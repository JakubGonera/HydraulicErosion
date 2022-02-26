using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropGenerator : MonoBehaviour
{
    public int numOfDrops = 1000;
    public float inertia = 0.1f;
    public float capacity = 2f;
    public float gravity = 1f;
    public float evaporation = .1f;
    public float deposition = .01f;
    public int radius = 2;
    public float erosion = 0.01f;

    public int dropsPerFrame = 1024;

    Color[] pixels;
    float[] map;
    int mapSize;
    Texture2D erodedTex;
    public int dropsSoFar;

    public MeshGenerator meshGenerator;
    public RawImage rawImage;

    public ComputeShader erosionShader;
    public Shader terrainShader;
    public Material terrainMaterial; 
    ComputeBuffer mapBuffer;
    ComputeBuffer originalMapBuffer;
    bool bufferInstantiated = false;

    int kernelID;
    int groupNumber = 1;

    public void Start()
    {
        dropsSoFar = numOfDrops;
        kernelID = erosionShader.FindKernel("CSMain");
    }

    public void StartSimulation(Texture2D heightMap)
    {
        pixels = heightMap.GetPixels();
        map = new float[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            map[i] = pixels[i].grayscale;
        }

        mapSize = heightMap.width;
        erodedTex = new Texture2D(mapSize, mapSize);
        dropsSoFar = 0;
        rawImage.texture = erodedTex;

        if (bufferInstantiated)
        {
            mapBuffer.Release();
            originalMapBuffer.Release();
        }
        else
            bufferInstantiated = true;

        mapBuffer = new ComputeBuffer(map.Length, sizeof(float));
        originalMapBuffer = new ComputeBuffer(map.Length, sizeof(float));
        mapBuffer.SetData(map);
        originalMapBuffer.SetData(map);

        terrainMaterial.SetBuffer("_Map", mapBuffer);
        terrainMaterial.SetBuffer("_OriginalMap", originalMapBuffer);
    }

    public void Update()
    {
        if (dropsSoFar < numOfDrops)
        {
            int numThreads = dropsPerFrame / groupNumber;

            //Pass the heightmap
            erosionShader.SetBuffer(kernelID, "map", mapBuffer);

            //Pass random positions for the droplets
            List<int> seeds = new List<int>();
            for (int i = 0; i < numThreads * groupNumber; i++)
            {
                seeds.Add(Random.Range(0, mapSize - 1));
                seeds.Add(Random.Range(0, mapSize - 1));
            }

            ComputeBuffer seedsBuffer = new ComputeBuffer(seeds.Count, sizeof(int));
            seedsBuffer.SetData(seeds);
            erosionShader.SetBuffer(kernelID, "seeds", seedsBuffer);

            //Set parameters
            erosionShader.SetInt("mapSize", mapSize);
            erosionShader.SetInt("radius", radius);
            erosionShader.SetFloat("inertia", inertia);
            erosionShader.SetFloat("capacity", capacity);
            erosionShader.SetFloat("gravity", gravity);
            erosionShader.SetFloat("evaporation", evaporation);
            erosionShader.SetFloat("deposition", deposition);
            erosionShader.SetFloat("erosion", erosion);

            erosionShader.Dispatch(kernelID, numThreads, 1, 1);

            mapBuffer.GetData(map);

            seedsBuffer.Release();

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(map[i], map[i], map[i]);
            }
            erodedTex.SetPixels(pixels);
            erodedTex.Apply();

            dropsSoFar += numThreads * groupNumber;
        }
    }

    void OnDestroy()
    {
        mapBuffer.Release();
        originalMapBuffer.Release();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropGenerator : MonoBehaviour
{
    //Erosion parameters
    public int numOfDrops = 1000;
    public float inertia = 0.1f;
    public float capacity = 2f;
    public float gravity = 1f;
    public float evaporation = .1f;
    public float deposition = .01f;
    public int radius = 2;
    public float erosion = 0.01f;

    public int dropsPerFrame = 1024;

    //Variables needed for handling of the heightmap and generating the texture
    Color[] pixels;
    float[] map;
    int mapSize;
    Texture2D erodedTex;

    public int dropsSoFar;

    //Heightmap display
    public RawImage rawImage;

    //Shader objects and buffers
    public ComputeShader erosionShader;
    public Shader terrainShader;
    public Material terrainMaterial; 
    ComputeBuffer mapBuffer;
    ComputeBuffer originalMapBuffer;
    bool bufferInstantiated = false;

    //The "ID" of a shader, found in the start method
    int kernelID;
    int groupNumber = 64;

    public void Start()
    {
        //Set dropsSoFar to numOfDrops so as to not start the erosion on program start
        dropsSoFar = numOfDrops;
        //Find the shader ID
        kernelID = erosionShader.FindKernel("CSMain");
    }

    public void StartSimulation(Texture2D heightMap)
    {
        //Convert the texture to float array
        pixels = heightMap.GetPixels();
        map = new float[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            map[i] = pixels[i].grayscale;
        }

        mapSize = heightMap.width;
        //Initialize the texture used to display heightmap
        erodedTex = new Texture2D(mapSize, mapSize);
        dropsSoFar = 0;
        rawImage.texture = erodedTex;

        //If buffers were already used before, free the memory they occupied
        if (bufferInstantiated)
        {
            mapBuffer.Release();
            originalMapBuffer.Release();
        }
        else
            bufferInstantiated = true;

        //Initialize new buffers and fill them with the data
        mapBuffer = new ComputeBuffer(map.Length, sizeof(float));
        originalMapBuffer = new ComputeBuffer(map.Length, sizeof(float));
        mapBuffer.SetData(map);
        originalMapBuffer.SetData(map);

        //Attach the buffers to the surface shader
        terrainMaterial.SetBuffer("_Map", mapBuffer);
        terrainMaterial.SetBuffer("_OriginalMap", originalMapBuffer);
    }

    public void Update()
    {
        if (dropsSoFar < numOfDrops)
        {
            //Calculate how many thread groups are needed to dispatch
            int numThreads = dropsPerFrame / groupNumber;

            //Pass the heightmap
            erosionShader.SetBuffer(kernelID, "map", mapBuffer);

            //Pass random positions for the droplets
            List<int> randomPositions = new List<int>();
            for (int i = 0; i < numThreads * groupNumber; i++)
            {
                randomPositions.Add(Random.Range(0, mapSize - 1));
                randomPositions.Add(Random.Range(0, mapSize - 1));
            }

            ComputeBuffer positionsBuffer = new ComputeBuffer(randomPositions.Count, sizeof(int));
            positionsBuffer.SetData(randomPositions);
            erosionShader.SetBuffer(kernelID, "positions", positionsBuffer);

            //Set parameters
            erosionShader.SetInt("mapSize", mapSize);
            erosionShader.SetInt("radius", radius);
            erosionShader.SetFloat("inertia", inertia);
            erosionShader.SetFloat("capacity", capacity);
            erosionShader.SetFloat("gravity", gravity);
            erosionShader.SetFloat("evaporation", evaporation);
            erosionShader.SetFloat("deposition", deposition);
            erosionShader.SetFloat("erosion", erosion);

            //Run the compute shader
            erosionShader.Dispatch(kernelID, numThreads, 1, 1);

            //Retrieve data
            mapBuffer.GetData(map);

            //Clean up the random positions
            positionsBuffer.Release();

            //Convert the float array to a texture
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
        //End of program - clean up the GPU memory
        mapBuffer.Release();
        originalMapBuffer.Release();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Drop
{
    public Drop(Vector2 _pos, Vector2 _dir)
    {
        pos = _pos;
        dir = _dir;
        water = 1f;
        sediment = 0;
        vel = 1f;
    }
    public Vector2 pos, dir;
    public float water, sediment, vel;
    public bool isOnMap(int size)
    {
        return (pos.x >= 0 && pos.x < (size - 1) && pos.y >= 0 && pos.y < (size - 1));
    }
}

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
    int kernelID;
    int groupNumber = 1;

    public void Start()
    {
        dropsSoFar = numOfDrops;
        kernelID = erosionShader.FindKernel("CSMain");
    }

    //Texture2D ShaderTest(Color[] heightMap, int mapSize)
    //{
    //    float[] map = new float[heightMap.Length];

    //    for (int i = 0; i < heightMap.Length; i++)
    //    {
    //        map[i] = heightMap[i].grayscale;
    //    }

    //    ComputeBuffer mapBuffer = new ComputeBuffer(map.Length, sizeof(float));
    //    mapBuffer.SetData(map);
    //    erosionShader.SetBuffer(kernelID, "map", mapBuffer);

    //    erosionShader.SetInt("mapSize", map.Length);

    //    erosionShader.Dispatch(kernelID, 1, 1, 1);
    //    mapBuffer.GetData(map);

    //    mapBuffer.Release();

    //    Texture2D tex = new Texture2D(mapSize, mapSize);
    //    for (int i = 0; i < heightMap.Length; i++)
    //    {
    //        heightMap[i] = new Color(map[i], map[i], map[i]);
    //    }
    //    tex.SetPixels(heightMap);
    //    tex.Apply();
    //    return tex;
    //}

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
    }

    public void Update()
    {
        if (dropsSoFar < numOfDrops)
        {
            int numThreads = dropsPerFrame / groupNumber;

            //Pass the heightmap
            ComputeBuffer mapBuffer = new ComputeBuffer(map.Length, sizeof(float));
            mapBuffer.SetData(map);
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

            mapBuffer.Release();
            seedsBuffer.Release();

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(map[i], map[i], map[i]);
            }
            erodedTex.SetPixels(pixels);
            erodedTex.Apply();

            meshGenerator.Construct(erodedTex);
            dropsSoFar += numThreads * groupNumber;

            //for (int i = 0; i < dropsPerFrame && dropsSoFar < numOfDrops; i++)
            //{
            //    Vector2 startPos = new Vector2(Random.Range(0, mapSize - 1), Random.Range(0, mapSize - 1));
            //    Vector2 startDir = new Vector2(0, 0);
            //    Drop d = new Drop(startPos, startDir);
            //    while (d.water > 0.01 && d.isOnMap(mapSize))
            //    {
            //        DropStep(ref pixels, ref d, mapSize);
            //    }
            //    ++dropsSoFar;
            //}
            //erodedTex.SetPixels(pixels);
            //erodedTex.Apply();
            //meshGenerator.Construct(erodedTex);
        }
    }

    float InterpolateHeight(Vector2 pos, ref Color[] heights, int size)
    {
        float u, v;
        int x, y;
        u = pos.x - Mathf.Floor(pos.x);
        v = pos.y - Mathf.Floor(pos.y);
        x = (int)Mathf.Floor(pos.x);
        y = (int)Mathf.Floor(pos.y);

        float h11 = heights[y * size + x].grayscale;
        float h12 = heights[(y + 1) * size + x].grayscale;
        float h21 = heights[y * size + x + 1].grayscale;
        float h22 = heights[(y + 1) * size + x + 1].grayscale;

        return h11 * (1 - u) * (1 - v)
            + h21 * u * (1 - v)
            + h12 * (1 - u) * v
            + h22 * u * v;
    }

    void Deposit(ref Color[] heights, float u, float v, int x, int y, float amount, int size)
    {
        float h11 = heights[y * size + x].grayscale;
        float h12 = heights[(y + 1) * size + x].grayscale;
        float h21 = heights[y * size + x + 1].grayscale;
        float h22 = heights[(y + 1) * size + x + 1].grayscale;

        h11 += amount * (1 - u) * (1 - v);
        h21 += amount * u * (1 - v);
        h12 += amount * (1 - u) * v;
        h22 += amount * u * v;

        heights[y * size + x] = new Color(h11, h11, h11);
        heights[(y + 1) * size + x] = new Color(h12, h12, h12);
        heights[y * size + x + 1] = new Color(h21, h21, h21);
        heights[(y + 1) * size + x + 1] = new Color(h22, h22, h22);
    }

    void Erode(ref Color[] heights, Vector2 pos, float amount, int size)
    {
        int x = (int)Mathf.Floor(pos.x);
        int y = (int)Mathf.Floor(pos.y);
        float sum = 0;
        for (int i = x - radius - 1; i < x + radius + 1; i++)
        {
            for (int j = y - radius - 1; j < y + radius + 1; j++)
            {
                if(i >= 0 && j >= 0 && i < size && j < size)
                {
                    sum += Mathf.Max(0, radius - (pos - new Vector2(i, j)).magnitude);
                }
            }
        }
        for (int i = x - radius - 1; i < x + radius + 1; i++)
        {
            for (int j = y - radius - 1; j < y + radius + 1; j++)
            {
                if (i >= 0 && j >= 0 && i < size && j < size)
                {
                    float oldHeight = heights[j * size + i].grayscale;
                    float weightedSediment = 
                        (Mathf.Max(0f, radius - (pos - new Vector2(i, j)).magnitude)/sum) * amount;
                    heights[j * size + i] = new Color(oldHeight - weightedSediment, 
                                                      oldHeight - weightedSediment, 
                                                      oldHeight - weightedSediment);
                }
            }
        }
    }

    void DropStep(ref Color[] heights, ref Drop d, int size)
    {
        //calculate position in the cell
        float u, v;
        int x, y;
        u = d.pos.x - Mathf.Floor(d.pos.x);
        v = d.pos.y - Mathf.Floor(d.pos.y);
        x = (int)Mathf.Floor(d.pos.x);
        y = (int)Mathf.Floor(d.pos.y);

        //calculate the gradient
        Vector2 grad = new Vector2();
        grad.x = (1f - u) * (heights[y * size + x + 1].grayscale - heights[y * size + x].grayscale) 
                + u * (heights[(y + 1) * size + x + 1].grayscale - heights[(y + 1) * size + x].grayscale);
        grad.y = (1f - v) * (heights[(y + 1) * size + x].grayscale - heights[y * size + x].grayscale) 
                + v * (heights[(y + 1) * size + x + 1].grayscale - heights[y * size + x + 1].grayscale);

        //new direction
        d.dir = d.dir * inertia - grad * (1 - inertia);
        d.dir.Normalize();

        //calculate new position and get new height
        float hOld = InterpolateHeight(d.pos, ref heights, size);
        Vector2 posOld = d.pos;
        d.pos = d.pos + d.dir;
        if (!d.isOnMap(size))
        {
            return;
        }
        float hNew = InterpolateHeight(d.pos, ref heights, size);
        float hDiff = hNew - hOld;

        //Erode or deposit
        if(hDiff > 0)
        {
            float valDeposited = Mathf.Min(hDiff, d.sediment);
            Deposit(ref heights, u, v, x, y, valDeposited, size);
            d.sediment -= valDeposited;
        }
        else
        {
            float c = -hDiff * d.vel * d.water * capacity;
            if(c < d.sediment)
            {
                float valDeposited = (d.sediment - c) * deposition;
                Deposit(ref heights, u, v, x, y, valDeposited, size);
                d.sediment -= valDeposited;
            }
            else
            {
                float valEroded = Mathf.Min((c - d.sediment) * erosion, -hDiff);
                Erode(ref heights, posOld, valEroded, size);
                d.sediment += valEroded;
            }
        }
        d.vel = Mathf.Sqrt(d.vel * d.vel + hDiff * gravity);
        d.water = d.water * (1 - evaporation);
    }
}

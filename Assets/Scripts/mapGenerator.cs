using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int size;
    public int seed;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public float scale = 0.1f;

    public Material material;
    Texture2D perlinTex;
    public MeshGenerator meshGenerator;

    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        perlinTex = new Texture2D(size, size);
        material.mainTexture = perlinTex;
        Color[] map = new Color[size * size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float sample = 0;
                float freq = 1;
                float amp = 0.7f;
                float xCoord = (float)i * scale, yCoord = (float)j * scale;
                for (int k = 0; k < octaves; k++)
                {

                    sample += Mathf.PerlinNoise((xCoord + seed * (k + 1)) * freq, (yCoord + seed * (k + 1)) * freq) * amp;
                    freq = freq * lacunarity;
                    amp *= persistance;
                }
                map[i * size + j] = new Color(sample, sample, sample);
            }
        }

        perlinTex.SetPixels(map);
        perlinTex.Apply();

        meshGenerator.Construct(perlinTex);
    }
}

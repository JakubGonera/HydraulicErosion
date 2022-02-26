using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public int size;
    public int seed;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public float scale = 0.1f;

    Texture2D perlinTex;
    public MeshGenerator meshGenerator;

    public Texture2D Generate()
    {
        perlinTex = new Texture2D(size, size);
        Color[] map = new Color[size * size];
        float[] tempValues = new float[size * size];


        float min = 20f, max = -20f;
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float sample = 0;
                float freq = 1;
                float amp = 0.8f;
                float xCoord = (float)i * scale, yCoord = (float)j * scale;
                for (int k = 0; k < octaves; k++)
                {

                    sample += Mathf.PerlinNoise((xCoord + (seed/100) * (k + 1)) * freq, (yCoord + (seed/100) * (k + 1)) * freq) * amp;
                    freq *= lacunarity;
                    amp *= persistance;
                }
                tempValues[i * size + j] = sample;
                min = Mathf.Min(min, sample);
                max = Mathf.Max(max, sample);
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float sample = 0.1f + 0.9f * (tempValues[i * size + j] - min) / (max - min);
                map[i * size + j] = new Color(sample, sample, sample);
            }
        }

        perlinTex.SetPixels(map);
        perlinTex.Apply();

        return perlinTex;
    }
}

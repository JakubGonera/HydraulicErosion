using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    //Original map parameters
    public int size;
    public int seed;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public float scale = 0.1f;

    Texture2D perlinTex;
    public MeshGenerator meshGenerator;

    //Generate heightmap with Perlin noise
    public Texture2D Generate()
    {
        perlinTex = new Texture2D(size, size);
        Color[] map = new Color[size * size];
        float[] tempValues = new float[size * size];

        //These variables will hold the maximum and minimum values the noise will take to scale it to [0;1] range
        float min = 20f, max = -20f;
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                //The value at this pixel
                float sample = 0;
                //Variables used to scale layers of noise - each one will be more squished (frequency), and have less impact (amplitude)
                float freq = 1;
                float amp = 0.8f;

                //Original sampling points
                float xCoord = (float)i * scale, yCoord = (float)j * scale;
                for (int k = 0; k < octaves; k++)
                {
                    //Sample the noise with offset for each layer
                    sample += Mathf.PerlinNoise((xCoord + (seed/100) * (k + 1)) * freq, (yCoord + (seed/100) * (k + 1)) * freq) * amp;
                    //Make the changes for the next layer
                    freq *= lacunarity;
                    amp *= persistance;
                }
                //Store pre-scaled value
                tempValues[i * size + j] = sample;
                //Test for maximum and minimum
                min = Mathf.Min(min, sample);
                max = Mathf.Max(max, sample);
            }
        }

        //Scale all values to [0;1] range
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float sample = 0.1f + 0.9f * (tempValues[i * size + j] - min) / (max - min);
                map[i * size + j] = new Color(sample, sample, sample);
            }
        }

        //Gen texture
        perlinTex.SetPixels(map);
        perlinTex.Apply();

        return perlinTex;
    }
}

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

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.01f);
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return StartCoroutine("Wait");
        Generate();
    }

    public Texture2D Generate()
    {
        perlinTex = new Texture2D(size, size);
        Color[] map = new Color[size * size];

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

                    sample += Mathf.PerlinNoise((xCoord + seed * (k + 1)) * freq, (yCoord + seed * (k + 1)) * freq) * amp;
                    freq *= lacunarity;
                    amp *= persistance;
                }
                map[i * size + j] = new Color(sample, sample, sample);
            }
        }

        perlinTex.SetPixels(map);
        perlinTex.Apply();

        return perlinTex;
    }
}

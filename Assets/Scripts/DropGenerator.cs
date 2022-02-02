using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Drop
{
    public Drop(Vector2 _pos, Vector2 _dir)
    {
        pos = _pos;
        dir = _dir;
        water = 10f;
        sediment = 0;
        vel = 0;
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

    public Texture2D StartSimulation(Texture2D heightMap)
    {
        Color[] pixels = heightMap.GetPixels();
        int mapSize = heightMap.width;
        for (int i = 0; i < numOfDrops; i++)
        {
            Vector2 startPos = new Vector2(Random.Range(0, mapSize - 1), Random.Range(0, mapSize - 1));
            Vector2 startDir = new Vector2(1, 1);
            Drop d = new Drop(startPos, startDir);
            while(d.water > 0.01 && d.isOnMap(mapSize))
            {
                DropStep(ref pixels, ref d, mapSize);
            }
        }
        Texture2D newTex = new Texture2D(mapSize, mapSize);
        newTex.SetPixels(pixels);
        newTex.Apply();
        return newTex;
    }

    //bilinear interpolation
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
            for (int j = y - radius - 1; j < radius + 1; j++)
            {
                if (i >= 0 && j >= 0 && i < size && j < size)
                {
                    float oldHeight = heights[y * size + x].grayscale;
                    float weightedSediment = Mathf.Max(0, radius - (pos - new Vector2(i, j)).magnitude)/sum * amount;
                    heights[y * size + x] = new Color(oldHeight - weightedSediment, oldHeight - weightedSediment, oldHeight - weightedSediment);
                }
            }
        }
    }

    void DropStep(ref Color[] heights, ref Drop d, int size)
    {
        //Calculate the position in the cell
        float u, v;
        int x, y;
        u = d.pos.x - Mathf.Floor(d.pos.x);
        v = d.pos.y - Mathf.Floor(d.pos.y);
        x = (int)Mathf.Floor(d.pos.x);
        y = (int)Mathf.Floor(d.pos.y);

        Vector2 grad = new Vector2();
        grad.x = (1f - u) * (heights[y * size + x + 1].grayscale - heights[y * size + x].grayscale) 
                + u * (heights[(y + 1) * size + x + 1].grayscale - heights[(y + 1) * size + x].grayscale);
        grad.y = (1f - v) * (heights[(y + 1) * size + x].grayscale - heights[y * size + x].grayscale) 
                + v * (heights[(y + 1) * size + x + 1].grayscale - heights[y * size + x + 1].grayscale);

        d.dir = d.dir * inertia - grad * (1 - inertia);
        d.dir.Normalize();

        float hOld = InterpolateHeight(d.pos, ref heights, size);
        Vector2 posOld = d.pos;
        d.pos = d.pos + d.dir;
        if (!d.isOnMap(size))
        {
            return;
        }
        float hNew = InterpolateHeight(d.pos, ref heights, size);
        float hDiff = hNew - hOld;
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

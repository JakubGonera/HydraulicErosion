using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    GameObject meshGO;
    int resolution;
    public int size = 20;
    public float height = 2f;
    public List<Material> terrainMaterials;
    MeshRenderer meshRenderer;
    Mesh procMesh;

    // Start is called before the first frame update
    void Start()
    {
        meshGO = new GameObject("Terrain");
        meshGO.AddComponent<MeshFilter>();
        meshGO.AddComponent<MeshRenderer>().materials = terrainMaterials.ToArray();
        meshGO.tag = "Terrain";
        meshRenderer = meshGO.GetComponent<MeshRenderer>();
        procMesh = new Mesh();
        meshGO.GetComponent<MeshFilter>().mesh = procMesh;

    }

    public void Construct(Texture2D heightMap)
    {
        meshGO.transform.position = new Vector3(0, 0, 0);
        resolution = heightMap.width;
        List<int> triangles = new List<int>();
        List<Vector3> verticies = new List<Vector3>();
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                verticies.Add(new Vector3(((float)i/resolution) * (float)size, 
                                            heightMap.GetPixel(i, j).grayscale * size * height/(10.0f), 
                                            ((float)j/resolution) * (float)size));
                if(i != resolution - 1 && j != resolution - 1)
                {
                    //Triangle 1
                    triangles.Add(resolution * i + j);
                    triangles.Add(resolution * i + j + 1);
                    triangles.Add(resolution * (i + 1) + j + 1);

                    //Triangle 2
                    triangles.Add(resolution * i + j);
                    triangles.Add(resolution * (i + 1) + j + 1);
                    triangles.Add(resolution * (i + 1) + j);
                }
            }
        }

        Vector2[] uvs = new Vector2[verticies.Count];
        for (int i = 0; i < verticies.Count; i++)
        {
            uvs[i] = new Vector2(verticies[i].x, verticies[i].z);
        }

        procMesh.vertices = verticies.ToArray();
        procMesh.uv = uvs;
        procMesh.triangles = triangles.ToArray();
        procMesh.RecalculateNormals();

        meshGO.transform.position = -meshRenderer.bounds.center;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

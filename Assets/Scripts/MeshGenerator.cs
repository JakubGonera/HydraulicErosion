using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    GameObject meshGO;
    public int size = 20;
    public float height = 2f;
    public List<Material> terrainMaterials;
    MeshRenderer meshRenderer;
    Mesh procMesh;
    public int resolution = 256;//Make it automatic
    Vector3[] vertices;


    // Start is called before the first frame update
    void Start()
    {
        meshGO = new GameObject("Terrain");
        meshGO.AddComponent<MeshFilter>();
        meshGO.AddComponent<MeshRenderer>().materials = terrainMaterials.ToArray();
        meshGO.tag = "Terrain";
        meshRenderer = meshGO.GetComponent<MeshRenderer>();
        procMesh = new Mesh();
        procMesh.MarkDynamic();
        meshGO.GetComponent<MeshFilter>().mesh = procMesh;

        vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 2 * 3];
        int triangleCounter = 0;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                if (i != resolution - 1 && j != resolution - 1)
                {
                    //Triangle 1
                    triangles[triangleCounter] = resolution * i + j;
                    triangles[triangleCounter + 1] = resolution * i + j + 1;
                    triangles[triangleCounter + 2] = resolution * (i + 1) + j + 1;

                    //Triangle 2
                    triangles[triangleCounter + 3] = resolution * i + j;
                    triangles[triangleCounter + 4] = resolution * (i + 1) + j + 1;
                    triangles[triangleCounter + 5] = resolution * (i + 1) + j;
                    triangleCounter += 6;
                }
            }
        }

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                vertices[i * resolution + j] = new Vector3(i, 0, j);
            }
        }

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        procMesh.vertices = vertices;
        procMesh.uv = uvs;
        procMesh.triangles = triangles;
        procMesh.RecalculateNormals();
        procMesh.RecalculateBounds();

        meshGO.transform.position = -meshRenderer.bounds.center;
    }

    public void Construct(Texture2D heightMap)
    {
        //meshGO.transform.position = new Vector3(0, 0, 0);
        ////int[] triangles = new int[(resolution - 1) * (resolution - 1) * 2 * 3];
        ////Vector3[] verticies = new Vector3[resolution * resolution];
        ////int triangleCounter = 0;
        //for (int i = 0; i < resolution; i++)
        //{
        //    for (int j = 0; j < resolution; j++)
        //    {
        //        vertices[i * resolution + j] = new Vector3(((float)i / resolution) * (float)size, 
        //            heightMap.GetPixel(i, j).grayscale * size * height / (10.0f), 
        //            ((float)j / resolution) * (float)size);
        //        //if(i != resolution - 1 && j != resolution - 1)
        //        //{
        //        //    //Triangle 1
        //        //    procMesh.triangles[triangleCounter] = resolution * i + j;
        //        //    procMesh.triangles[triangleCounter + 1] = resolution * i + j + 1;
        //        //    procMesh.triangles[triangleCounter + 2] = resolution * (i + 1) + j + 1;

        //        //    //Triangle 2
        //        //    procMesh.triangles[triangleCounter + 3] = resolution * i + j;
        //        //    procMesh.triangles[triangleCounter + 4] = resolution * (i + 1) + j + 1;
        //        //    procMesh.triangles[triangleCounter + 5] = resolution * (i + 1) + j;
        //        //    triangleCounter += 6;
        //        //}
        //    }
        //}

        //procMesh.vertices = vertices;
        //procMesh.RecalculateBounds();
        //procMesh.RecalculateNormals();

        //meshGO.transform.position = -meshRenderer.bounds.center;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

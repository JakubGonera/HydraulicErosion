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
    public int resolution = 256;
    Vector3[] vertices;


    // Start is called before the first frame update
    void Start()
    {
        //Create new GameObject with components needed for rendering mesh
        meshGO = new GameObject("Terrain");
        meshGO.AddComponent<MeshFilter>();
        meshGO.AddComponent<MeshRenderer>().materials = terrainMaterials.ToArray();
        meshGO.tag = "Terrain";
        meshRenderer = meshGO.GetComponent<MeshRenderer>();
        procMesh = new Mesh();
        procMesh.MarkDynamic();
        meshGO.GetComponent<MeshFilter>().mesh = procMesh;

        //Generate a plane
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

        //Assign the generated data to the object
        procMesh.vertices = vertices;
        procMesh.triangles = triangles;
        procMesh.RecalculateNormals();
        
        //Calculate the bounds of the mesh and center the object so the middle point is at (0,0,0)
        procMesh.RecalculateBounds();

        meshGO.transform.position = -meshRenderer.bounds.center;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatMesh : MonoBehaviour
{
    public int n = 20; // Set the value of n here

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> indices = new List<int>();
    private Mesh mesh;

    void Start()
    {
        GenerateGrid();
        CreateMesh();
    }

    private void GenerateGrid()
    {
        int nx = n;
        int nz = n;
        float dx = 2.0f / nx;
        float dz = 2.0f / nz;

        vertices.Clear();
        for (int i = 0; i <= nx; i++)
        {
            for (int j = 0; j <= nz; j++)
            {
                Vector3 vertexPosition = new Vector3(-1.0f + i * dx, 0.0f, -1.0f + j * dz);
                vertices.Add(vertexPosition);
            }
        }

        indices.Clear();
        for (int i = 0; i < nx; i++)
        {
            for (int j = 0; j < nz; j++)
            {
                int idx = j + i * (nz + 1);
                int J = 1;
                int I = nz + 1;

                indices.Add(idx + J);
                indices.Add(idx + I);
                indices.Add(idx );

                indices.Add(idx + J );
                indices.Add(idx + I + J);
                indices.Add(idx + I);
            }
        }
    }

    private void CreateMesh()
    {
        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();

        mesh.RecalculateNormals();

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        //meshRenderer.material = new Material(Shader.Find("Standard")); // You can change the material as per your requirement
    }
}

using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CylindricalScreen : MonoBehaviour
{
    public float radius = 1;
    public float height = 1;
    public float horizontalArcAngle = 180.0f;

    private Mesh _mesh;

    public void Rebuild()
    {
        int segmentsPerUnit = 2;

        int xSegments = (int)radius * segmentsPerUnit * 2;
        int ySegments = (int)height * segmentsPerUnit;

        MeshFilter meshFilter = (MeshFilter)GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found!");
            return;
        }

        _mesh = meshFilter.sharedMesh;

        int numTriangles = xSegments * ySegments * 6;
        int numVertices = (xSegments + 1) * (ySegments + 1);

        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] triangles = new int[numTriangles];
        Vector4[] tangents = new Vector4[numVertices];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        int index = 0;
        float uvFactorX = 1.0f / xSegments;
        float uvFactorY = 1.0f / ySegments;
        //float scaleX = Mathf.PI / xSegments;
        float viewAngle = horizontalArcAngle * Mathf.Deg2Rad;
        float scaleX = viewAngle / xSegments;
        float scaleY = height / ySegments;

        // Populate vertices, tangents, and UVs
        for (int y = 0; y <= ySegments; y++)
        {
            for (int x = 0; x <= xSegments; x++)
            {

                float startAngle = Mathf.PI + (viewAngle - Mathf.PI) / 2.0f;
                //float angle = Mathf.PI - (float)x * scaleX;
                float angle = startAngle - (float)x * scaleX;
                float px = Mathf.Cos(angle) * radius;
                float py = (float)y * scaleY - height / 2f;
                float pz = Mathf.Sin(angle) * radius;
                vertices[index] = new Vector3(px, py, pz);

                tangents[index] = tangent;
                uvs[index++] = new Vector2((float)x * uvFactorX, (float)y * uvFactorY);
            }
        }

        // Generate triangles based on the index of vertices
        index = 0;
        int hCount = xSegments + 1;
        //		int vCount = ySegments + 1;
        for (int y = 0; y < ySegments; y++)
        {
            for (int x = 0; x < xSegments; x++)
            {
                triangles[index] = (y * hCount) + x;
                triangles[index + 1] = ((y + 1) * hCount) + x;
                triangles[index + 2] = (y * hCount) + x + 1;

                triangles[index + 3] = ((y + 1) * hCount) + x;
                triangles[index + 4] = ((y + 1) * hCount) + x + 1;
                triangles[index + 5] = (y * hCount) + x + 1;
                index += 6;
            }
        }

        _mesh.vertices = vertices;
        _mesh.uv = uvs;
        _mesh.triangles = triangles;
        _mesh.tangents = tangents;

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
        ;
    }

    // Use this for initialization
    void Start()
    {
        Rebuild();
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = _mesh;
    }
}

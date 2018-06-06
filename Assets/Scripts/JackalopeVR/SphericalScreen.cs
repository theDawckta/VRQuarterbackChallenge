using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SphericalScreen : MonoBehaviour
{
    public float radius = 1;
    public int horizontalSegments = 128;
    public int verticalSegments = 64;

    public void Rebuild()
    {
        MeshFilter meshFilter = (MeshFilter)GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found!");
            return;
        }

        Mesh mesh = meshFilter.sharedMesh;

        int poleVertical = 3;
        int hSegments = horizontalSegments;
        int vSegments = verticalSegments + poleVertical * 2;
        int numTriangles = hSegments * vSegments * 6;
        int numVertices = (hSegments + 1) * (vSegments + 1);

        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] triangles = new int[numTriangles];
        Vector4[] tangents = new Vector4[numVertices];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        // Populate vertices, tangents, and UVs
        int index = 0;
        for (int y = 0; y <= vSegments; y++)
        {
            float yf = 0.0f;
            if (y <= poleVertical)
            {
                yf = (float)y / (float)(poleVertical + 1) / (float)verticalSegments;
            }
            else if (y >= vSegments - poleVertical)
            {
                yf = (float)(verticalSegments - 1 + ((float)(y - (vSegments - poleVertical - 1)) / (poleVertical + 1))) / verticalSegments;
            }
            else
            {
                yf = (float)(y - poleVertical) / (float)vSegments;
            }
            float lat = ((float)yf - 0.5f) * Mathf.PI;
            float cosLat = Mathf.Cos(lat);

            for (int x = 0; x <= hSegments; x++)
            {
                float xf = (float)x / (float)hSegments;
                float lon = (0.25f + xf) * Mathf.PI * 2.0f;

                if (x == hSegments)
                {
                    vertices[index] = vertices[y * (hSegments + 1)];
                }
                else
                {
                    float px = radius * Mathf.Cos(lon) * cosLat;
                    float py = radius * Mathf.Sin(lat);
                    float pz = radius * Mathf.Sin(lon) * cosLat;
                    vertices[index] = new Vector3(px, py, pz);
                }

                tangents[index] = tangent;
                uvs[index++] = new Vector2(xf, yf);
            }
        }

        // Generate triangles based on the index of vertices
        index = 0;
        for (int y = 0; y < vSegments; y++)
        {
            for (int x = 0; x < hSegments; x++)
            {
                //				triangles [index] = (y * (hSegments + 1)) + x;
                //				triangles [index + 1] = ((y + 1) * (hSegments + 1)) + x;
                //				triangles [index + 2] = (y * (hSegments + 1)) + x + 1;
                //
                //				triangles [index + 3] = ((y + 1) * (hSegments + 1)) + x;
                //				triangles [index + 4] = ((y + 1) * (hSegments + 1)) + x + 1;
                //				triangles [index + 5] = (y * (hSegments + 1)) + x + 1;
                triangles[index] = (y * (hSegments + 1)) + x;
                triangles[index + 1] = (y * (hSegments + 1)) + x + 1;
                triangles[index + 2] = ((y + 1) * (hSegments + 1)) + x;

                triangles[index + 3] = ((y + 1) * (hSegments + 1)) + x;
                triangles[index + 4] = (y * (hSegments + 1)) + x + 1;
                triangles[index + 5] = ((y + 1) * (hSegments + 1)) + x + 1;
                index += 6;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.tangents = tangents;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        ;
    }
}

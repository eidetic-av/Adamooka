using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class LiveScanMeshVisualizer : MonoBehaviour
{
    public static LiveScanMeshVisualizer Instance;

    [Tooltip("Time in seconds between mesh updates.")]
    public float UpdateMeshInterval = 0.25f;
    public int SampleDepth = 8;

    float LastMeshUpdateTime;

    public Mesh Mesh;

    public bool DisableMeshUpdate = false;
    public bool BlockKinectUpdate = false;

    void Start()
    {
        Instance = this;
        CreateMesh();
    }

    void Update()
    {
        // update the mesh
        if (!DisableMeshUpdate)
            UpdateMesh();
    }

    private void CreateMesh()
    {
        Mesh = new Mesh();
        Mesh.name = "LiveScanMesh";

        GetComponent<MeshFilter>().mesh = Mesh;
    }

    private void UpdateMesh()
    {
        if ((Time.time - LastMeshUpdateTime) >= UpdateMeshInterval)
        {
            float[] points = PointCloudReceiver.Instance.Vertices;
            if (points.Length <= 3) return;

            int totalPointCount = (points.Length / 3);
            int sampledPointCount = totalPointCount / SampleDepth;

            Vector3[] vertices = new Vector3[sampledPointCount];
            
            for (int i = 0; i + 2 < sampledPointCount; i += (3 * SampleDepth))
            {
                vertices[i] = (new Vector3(points[i + 0], points[i + 1], -points[i + 2]));       
            }

            if (!BlockKinectUpdate)
            {
                Mesh.Clear();
                Mesh.vertices = vertices;
                Mesh.triangles = ConvexHull.Generate(Mesh.vertices);
                Mesh.RecalculateNormals();
                Mesh.RecalculateBounds();
            }

            // if (UpdateMeshCollider)
            // {
            //     MeshCollider meshCollider = GetComponent<MeshCollider>();

            //     if (meshCollider)
            //     {
            //         meshCollider.sharedMesh = null;
            //         meshCollider.sharedMesh = Mesh;
            //     }
            // }

            LastMeshUpdateTime = Time.time;
        }
    }

}

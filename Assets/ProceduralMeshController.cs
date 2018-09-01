using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class ProceduralMeshController : MonoBehaviour
{
    public static ProceduralMeshController Instance;

    public MeshFilter BaseMeshFilter;
    public SkinnedMeshRenderer BaseSkinnedMeshRenderer;
    public Vector3 BaseMeshScale = Vector3.one;
    public Vector3 BaseMeshOffset = Vector3.zero;

    [Range(0, 1)]
    public float Interpolation = 0;
    public float InterpolationPower = 1;
    public Vector3 NoisePlaneSpan = Vector3.one;
    public Vector3 NoisePlaneOffset = Vector3.zero;

    public bool ControlInterpolationWithAirSticks = true;
    public Vector2 AirsticksControlMinMax = Vector2.one;

    private Mesh Mesh;
    // Use this for initialization
    void Start()
    {
        Instance = this;

        Mesh = new Mesh();
        Mesh.name = "ProceduralMesh";
        GetComponent<MeshFilter>().mesh = Mesh;

        // once the mesh filter is attached to the particle system in the 
        // inspector, we can just turn the renderer off
        GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (BaseMeshFilter == null && BaseSkinnedMeshRenderer == null) return;

        if (ControlInterpolationWithAirSticks)
            Interpolation = Mathf.Clamp(AirSticks.Right.Position.y.Map(AirsticksControlMinMax.x, AirsticksControlMinMax.y, 0, 1), 0, 1);

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        Vector3[] meshVertices;
        if (BaseMeshFilter != null) {
            meshVertices = BaseMeshFilter.sharedMesh.vertices;
        } else {
           BaseSkinnedMeshRenderer.BakeMesh(mesh);
           meshVertices = mesh.vertices;
        }

        var vertices = new Vector3[meshVertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            // generate a vertex position based on noise
            var noiseX = (Random.value * NoisePlaneSpan.x) + NoisePlaneOffset.x;
            var noiseY = (Random.value * NoisePlaneSpan.y) + NoisePlaneOffset.y;
            var noiseZ = (Random.value * NoisePlaneSpan.z) + NoisePlaneOffset.z;
            // interpolate between the noisy vertex and the mesh's vertex
            var lerp = Mathf.Pow(Interpolation, InterpolationPower);
            var x = Mathf.Lerp((meshVertices[i].x * BaseMeshScale.x) + BaseMeshOffset.x, noiseX, lerp);
            var y = Mathf.Lerp((meshVertices[i].y * BaseMeshScale.y) + BaseMeshOffset.y, noiseY, lerp);
            var z = Mathf.Lerp((meshVertices[i].z * BaseMeshScale.z) + BaseMeshOffset.z, noiseZ, lerp);
            // assign it to the vertex array
            vertices[i] = new Vector3(x, y, z);
        }

        // triangles can be dummy since we are not rendering faces
        // so just fill up the array so unity is happy
        var triangleLength = vertices.Length;
        // make sure it is a multiple of 3
        while (triangleLength % 3 != 0)
        {
            triangleLength++;
        }
        int[] triangles = new int[triangleLength];
        for (int i = 0; i < triangles.Length; i++)
        {
            var t = i;
            if (i >= vertices.Length) t = 0;
            triangles[i] = t;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}

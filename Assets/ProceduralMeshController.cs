using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Eidetic.Andamooka;

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
    private MeshFilter MeshFilter;

    private Vector3[] Vertices;
    private int[] Triangles;
    private Vector3[] BaseMeshVertices;
    // Use this for initialization
    void Start()
    {
        Instance = this;

        MeshFilter = GetComponent<MeshFilter>();

        Mesh = new Mesh();
        Mesh.name = "ProceduralMesh";
        MeshFilter.mesh = Mesh;

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

        if (BaseMeshFilter != null) {
            BaseMeshVertices = BaseMeshFilter.mesh.vertices;
        } else {
           BaseSkinnedMeshRenderer.BakeMesh(Mesh);
           BaseMeshVertices = Mesh.vertices;
        }

        Vertices = new Vector3[BaseMeshVertices.Length];
        for (int i = 0; i < Vertices.Length; i++)
        {
            // generate a vertex position based on noise
            var noiseX = (Random.value * NoisePlaneSpan.x) + NoisePlaneOffset.x;
            var noiseY = (Random.value * NoisePlaneSpan.y) + NoisePlaneOffset.y;
            var noiseZ = (Random.value * NoisePlaneSpan.z) + NoisePlaneOffset.z;
            // interpolate between the noisy vertex and the mesh's vertex
            var lerp = Mathf.Pow(Interpolation, InterpolationPower);
            var x = Mathf.Lerp((BaseMeshVertices[i].x * BaseMeshScale.x) + BaseMeshOffset.x, noiseX, lerp);
            var y = Mathf.Lerp((BaseMeshVertices[i].y * BaseMeshScale.y) + BaseMeshOffset.y, noiseY, lerp);
            var z = Mathf.Lerp((BaseMeshVertices[i].z * BaseMeshScale.z) + BaseMeshOffset.z, noiseZ, lerp);
            // assign it to the vertex array
            Vertices[i] = new Vector3(x, y, z);
        }

        // triangles can be dummy since we are not rendering faces
        // so just fill up the array so unity is happy
        var triangleLength = Vertices.Length;
        // make sure it is a multiple of 3
        while (triangleLength % 3 != 0)
        {
            triangleLength++;
        }
        Triangles = new int[triangleLength];
        for (int i = 0; i < Triangles.Length; i++)
        {
            var t = i;
            if (i >= Vertices.Length) t = 0;
            Triangles[i] = t;
        }

        Mesh.Clear();
        Mesh.vertices = Vertices;
        Mesh.triangles = Triangles;
    }
}

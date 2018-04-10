using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SmoothMyMesh : MonoBehaviour
{

    [System.Serializable]
    enum FilterType
    {
        Laplacian, HC
    };

    MeshFilter filter
    {
        get
        {
            if (_filter == null)
            {
                _filter = GetComponent<MeshFilter>();
            }
            return _filter;
        }
    }

    MeshFilter _filter;

    [SerializeField, Range(0f, 1f)] public float NoiseIntensity = 0.5f;
    [SerializeField] bool ContinuousUpdate = false;
    float LastUpdateTime;
    [SerializeField] FilterType Type;
    [SerializeField, Range(0, 20)] int Times = 3;
    [SerializeField, Range(0f, 1f)] float HCAlpha = 0.5f;
    [SerializeField, Range(0f, 1f)] float HCBeta = 0.5f;

    bool LimitingMesh = false;
    LimitMesh LimitMesh;

    void Start()
    {
        ApplySmoothing();
        if (gameObject.GetComponent<LimitMesh>() != null)
        {
            LimitMesh = gameObject.GetComponent<LimitMesh>();
            LimitingMesh = true;
        }
    }

    void ApplySmoothing()
    {
        var mesh = filter.mesh;
        if (mesh.vertexCount != 0)
        {
            filter.mesh = ApplyNormalNoise(mesh);

            switch (Type)
            {
                case FilterType.Laplacian:
                    if (!LimitingMesh)
                        filter.mesh = MeshSmoothing.LaplacianFilter(filter.mesh, Times);
                    else
                    {
                        filter.mesh = MeshSmoothing.LaplacianFilter(filter.mesh, LimitMesh, Times);
                    }
                    break;
                case FilterType.HC:
                    if (!LimitingMesh)
                        filter.mesh = MeshSmoothing.HCFilter(filter.mesh, Times, HCAlpha, HCBeta);
                    else
                    {
                        filter.mesh = MeshSmoothing.HCFilter(filter.mesh, LimitMesh.LimitedTriangles, Times, HCAlpha, HCBeta);
                    }
                    break;
            }
        }

        LastUpdateTime = Time.time;
    }

    void LateUpdate()
    {
        if (ContinuousUpdate)
        {
            ApplySmoothing();
        }
    }

    Mesh ApplyNormalNoise(Mesh mesh)
    {

        var vertices = mesh.vertices;
        var normals = mesh.normals;
        for (int i = 0, n = mesh.vertexCount; i < n; i++)
        {
            vertices[i] = vertices[i] + normals[i] * Random.value * NoiseIntensity;
        }
        mesh.vertices = vertices;

        return mesh;
    }

}


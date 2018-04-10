using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputMappingController : MonoBehaviour
{

    public Vector3[] VertexPositions = new Vector3[4];
    MeshFilter OutputQuadMeshFilter;
    // Use this for initialization
    void Start()
    {
        //OutputQuadMeshFilter = GameObject.Find("OutputQuad").GetComponent<MeshFilter>();
        //VertexPositions[0] = OutputQuadMeshFilter.mesh.vertices[0];
        //VertexPositions[1] = OutputQuadMeshFilter.mesh.vertices[1];
        //VertexPositions[2] = OutputQuadMeshFilter.mesh.vertices[2];
        //VertexPositions[3] = OutputQuadMeshFilter.mesh.vertices[3];
    }

    //Update is called once per frame
    //void Update()
    //{
    //var mesh = OutputQuadMeshFilter.mesh;
    //mesh.vertices = VertexPositions;
    //}
}

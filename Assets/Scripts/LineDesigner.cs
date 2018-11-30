using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineInterrupter
{
    public class LineDesigner : MonoBehaviour
    {
        LineInterrupt LineInterrupt;
        MeshFilter TestQuadMeshFilter;
        // Use this for initialization
        void Start()
        {
            LineInterrupt = gameObject.GetComponent<LineInterrupt>();
            TestQuadMeshFilter = GameObject.Find("TestQuad").GetComponent<MeshFilter>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector3[] quadVertices = new Vector3[4];
            foreach (LineRenderer renderer in LineInterrupt.LeftInterruptLineRenderers)
            {
                Vector3[] positions = new Vector3[renderer.positionCount];
                renderer.GetPositions(positions);
                quadVertices[0] = positions[0];
                quadVertices[1] = positions[1];
                quadVertices[2] = positions[0] + (Vector3.left * 10);
                quadVertices[3] = positions[1] + (Vector3.left * 10);
                break;
            }
            var mesh = TestQuadMeshFilter.mesh;
            mesh.vertices = quadVertices;
        }   
    }
}

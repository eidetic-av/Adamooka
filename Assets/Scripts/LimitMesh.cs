using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Eidetic.Utility;

public class LimitMesh : MonoBehaviour
{

    [Range(1, 100000)]
    public int TriangleLimit = 5001;

    MeshFilter MeshFilter;
    public List<int> LimitedTriangles { get; private set; } = new List<int>();
    public bool ControlWithMouse = false;

    [HideInInspector()]
    public bool Limiting = true;

    void Start()
    {
        MeshFilter = gameObject.GetComponent<MeshFilter>();
    }

    private void Update()
    {
        if (ControlWithMouse)
        {
            TriangleLimit = Mathf.RoundToInt(Input.mousePosition.y.Map(0, Screen.height, 1, 100000));
        }
    }

    void LateUpdate()
    {
        while (TriangleLimit % 3 != 0 || TriangleLimit <= 0)
        {
            TriangleLimit += 1;
        }
        if (KinectManager.Instance.GetUsersCount() != 0)
        {
            var mesh = MeshFilter.mesh;
            int[] allTriangles = mesh.triangles;
            int[] limitedTriangles = new int[TriangleLimit];
            if (limitedTriangles.Length < allTriangles.Length)
            {
                Array.Copy(allTriangles, limitedTriangles, TriangleLimit);
                mesh.triangles = limitedTriangles;
                Limiting = true;
            } else
            {
                mesh.triangles = allTriangles;
                Limiting = false;
            }
            MeshFilter.mesh = mesh;
            LimitedTriangles = limitedTriangles.ToList();
        }
    }
}

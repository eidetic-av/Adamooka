/*
 * @author mattatz / http://mattatz.github.io
 * @author Eidetic

 * https://www.researchgate.net/publication/220507688_Improved_Laplacian_Smoothing_of_Noisy_Surface_Meshes
 * http://graphics.stanford.edu/courses/cs468-12-spring/LectureSlides/06_smoothing.pdf
 * http://wiki.unity3d.com/index.php?title=MeshSmoother
 * 
 * Original MeshSmoother by mattatz, additions added by Eidetic
 */

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;

public class MeshSmoothing
{

    public static bool OffsetTriangles = false;
    public static List<int> PreviousTriangles;
    public static int[] LatestTriangles;

    public static Mesh LaplacianFilter(Mesh mesh, int times, JobHandle jobHandlerDependency)
    {
        mesh.vertices = LaplacianFilter(mesh.vertices, mesh.triangles, times, jobHandlerDependency);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Mesh LaplacianFilter(Mesh mesh, LimitMesh limitMesh, int times, JobHandle jobHandlerDependency)
    {
        if (!limitMesh.Limiting) return LaplacianFilter(mesh, times, jobHandlerDependency);

        var limitedTriangles = limitMesh.LimitedTriangles.ToArray();
        if (limitedTriangles.Length == 0) limitedTriangles = mesh.triangles;
        var limitedVertexCount = limitedTriangles.Max() + 1;
        var limitedVertices = new Vector3[limitedVertexCount];

        Array.Copy(mesh.vertices, limitedVertices, limitedVertices.Length);

        mesh.vertices = LaplacianFilter(limitedVertices, limitedTriangles, times, jobHandlerDependency);
        if (!OffsetTriangles || LatestTriangles == null)
        {
            mesh.triangles = limitedTriangles;
            LatestTriangles = limitedTriangles;
        } 
        else
        {
            // use the last triangles not the current ones to provide a noisy effect
            PreviousTriangles = LatestTriangles.ToList();
            int lastVertex = mesh.vertices.Length;
            // remove any triangles that reference vertices that don't exist in this frame
            PreviousTriangles.RemoveAll(v => (v >= lastVertex));
            // and make sure the count is divisable by three to make a full triangle
            while((PreviousTriangles.Count % 3) != 0)
                PreviousTriangles.Add(lastVertex);
            mesh.triangles = PreviousTriangles.ToArray();
            LatestTriangles = limitedTriangles;
        }
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public static Dictionary<int, VertexConnection> Network;
    public static Vector3[] LatestVertices;

    public static Vector3[] LaplacianFilter(Vector3[] sourceVertices, int[] sourceTriangles, int times, JobHandle initialJobHandlerDependency)
    {
        var filterJobHandlers = new List<JobHandle>();
        int batchSize = 250;

        Network = VertexConnection.BuildNetwork(sourceTriangles);
        LatestVertices = sourceVertices;

        for (int i = 0; i < times; i++)
        {

            var vertexArray = new NativeArray<Vector3>(LatestVertices, Allocator.TempJob);
            var trianglesArray = new NativeArray<int>(sourceTriangles, Allocator.TempJob);

            var job = new LaplactianFilterJob
            {
                vertices = vertexArray,
                triangles = trianglesArray
            };

            if (i == 0)
            {
                filterJobHandlers.Add(job.Schedule(vertexArray.Length, batchSize, initialJobHandlerDependency));
            }
            else
            {
                filterJobHandlers.Add(job.Schedule(vertexArray.Length, batchSize, filterJobHandlers[i - 1]));
            }

            filterJobHandlers.Last().Complete();

            // set vertices
            LatestVertices = new Vector3[vertexArray.Length];
            vertexArray.CopyTo(LatestVertices);

            vertexArray.Dispose();
            trianglesArray.Dispose();

        }

        return LatestVertices;
    }

    public struct LaplactianFilterJob : IJobParallelFor
    {
        public NativeArray<Vector3> vertices;
        public NativeArray<int> triangles;

        public void Execute(int i)
        {
            if (!Network.ContainsKey(i)) return;

            var connection = Network[i].Connection;
            var v = Vector3.zero;
            foreach (int adj in connection)
            {
                v += LatestVertices[adj];
            }
            vertices[i] = v / connection.Count;
        }
    };

    static Vector3[] LaplacianFilter(Dictionary<int, VertexConnection> network, Vector3[] origin, List<int> limitedTriangles)
    {
        int limitedVertexCount = limitedTriangles.Max() + 1;
        Vector3[] vertices = new Vector3[limitedVertexCount];
        for (int i = 0, n = limitedVertexCount; i < n; i++)
        {
            if (!network.ContainsKey(i)) continue;
            var connection = network[i].Connection;
            var v = Vector3.zero;
            foreach (int adj in connection)
            {
                if (adj >= origin.Length) continue;
                v += origin[adj];
            }
            vertices[i] = v / connection.Count;
        }
        return vertices;
    }

    /*
     * HC (Humphrey’s Classes) Smooth Algorithm - Reduces Shrinkage of Laplacian Smoother
     * alpha 0.0 ~ 1.0
     * beta  0.0 ~ 1.0
    */
    public static Mesh HCFilter(Mesh mesh, int times = 5, float alpha = 0.5f, float beta = 0.75f)
    {
        mesh.vertices = HCFilter(mesh.vertices, mesh.triangles, times, alpha, beta);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Mesh HCFilter(Mesh mesh, List<int> limitedTriangles, int times = 5, float alpha = 0.5f, float beta = 0.75f)
    {
        mesh.vertices = HCFilter(mesh.vertices, limitedTriangles, times, alpha, beta);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    static Vector3[] HCFilter(Vector3[] vertices, int[] triangles, int times, float alpha, float beta)
    {
        alpha = Mathf.Clamp01(alpha);
        beta = Mathf.Clamp01(beta);

        var network = VertexConnection.BuildNetwork(triangles);

        Vector3[] origin = new Vector3[vertices.Length];
        Array.Copy(vertices, origin, vertices.Length);
        for (int i = 0; i < times; i++)
        {
            vertices = HCFilter(network, origin, vertices, triangles, alpha, beta);
        }
        return vertices;
    }

    static Vector3[] HCFilter(Vector3[] vertices, List<int> limitedTriangles, int times, float alpha, float beta)
    {
        alpha = Mathf.Clamp01(alpha);
        beta = Mathf.Clamp01(beta);

        var limitedTrianglesArray = limitedTriangles.ToArray();

        var network = VertexConnection.BuildNetwork(limitedTrianglesArray);
        var limitedVertexCount = limitedTriangles.Max();
        Vector3[] origin = new Vector3[limitedVertexCount];
        Array.Copy(vertices, origin, limitedVertexCount);
        for (int i = 0; i < times; i++)
        {
            vertices = HCFilter(network, origin, vertices, limitedTrianglesArray, alpha, beta);
        }
        return vertices;
    }

    public static Vector3[] HCFilter(Dictionary<int, VertexConnection> network, Vector3[] o, Vector3[] q, int[] triangles, float alpha, float beta)
    {
        //Vector3[] p = LaplacianFilter(network, q, triangles);
        //Vector3[] b = new Vector3[o.Length];

        //for (int i = 0; i < p.Length; i++)
        //{
        //    b[i] = p[i] - (alpha * o[i] + (1f - alpha) * q[i]);
        //}

        //for (int i = 0; i < p.Length; i++)
        //{
        //    var adjacents = network[i].Connection;
        //    var bs = Vector3.zero;
        //    foreach (int adj in adjacents)
        //    {
        //        bs += b[adj];
        //    }
        //    p[i] = p[i] - (beta * b[i] + (1 - beta) / adjacents.Count * bs);
        //}

        //return p;
        return null;
    }

}



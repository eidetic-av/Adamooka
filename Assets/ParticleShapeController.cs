using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class ParticleShapeController : MonoBehaviour
{
    public static ParticleShapeController Instance;

    public ParticleSystem OutputParticleSystem;

    public ParticleSystem ReferenceParticleSystem;

    public List<MeshFilter> BaseMeshFilters;

    [Range(0, 1)]
    public float Interpolation = 0;
    public float InterpolationPower = 1;

    // Nth skip interpolates every nth particle at a different rate
    // this allows for some "looser" particles, not as aligned to the meshes
    public bool NthSkipActive = true;
    [Range(2, 15)]
    public int NthSkip = 0;
    [Range(0.75f, 0.999f)]
    public float NthSkipInterpolation = 0;

    private Mesh Mesh;
    // Use this for initialization
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (BaseMeshFilters == null) return;

        // update output system settings to current reference system settings
        var mainModule = OutputParticleSystem.main;
        mainModule.maxParticles = ReferenceParticleSystem.main.maxParticles;

        OutputParticleSystem.Clear();
        OutputParticleSystem.Emit(ReferenceParticleSystem.particleCount);

        var outputParticles = new ParticleSystem.Particle[ReferenceParticleSystem.particleCount];
        OutputParticleSystem.GetParticles(outputParticles);

        // this is the particle system that is used as a reference for positions.
        // it runs by itself and we interpolate the above system between this and 
        // the position of the meshes vertices
        var referenceParticles = new ParticleSystem.Particle[ReferenceParticleSystem.particleCount];
        ReferenceParticleSystem.GetParticles(referenceParticles);

        int setParticlesCount = 0;
        int meshStartIndex = 0;
        // iterate through each mesh in the list and assign particle positions
        foreach (MeshFilter meshFilter in BaseMeshFilters)
        {
            // get the vertex positions of the mesh
            Vector3[] meshVertices = meshFilter.sharedMesh.vertices;

            // create a matrix from the mesh's gameobject transform so that the particles can be correctly transformed
            Transform meshTransform = meshFilter.gameObject.transform;
            Matrix4x4 matrix = Matrix4x4.TRS(meshTransform.position, meshTransform.rotation, meshTransform.localScale);

            // find out how many particles we should assign to this mesh
            var particleCount = Mathf.FloorToInt((float)outputParticles.Length / BaseMeshFilters.Count);

            // there are likely more particles than vertices, so we can supersample the 
            // vertex data and draw the particles along the edges of the mesh rather than specifically at
            // each vertex
            // so lets find out the number of particles we can use per vertex of the mesh
            var particlesPerVertex = particleCount / (float)meshVertices.Length;
            // if it is less than one particle per vertex though, set it to 1
            if (particlesPerVertex < 1)
            {
                particlesPerVertex = 1;
            }
            else
            {
                // but if it is more than 1 round it to a whole number
                particlesPerVertex = Mathf.FloorToInt(particlesPerVertex);
            }

            // iterate through each point of the mesh
            for (int i = meshStartIndex; i < meshStartIndex + meshVertices.Length; i++)
            {
                // break if the particle system isn't big enough
                if (setParticlesCount >= referenceParticles.Length) break;

                List<Vector3> positions = new List<Vector3>();

                // if there is one particle per vertex, set one particle to the vertex position
                if (particlesPerVertex == 1)
                {
                    positions.Add(meshVertices[i - meshStartIndex]);
                }
                else
                {
                    // otherwise, set multiple particles to space out between the current mesh vertex and the next

                    // use a hermite interpolation algorithm to figure out the position of each particle along
                    // the edge of the mesh

                    var index = i - meshStartIndex;

                    // for this algorithm we need four points
                    Vector3 p0, p1, p2, p3;

                    if ((index - 1 >= 0) && (index + 2 < meshVertices.Length))
                    {
                        // if points surrounding p1->p2 exist, we can just get them
                        p0 = meshVertices[index - 1];
                        p1 = meshVertices[index];
                        p2 = meshVertices[index + 1];
                        p3 = meshVertices[index + 2];

                        for (int p = 0; p < particlesPerVertex; p++)
                        {
                            var mu = (float)p / particlesPerVertex;
                            positions.Add(HermiteInterpolate3D(p0, p1, p2, p3, mu, 0, 0));
                        }

                    }
                    // if we don't have access to the start point for hermite interpolation,
                    // just use a linear interpolation
                    else if (index - 1 < 0)
                    {
                        p1 = meshVertices[index];
                        p2 = meshVertices[index + 1];

                        for (int p = 0; p < particlesPerVertex; p++)
                        {
                            var mu = (float)p / particlesPerVertex;
                            positions.Add(Lerp3D(p1, p2, mu));
                        }
                    }
                }

                for (int k = 0; k < positions.Count; k++)
                {
                    // apply the transform to the vertex positions aquired through the (untransformed) meshFilter
                    positions[k] = matrix.MultiplyPoint3x4(positions[k]);
                    // perform the interpolation from the reference particle system to our mesh vertices based one
                    var reference = referenceParticles[setParticlesCount].position;

                    var lerp = Mathf.Pow(Interpolation, InterpolationPower);

                    // alter the interpolation if NthSkip is activated
                    if (NthSkipActive) {
                        if (setParticlesCount % NthSkip == 0) {
                            lerp = Mathf.Pow((NthSkipInterpolation * Interpolation), InterpolationPower);
                        }
                    }

                    var x = Mathf.Lerp(reference.x, positions[k].x, lerp);
                    var y = Mathf.Lerp(reference.y, positions[k].y, lerp);
                    var z = Mathf.Lerp(reference.z, positions[k].z, lerp);
                    // set the particle position
                    outputParticles[setParticlesCount].position = new Vector3(x, y, z);
                    setParticlesCount++;
                }

                // update particleIndex on the last vertex 
                // so that we don't set the same particles again
                if (i == meshStartIndex + meshVertices.Length - 1)
                {
                    meshStartIndex = meshStartIndex + meshVertices.Length;
                    break;
                }
            }
        }

        OutputParticleSystem.SetParticles(outputParticles, OutputParticleSystem.particleCount);
    }

    double HermiteInterpolate(double y0, double y1, double y2, double y3, double mu, double tension, double bias)
    {
        double m0, m1, mu2, mu3;
        double a0, a1, a2, a3;

        mu2 = mu * mu;
        mu3 = mu2 * mu;
        m0 = (y1 - y0) * (1 + bias) * (1 - tension) / 2;
        m0 += (y2 - y1) * (1 - bias) * (1 - tension) / 2;
        m1 = (y2 - y1) * (1 + bias) * (1 - tension) / 2;
        m1 += (y3 - y2) * (1 - bias) * (1 - tension) / 2;
        a0 = 2 * mu3 - 3 * mu2 + 1;
        a1 = mu3 - 2 * mu2 + mu;
        a2 = mu3 - mu2;
        a3 = -2 * mu3 + 3 * mu2;

        return (a0 * y1 + a1 * m0 + a2 * m1 + a3 * y2);
    }

    Vector3 HermiteInterpolate3D(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, double mu, double tension, double bias)
    {
        var x = (float)HermiteInterpolate(p0.x, p1.x, p2.x, p3.x, mu, tension, bias);
        var y = (float)HermiteInterpolate(p0.y, p1.y, p2.y, p3.y, mu, tension, bias);
        var z = (float)HermiteInterpolate(p0.z, p1.z, p2.z, p3.z, mu, tension, bias);
        return new Vector3(x, y, z);
    }

    Vector3 Lerp3D(Vector3 p1, Vector3 p2, float mu)
    {
        var x = Mathf.Lerp(p1.x, p2.x, mu);
        var y = Mathf.Lerp(p1.y, p2.y, mu);
        var z = Mathf.Lerp(p1.z, p2.z, mu);
        return new Vector3(x, y, z);
    }
}

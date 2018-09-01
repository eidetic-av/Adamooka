using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualParticleSystemController : MonoBehaviour
{
	public ProceduralMeshController BirthSystemController;
	public ParticleShapeController StrictSystemController;

	public List<MeshFilter> Meshes = new List<MeshFilter>();
	public int SelectedMeshIndex = 0;
	public bool ActivateSelectedMesh = false;
	MeshFilter ActiveMesh;

	public Vector3 MeshScale = Vector3.one;

    [Range(0, 1)]
    public float Interpolation = 0;

    void Start()
    {
    }

    void Update()
    {
    }
}

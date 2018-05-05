using Utility;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Eidetic.Unity.Utility;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshTools : MonoBehaviour
{

    public enum FilterType
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

    public class NoiseAndSmoothing
    {
        public bool OffsetTriangles = false;

        public bool ControlNoiseWithMouse = false;
        public bool ControlMaterialFilmWithMouse = false;
        public bool ControlMaterialSpecWithMouse = false;
        public bool ControlMaterialNoiseWithMouse = false;
        public bool ControlMaterialColorWithMouse = false;
        public bool ControlLightingIntensityWithMouse = false;

        public float NoiseIntensity = 0.1f;
        public float NewNoiseIntensity = 0.1f;
        public static float CurrentNoiseIntensity;
        public bool ContinuousUpdate = true;
        public float LastUpdateTime;
        public FilterType Type = FilterType.Laplacian;
        public int SmoothingTimes = 3;
        public float HCAlpha = 0.5f;
        public float HCBeta = 0.5f;

        public bool RandomisePositionOnMouse = false;
        public float NoiseChangeDamping;
    }

    public NoiseAndSmoothing Noise = new NoiseAndSmoothing();

    public bool EnableExplode;
    public bool EnableAirsticksControl;

    public Prototyping Prototyping;

    bool LimitingMesh = false;
    LimitMesh LimitMesh;

    public static int[] LastTriangles;

    public int ActiveMaterial = 0;

    Light Light;

    Renderer Renderer;

    Vector3 LastMousePosition = Vector3.zero;

    public bool AnimateOutline = true;

    UserMeshVisualizer UserMeshVisualizer;

    void Start()
    {
        Renderer = gameObject.GetComponent<Renderer>();
        Light = GameObject.Find("SceneLight").GetComponent<Light>();
        Prototyping = GameObject.Find("Prototyping").GetComponent<Prototyping>();
        UserMeshVisualizer = GameObject.Find("UserMesh").GetComponent<UserMeshVisualizer>();
        ApplySmoothing();
        if (gameObject.GetComponent<LimitMesh>() != null)
        {
            LimitMesh = gameObject.GetComponent<LimitMesh>();
            LimitingMesh = true;
        }


        AirSticks.Right.NoteOn += ExplodeA;
        AirSticks.Left.NoteOn += ExplodeB;

        MidiManager.OneFiveNine.Beep += BangOutline;

        gameObject.InstanceMaterial();
    }

    float Outline, NewOutline = 0.2f;
    float OutlineAnimationDamp = 5f;

    public bool AnimateWireframeAlpha = true;
    float WireframeAlpha, NewWireframeAlpha = 0.49f;
    float WireframeAlphaAnimationDamp = 8f;
    Color WireframeColor = new Color(1, 1, 1, 0.49f);

    void BangOutline()
    {
        Outline = 1f;
        NewOutline = 0f;
    }

    public void ExplodeA()
    {
        Explode(Prototyping.Vectors[0].x, Prototyping.Vectors[0].y);
    }

    public void ExplodeB()
    {
        Explode(Prototyping.Vectors[1].x, Prototyping.Vectors[1].y);
    }

    void Explode(float explosionIntensity, float dampRate = 10f)
    {
        if (EnableExplode)
        {
            Noise.NewNoiseIntensity = 0.01f;
            Noise.NoiseIntensity = explosionIntensity;
            Noise.NoiseChangeDamping = dampRate;
        }
    }

    void ApplySmoothing()
    {
        if (UserMeshVisualizer.DisableMeshUpdate)
            return;
        var mesh = filter.mesh;
        JobHandle normalNoiseJobHandler;

        if (mesh.vertexCount != 0)
        {
            filter.mesh = ApplyNormalNoise(mesh, out normalNoiseJobHandler);

            switch (Noise.Type)
            {
                case FilterType.Laplacian:
                    if (!LimitingMesh)
                        filter.mesh = MeshSmoothing.LaplacianFilter(filter.mesh, Noise.SmoothingTimes, normalNoiseJobHandler);
                    else
                    {
                        filter.mesh = MeshSmoothing.LaplacianFilter(filter.mesh, LimitMesh, Noise.SmoothingTimes, normalNoiseJobHandler);
                    }
                    break;
                case FilterType.HC:
                    if (!LimitingMesh)
                        filter.mesh = MeshSmoothing.HCFilter(filter.mesh, Noise.SmoothingTimes, Noise.HCAlpha, Noise.HCBeta);
                    else
                    {
                        filter.mesh = MeshSmoothing.HCFilter(filter.mesh, LimitMesh.LimitedTriangles, Noise.SmoothingTimes, Noise.HCAlpha, Noise.HCBeta);
                    }
                    break;
            }
        }

        Noise.LastUpdateTime = Time.time;
    }

    private void Update()
    {
        if (AnimateOutline)
        {
            if (Mathf.Abs(Outline - NewOutline) > 0)
            {
                Outline = Outline + (NewOutline - Outline) / OutlineAnimationDamp;
            }
            var color = Renderer.material.GetColor("_OutlineColor");
            color.a = Outline;
            Renderer.material.SetColor("_OutlineColor", color);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ExplodeA();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ExplodeB();
        }

        //if (Input.GetKeyDown(KeyCode.Alpha0))
        //{
        //    Renderer.enabled = !Renderer.enabled;
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    Noise.OffsetTriangles = !Noise.OffsetTriangles;
        //}
        MeshSmoothing.OffsetTriangles = Noise.OffsetTriangles;

        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    Noise.ControlNoiseWithMouse = !Noise.ControlNoiseWithMouse;
        //}
        //if (Noise.ControlNoiseWithMouse)
        //{
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    Noise.ControlLightingIntensityWithMouse = !Noise.ControlLightingIntensityWithMouse;
        //}
        if (Noise.ControlLightingIntensityWithMouse)
        {
            Light.intensity = Input.mousePosition.x.Map(0f, Screen.width, 0f, 2f);
        }
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    ActiveMaterial++;
        //    if (ActiveMaterial > 3) ActiveMaterial = 0;
        //    switch (ActiveMaterial)
        //    {
        //        case 0:
        //            Renderer.material = Resources.Load<UnityEngine.Material>("Pink");
        //            break;
        //        case 1:
        //            Renderer.material = Resources.Load<UnityEngine.Material>("Rainbow Wireframe");
        //            break;
        //        case 2:
        //            Renderer.material = Resources.Load<UnityEngine.Material>("Iridescence");
        //            break;
        //        case 3:
        //            Renderer.material = Resources.Load<UnityEngine.Material>("Blue");
        //            break;
        //    }
        //}
        //IridescenceUpdate();

        if (EnableAirsticksControl)
        {
            Noise.NoiseIntensity = AirSticks.Left.EulerAngles.x.Map(0f, 1f, -.3f, .5f);
            Noise.SmoothingTimes = Mathf.RoundToInt(AirSticks.Right.EulerAngles.x.Map(1f, 0f, 0f, 5f));

            WireframeAlpha = AirSticks.Right.EulerAngles.z.Map(0f, 1f, 0.7f, 1f);
            WireframeColor.a = WireframeAlpha;
            SetMaterialColor("_Color", WireframeColor);
        }

        NoiseAndSmoothing.CurrentNoiseIntensity = Noise.NoiseIntensity;

        // if (Input.GetMouseButtonDown(0))
        // {
        //     Noise.NewNoiseIntensity = Random.Range(-0.0005f, 0.0005f);
        // } else if (Input.GetMouseButtonDown(1)) {
        //     var mouse = Input.mousePosition.x.Map(0f, Screen.width, 0f, 1f);
        // }


        if (Mathf.Abs(Noise.NoiseIntensity - Noise.NewNoiseIntensity) > 0)
        {
            Noise.NoiseIntensity = Noise.NoiseIntensity + (Noise.NewNoiseIntensity - Noise.NoiseIntensity) / Noise.NoiseChangeDamping;
        }
        if (AnimateWireframeAlpha)
        {
            if (Mathf.Abs(WireframeAlpha - NewWireframeAlpha) > 0)
            {
                WireframeAlpha = WireframeAlpha + (NewWireframeAlpha - WireframeAlpha) / WireframeAlphaAnimationDamp;
                WireframeColor.a = WireframeAlpha;
                SetMaterialColor("_Color", WireframeColor);
            }
        }

        if (NoteOn)
        {
            NewWireframeAlpha = 0.49f;
            WireframeAlpha = 1f;
            NoteOn = false;
        }

        // if (Noise.RandomisePositionOnMouse)
        // {

        //     var transform = gameObject.transform;
        //     var newPosition = new Vector3(Random.Range(-0.0005f, 0.0005f), transform.localPosition.y, transform.localPosition.z);
        //     transform.localPosition = newPosition;

        // }

    }

    bool NoteOn = false;

    void DoNoteOn()
    {
        NoteOn = true;
    }

    void LateUpdate()
    {
        if (Noise.ContinuousUpdate)
        {
            ApplySmoothing();
        }
    }

    void IridescenceUpdate()
    {
        if (ActiveMaterial != 2) return;
        //if (Input.GetKeyDown(KeyCode.Alpha9))
        //{
        //    Noise.ControlMaterialFilmWithMouse = !Noise.ControlMaterialFilmWithMouse;
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha8))
        //{
        //    Noise.ControlMaterialSpecWithMouse = !Noise.ControlMaterialSpecWithMouse;
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha7))
        //{
        //    Noise.ControlMaterialNoiseWithMouse = !Noise.ControlMaterialNoiseWithMouse;
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha6))
        //{
        //    Noise.ControlMaterialColorWithMouse = !Noise.ControlMaterialColorWithMouse;
        //}

        if (Noise.ControlMaterialFilmWithMouse)
        {
            SetMaterialsFloat("_Thinfilm_Strength", Input.mousePosition.x.Map(0f, Screen.width, -0f, 50f));
            SetMaterialsFloat("_Thinfilm_Color_Freq", Input.mousePosition.y.Map(Screen.height, 0f, 1f, 1000f));
        }

        if (Noise.ControlMaterialSpecWithMouse)
        {
            SetMaterialsFloat("_SpecPower", Input.mousePosition.x.Map(0f, Screen.width, 0.0001f, 0.1f));
        }

        if (Noise.ControlMaterialNoiseWithMouse)
        {
            SetMaterialsFloat("_NoiseMultiplier", Input.mousePosition.x.Map(0f, Screen.width, 0.00001f, 20f));
            Debug.Log(Input.mousePosition.x.Map(0f, Screen.width, 0.00001f, 20f));
            SetMaterialsFloat("_NoiseOffset", Input.mousePosition.y.Map(Screen.height, 0f, 0f, 20f));
        }

        if (Noise.ControlMaterialColorWithMouse)
        {
            SetMaterialsFloat("_ColorNoiseMultiplier", Input.mousePosition.x.Map(0f, Screen.width, 1f, 20f));
            SetMaterialsFloat("_ColorOffset", Input.mousePosition.y.Map(Screen.height, 0f, 0f, 20f));
        }

    }

    void SetMaterialsFloat(string name, float f)
    {
        UnityEngine.Material[] mats = Renderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].SetFloat(name, f);
        }
    }

    void SetMaterialColor(string name, Color c)
    {
        UnityEngine.Material[] mats = Renderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].SetColor(name, c);
        }
    }

    Mesh ApplyNormalNoise(Mesh sourceMesh, out JobHandle jobHandler)
    {
        var mesh = sourceMesh;

        // get the current mesh data
        var vertices = mesh.vertices;
        var normals = mesh.normals;

        // by instantiating the native arrays we are manually allocating memory for the parrallel processing job
        var vertexArray = new NativeArray<Vector3>(vertices, Allocator.TempJob);
        var normalArray = new NativeArray<Vector3>(normals, Allocator.TempJob);

        // Instantiate the job
        var normalNoiseJob = new NormalNoiseJob
        {
            Vertices = vertexArray,
            Normals = normalArray
        };

        // schedule the job for asynchronous processing
        // split the job into batches; loops of 250
        int batchSize = 250;
        jobHandler = normalNoiseJob.Schedule(mesh.vertexCount, batchSize);
        // make sure the multi-threaded job completes before moving on in this method
        jobHandler.Complete();

        // now copy the resulting data from the job back into the mesh
        vertexArray.CopyTo(vertices);
        normalArray.CopyTo(normals);

        // and dispose of the memory we manually allocated before
        vertexArray.Dispose();
        normalArray.Dispose();

        Mesh outputMesh = sourceMesh;
        outputMesh.vertices = vertices;
        outputMesh.normals = normals;

        if (Noise.OffsetTriangles)
        {
            // if we are offsetting the triangles
            // remove any triangles that reference vertices that don't exist in this frame
            int lastVertex = outputMesh.vertices.Length;
            if (MeshSmoothing.PreviousTriangles == null)
                MeshSmoothing.PreviousTriangles = mesh.triangles.ToList();
            MeshSmoothing.PreviousTriangles.RemoveAll(v => (v >= lastVertex));
            // and make sure the count is divisable by three to make a full triangle
            while ((MeshSmoothing.PreviousTriangles.Count % 3) != 0)
                MeshSmoothing.PreviousTriangles.Add(lastVertex);
            outputMesh.triangles = MeshSmoothing.PreviousTriangles.ToArray();
            MeshSmoothing.LatestTriangles = outputMesh.triangles;
        }

        return outputMesh;
    }

    struct NormalNoiseJob : IJobParallelFor
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<Vector3> Normals;

        public void Execute(int i)
        {
            Vertices[i] = Vertices[i] + Normals[i] * RandomGenerator.NextFloat() * NoiseAndSmoothing.CurrentNoiseIntensity;
        }
    }

}


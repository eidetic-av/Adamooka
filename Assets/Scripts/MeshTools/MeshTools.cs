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

    public MeshTools Instance;

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
        public int SmoothingTimes = 6;
        public float HCAlpha = 0.5f;
        public float HCBeta = 0.5f;

        public bool RandomisePositionOnMouse = false;
        public float NoiseChangeDamping;
    }

    public NoiseAndSmoothing Noise = new NoiseAndSmoothing();

    public bool EnableExplode;
    public bool EnableDesmondAirsticksControl;
    public Vector2 DesmondInstensityMinMax = new Vector2(-0.3f, 0.5f);
    public Vector2Int DesmondSmoothingMinMax = new Vector2Int(0, 5);
    public bool DampNoiseIntensity = false;
    public float NoiseIntensityDamping = 1f;


    public BlockoutController WhiteBlockoutController;
    public bool FadeInWhiteBlockout = false;
    public bool FadeOutWhiteBlockout = false;
    public int WhiteBlockoutFadeInTime = 10000;
    public int WhiteBlockoutFadeOutTime = 250;

    public bool EnableGranRelatedTransition;

    public float GranRelatedTransitionBaseNoiseIntensity = 0f;
    public float GranRelatedTransitionExplodeIntensity = 0f;
    public float GranRelatedTransitionExplodeDamping = 1f;

    public float GranRelatedTransitionReversionNoiseIntensity = 0f;
    public float GranRelatedTransitionReversionDamping = 0f;

    public bool EnableMainGranRelatedScene = false;

    public bool EnableGranRelatedAirsticksControl;
    public bool EnableGranRelatedControlIntensity;
    public bool EnableGranRelatedControlSmoothing;
    private bool GranControl;
    public bool EnableGranRelatedExplode = false;

    public float GranRelatedExplodeReversionDamping = 5f;
    public Vector2 GranRelatedNoiseIntensity = new Vector2(-.3f, 5f);
    public Vector2 GranRelatedSmoothing = new Vector2(0f, 5f);

    public float GranRelatedLeftExplodeIntensity = 1f;
    public float GranRelatedLeftExplodeDamping = 3f;
    public float GranRelatedRightExplodeIntensity = 1f;
    public float GranRelatedRightExplodeDamping = 3f;

    public IridescenceController Iridescence;
    public bool ActivateGranRelatedIridescence = false;
    public float IridescenceFilmStrength = 50;
    public float IridescenceFilmFrequency = 1;
    public float IridescenceFilmAnimationRate = 2;
    public float IridescenceColourAnimationRate = 2;

    public bool ActivateGranRelatedColourCycle = false;
    public GameObject OutputQuadToTint;
    public float TintSaturation = 0.6f;
    public bool ControlSaturationWithAirsticks = false;
    public Vector2 AirsticksSaturation = new Vector2(0, 1);
    public bool OnlyControlSaturationWhileNoteOn = false;
    public Vector2 TintCycleDistance = new Vector2(20, 40);
    public bool ResetTint = false;

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
        Instance = this;
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

        
        //Desmond

        AirSticks.Right.NoteOn += ExplodeA;
        AirSticks.Left.NoteOn += ExplodeB;

        //Gran Related cuts

        AirSticks.Right.NoteOn += () => GranRelatedOn(AirSticks.Hand.Right);
        AirSticks.Left.NoteOn += () => GranRelatedOn(AirSticks.Hand.Left);

        AirSticks.Right.NoteOff += GranRelatedOff;
        AirSticks.Left.NoteOff += GranRelatedOff;

        Iridescence = gameObject.GetComponent<IridescenceController>();


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

    void GranRelatedExplode(AirSticks.Hand hand)
    {
        if (!EnableGranRelatedExplode) return;
        if (!EnableGranRelatedTransition)
        {
            if (hand == AirSticks.Hand.Left)
            {
                Noise.NewNoiseIntensity = 0.01f;
                Noise.NoiseIntensity = GranRelatedLeftExplodeIntensity;
                Noise.NoiseChangeDamping = GranRelatedLeftExplodeDamping;
            }
            else if (hand == AirSticks.Hand.Right)
            {
                Noise.NewNoiseIntensity = 0.01f;
                Noise.NoiseIntensity = GranRelatedRightExplodeIntensity;
                Noise.NoiseChangeDamping = GranRelatedRightExplodeDamping;
            }
            if (ActivateGranRelatedColourCycle)
            {
                var material = OutputQuadToTint.GetComponent<Renderer>().material;
                var currentColor = material.GetColor("_TintColor");
                float h, s, v;
                Color.RGBToHSV(currentColor, out h, out s, out v);
                s = TintSaturation;
                h = (h + Random.value.Map(0, 1, TintCycleDistance.x, TintCycleDistance.y)) % 1;
                material.SetColor("_TintColor", Color.HSVToRGB(h, s, 1));
            }
        } else
        {
            if (WhiteBlockoutController.Full || WhiteBlockoutController.FadingIn)
            {
                WhiteBlockoutController.FadeOut(WhiteBlockoutFadeOutTime);
            }
            UserMeshVisualizer.BlockKinectUpdate = false;
            UserMeshVisualizer.ControlRotationSliderByAirsticks = true;
            UserMeshVisualizer.StartRotateAnimation = true;
            Noise.NewNoiseIntensity = GranRelatedTransitionBaseNoiseIntensity;
            Noise.NoiseIntensity = GranRelatedTransitionExplodeIntensity;
            Noise.SmoothingTimes = 6;
            Noise.NoiseChangeDamping = GranRelatedTransitionExplodeDamping;
        }
    }

    void ResetOutputQuadTint()
    {
        var material = OutputQuadToTint.GetComponent<Renderer>().material;
        material.SetColor("_TintColor", new Color(1, 1, 1, 1));
    }

    void GranRelatedExplodeReversion()
    {
        if (!EnableGranRelatedTransition)
        {
            Noise.NewNoiseIntensity = 0.01f;
            Noise.NoiseChangeDamping = GranRelatedExplodeReversionDamping;
        } else
        {
            UserMeshVisualizer.BlockKinectUpdate = true;
            Noise.NewNoiseIntensity = GranRelatedTransitionReversionNoiseIntensity;
            Noise.NoiseChangeDamping = GranRelatedTransitionReversionDamping;
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
        if (EnableMainGranRelatedScene)
        {
            EnableGranRelatedTransition = false;
            EnableGranRelatedAirsticksControl = true;
            EnableGranRelatedControlIntensity = true;
            EnableGranRelatedControlSmoothing = true;
            UserMeshVisualizer.ControlRotationSliderByAirsticks = true;
            UserMeshVisualizer.StartRotateAnimation = true;
            UserMeshVisualizer.BlockKinectUpdate = false;
            UserMeshVisualizer.DisableMeshUpdate = false;
            EnableMainGranRelatedScene = false;
        }


        if (ActivateGranRelatedColourCycle)
        {
            if (ControlSaturationWithAirsticks)
            {
                var updateSaturation = true;
                if (OnlyControlSaturationWhileNoteOn && (!AirSticks.Left.NoteIsOn && !AirSticks.Right.NoteIsOn))
                {
                    updateSaturation = false;
                }
                if (updateSaturation)
                {
                    var material = OutputQuadToTint.GetComponent<Renderer>().material;
                    var currentColor = material.GetColor("_TintColor");
                    float h, s, v;
                    Color.RGBToHSV(currentColor, out h, out s, out v);
                    s = AirSticks.Right.Position.y.Map(-1, 1, AirsticksSaturation.x, AirsticksSaturation.y);
                    material.SetColor("_TintColor", Color.HSVToRGB(h, s, v));
                }
            }
        }

        if (ResetTint)
        {
            ResetOutputQuadTint();
            ResetTint = false;
        }

        if (FadeInWhiteBlockout)
        {
            WhiteBlockoutController.FadeIn(WhiteBlockoutFadeInTime);
            FadeInWhiteBlockout = false;
        } else if (FadeOutWhiteBlockout)
        {
            WhiteBlockoutController.FadeOut(WhiteBlockoutFadeOutTime);
            FadeOutWhiteBlockout = false;
        }

        if (ActivateGranRelatedIridescence)
        {
            Iridescence.Activate = true;
            Iridescence.FilmStrength = IridescenceFilmStrength;
            Iridescence.FilmFrequency = IridescenceFilmFrequency;
            Iridescence.AnimateColorOffset = true;
            Iridescence.ColorOffsetAnimationRate = IridescenceColourAnimationRate;
            Iridescence.AnimateNoiseOffset = true;
            Iridescence.NoiseOffsetAnimationRate = IridescenceFilmAnimationRate;
            ActivateGranRelatedIridescence = false;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ExplodeA();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ExplodeB();
        }

        if (EnableGranRelatedAirsticksControl || EnableGranRelatedTransition)
        {
            if (GranControl)
            {
                if (EnableGranRelatedControlIntensity || EnableGranRelatedTransition)
                {
                    Noise.NoiseIntensity = AirSticks.Left.EulerAngles.x.Map(0f, 1f, GranRelatedNoiseIntensity.x, GranRelatedNoiseIntensity.y);
                }

                if (EnableGranRelatedControlSmoothing)
                {
                    Noise.SmoothingTimes = Mathf.RoundToInt(AirSticks.Right.EulerAngles.x.Map(1f, 0f, GranRelatedSmoothing.x, GranRelatedSmoothing.y));
                }
            }
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

        if (EnableDesmondAirsticksControl)
        {
            Noise.NoiseIntensity = AirSticks.Left.EulerAngles.x.Map(0f, 1f, DesmondInstensityMinMax.x, DesmondInstensityMinMax.y);
            
            Noise.SmoothingTimes = Mathf.RoundToInt(AirSticks.Right.EulerAngles.x.Map(1f, 0f, DesmondSmoothingMinMax.x, DesmondSmoothingMinMax.y));
        
            WireframeAlpha = 1f;
            WireframeColor.a = WireframeAlpha;
            SetMaterialColor("_Color", WireframeColor);
        }

        NoiseAndSmoothing.CurrentNoiseIntensity = Noise.NoiseIntensity;

        if (Mathf.Abs(Noise.NoiseIntensity - Noise.NewNoiseIntensity) > 0)
        {
            Noise.NoiseIntensity = Noise.NoiseIntensity + (Noise.NewNoiseIntensity - Noise.NoiseIntensity) / Noise.NoiseChangeDamping;
        }


        

        // if (Input.GetMouseButtonDown(0))
        // {
        //     Noise.NewNoiseIntensity = Random.Range(-0.0005f, 0.0005f);
        // } else if (Input.GetMouseButtonDown(1)) {
        //     var mouse = Input.mousePosition.x.Map(0f, Screen.width, 0f, 1f);
        // }

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

    void GranRelatedOn(AirSticks.Hand hand)
    {
        GranRelatedExplode(hand);
        if (!EnableGranRelatedTransition) GranControl = true;
    }

    void GranRelatedOff()
    {
        GranRelatedExplodeReversion();
        if (!EnableGranRelatedTransition) GranControl = false;
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


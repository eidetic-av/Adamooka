using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LineInterrupter
{
    public class LineInterrupt : MonoBehaviour
    {
        static readonly int[] StandardQuadTriangles = new int[] {
        0, 1, 2,
        2, 1, 3
    };

        public Material LineMaterial;
        [Range(1, 1000)] public int _LineCountY;
        int LineCountY;
        [Range(2, 1000)] public int _LineCountZ;
        int LineCountZ;
        public Vector2 LineWidth = new Vector2(0.01f, 0.01f);
        public float _Height = 2f;
        float Height;
        public float _Width = 2f;
        float Width;
        public float _Depth = 2f;
        float Depth;
        public bool VisualiseStage = false;
        public bool VisualiseLeftRaycasts = true;
        public bool VisualiseRightRaycasts = true;
        public bool VisualiseLeftConnectors = true;
        public bool VisualiseRightConnectors = true;

        bool IsVisualisingStage;
        bool IsVisualisingLeftRaycasts;
        bool IsVisualisingRightRaycasts;
        bool IsVisualisingLeftConnectors;
        bool IsVisualisingRightConnectors;

        GameObject Floor;
        GameObject WallLeft;
        GameObject WallRight;

        List<Vector3> SpaceOffsets = new List<Vector3>();
        List<Vector3> SpaceOffsetTimes = new List<Vector3>();
        List<Vector3> SpaceOffsetSpeeds = new List<Vector3>();
        [Range(0.01f, 100)]
        public float YSpacingOffset, ZSpacingOffset;
        [Range(0.01f, 100)]
        public float MinSpaceOffsetSpeed, MaxSpaceOffsetSpeed;
        float YSpacing;
        float ZSpacing;

        public List<LineRenderer> LeftInterruptLineRenderers { set; get; } = new List<LineRenderer>();
        public List<LineRenderer> LeftConnectingQuadLineRenderers { set; get; } = new List<LineRenderer>();
        public List<LineRenderer> RightRaycastLineRenderers { set; get; } = new List<LineRenderer>();
        public List<LineRenderer> RightConnectingQuadLineRenderers { set; get; } = new List<LineRenderer>();
        public List<GameObject> LeftInterruptLineObjects { set; get; } = new List<GameObject>();
        public List<GameObject> LeftConnectingQuadObjects { set; get; } = new List<GameObject>();
        public List<GameObject> RightRaycastLineObjects { set; get; } = new List<GameObject>();
        public List<GameObject> RightConnectingQuadObjects { set; get; } = new List<GameObject>();

        public bool RandomizeLineColours = true;
        [Range(0.01f, 1)]
        public float LineOpacity = .5f;

        public bool HighlightBody = false;
        [Range(0, 1)]
        public float HighlightGain = 0;

        GameObject Users;
        EndPointAnimator EndPointAnimator;

        public Vector3 RotationRate = Vector3.zero;
        public Vector3 LinesRotationOffset = Vector3.zero;
        public Vector3 BodyRotationOffset = Vector3.zero;

        struct CubeVertices
        {
            public Vector3 TopLeftFront,
            TopLeftRear,
            TopRightFront,
            TopRightRear,
            BottomLeftFront,
            BottomLeftRear,
            BottomRightFront,
            BottomRightRear;
        }
        CubeVertices Stage = new CubeVertices();

        // Use this for initialization
        void Start()
        {
            Users = GameObject.Find("Users");
            EndPointAnimator = gameObject.GetComponent<EndPointAnimator>();
            LoadMaterial();
            CalculateStage();
            UpdateLineCounts();
        }

        void LoadMaterial()
        {
            //LineMaterial = (Material)Resources.Load("UCLAGameLab/WireFrame/Materials/Rainbow Wireframe");
        }

        void CreateLineRenderer(out GameObject lineObject, out LineRenderer lineRenderer)
        {
            lineObject = new GameObject("Line");
            lineObject.layer = 9;
            lineObject.hideFlags = HideFlags.HideInHierarchy;
            lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = LineMaterial;
            lineRenderer.startWidth = LineWidth.x;
            lineRenderer.endWidth = LineWidth.y;

            if (RandomizeLineColours)
            {
                lineRenderer.material.color = RandomizeColor();
            }
        }

        Color RandomizeColor()
        {
            Color color = new Color();
            color.r = Random.value;
            color.g = Random.value;
            color.b = Random.value;
            color.a = LineOpacity;
            return color;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                RotationRate.x += 0.05f;
                RotationRate.y += 0.05f;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                RotationRate.x -= 0.05f;
                RotationRate.y -= 0.05f;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                EndPointAnimator.Rate -= 0.02f;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                EndPointAnimator.Rate += 0.02f;
            }
            if (ChangedSize())
            {
                RemoveStage();
                CalculateStage();
                UpdateLineCounts();
            }
            if (VisualiseStage)
            {
                DrawStage();
                IsVisualisingStage = true;
            }
            else if (IsVisualisingStage != VisualiseStage)
            {
                // if we don't want to visualise the stage,
                // but we were before, remove the floor/wall objects
                RemoveStage();
                IsVisualisingStage = false;
            }
            // update editor changes
            if (IsVisualisingLeftRaycasts != VisualiseLeftRaycasts)
                IsVisualisingLeftRaycasts = VisualiseLeftRaycasts;
            if (IsVisualisingRightRaycasts != VisualiseRightRaycasts)
                IsVisualisingRightRaycasts = VisualiseRightRaycasts;
            if (IsVisualisingLeftConnectors != VisualiseLeftConnectors)
                IsVisualisingLeftConnectors = VisualiseLeftConnectors;
            if (IsVisualisingRightConnectors != VisualiseRightConnectors)
                IsVisualisingRightConnectors = VisualiseRightConnectors;

            UpdateSpaceOffsets();

            RotateLines(LeftInterruptLineObjects);
            RotateLines(LeftConnectingQuadObjects);
            RotateLines(RightRaycastLineObjects);
            RotateLines(RightConnectingQuadObjects);
            // RotateBody();

            RenderLines();
        }

        void RotateLines(List<GameObject> gameObjects)
        {
            foreach (GameObject lineObject in gameObjects)
            {
                // rotate around it's centre
                var startingPosition = lineObject.transform.position;
                lineObject.transform.position = Vector3.zero;

                Vector3 rotationAnchor = Vector3.zero + LinesRotationOffset;

                lineObject.transform.RotateAroundLocal(Vector3.up, RotationRate.x);

                // move back to actual position
                lineObject.transform.position = startingPosition;
            }
        }

        void RotateBody()
        {
                var startingPosition = Users.transform.position;
                Users.transform.position = Vector3.zero;

                Vector3 rotationAnchor = Vector3.zero + BodyRotationOffset;

                Users.transform.RotateAroundLocal(Vector3.up, RotationRate.y);

                // move back to actual position
                Users.transform.position = startingPosition;
        }

        void RenderLines()
        {
            for (int z = 0; z < LineCountZ; z++)
            {
                for (int y = 0; y < LineCountY; y++)
                {
                    // Left Side first:

                    // Horizontal interrupted lines
                    bool interrupted;
                    Vector3 leftEnd = DrawLeftRaycastLine(y, z, out interrupted);
                    DrawLeftConnectionLines(y, z, interrupted);

                    // Right side
                    DrawRightRaycastLine(y, z, interrupted);
                    DrawRightConnectionLines(y, z, interrupted);
                }
            }
        }

        Vector3 DrawLeftRaycastLine(int y, int z, out bool interrupted)
        {
            int lineNumber = (z * (LineCountY)) + (y);

            bool newLine = LeftInterruptLineRenderers.Count <= lineNumber;

            Vector3 spaceOffset = SpaceOffsets[lineNumber];

            Vector3 startPoint = transform.position + new Vector3(
                -1 * (Width / 2),
                -(Height / 2) + (YSpacing * y) + (YSpacing / 2) + spaceOffset.y,
                -(Depth / 2) + (ZSpacing * z) + (ZSpacing / 2) + spaceOffset.z);
            Vector3 endPoint = transform.position + new Vector3(
                startPoint.x - (-1 * Width),
                -(Height / 2) + (YSpacing * y) + (YSpacing / 2) + spaceOffset.y,
                -(Depth / 2) + (ZSpacing * z) + (ZSpacing / 2) + spaceOffset.z);
            // if raycast finds a hit, cut the line short
            RaycastHit hitInfo;
            if (Physics.Raycast(startPoint, Vector3.right, out hitInfo, Width))
            {
                interrupted = true;
                endPoint.x = startPoint.x - (-1 * (hitInfo.distance));
            }
            else
            {
                interrupted = false;
            }
            // draw line
            LineRenderer lineRenderer;
            if (newLine)
            {
                // if it's new, create it
                GameObject lineObject;
                CreateLineRenderer(out lineObject, out lineRenderer);
                LeftInterruptLineObjects.Add(lineObject);
                LeftInterruptLineRenderers.Add(lineRenderer);
            }
            else
            {
                lineRenderer = LeftInterruptLineRenderers[lineNumber];
            }

            lineRenderer.SetPositions(new Vector3[] {
                startPoint, endPoint
            });


            bool render = interrupted;
            if (!IsVisualisingLeftRaycasts) render = false;
            lineRenderer.enabled = render;

            return endPoint;
        }

        void DrawLeftConnectionLines(int y, int z, bool interrupted)
        {
            // can't draw from 0 index because there is no points to draw
            // the quad BACK onto
            if (z == 0 || y == 0) return;

            // the index of the quad we are drawing:
            int quadNumber = (y - 1) + ((z - 1) * (LineCountY - 1));

            // line indices we will draw the quad with:
            int lineNumber = y + (LineCountY * z);

            int indexA = lineNumber;
            int indexB = lineNumber - 1;
            int indexC = lineNumber - LineCountY;
            int indexD = lineNumber - 1 - LineCountY;

            // grab the position vectors for each index based on the end point (1)
            // of the corresponding line
            Vector3 vertexA = LeftInterruptLineRenderers[indexA].GetPosition(1);
            Vector3 vertexB = LeftInterruptLineRenderers[indexB].GetPosition(1);
            Vector3 vertexC = LeftInterruptLineRenderers[indexC].GetPosition(1);
            Vector3 vertexD = LeftInterruptLineRenderers[indexD].GetPosition(1);

            // find out if the line is clipping its bounds
            if (Mathf.Abs(vertexA.x) == Width / 2 ||
                Mathf.Abs(vertexA.x) < 0 ||
                    Mathf.Abs(vertexB.x) == Width / 2 ||
                Mathf.Abs(vertexB.x) < 0 ||
                    Mathf.Abs(vertexC.x) == Width / 2 ||
                Mathf.Abs(vertexC.x) < 0 ||
                    Mathf.Abs(vertexD.x) == Width / 2 ||
                Mathf.Abs(vertexD.x) < 0)
            {
                interrupted = false;
            }

            GameObject lineObject;
            LineRenderer lineRenderer;

            if (LeftConnectingQuadObjects.Count <= quadNumber)
            {
                CreateLineRenderer(out lineObject, out lineRenderer);
                LeftConnectingQuadObjects.Add(lineObject);
                LeftConnectingQuadLineRenderers.Add(lineRenderer);

                // uncomment to show line in inspector
                // lineObject.hideFlags = HideFlags.None;
            }
            else
            {
                lineObject = LeftConnectingQuadObjects[quadNumber];
                lineRenderer = LeftConnectingQuadLineRenderers[quadNumber];
            }

            // unhide the line from the inspector
            // lineObject.hideFlags = HideFlags.None;

            // if it's not interrupted, don't render any lines
            bool render = interrupted;
            if (!IsVisualisingLeftConnectors) render = false;
            lineRenderer.enabled = render;

            lineRenderer.positionCount = 7;
            lineRenderer.SetPositions(new Vector3[]{
              vertexA,
              vertexB,
              vertexC,
              vertexD,
              vertexB,
              vertexC,
              vertexA
            });

            if (HighlightBody)
            {
                Color lineColor = lineRenderer.material.color;
                lineColor.a = LineOpacity + HighlightGain;
                lineRenderer.material.color = lineColor;
            }
        }

        void DrawRightRaycastLine(int y, int z, bool interrupted)
        {
            int lineNumber = y + (LineCountY * z);
            bool newLine = RightRaycastLineRenderers.Count <= lineNumber;

            // get the line, and if it doesn't exist initialise it
            LineRenderer lineRenderer;
            if (newLine)
            {
                // if it's new, create it
                GameObject lineObject;
                CreateLineRenderer(out lineObject, out lineRenderer);
                RightRaycastLineObjects.Add(lineObject);
                RightRaycastLineRenderers.Add(lineRenderer);
            }
            else
            {
                lineRenderer = RightRaycastLineRenderers[lineNumber];
            }

            if (!interrupted)
            {
                // it the left side hasn't been interrupted,
                // don't bother drawing this one
                lineRenderer.enabled = false;
                return;
            }
            else
            {
                // if it has, determine the interrupt point on this side

                // grab the space offset
                Vector3 spaceOffset = SpaceOffsets[lineNumber];

                Vector3 startPoint = transform.position + new Vector3(
                    (Width / 2),
                    -(Height / 2) + (YSpacing * y) + (YSpacing / 2) + spaceOffset.y,
                    -(Depth / 2) + (ZSpacing * z) + (ZSpacing / 2) + spaceOffset.z);
                Vector3 endPoint = transform.position + new Vector3(
                    startPoint.x - Width,
                    -(Height / 2) + (YSpacing * y) + (YSpacing / 2) + spaceOffset.y,
                    -(Depth / 2) + (ZSpacing * z) + (ZSpacing / 2) + spaceOffset.z);
                RaycastHit hitInfo;
                if (Physics.Raycast(startPoint, Vector3.left, out hitInfo, Width))
                {
                    endPoint.x = startPoint.x - hitInfo.distance;
                }

                lineRenderer.enabled = true;

                lineRenderer.SetPositions(new Vector3[] {
                startPoint, endPoint
            });
            }

            bool render = interrupted;
            if (!IsVisualisingRightRaycasts) render = false;
            lineRenderer.enabled = render;

        }

        void DrawRightConnectionLines(int y, int z, bool interrupted)
        {
            // can't draw from 0 index because there is no points to draw
            // the quad BACK onto
            if (z == 0 || y == 0) return;

            // the index of the quad we are drawing:
            int quadNumber = (y - 1) + ((z - 1) * (LineCountY - 1));

            // line indices we will draw the quad with:
            int lineNumber = y + (LineCountY * z);

            int indexA = lineNumber;
            int indexB = lineNumber - 1;
            int indexC = lineNumber - LineCountY;
            int indexD = lineNumber - 1 - LineCountY;

            // grab the position vectors for each index based on the end point (1)
            // of the corresponding line
            Vector3 vertexA = RightRaycastLineRenderers[indexA].GetPosition(1);
            Vector3 vertexB = RightRaycastLineRenderers[indexB].GetPosition(1);
            Vector3 vertexC = RightRaycastLineRenderers[indexC].GetPosition(1);
            Vector3 vertexD = RightRaycastLineRenderers[indexD].GetPosition(1);

            // find out if the line is clipping its bounds
            if (Mathf.Abs(vertexA.x) == 0 ||
                Mathf.Abs(vertexA.x) < -Width / 2 ||
                    Mathf.Abs(vertexB.x) == 0 ||
                Mathf.Abs(vertexB.x) < -Width / 2 ||
                    Mathf.Abs(vertexC.x) == 0 ||
                Mathf.Abs(vertexC.x) < -Width / 2 ||
                    Mathf.Abs(vertexD.x) == 0 ||
                Mathf.Abs(vertexD.x) < -Width / 2)
            {
                interrupted = false;
            }

            GameObject lineObject;
            LineRenderer lineRenderer;

            if (RightConnectingQuadObjects.Count <= quadNumber)
            {
                CreateLineRenderer(out lineObject, out lineRenderer);
                RightConnectingQuadObjects.Add(lineObject);
                RightConnectingQuadLineRenderers.Add(lineRenderer);

                // uncomment to show line in inspector
                // lineObject.hideFlags = HideFlags.None;
            }
            else
            {
                lineObject = RightConnectingQuadObjects[quadNumber];
                lineRenderer = RightConnectingQuadLineRenderers[quadNumber];
            }

            // unhide the line from the inspector
            // lineObject.hideFlags = HideFlags.None;

            // check for non initialised previous vertices
            if (vertexA.x == 0 ||
                    vertexB.x == 0 ||
                    vertexC.x == 0 ||
                    vertexD.x == 0)
            {
                // if this is true, it means at least one vertex 
                // spans the width of the stage, which mean's it's not
                // fully interrupted, so don't draw
                interrupted = false;
            }

            // if it's not interrupted, don't render any lines
            if (!interrupted)
            {
                lineRenderer.enabled = false;
            }
            // if it is interrupted, draw away
            else
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 7;
                lineRenderer.SetPositions(new Vector3[]{
              vertexA,
              vertexB,
              vertexC,
              vertexD,
              vertexB,
              vertexC,
              vertexA
            });
            }


            bool render = interrupted;
            if (!IsVisualisingRightConnectors) render = false;
            lineRenderer.enabled = render;
        }

        bool ChangedSize()
        {
            return (_Height != Height || _Width != Width || _Depth != Depth
            || _LineCountY != LineCountY || _LineCountZ != LineCountZ);
        }

        void CalculateStage()
        {
            Height = _Height;
            Width = _Width;
            Depth = _Depth;

            Vector3 pos = transform.position;

            Stage.TopLeftFront = new Vector3(pos.x - Width / 2, pos.y + Height / 2, pos.z - Depth / 2);
            Stage.TopLeftRear = new Vector3(pos.x - Width / 2, pos.y + Height / 2, pos.z + Depth / 2);
            Stage.TopRightFront = new Vector3(pos.x + Width / 2, pos.y + Height / 2, pos.z - Depth / 2);
            Stage.TopRightRear = new Vector3(pos.x + Width / 2, pos.y + Height / 2, pos.z + Depth / 2);
            Stage.BottomLeftFront = new Vector3(pos.x - Width / 2, pos.y - Height / 2, pos.z - Depth / 2);
            Stage.BottomLeftRear = new Vector3(pos.x - Width / 2, pos.y - Height / 2, pos.z + Depth / 2);
            Stage.BottomRightFront = new Vector3(pos.x + Width / 2, pos.y - Height / 2, pos.z - Depth / 2);
            Stage.BottomRightRear = new Vector3(pos.x + Width / 2, pos.y - Height / 2, pos.z + Depth / 2);
        }

        void CalculateSpacing()
        {
            InitialiseSpaceOffsets();
            YSpacing = Height / LineCountY;
            ZSpacing = Depth / LineCountZ;
        }

        void InitialiseSpaceOffsets()
        {
            SpaceOffsets.Clear();
            for (int i = 0; i < LineCountY * LineCountZ; i++)
            {
                SpaceOffsets.Add(new Vector3());
                SpaceOffsetTimes.Add(new Vector3());
                SpaceOffsetSpeeds.Add(new Vector3());
                RandomiseSpaceOffsetSpeed(i);
            }
        }

        void RandomiseSpaceOffsetSpeed(int offsetNumber)
        {
            SpaceOffsetSpeeds[offsetNumber] = new Vector3(
                0,
                Random.Range(MinSpaceOffsetSpeed, MaxSpaceOffsetSpeed),
                Random.Range(MinSpaceOffsetSpeed, MaxSpaceOffsetSpeed)
            );
        }

        void UpdateSpaceOffsets()
        {
            for (int i = 0; i < SpaceOffsets.Count; i++)
            {
                SpaceOffsetTimes[i] = new Vector3(
                    0,
                    SpaceOffsetTimes[i].y + (Time.deltaTime * SpaceOffsetSpeeds[i].y),
                    SpaceOffsetTimes[i].z + (Time.deltaTime * SpaceOffsetSpeeds[i].z)
                );

                SpaceOffsets[i] = new Vector3(
                    0,
                    Mathf.Sin(SpaceOffsetTimes[i].y) * YSpacingOffset,
                    Mathf.Sin(SpaceOffsetTimes[i].z) * ZSpacingOffset
                );
            }
        }

        void UpdateLineCounts()
        {
            // force even numbers so that we can draw triangles correctly
            if (_LineCountY % 2 != 0) _LineCountY += 1;
            if (_LineCountZ % 2 != 0) _LineCountZ += 1;

            LineCountY = _LineCountY;
            LineCountZ = _LineCountZ;

            // wipe lines for recalculation
            LeftInterruptLineObjects.ForEach(g => GameObject.Destroy(g));
            LeftInterruptLineObjects.Clear();

            RightRaycastLineObjects.ForEach(g => GameObject.Destroy(g));
            RightRaycastLineObjects.Clear();

            LeftConnectingQuadObjects.ForEach(g => GameObject.Destroy(g));
            LeftConnectingQuadObjects.Clear();

            RightConnectingQuadObjects.ForEach(g => GameObject.Destroy(g));
            RightConnectingQuadObjects.Clear();

            LeftInterruptLineRenderers.Clear();

            RightRaycastLineRenderers.Clear();

            LeftConnectingQuadLineRenderers.Clear();

            RightConnectingQuadLineRenderers.Clear();

            // recalculate spacing offsets
            CalculateSpacing();
        }

        void DrawStage()
        {
            // Draw the floor
            if (Floor == null)
            {
                Floor = new GameObject("Stage Floor");
                Floor.transform.parent = gameObject.transform;
                Floor.hideFlags = HideFlags.NotEditable;
                Floor.hideFlags = HideFlags.HideInHierarchy;
                MeshFilter meshFilter = Floor.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = Floor.AddComponent<MeshRenderer>();
                meshFilter.mesh.SetVertices(new List<Vector3>() {
                Stage.BottomLeftFront,
                Stage.BottomLeftRear,
                Stage.BottomRightFront,
                Stage.BottomRightRear
            });
                meshFilter.mesh.SetTriangles(StandardQuadTriangles, 0);
                meshRenderer.material = Resources.Load<Material>("Materials\\Stage");
            }
            if (WallLeft == null)
            {
                WallLeft = new GameObject("Stage Wall Left");
                WallLeft.transform.parent = gameObject.transform;
                WallLeft.hideFlags = HideFlags.NotEditable;
                WallLeft.hideFlags = HideFlags.HideInHierarchy;
                MeshFilter meshFilter = WallLeft.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = WallLeft.AddComponent<MeshRenderer>();
                meshFilter.mesh.SetVertices(new List<Vector3>() {
                Stage.BottomLeftFront,
                Stage.BottomLeftRear,
                Stage.TopLeftFront,
                Stage.TopLeftRear
            });
                meshFilter.mesh.SetTriangles(StandardQuadTriangles, 0);
                meshRenderer.material = Resources.Load<Material>("Materials\\Stage");
            }
            if (WallRight == null)
            {
                WallRight = new GameObject("Stage Wall Right");
                WallRight.transform.parent = gameObject.transform;
                WallRight.hideFlags = HideFlags.NotEditable;
                WallRight.hideFlags = HideFlags.HideInHierarchy;
                MeshFilter meshFilter = WallRight.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = WallRight.AddComponent<MeshRenderer>();
                meshFilter.mesh.SetVertices(new List<Vector3>() {
                Stage.BottomRightFront,
                Stage.BottomRightRear,
                Stage.TopRightFront,
                Stage.TopRightRear
            });
                meshFilter.mesh.SetTriangles(StandardQuadTriangles, 0);
                meshRenderer.material = Resources.Load<Material>("Materials\\Stage");
            }
        }

        void RemoveStage()
        {
            if (Floor != null)
            {
                GameObject.Destroy(Floor);
                Floor = null;
            }
            if (WallLeft != null)
            {
                GameObject.Destroy(WallLeft);
                WallLeft = null;
            }
            if (WallRight != null)
            {
                GameObject.Destroy(WallRight);
                WallRight = null;
            }
        }

    }
}
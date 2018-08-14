using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class UserMeshVisualizer : MonoBehaviour
{
    public static UserMeshVisualizer Instance;

    [Tooltip("Auto position the UserMesh object and a Camera (set below) based on the height of the camera.")]
    public bool AutoPosition = false;
    private bool PerformedAutoPosition = false;

    [Tooltip("Index of the player, tracked by this component. -1 means all players, 0 - the 1st player only, 1 - the 2nd player only, etc.")]
    public int playerIndex = -1;

    [Tooltip("Whether the mesh is facing the player or not.")]
    public bool mirroredMovement = true;

    [Tooltip("Kinect position in the world, used as origin for user movements.")]
    public Vector3 originPosition = Vector3.zero;

    [Tooltip("Whether the z-movement should be inverted or not.")]
    public bool invertedZMovement = false;

    [Tooltip("Smooth factor used for user movements.")]
    public float smoothFactor = 0f;

    [Tooltip("Camera used to overlay the mesh over the color background.")]
    public Camera foregroundCamera;

    [Tooltip("Whether to update the mesh collider as well, when the user mesh changes.")]
    public bool updateMeshCollider = false;

    [Tooltip("Number of pixels per direction in a sample.")]
    public int sampleSize = 2;

    [Tooltip("Time in seconds between mesh updates.")]
    public float updateMeshInterval = 0.1f;

    float lastMeshUpdateTime;

    public Mesh Mesh;

    public bool OnlyComputeTrackedUsers = false;

    public bool DisableMeshUpdate = false;
    public bool BlockKinectUpdate = false;

    int LimitPosition;
    public bool LimitToLeftHand = false;
    public bool LimitToRightHand = false;

    public Vector3 HandLimit = new Vector3(0.1f, 0.1f, 0.1f);

    public GameObject Parent;
    public Vector3 NewOffsetRotation = Vector3.zero;
    public Vector3 NewOffsetPosition = Vector3.zero;
    public float OffsetRotationBangAngle = -40f;
    public float OffsetRotationDampRate = 4f;
    public Vector3 OffsetRotationShift = new Vector3(0, 0, 0);
    public Vector3 VertexAverageSnapshot = new Vector3(-99, 0, 0);
    int OffsetRotationCount = 0;

    bool AnimateUserRotation = false;
    float AnimateRotationAngle = 0;

    public bool TriggerAnimatesRotation = false;

    public static Vector3[] vertices;
    public static Vector3 gameObjectPosition;
    private Vector2[] uvs;
    private int[] triangles;

    private KinectManager manager = null;

    private KinectInterop.SensorData sensorData = null;
    //private Vector3[] spaceCoords = null;
    private Matrix4x4 kinectToWorld = Matrix4x4.identity;


    private int depthWidth = 0;
    private int depthHeight = 0;

    private int sampledWidth = 0;
    private int sampledHeight = 0;

    public static long userId = 0;
    private byte userBodyIndex = 255;
    private Vector3 userMeshPos = Vector3.zero;

    private byte[] vertexType;
    private int[] vertexIndex;

    public bool DoOrigin = false;
    public bool DoSetMedianVertexPosition = false;

    private bool DoRotate = false;

    public bool StartRotateAnimation = false;
    public bool StopRotateAnimation = true;

    [Range(-1, 1)]
    public float NewRotationSlider = 0;
    private float RotationSlider = 0;
    [Range(1, 30)]
    public float RotationSliderDamping = 2;
    public bool ControlRotationSliderByAirsticks = false;
    public float MaxRotationAngle = 90f;

    void Start()
    {
        Instance = this;
        manager = KinectManager.Instance;
        Parent = GameObject.Find("Users");

        if (manager != null)
        {
            sensorData = manager.GetSensorData();

            depthWidth = manager.GetDepthImageWidth();
            depthHeight = manager.GetDepthImageHeight();

            sampledWidth = depthWidth / sampleSize;
            sampledHeight = depthHeight / sampleSize;

            //spaceCoords = new Vector3[depthWidth * depthHeight];

            if (sensorData.depth2SpaceCoords == null)
            {
                sensorData.depth2SpaceCoords = new Vector3[depthWidth * depthHeight];
            }

            vertexType = new byte[sampledWidth * sampledHeight];
            vertexIndex = new int[sampledWidth * sampledHeight];

            CreateMesh(sampledWidth, sampledHeight);
        }
    }

    void Update()
    {
        if (manager == null || !manager.IsInitialized())
            return;

        if (StartRotateAnimation)
        {
            StartAnimateUserRotation();
            StartRotateAnimation = false;
        }

        if (StopRotateAnimation)
        {
            EndAnimateUserRotation();
            StopRotateAnimation = false;
        }

        if (DoOrigin)
        {
            DoOrigin = false;
            NewOffsetPosition = Vector3.zero;
            NewOffsetRotation = Vector3.zero;
        }

        if (ControlRotationSliderByAirsticks)
        {
            var averageHandYRotation = (AirSticks.Right.EulerAngles.y + AirSticks.Left.EulerAngles.y) / 2;
            NewRotationSlider = averageHandYRotation;
        }
        if (Mathf.Abs(NewRotationSlider - RotationSlider) > 0)
        {
            RotationSlider = RotationSlider + (NewRotationSlider - RotationSlider) / RotationSliderDamping;
        }

        if (DoRotate)
        {
            if (VertexAverageSnapshot.x == -99)
                SetMedianVertexPosition();

            // reset to 0 every frame so we can just control the rotationAngle value
            Parent.transform.localEulerAngles = Vector3.zero;
            Parent.transform.localPosition = Vector3.zero;
            Parent.transform.eulerAngles = Vector3.zero;
            Parent.transform.position = Vector3.zero;

            var rotateAngle = RotationSlider * MaxRotationAngle;
            Parent.transform.RotateAround(VertexAverageSnapshot, Vector3.up, rotateAngle);

        } else
        {
            VertexAverageSnapshot = new Vector3(-99, 0, 0);
        }

        //if (Input.GetKeyDown(KeyCode.Alpha8))
        //{
        //    ResetOffsetRotation();
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha9))
        //{
        //    OffsetRotationBang(OffsetRotationBangAngle, true);
        //}

        if (AnimateUserRotation)
        {
            //Parent.transform.RotateAround(VertexAverageSnapshot, Vector3.up, RotationSlider);
        }
        else
        {
            Parent.transform.position = DampPosition(Parent.transform.position, NewOffsetPosition, OffsetRotationDampRate);
            Parent.transform.localEulerAngles = DampPosition(Parent.transform.localEulerAngles, NewOffsetRotation, OffsetRotationDampRate);
        }

        if (DoSetMedianVertexPosition)
        {
            DoSetMedianVertexPosition = false;
            SetMedianVertexPosition();
        }

        if (manager.GetUsersCount() != 0)
        {
            if (AutoPosition && !PerformedAutoPosition)
            {
                var sensorHeight = KinectManager.Instance.sensorHeight;
                var sensorAngle = KinectManager.Instance.sensorAngle;
                // centers the middle of the kinect projection to 0
                transform.position = new Vector3(transform.position.x, -(sensorHeight / 2), transform.position.z);
                PerformedAutoPosition = true;
            }
            else if (!AutoPosition)
            {
                PerformedAutoPosition = false;
            }
        }

        // get user texture
        //Renderer renderer = GetComponent<Renderer>();
        //if (renderer && renderer.material && renderer.material.mainTexture == null)
        //{
        //    BackgroundRemovalManager backManager = BackgroundRemovalManager.Instance;
        //    renderer.material.mainTexture = backManager ? (Texture)sensorData.depth2ColorTexture : (Texture)manager.GetUsersLblTex();
        //}

        // get kinect-to-world matrix
        kinectToWorld = manager.GetKinectToWorldMatrix();

        if (playerIndex >= 0 && OnlyComputeTrackedUsers == false)
        {

            long lastUserId = userId;
            userId = manager.GetUserIdByIndex(playerIndex);

            userBodyIndex = (byte)manager.GetBodyIndexByUserId(userId);
            if (userBodyIndex == 255)
                userBodyIndex = 222;

            // check for color overlay
            if (foregroundCamera)
            {
                // get the background rectangle (use the portrait background, if available)
                Rect backgroundRect = foregroundCamera.pixelRect;
                PortraitBackground portraitBack = PortraitBackground.Instance;

                if (portraitBack && portraitBack.enabled)
                {
                    backgroundRect = portraitBack.GetBackgroundRect();
                }

                // get user position
                userMeshPos = manager.GetJointPosColorOverlay(userId, (int)KinectInterop.JointType.SpineBase, foregroundCamera, backgroundRect);
            }
            else
            {
                // get user position
                userMeshPos = manager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.SpineBase);
            }

            if (!mirroredMovement)
            {
                //userMeshPos.x = -userMeshPos.x;
                userMeshPos.x = 0f;
            }

            if (foregroundCamera == null)
            {
                // convert kinect pos to world coords, when there is no color overlay
                userMeshPos = kinectToWorld.MultiplyPoint3x4(userMeshPos);
            }

            // set transform position
            Vector3 newUserPos = userMeshPos + originPosition; // manager.GetJointPosition(userId, (int)KinectInterop.JointType.SpineBase) + originPosition;

            if (invertedZMovement)
            {
                newUserPos.z = -newUserPos.z;
            }

            transform.position = lastUserId != 0 && smoothFactor != 0f ? Vector3.Lerp(transform.position, newUserPos, smoothFactor * Time.deltaTime) : newUserPos;
            gameObjectPosition = transform.position;

        }
        else
        {
            userId = 0;
            userBodyIndex = 255;
            userMeshPos = Vector3.zero;
        }

        // update the mesh
        if (!DisableMeshUpdate)
            UpdateMesh();

        PushMeshToNetworkClients();


        if (Input.GetKeyDown(KeyCode.L))
        {
            LimitPosition++;
            if (LimitPosition > 1) LimitPosition = 0;
            switch (LimitPosition)
            {
                case 0:
                    LimitToLeftHand = false;
                    LimitToRightHand = false;
                    break;
                case 1:
                    LimitToLeftHand = true;
                    LimitToRightHand = true;
                    break;
            }
        }



    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    private void CreateMesh(int width, int height)
    {
        Mesh = new Mesh();
        Mesh.name = "UserMesh";

        GetComponent<MeshFilter>().mesh = Mesh;
    }

    void OffsetRotationBang(float angle, bool pause)
    {
        // find the median point to rotate around if we don't already have it
        if (VertexAverageSnapshot.x == -99)
        {
            SetMedianVertexPosition();
        }

        OffsetRotationCount++;

        Parent.transform.localEulerAngles = Vector3.zero;
        Parent.transform.localPosition = Vector3.zero;
        Parent.transform.RotateAround(VertexAverageSnapshot, Vector3.up, angle * OffsetRotationCount);
        NewOffsetRotation = Parent.transform.localEulerAngles;
        NewOffsetPosition = Parent.transform.localPosition;
        // reset now that we have set the new positions to move towards
        Parent.transform.RotateAround(VertexAverageSnapshot, Vector3.up, -angle);

        // Disbale updating
        DisableMeshUpdate = pause;
    }

    public void SetMedianVertexPosition()
    {
        // first list all the points
        List<float> xPositions = new List<float>();
        List<float> yPositions = new List<float>();
        List<float> zPositions = new List<float>();
        for (int i = 0; i < vertices.Length; i++)
        {
            xPositions.Add(vertices[i].x);
            yPositions.Add(vertices[i].y);
            zPositions.Add(vertices[i].z);
        }
        // sort the lists into order 
        xPositions.Sort();
        yPositions.Sort();
        zPositions.Sort();

        // and grab the middle(ish) value of the array
        VertexAverageSnapshot = new Vector3(
            xPositions[Mathf.FloorToInt(xPositions.Count / 2f)] + transform.localPosition.x + OffsetRotationShift.x,
            yPositions[Mathf.FloorToInt(yPositions.Count / 2f)] + transform.localPosition.y + OffsetRotationShift.y,
            zPositions[Mathf.FloorToInt(zPositions.Count / 2f)] + transform.localPosition.z + OffsetRotationShift.z
        );
    }

    void ResetOffsetRotation()
    {
        NewOffsetRotation = Vector3.zero;
        NewOffsetPosition = Vector3.zero;
        VertexAverageSnapshot = new Vector3(-99, 0, 0);
        OffsetRotationCount = 0;
        DisableMeshUpdate = false;
    }

    public void StartAnimateUserRotation()
    {
        AnimateUserRotation = true;
        OffsetRotationDampRate = 1;
        SetMedianVertexPosition();
        DoRotate = true;
    }

    public void EndAnimateUserRotation()
    {
        if (AnimateUserRotation)
        {
            AnimateUserRotation = false;
            NewOffsetRotation = Vector3.zero;
            NewOffsetPosition = Vector3.zero;
            VertexAverageSnapshot = new Vector3(-99, 0, 0);
            AnimateRotationAngle = 0;
        }
        DoRotate = false;

    }

    private void PushMeshToNetworkClients()
    {
        if (Mesh != null)
        {
            int vertexCount = Mesh.vertexCount;
            // Debug.Log(vertexCount);
        }
    }

    private void UpdateMesh()
    {
        if (sensorData.depthImage != null && sensorData.bodyIndexImage != null &&
            sensorData.depth2SpaceCoords != null && sensorData.spaceCoordsBufferReady)
        {
            if ((Time.time - lastMeshUpdateTime) >= updateMeshInterval)
            {
                int vCount = 0, tCount = 0;
                EstimateUserVertices(out vCount, out tCount);

                vertices = new Vector3[vCount];
                uvs = new Vector2[vCount];
                triangles = new int[6 * tCount];

                int index = 0, vIndex = 0, tIndex = 0, xyIndex = 0;
                for (int y = 0; y < depthHeight; y += sampleSize)
                {
                    int xyStartIndex = xyIndex;

                    for (int x = 0; x < depthWidth; x += sampleSize)
                    {
                        //Vector3 vSpacePos = spaceCoords[xyIndex];
                        Vector3 vSpacePos = sensorData.depth2SpaceCoords[xyIndex];

                        if (vertexType[index] != 0 &&
                           !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
                        {
                            // check for color overlay
                            if (foregroundCamera)
                            {
                                // get the background rectangle (use the portrait background, if available)
                                Rect backgroundRect = foregroundCamera.pixelRect;
                                PortraitBackground portraitBack = PortraitBackground.Instance;

                                if (portraitBack && portraitBack.enabled)
                                {
                                    backgroundRect = portraitBack.GetBackgroundRect();
                                }

                                // TODO: The following returns null!?:

                                Vector2 vColorPos = sensorData.depth2ColorCoords[xyIndex];
                                ushort depthValue = sensorData.depthImage[xyIndex];

                                if (!float.IsInfinity(vColorPos.x) && !float.IsInfinity(vColorPos.y) && depthValue > 0)
                                {
                                    float xScaled = (float)vColorPos.x * backgroundRect.width / sensorData.colorImageWidth;
                                    float yScaled = (float)vColorPos.y * backgroundRect.height / sensorData.colorImageHeight;

                                    float xScreen = backgroundRect.x + xScaled;
                                    float yScreen = backgroundRect.y + backgroundRect.height - yScaled;
                                    float zDistance = (float)depthValue / 1000f;

                                    vSpacePos = foregroundCamera.ScreenToWorldPoint(new Vector3(xScreen, yScreen, zDistance));
                                }
                            }

                            if (!mirroredMovement)
                            {
                                vSpacePos.x = -vSpacePos.x;
                            }

                            if (foregroundCamera == null)
                            {
                                // convert space to world coords, when there is no color overlay
                                vSpacePos = kinectToWorld.MultiplyPoint3x4(vSpacePos);
                            }

                            if (LimitToLeftHand || LimitToRightHand)
                            {
                                var manager = KinectManager.Instance;
                                List<int> alteredIndices = new List<int>();
                                if (LimitToRightHand)
                                {
                                    Vector3 handPosition = manager.GetJointPosition(manager.GetUserIdByIndex(0), (int)KinectInterop.JointType.HandRight);
                                    if (Mathf.Abs(vSpacePos.x - handPosition.x) > HandLimit.x)
                                    {
                                        vertices[vIndex] = handPosition;
                                    }
                                    else if (Mathf.Abs(vSpacePos.y - handPosition.y) > HandLimit.y)
                                    {
                                        vertices[vIndex] = handPosition;
                                    }
                                    else if (Mathf.Abs(vSpacePos.z - handPosition.z) > HandLimit.z)
                                    {
                                        vertices[vIndex] = handPosition;
                                    }
                                    else
                                    {
                                        vertices[vIndex] = vSpacePos - userMeshPos;
                                        alteredIndices.Add(vIndex);
                                    }
                                }
                                if (LimitToLeftHand)
                                {
                                    Vector3 handPosition = manager.GetJointPosition(manager.GetUserIdByIndex(0), (int)KinectInterop.JointType.HandLeft);
                                    if (Mathf.Abs(vSpacePos.x - handPosition.x) > HandLimit.x)
                                    {
                                        vertices[vIndex] = handPosition;
                                    }
                                    else if (Mathf.Abs(vSpacePos.y - handPosition.y) > HandLimit.y)
                                    {
                                        vertices[vIndex] = handPosition;
                                    }
                                    else if (Mathf.Abs(vSpacePos.z - handPosition.z) > HandLimit.z)
                                    {
                                        vertices[vIndex] = handPosition;
                                    }
                                    else
                                    {
                                        if (!alteredIndices.Contains(vIndex))
                                            vertices[vIndex] = vSpacePos - userMeshPos;
                                    }
                                }

                            }
                            else
                            {
                                vertices[vIndex] = vSpacePos - userMeshPos;
                            }



                            uvs[vIndex] = new Vector2((float)x / depthWidth, (float)y / depthHeight);
                            vIndex++;

                            if (vertexType[index] == 3)
                            {
                                if (mirroredMovement)
                                {
                                    triangles[tIndex++] = vertexIndex[index];  // top left
                                    triangles[tIndex++] = vertexIndex[index + 1];  // top right
                                    triangles[tIndex++] = vertexIndex[index + sampledWidth];  // bottom left

                                    triangles[tIndex++] = vertexIndex[index + sampledWidth];  // bottom left
                                    triangles[tIndex++] = vertexIndex[index + 1];  // top right
                                    triangles[tIndex++] = vertexIndex[index + sampledWidth + 1];  // bottom right
                                }
                                else
                                {
                                    triangles[tIndex++] = vertexIndex[index + 1];  // top left
                                    triangles[tIndex++] = vertexIndex[index];  // top right
                                    triangles[tIndex++] = vertexIndex[index + sampledWidth + 1];  // bottom left

                                    triangles[tIndex++] = vertexIndex[index + sampledWidth + 1];  // bottom left
                                    triangles[tIndex++] = vertexIndex[index];  // top right
                                    triangles[tIndex++] = vertexIndex[index + sampledWidth];  // bottom right
                                }
                            }
                        }

                        index++;
                        xyIndex += sampleSize;
                    }

                    xyIndex = xyStartIndex + sampleSize * depthWidth;
                }

                // buffer is released
                lock (sensorData.spaceCoordsBufferLock)
                {
                    sensorData.spaceCoordsBufferReady = false;
                }

                if (!BlockKinectUpdate)
                {
                    Mesh.Clear();
                    Mesh.vertices = vertices;

                    Mesh.uv = uvs;
                    //mesh.normals = normals;
                    Mesh.triangles = triangles;
                    Mesh.RecalculateNormals();
                    Mesh.RecalculateBounds();
                }

                if (updateMeshCollider)
                {
                    MeshCollider meshCollider = GetComponent<MeshCollider>();

                    if (meshCollider)
                    {
                        meshCollider.sharedMesh = null;
                        meshCollider.sharedMesh = Mesh;
                    }
                }

                lastMeshUpdateTime = Time.time;
            }
        }
    }
    Vector3 DampPosition(Vector3 value, Vector3 goal, float dampRate)
    {
        if (Mathf.Abs(value.x - goal.x) > 0)
        {
            value.x = value.x + (goal.x - value.x) / dampRate;
        }
        if (Mathf.Abs(value.y - goal.y) > 0)
        {
            value.y = value.y + (goal.y - value.y) / dampRate;
        }
        if (Mathf.Abs(value.z - goal.z) > 0)
        {
            value.z = value.z + (goal.z - value.z) / dampRate;
        }
        return value;
    }

    //	// gets the average depth of the sample block
    //    private ushort GetSampleDepth(int x, int y)
    //    {
    //		int depthSum = 0, count = 0;
    //		int startIndex = y * depthWidth + x;
    //
    //		//for (int y1 = 0; y1 < SampleSize; y1++)
    //        {
    //			int pixelIndex = startIndex;
    //			
    //			//for (int x1 = 0; x1 < SampleSize; x1++)
    //            {
    //				//if(sensorData.bodyIndexImage[pixelIndex] != 255)
    //				{
    //					//if(userBodyIndex == 255 || sensorData.bodyIndexImage[pixelIndex] == userBodyIndex)
    //					{
    //						depthSum += sensorData.depthImage[pixelIndex];
    //						count++;
    //					}
    //				}
    //
    //				pixelIndex++;
    //            }
    //
    //			pixelIndex += depthWidth;
    //        }
    //
    //		return (ushort)(count > 0 ? (count > 1 ? depthSum / count : depthSum) : 0);
    //    }


    // estimates which and how many sampled vertices are valid
    private void EstimateUserVertices(out int count1, out int count3)
    {
        System.Array.Clear(vertexType, 0, vertexType.Length);

        Vector3[] vSpacePos = new Vector3[4];
        int rowIndex = 0;

        for (int y = 0; y < sampledHeight - 1; y++)
        {
            int pixIndex = rowIndex;

            for (int x = 0; x < sampledWidth - 1; x++)
            {
                if (IsUserSampleValid(x, y, ref vSpacePos[0]) && IsUserSampleValid(x + 1, y, ref vSpacePos[1]) &&
                   IsUserSampleValid(x, y + 1, ref vSpacePos[2]) && IsUserSampleValid(x + 1, y + 1, ref vSpacePos[3]))
                {
                    if (IsSpacePointsClose(vSpacePos, 0.01f))
                    {
                        vertexType[pixIndex] = 3;

                        vertexType[pixIndex + 1] = 1;
                        vertexType[pixIndex + sampledWidth] = 1;
                        vertexType[pixIndex + sampledWidth + 1] = 1;
                    }
                }

                pixIndex++;
            }

            rowIndex += sampledWidth;
        }

        // estimate counts
        count1 = 0;
        count3 = 0;

        for (int i = 0; i < vertexType.Length; i++)
        {
            if (vertexType[i] != 0)
            {
                vertexIndex[i] = count1;
                count1++;
            }
            else
            {
                vertexIndex[i] = 0;
            }

            if (vertexType[i] == 3)
            {
                count3++;
            }
        }
    }

    // checks if the space points are closer to each other than the minimum squared distance
    private bool IsSpacePointsClose(Vector3[] vSpacePos, float fMinDistSquared)
    {
        int iPosLength = vSpacePos.Length;

        for (int i = 0; i < iPosLength; i++)
        {
            for (int j = i + 1; j < iPosLength; j++)
            {
                Vector3 vDist = vSpacePos[j] - vSpacePos[i];
                if (vDist.sqrMagnitude > fMinDistSquared)
                    return false;
            }
        }

        return true;
    }

    // checks whether this sample block is valid for this user
    private bool IsUserSampleValid(int x, int y, ref Vector3 vSpacePos)
    {
        int startIndex = y * sampleSize * depthWidth + x * sampleSize;

        //for (int y1 = 0; y1 < SampleSize; y1++)
        {
            int pixelIndex = startIndex;
            //vSpacePos = spaceCoords[pixelIndex];
            vSpacePos = sensorData.depth2SpaceCoords[pixelIndex];

            //for (int x1 = 0; x1 < SampleSize; x1++)
            {
                if (userBodyIndex != 255)
                {
                    if (sensorData.bodyIndexImage[pixelIndex] == userBodyIndex &&
                       !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
                    {
                        return true;
                    }
                }
                else
                {
                    if (sensorData.bodyIndexImage[pixelIndex] != 255 &&
                       !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
                    {
                        return true;
                    }
                }

                pixelIndex++;
            }

            startIndex += depthWidth;
        }

        return false;
    }

}

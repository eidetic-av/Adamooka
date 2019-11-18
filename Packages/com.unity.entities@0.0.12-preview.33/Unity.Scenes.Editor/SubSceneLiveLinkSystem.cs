using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;
 
namespace Unity.Scenes.Editor
{
    [ExecuteAlways, UpdateInGroup(typeof(InitializationSystemGroup)), UpdateBefore(typeof(SubSceneStreamingSystem))]
    class SubSceneLiveLinkSystem : ComponentSystem
    {
        class GameObjectPrefabLiveLinkSceneTracker : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (var asset in importedAssets)
                {
                    if (asset.EndsWith(".prefab", true, System.Globalization.CultureInfo.InvariantCulture))
                        GlobalDirtyLiveLink();
                }
            }
        }
 
        static int GlobalDirtyID = 0;
        static int PreviousGlobalDirtyID = 0;
        MethodInfo m_GetDirtyIDMethod = typeof(Scene).GetProperty("dirtyID", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetMethod;
 
        UInt64 m_LiveLinkEditSceneViewMask = 1UL << 60;
        UInt64 m_LiveLinkEditGameViewMask = 1UL << 58;
        HashSet<SceneAsset> m_EditingSceneAssets = new HashSet<SceneAsset>();
 
        static void AddUnique(ref List<SubScene> list, SubScene scene)
        {
            if (list == null)
                list = new List<SubScene>(10);
            if (!list.Contains(scene))
                list.Add(scene);
        }
 
        protected override void OnUpdate()
        {
            List<SubScene> needLiveLinkSync = null;
            List<SubScene> cleanupScene = null;
            List<SubScene> markSceneLoadedFromLiveLink = null;
            List<SubScene> removeSceneLoadedFromLiveLink = null;
            m_EditingSceneAssets.Clear();
 
            var liveLinkEnabled = SubSceneInspectorUtility.LiveLinkEnabled;
 
            for (int i = 0; i != EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isSubScene)
                {
                    if (liveLinkEnabled)
                        EditorSceneManager.SetSceneCullingMask(scene, m_LiveLinkEditSceneViewMask);
                    else
                        EditorSceneManager.SetSceneCullingMask(scene, EditorSceneManager.DefaultSceneCullingMask | m_LiveLinkEditGameViewMask);
 
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                    if (scene.isLoaded && sceneAsset != null)
                        m_EditingSceneAssets.Add(sceneAsset);
                }
            }
 
            if (PreviousGlobalDirtyID != GlobalDirtyID)
            {
                Entities.ForEach((SubScene subScene) =>
                {
                    subScene.LiveLinkDirtyID = -1;
                });
                PreviousGlobalDirtyID = GlobalDirtyID;
            }
 
            Entities.ForEach((SubScene subScene) =>
            {
                var isLoaded = m_EditingSceneAssets.Contains(subScene.SceneAsset);
                if (isLoaded && liveLinkEnabled)
                {
                    if (subScene.LiveLinkDirtyID != GetSceneDirtyID(subScene.LoadedScene) || subScene.LiveLinkShadowWorld == null)
                        AddUnique(ref needLiveLinkSync, subScene);
                }
                else if (isLoaded && !liveLinkEnabled)
                {
                    var hasAnythingLoaded = false;
                    foreach (var s in subScene._SceneEntities)
                        hasAnythingLoaded |= EntityManager.HasComponent<SubSceneStreamingSystem.StreamingState>(s) || !EntityManager.HasComponent<SubSceneStreamingSystem.IgnoreTag>(s);
 
                    if (hasAnythingLoaded)
                    {
                        AddUnique(ref cleanupScene, subScene);
                        AddUnique(ref markSceneLoadedFromLiveLink, subScene);
                    }
                }
                else
                {
                    var isDrivenByLiveLink = false;
                    foreach (var s in subScene._SceneEntities)
                        isDrivenByLiveLink |= EntityManager.HasComponent<SubSceneStreamingSystem.IgnoreTag>(s);
 
                    if (isDrivenByLiveLink || subScene.LiveLinkShadowWorld != null)
                    {
                        AddUnique(ref cleanupScene, subScene);
                        AddUnique(ref removeSceneLoadedFromLiveLink, subScene);
                    }
                }
            });
 
            if (needLiveLinkSync != null)
            {
                foreach (var scene in needLiveLinkSync)
                {
                    if (!IsHotControlActive())
                        ApplyLiveLink(scene);
                    else
                        EditorUpdateUtility.EditModeQueuePlayerLoopUpdate();
                }
            }
 
 
            if (cleanupScene != null)
            {
                foreach (var scene in cleanupScene)
                {
                    CleanupScene(scene);
                }
            }
 
            if (markSceneLoadedFromLiveLink != null)
            {
                foreach (var scene in markSceneLoadedFromLiveLink)
                {
                    foreach (var sceneEntity in scene._SceneEntities)
                    {
                        if (!EntityManager.HasComponent<SubSceneStreamingSystem.IgnoreTag>(sceneEntity))
                            EntityManager.AddComponentData(sceneEntity, new SubSceneStreamingSystem.IgnoreTag());
                    }
                }
            }
 
            if (removeSceneLoadedFromLiveLink != null)
            {
                foreach (var scene in removeSceneLoadedFromLiveLink)
                {
                    foreach (var sceneEntity in scene._SceneEntities)
                    {
                        EntityManager.RemoveComponent<SubSceneStreamingSystem.IgnoreTag>(sceneEntity);
                    }
                }
            }
 
        }
 
        void CleanupScene(SubScene scene)
        {
            scene.CleanupLiveLink();
 
            var streamingSystem = World.GetExistingSystem<SubSceneStreamingSystem>();
 
            foreach (var sceneEntity in scene._SceneEntities)
            {
                streamingSystem.UnloadSceneImmediate(sceneEntity);
                EntityManager.DestroyEntity(sceneEntity);
            }
            scene._SceneEntities = new List<Entity>();
 
            scene.UpdateSceneEntities();
        }
 
        void ApplyLiveLink(SubScene scene)
        {
            var streamingSystem = World.GetExistingSystem<SubSceneStreamingSystem>();
 
            var isFirstTime = scene.LiveLinkShadowWorld == null;
            if (scene.LiveLinkShadowWorld == null)
                scene.LiveLinkShadowWorld = new World("LiveLink");
 
 
            using (var cleanConvertedEntityWorld = new World("Clean Entity Conversion World"))
            {
                if (isFirstTime)
                {
                    foreach (var s in scene._SceneEntities)
                    {
                        streamingSystem.UnloadSceneImmediate(s);
                        EntityManager.DestroyEntity(s);
                    }
 
                    var sceneEntity = EntityManager.CreateEntity();
                    EntityManager.SetName(sceneEntity, "Scene (LiveLink): " + scene.SceneName);
                    EntityManager.AddComponentObject(sceneEntity, scene);
                    EntityManager.AddComponentData(sceneEntity, new SubSceneStreamingSystem.StreamingState { Status = SubSceneStreamingSystem.StreamingStatus.Loaded});
                    EntityManager.AddComponentData(sceneEntity, new SubSceneStreamingSystem.IgnoreTag( ));
 
                    scene._SceneEntities = new List<Entity>();
                    scene._SceneEntities.Add(sceneEntity);
                }
 
                GameObjectConversionUtility.ConvertScene(scene.LoadedScene, scene.SceneGUID, cleanConvertedEntityWorld, GameObjectConversionUtility.ConversionFlags.AddEntityGUID | GameObjectConversionUtility.ConversionFlags.AssignName);
 
                var convertedEntityManager = cleanConvertedEntityWorld.EntityManager;
 
                var liveLinkSceneEntity = scene._SceneEntities[0];
 
                convertedEntityManager.AddSharedComponentData(convertedEntityManager.UniversalQuery, new SceneTag { SceneEntity = liveLinkSceneEntity });
 
                WorldDiffer.DiffAndApply(cleanConvertedEntityWorld, scene.LiveLinkShadowWorld, World);
 
                convertedEntityManager.Debug.CheckInternalConsistency();
                scene.LiveLinkShadowWorld.EntityManager.Debug.CheckInternalConsistency();
 
                var group = EntityManager.CreateEntityQuery(typeof(SceneTag), ComponentType.Exclude<EditorRenderData>());
                group.SetFilter(new SceneTag {SceneEntity = liveLinkSceneEntity});
 
                EntityManager.AddSharedComponentData(group, new EditorRenderData() { SceneCullingMask = m_LiveLinkEditGameViewMask, PickableObject = scene.gameObject });
 
                group.Dispose();
 
                scene.LiveLinkDirtyID = GetSceneDirtyID(scene.LoadedScene);
                EditorUpdateUtility.EditModeQueuePlayerLoopUpdate();
            }
        }
 
 
        static GameObject GetGameObjectFromAny(Object target)
        {
            Component component = target as Component;
            if (component != null)
                return component.gameObject;
            return target as GameObject;
        }
 
        internal static bool IsHotControlActive()
        {
            return GUIUtility.hotControl != 0;
        }
 
 
        UndoPropertyModification[] PostprocessModifications(UndoPropertyModification[] modifications)
        {
            foreach (var mod in modifications)
            {
                var target = GetGameObjectFromAny(mod.currentValue.target);
                if (target)
                {
                    var targetScene = target.scene;
                    Entities.ForEach((SubScene scene) =>
                    {
                        if (scene.IsLoaded && scene.LoadedScene == targetScene)
                        {
                            scene.LiveLinkDirtyID = -1;
                        }
                    });
                }
            }
 
            return modifications;
        }
 
        int GetSceneDirtyID(Scene scene)
        {
            if (scene.IsValid())
            {
                return (int)m_GetDirtyIDMethod.Invoke(scene, null);
            }
            else
                return -1;
        }
 
        static void GlobalDirtyLiveLink()
        {
            GlobalDirtyID++;
        }
 
        protected override void OnCreate()
        {
            Undo.postprocessModifications += PostprocessModifications;
            Undo.undoRedoPerformed += GlobalDirtyLiveLink;
 
            Camera.onPreCull += OnPreCull;
            RenderPipelineManager.beginCameraRendering += OnPreCull;
        }
 
        void OnPreCull(ScriptableRenderContext context, Camera camera) => OnPreCull(camera);
        void OnPreCull(Camera camera)
        {
            if (camera.cameraType == CameraType.Game)
            {
                camera.overrideSceneCullingMask = EditorSceneManager.DefaultSceneCullingMask | m_LiveLinkEditGameViewMask;
            }
            else if (camera.cameraType == CameraType.SceneView)
            {
                if (camera.scene.IsValid())
                {
                    camera.overrideSceneCullingMask = 0;
                }
                else
                {
                    camera.overrideSceneCullingMask = EditorSceneManager.DefaultSceneCullingMask | m_LiveLinkEditSceneViewMask;
                }
            }
 
        }
 
        protected override void OnDestroy()
        {
            Undo.postprocessModifications -= PostprocessModifications;
            Undo.undoRedoPerformed -= GlobalDirtyLiveLink;
 
            Camera.onPreCull -= OnPreCull;
            RenderPipelineManager.beginCameraRendering -= OnPreCull;
        }
    }
}
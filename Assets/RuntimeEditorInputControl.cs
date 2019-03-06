using RuntimeInspectorNamespace;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Eidetic.Unity.Utility;

namespace Eidetic.Unity.Runtime
{
    public class RuntimeEditorInputControl : MonoBehaviour
    {
        public List<GameObject> RuntimeControlObjects =>
            gameObject.GetComponentsInChildren<RuntimeController>().Select(rc => rc.gameObject).ToList();
        
        RectTransform RectTransform => RuntimeInspector.GameObject.GetComponent<RectTransform>();

        void Awake()
        {
            RuntimeInspector.Active = false;
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Set inspector focus to the index(+1) selected with alpha keys
                RuntimeControlObjects.SingleOrDefault(o =>
                        Input.GetKeyDown((RuntimeControlObjects.IndexOf(o) + 1).ToString()))?
                    .FocusInRuntimeInspector();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (RuntimeInspector.Active)
                    RuntimeInspector.Active = false;
            }
            else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                if (Input.GetKeyDown(KeyCode.F4))
                    Application.Quit();
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    RectTransform.position = new Vector3(Screen.width - 200, Screen.height - (Screen.height/2), 0);
                    RectTransform.anchorMin = new Vector2(1, 0);
                    RectTransform.anchorMax = new Vector2(1, 1);
                } else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    RectTransform.position = new Vector3(200, Screen.height - (Screen.height/2), 0);
                    RectTransform.anchorMin = new Vector2(1, 0);
                    RectTransform.anchorMax = new Vector2(1, 1);
                } else if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    RectTransform.position = new Vector3(Screen.width/2, Screen.height - (Screen.height/2), 0);
                    RectTransform.anchorMin = new Vector2(1, 0.2f);
                    RectTransform.anchorMax = new Vector2(1, 0.8f);
                }
            }
        }
    }
}
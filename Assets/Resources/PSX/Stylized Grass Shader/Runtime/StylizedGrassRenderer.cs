﻿//Stylized Grass Shader
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StylizedGrass
{
    [ExecuteInEditMode]
    [AddComponentMenu("Stylized Grass/Stylized Grass Renderer")]
    public class StylizedGrassRenderer : MonoBehaviour
    {
        public static StylizedGrassRenderer Instance;
        
        public bool debug = false;
       
        public RenderTexture vectorRT;

        [Tooltip("When a color map is assigned, this will be set as the active color map.\n\nHaving the Color Map Renderer component present would not longer be required.")]
        public GrassColorMap colorMap;
        [Tooltip("When enabled the grass Ambient and Gust strength values are multiplied by the WindZone's Main value")]
        public bool listenToWindZone;
        public WindZone windZone;
        
        public static int _BendMapUV = Shader.PropertyToID("_BendMapUV");
        private static int _GlobalWindParams = Shader.PropertyToID("_GlobalWindParams");
        private static int _GlobalWindDirection = Shader.PropertyToID("_GlobalWindDirection");

        public void OnEnable()
        {
            Instance = this;
            
#if UNITY_EDITOR
            UnityEditor.SceneView.duringSceneGui += OnSceneGUI;
#endif
            
            if (colorMap)
            {
                colorMap.SetActive();
            }
            else
            {
                if (!GrassColorMapRenderer.Instance) GrassColorMap.DisableGlobally();
            }
            
            #if UNITY_EDITOR && URP
            if (Application.isPlaying == false)
            {
                if (!PipelineUtilities.RenderFeatureAdded<GrassBendingFeature>())
                {
                    Debug.LogError("The \"Grass Bending Render Feature\" hasn't been added to the render pipeline. Check the inspector for setup instructions", this);
                    UnityEditor.EditorGUIUtility.PingObject(this);
                }
            }
            #endif
        }

        public void OnDisable()
        {
            Instance = null;

            //Shader needs to disable texture reading, since default global textures are gray
            Shader.SetGlobalVector(_BendMapUV, Vector4.zero);
            Shader.SetGlobalVector(_GlobalWindParams, Vector4.zero);

#if UNITY_EDITOR
            UnityEditor.SceneView.duringSceneGui -= OnSceneGUI;
#endif
        }
        
        public static void SetWindZone(WindZone windZone)
        {
            if (!Instance)
            {
                Debug.LogWarning("Tried to set Stylized Grass Renderer wind zone, but no instance is present");
                return;
            }

            Instance.windZone = windZone;
        }

        private void Update()
        {
            UpdateWind();
        }

        private double lastFrameTime;
        private double timeOffset;
        private Vector3 lastDirection;
        private Vector3 windDirection;

        private void UpdateWind()
        {
            if (listenToWindZone)
            {
                if (windZone)
                {
                    double deltaTime = Time.time - lastFrameTime;
                    lastFrameTime = Time.time;
                    
                    timeOffset += deltaTime * (double)windZone.windMain;

                    windDirection = windZone.transform.rotation * Vector3.forward;

                    windDirection = Vector3.Lerp(lastDirection, windDirection, (float)deltaTime).normalized;
                    lastDirection = windDirection;
                    
                    Shader.SetGlobalVector(_GlobalWindParams, new Vector4((float)timeOffset, windZone.windMain, windZone.windTurbulence, 1));
                    Shader.SetGlobalVector(_GlobalWindDirection, lastDirection);
                }
            }
            else
            {
                //When the .W component is 0, the shader uses material parameters to control wind
                Shader.SetGlobalVector(_GlobalWindParams, Vector4.zero);
            }
            
        }

        private void OnDrawGizmosSelected()
        {
            #if URP
            GrassBendingFeature.RenderBendVectors.DrawOrthographicViewGizmo();
            #endif
        }

        private void OnDrawGizmos()
        {
            if (listenToWindZone && windZone)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(windZone.transform.position, windZone.transform.position + (windDirection * 5f));
            }
        }


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI() //Has unwanted overhead, so exclude from build
        {
            DrawDebugGUI(false);
        }

        void DrawDebugGUI(bool sceneView)
        {
            vectorRT = (RenderTexture)Shader.GetGlobalTexture("_BendMap");

            if (!vectorRT) return;
            
            Rect imgRect = new Rect(5, 5, 256, 256);
            //Set to UI debug image
            if (debug && !sceneView)
            {
                GUI.DrawTexture(imgRect, vectorRT);
            }
            
            #if UNITY_EDITOR
            if (debug && sceneView)
            {
                Handles.BeginGUI();

                GUILayout.BeginArea(imgRect);

                EditorGUI.DrawTextureTransparent(imgRect, vectorRT);
                GUILayout.EndArea();
                Handles.EndGUI();
            }
            #endif
        }
#endif
        
#if UNITY_EDITOR
        private void OnSceneGUI(SceneView sceneView)
        {
            DrawDebugGUI(true);
        }
#endif
    }
}
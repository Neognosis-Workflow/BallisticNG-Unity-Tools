﻿using System.Collections.Generic;
using System.Linq;
using BallisticUnityTools.Placeholders;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Vlm
{
    /// <summary>
    /// Holds data for performing a bake.
    /// </summary>
    public class VlmBakeData
    {
        /// <summary>
        /// Whether to use a legacy bake.
        /// </summary>
        public static bool LegacyBake;

        /// <summary>
        /// Whether a light bounce pass should be performed.
        /// </summary>
        public static bool BounceLight;

        /// <summary>
        /// How far light bounces should go.
        /// </summary>
        public static float BounceDistance = 100.0f;

        /// <summary>
        /// How intense indirect lighting should be.
        /// </summary>
        public static float BounceIntensity = 0.3f;

        /// <summary>
        /// The maximum angle that light will bounce at.
        /// </summary>
        public static float BounceConeAngle = 120.0f;

        /// <summary>
        /// How far apart each bounce cone will be.
        /// </summary>
        public static float BounceConeMinimumDistance = 30.0f;

        /// <summary>
        /// How far away from a vertex to check for shadows using.
        /// </summary>
        public static float ShadowBias = 0.05f;

        /// <summary>
        /// Whether backfaces should be raycasts against.
        /// </summary>
        public static bool BackfaceShadows = true;

        /// <summary>
        /// The current bake data.
        /// </summary>
        public static VlmBakeData Current;

        public VlmBakeData()
        {
            Light[] allLights = Object.FindObjectsOfType<Light>();

            DirectionalLights = allLights.Where(l => l.type == LightType.Directional).ToList();
            Lights = allLights.Where(l => l.type != LightType.Directional).ToList();

            LightSponges = Object.FindObjectsOfType<AdvancedLightSponge>().ToList();

            SceneryMeshes = new List<VlmMeshObject>();
            TrmMeshes = new List<VlmMeshObject>();

            MeshFilter[] meshes = Object.FindObjectsOfType<MeshFilter>();

            foreach (MeshFilter mesh in meshes)
            {
                BallisticMeshCollider collider = mesh.GetComponent<BallisticMeshCollider>();
                bool isTrm = collider && (collider.Type == BallisticMeshCollider.CollisionType.TrackFloor || collider.Type == BallisticMeshCollider.CollisionType.TrackWall);

                bool isStatic = false;
#if UNITY_EDITOR
                isStatic = GameObjectUtility.AreStaticEditorFlagsSet(mesh.gameObject, StaticEditorFlags.ContributeGI);
#endif
                if (!isTrm && !isStatic) continue;

                VlmBakeOptionsComponent bakeOptions = mesh.GetComponent<VlmBakeOptionsComponent>();
                if (!bakeOptions || !bakeOptions.IgnoreLightmapper)
                {
                    if (isTrm) TrmMeshes.Add(new VlmMeshObject(mesh.gameObject, bakeOptions));
                    else SceneryMeshes.Add(new VlmMeshObject(mesh.gameObject, bakeOptions));
                }
            }
        }

        /// <summary>
        /// The scenery meshes for this bake.
        /// </summary>
        public List<VlmMeshObject> SceneryMeshes;

        /// <summary>
        /// The TRM meshes for this bake.
        /// </summary>
        public List<VlmMeshObject> TrmMeshes;

        /// <summary>
        /// The directional lights for this bake.
        /// </summary>
        public List<Light> DirectionalLights;

        /// <summary>
        /// The non directional lights for this bake.
        /// </summary>
        public List<Light> Lights;

        /// <summary>
        /// The light sponges for this bake.
        /// </summary>
        public List<AdvancedLightSponge> LightSponges;
    }
}
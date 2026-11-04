using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectNyx.Gameplay;
using ProjectNyx.Companion;
using NeoCyber.AR;

namespace ProjectNyx.Visuals
{
    /// <summary>
    /// [ES] Experiencia Visual Magistral: Cielo Ciberespacio 360° + Atmósfera VR (Unity 6 MR).
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] Master Visual Experience: 360° Cyberspace Skybox + VR Atmosphere (Unity 6 MR).
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    [ExecuteAlways]
    public class GalacticFontaineVisualExperience : MonoBehaviour
    {
        [Header("Materiales y Texturas Galácticas")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Material bulletLineMaterial;

        [Header("Referencias de Entorno")]
        [SerializeField] private ParticleSystem manaMotesEmitter;
        private GameObject reticleInstance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootGalacticExperience()
        {
            if (FindAnyObjectByType<GalacticFontaineVisualExperience>() == null)
            {
                GameObject host = new GameObject("GalacticFontaine_VisualMaster_Host");
                host.AddComponent<GalacticFontaineVisualExperience>();
                DontDestroyOnLoad(host);
            }
        }

        private void Awake()
        {
            SetupCelestialAtmosphere();
            SetupFontaineMaterials();
        }

        private void OnEnable()
        {
            SetupCelestialAtmosphere();
            SetupFontaineMaterials();
            EnsureCanalsAndSkyDome();
            if (Application.isPlaying)
            {
                SetupFloatingManaParticles();
                SetupGgoCombatReticle();
            }
        }

        private void Start()
        {
            EnsureCanalsAndSkyDome();
            if (Application.isPlaying)
            {
                SetupFloatingManaParticles();
                SetupGgoCombatReticle();
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                UpdateGgoBulletLines();
            }
        }

        public void SetupCelestialAtmosphere()
        {
            Texture2D skyTex = Resources.Load<Texture2D>("Textures/Environment/skybox_cyberspace_360_panorama");
            if (skyTex == null) skyTex = Resources.Load<Texture2D>("Textures/Environment/skybox_galactic_fontaine");

            Shader skyShader = Shader.Find("Skybox/Panoramic") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");

            if (skyTex != null && skyShader != null)
            {
                skyboxMaterial = new Material(skyShader);
                skyboxMaterial.name = "Mat_Cyberspace_Skybox_360";
                skyboxMaterial.mainTexture = skyTex;
                if (skyboxMaterial.HasProperty("_Exposure")) skyboxMaterial.SetFloat("_Exposure", 1.25f);
                if (skyboxMaterial.HasProperty("_Rotation")) skyboxMaterial.SetFloat("_Rotation", 185f);

                RenderSettings.skybox = skyboxMaterial;
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.28f, 0.48f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.18f, 0.34f, 0.52f);
            RenderSettings.ambientGroundColor = new Color(0.10f, 0.14f, 0.24f);
            RenderSettings.ambientIntensity = 1.50f;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.04f, 0.07f, 0.18f);
            RenderSettings.fogDensity = 0.0045f;

            Light dirLight = FindAnyObjectByType<Light>();
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.color = new Color(0.90f, 0.96f, 1.0f);
                dirLight.intensity = 1.45f;
                dirLight.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
            }
        }

        private void SetupFontaineMaterials()
        {
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            bulletLineMaterial = new Material(unlit);
            bulletLineMaterial.name = "Mat_GGO_BulletLine_Laser";
            bulletLineMaterial.color = new Color(1.0f, 0.05f, 0.35f, 0.9f);
        }

        private void EnsureCanalsAndSkyDome()
        {
            GameObject vrRoot = GameObject.Find("FullVR_MegacitySkyline_Root");
            if (vrRoot != null)
            {
                Transform oldCanals = vrRoot.transform.Find("Fontaine_Canals_Root");
                if (oldCanals != null)
                {
                    oldCanals.gameObject.SetActive(false);
                }
                BuildCelestialSkyDome(vrRoot);
            }
        }

        private void BuildCelestialSkyDome(GameObject vrRoot)
        {
            if (vrRoot.transform.Find("Celestial_CosmicDome_Instance") != null) return;

            Texture2D skyTex = skyboxMaterial != null ? (Texture2D)skyboxMaterial.mainTexture : null;
            if (skyTex == null) return;

            GameObject dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Celestial_CosmicDome_Instance";
            dome.transform.SetParent(vrRoot.transform, false);
            dome.transform.localPosition = Vector3.zero;
            dome.transform.localScale = new Vector3(-450f, 450f, 450f);

            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
            Material domeMat = new Material(unlit);
            domeMat.mainTexture = skyTex;
            if (domeMat.HasProperty("_BaseMap")) domeMat.SetTexture("_BaseMap", skyTex);
            dome.GetComponent<MeshRenderer>().sharedMaterial = domeMat;

            Collider col = dome.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }
        }

        private void SetupFloatingManaParticles()
        {
            if (manaMotesEmitter != null) return;

            GameObject pObj = new GameObject("Ambient_Fontaine_ManaMotes");
            pObj.transform.SetParent(transform, false);
            pObj.transform.localPosition = new Vector3(0f, 1.5f, 4f);

            manaMotesEmitter = pObj.AddComponent<ParticleSystem>();
            var main = manaMotesEmitter.main;
            main.maxParticles = 160;
            main.startLifetime = 6.0f;
            main.startSpeed = 0.25f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.08f);

            var emission = manaMotesEmitter.emission;
            emission.rateOverTime = 20f;

            var shape = manaMotesEmitter.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(24f, 8f, 32f);
        }

        private void UpdateGgoBulletLines()
        {
            bool isAiming = false;
            if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl))
            {
                isAiming = true;
            }
            if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) ||
                OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch) ||
                OVRInput.Get(OVRInput.Button.SecondaryHandTrigger, OVRInput.Controller.All))
            {
                isAiming = true;
            }

            if (reticleInstance != null && reticleInstance.activeSelf != isAiming)
            {
                reticleInstance.SetActive(isAiming);
            }

            NeoCyberSpiderDroneAI[] spiders = FindObjectsByType<NeoCyberSpiderDroneAI>(FindObjectsSortMode.None);
            QuantumCenturionBossAI boss = FindAnyObjectByType<QuantumCenturionBossAI>();

            if (!isAiming)
            {
                foreach (var spider in spiders)
                {
                    if (spider == null) continue;
                    LineRenderer lr = spider.GetComponent<LineRenderer>();
                    if (lr != null && lr.enabled) lr.enabled = false;
                }
                if (boss != null)
                {
                    LineRenderer bossLr = boss.GetComponent<LineRenderer>();
                    if (bossLr != null && bossLr.enabled) bossLr.enabled = false;
                }
                return;
            }

            Transform playerHead = Camera.main != null ? Camera.main.transform : null;
            if (playerHead == null) return;

            foreach (var spider in spiders)
            {
                if (spider == null || !spider.gameObject.activeInHierarchy || spider.IsDying) continue;
                LineRenderer lr = spider.GetComponent<LineRenderer>();
                if (lr == null)
                {
                    lr = spider.gameObject.AddComponent<LineRenderer>();
                    lr.startWidth = 0.025f;
                    lr.endWidth = 0.008f;
                    lr.material = bulletLineMaterial;
                    lr.positionCount = 2;
                }
                lr.enabled = true;
                lr.SetPosition(0, spider.transform.position + Vector3.up * 0.45f);
                lr.SetPosition(1, playerHead.position - Vector3.up * 0.35f);
            }

            if (boss != null && boss.gameObject.activeInHierarchy && !boss.IsDead)
            {
                LineRenderer bossLr = boss.GetComponent<LineRenderer>();
                if (bossLr == null)
                {
                    bossLr = boss.gameObject.AddComponent<LineRenderer>();
                    bossLr.startWidth = 0.055f;
                    bossLr.endWidth = 0.02f;
                    bossLr.material = bulletLineMaterial;
                    bossLr.positionCount = 2;
                }
                bossLr.enabled = true;
                bossLr.SetPosition(0, boss.transform.position + Vector3.up * 1.8f);
                bossLr.SetPosition(1, playerHead.position);
            }
        }

        private void SetupGgoCombatReticle()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Transform existing = cam.transform.Find("GGO_Anime_Reticle");
            if (existing != null)
            {
                reticleInstance = existing.gameObject;
                reticleInstance.SetActive(false);
                return;
            }

            reticleInstance = new GameObject("GGO_Anime_Reticle");
            reticleInstance.transform.SetParent(cam.transform, false);
            reticleInstance.transform.localPosition = new Vector3(0f, 0f, 1.2f);
            reticleInstance.transform.localRotation = Quaternion.identity;

            LineRenderer circleLr = reticleInstance.AddComponent<LineRenderer>();
            circleLr.useWorldSpace = false;
            circleLr.startWidth = 0.008f;
            circleLr.endWidth = 0.008f;
            circleLr.positionCount = 33;
            circleLr.loop = true;
            circleLr.material = bulletLineMaterial;

            float radius = 0.065f;
            for (int i = 0; i <= 32; i++)
            {
                float angle = i * (Mathf.PI * 2f / 32f);
                circleLr.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
            reticleInstance.SetActive(false);
        }
    }
}

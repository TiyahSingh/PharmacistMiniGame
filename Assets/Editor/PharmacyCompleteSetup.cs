#if UNITY_EDITOR
using PharmacySim.Audio;
using PharmacySim.Core;
using PharmacySim.Data;
using PharmacySim.Inventory;
using PharmacySim.Labeling;
using PharmacySim.Prescription;
using PharmacySim.Progression;
using PharmacySim.Pills;
using PharmacySim.Save;
using PharmacySim.Setup;
using PharmacySim.Tools;
using PharmacySim.Tray;
using PharmacySim.UI;
using PharmacySim.Verification;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace PharmacySim.Editor
{
    /// <summary>
    /// One-click full project setup: URP, data, prefabs, input, scene, UI, build settings.
    /// </summary>
    [InitializeOnLoad]
    public static class PharmacyCompleteSetup
    {
        private const string ScenePath = "Assets/Scenes/PharmacyLab.unity";
        private const string SetupFlag = "PharmacySim_FullSetup_v1";

        static PharmacyCompleteSetup()
        {
            EditorApplication.delayCall += TryAutoSetup;
        }

        private static void TryAutoSetup()
        {
            if (SessionState.GetBool(SetupFlag, false)) return;
            if (File.Exists(ScenePath)) return;
            Debug.Log("[Pharmacy Sim] Running automatic full project setup...");
            RunFullSetup();
        }

        [MenuItem("Pharmacy Sim/Setup Everything (Full Project)")]
        public static void RunFullSetup()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Pharmacy Sim", "Importing TMP...", 0.05f);
                ImportTMP();

                EditorUtility.DisplayProgressBar("Pharmacy Sim", "Configuring URP...", 0.15f);
                EnsureURP();

                EditorUtility.DisplayProgressBar("Pharmacy Sim", "Creating materials & input...", 0.25f);
                var materials = CreateMaterials();
                CreateInputActionsAsset();

                EditorUtility.DisplayProgressBar("Pharmacy Sim", "Creating pill prefab...", 0.35f);
                PharmacyAssetGenerator.CreatePillPrefabTemplate();

                EditorUtility.DisplayProgressBar("Pharmacy Sim", "Generating medications...", 0.45f);
                PharmacyAssetGenerator.GenerateSampleMedications();

                EditorUtility.DisplayProgressBar("Pharmacy Sim", "Building scene...", 0.65f);
                BuildPharmacyScene(materials);

                EditorUtility.DisplayProgressBar("Pharmacy Sim", "Build settings...", 0.9f);
                ConfigurePlayerAndBuildSettings();

                SessionState.SetBool(SetupFlag, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Pharmacy Sim",
                    "Setup complete.\n\nOpen Assets/Scenes/PharmacyLab.unity and press Play.\n\nControls:\n• Shake slider or drag tray / left stick\n• Click pill to verify imprint\n• Hold E + click to dispense\n• Use UI buttons for bottle, label, scan, submit",
                    "Open Scene");
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(ScenePath);
            }
            catch (System.Exception ex)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Pharmacy Sim Setup Failed", ex.Message, "OK");
            }
        }

        private static void ImportTMP()
        {
            if (TMP_Settings.defaultFontAsset != null) return;
            var importerType = System.Type.GetType("TMPro.TMP_PackageResourceImporter, Unity.TextMeshPro.Editor");
            if (importerType != null)
            {
                var method = importerType.GetMethod("ImportResources",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                method?.Invoke(null, new object[] { true, false, false });
            }
        }

        private static void EnsureURP()
        {
            var urpType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
            if (urpType == null)
            {
                Debug.LogWarning("[Pharmacy Sim] URP package not ready — using built-in pipeline.");
                return;
            }

            const string folder = "Assets/Settings";
            const string urpPath = folder + "/PharmacyURP.asset";
            const string rendererPath = folder + "/PharmacyForwardRenderer.asset";
            Directory.CreateDirectory(folder);

            var rendererType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalRendererData, Unity.RenderPipelines.Universal.Runtime");
            ScriptableObject rendererData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(rendererPath);
            if (rendererData == null && rendererType != null)
            {
                rendererData = ScriptableObject.CreateInstance(rendererType);
                AssetDatabase.CreateAsset(rendererData, rendererPath);
            }

            ScriptableObject urpAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(urpPath);
            if (urpAsset == null)
            {
                urpAsset = ScriptableObject.CreateInstance(urpType);
                AssetDatabase.CreateAsset(urpAsset, urpPath);
            }

            if (rendererData != null)
            {
                var so = new SerializedObject(urpAsset);
                var list = so.FindProperty("m_RendererDataList");
                list.ClearArray();
                list.InsertArrayElementAtIndex(0);
                list.GetArrayElementAtIndex(0).objectReferenceValue = rendererData;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            GraphicsSettings.defaultRenderPipeline = urpAsset as RenderPipelineAsset;
            QualitySettings.renderPipeline = urpAsset as RenderPipelineAsset;
        }

        private static PharmacyMaterials CreateMaterials()
        {
            const string dir = "Assets/Materials";
            Directory.CreateDirectory(dir);

            var tray = CreateColorMaterial(dir + "/Tray.mat", new Color(0.92f, 0.94f, 0.96f));
            var counter = CreateColorMaterial(dir + "/Counter.mat", new Color(0.85f, 0.87f, 0.9f));
            var bottle = CreateColorMaterial(dir + "/Bottle.mat", new Color(0.75f, 0.85f, 0.95f, 0.6f));
            var shelf = CreateColorMaterial(dir + "/Shelf.mat", new Color(0.78f, 0.8f, 0.82f));
            var pill = CreateColorMaterial(dir + "/Pill.mat", Color.white);

            var bottleShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (bottle.shader != bottleShader) bottle.shader = bottleShader;
            bottle.SetFloat("_Surface", 1);
            bottle.SetFloat("_Blend", 0);
            bottle.renderQueue = 3000;

            return new PharmacyMaterials(tray, counter, bottle, shelf, pill);
        }

        private static Material CreateColorMaterial(string path, Color c)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { color = c };
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void CreateInputActionsAsset()
        {
            const string resourcesDir = "Assets/Resources";
            Directory.CreateDirectory(resourcesDir);
            const string path = resourcesDir + "/PharmacyControls.inputactions";

            if (File.Exists(path)) return;

            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Workstation");
            var shake = map.AddAction("TrayShake", InputActionType.Value, expectedControlType: "Vector2");
            shake.AddBinding("<Gamepad>/leftStick");
            shake.AddBinding("<Mouse>/delta");
            asset.AddActionMap(map);

            var json = asset.ToJson();
            File.WriteAllText(path, json);
            AssetDatabase.ImportAsset(path);
        }

        private static void BuildPharmacyScene(PharmacyMaterials mats)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var db = AssetDatabase.LoadAssetAtPath<MedicationDatabase>("Assets/Data/MedicationDatabase.asset");

            // Lighting
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.95f;
            light.color = new Color(1f, 0.98f, 0.95f);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.75f, 0.8f, 0.85f);
            RenderSettings.ambientEquatorColor = new Color(0.65f, 0.68f, 0.7f);
            RenderSettings.ambientGroundColor = new Color(0.45f, 0.47f, 0.5f);

            // Camera
            var camGo = new GameObject("WorkstationCamera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.82f, 0.86f, 0.9f);
            camGo.transform.position = new Vector3(0.9f, 1.35f, 0.9f);
            camGo.transform.rotation = Quaternion.Euler(45f, -45f, 0f);
            camGo.AddComponent<AudioListener>();

            // Environment
            var counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counter.name = "Counter";
            counter.transform.position = new Vector3(0f, 0.78f, 0f);
            counter.transform.localScale = new Vector3(2.2f, 0.08f, 1.4f);
            counter.GetComponent<Renderer>().sharedMaterial = mats.Counter;

            var shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = "InventoryShelf";
            shelf.transform.position = new Vector3(-1.1f, 1.05f, -0.45f);
            shelf.transform.localScale = new Vector3(0.5f, 1.2f, 0.35f);
            shelf.GetComponent<Renderer>().sharedMaterial = mats.Shelf;
            var inventory = shelf.AddComponent<InventoryShelf>();
            PopulateInventory(inventory, db);

            // Tray
            var trayRoot = new GameObject("SortingTray");
            trayRoot.transform.position = new Vector3(0f, 0.92f, 0.1f);
            var tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tray.name = "TraySurface";
            tray.transform.SetParent(trayRoot.transform);
            tray.transform.localPosition = Vector3.zero;
            tray.transform.localScale = new Vector3(0.55f, 0.04f, 0.42f);
            tray.GetComponent<Renderer>().sharedMaterial = mats.Tray;
            var trayRb = tray.AddComponent<Rigidbody>();
            trayRb.isKinematic = true;
            var shaker = trayRoot.AddComponent<TrayShaker>();
            SetField(shaker, "trayTransform", tray.transform);
            SetField(shaker, "trayRigidbody", trayRb);

            var spawnArea = new GameObject("SpawnArea").transform;
            spawnArea.SetParent(trayRoot.transform);
            spawnArea.localPosition = new Vector3(0f, 0.05f, 0f);

            var poolGo = new GameObject("PillPool");
            var pool = poolGo.AddComponent<PillPool>();
            var sorter = trayRoot.AddComponent<SortingTrayController>();
            SetField(sorter, "pillPool", pool);
            SetField(sorter, "trayShaker", shaker);
            SetField(sorter, "spawnArea", spawnArea);

            // Sieve below tray
            var sieve = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sieve.name = "SieveCatch";
            sieve.transform.position = new Vector3(0f, 0.84f, 0.35f);
            sieve.transform.localScale = new Vector3(0.45f, 0.02f, 0.3f);
            sieve.GetComponent<Renderer>().sharedMaterial = mats.Tray;
            var sieveCol = sieve.GetComponent<BoxCollider>();
            sieveCol.isTrigger = true;
            var sieveFilter = sieve.AddComponent<SieveFilter>();
            SetField(sieveFilter, "filterType", SieveFilterType.CapsuleSlot);
            SetField(sieveFilter, "allowedShapes", new[] { PillShape.Capsule });
            SetField(sieveFilter, "pillLayer", LayerMask.GetMask("Pill"));

            var inputBridge = trayRoot.AddComponent<TrayInputBridge>();
            SetField(inputBridge, "trayShaker", shaker);
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Resources/PharmacyControls.inputactions");
            SetField(inputBridge, "inputAsset", inputAsset);

            // Bottle station
            var bottleGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottleGo.name = "PrescriptionBottle";
            bottleGo.transform.position = new Vector3(0.55f, 0.9f, -0.25f);
            bottleGo.transform.localScale = new Vector3(0.12f, 0.15f, 0.12f);
            bottleGo.GetComponent<Renderer>().sharedMaterial = mats.Bottle;
            var bottleAnchor = new GameObject("LabelAnchor").transform;
            bottleAnchor.SetParent(bottleGo.transform);
            bottleAnchor.localPosition = new Vector3(0f, 0.05f, 0.08f);
            var filler = bottleGo.AddComponent<BottleFiller>();
            var applicator = bottleGo.AddComponent<LabelApplicator>();
            SetField(applicator, "bottleTransform", bottleGo.transform);
            SetField(applicator, "labelAnchor", bottleAnchor);

            // Label printer
            var printerGo = new GameObject("LabelPrinter");
            printerGo.transform.position = new Vector3(0.75f, 0.92f, -0.1f);
            var labelSpawn = new GameObject("LabelSpawn").transform;
            labelSpawn.SetParent(printerGo.transform);
            labelSpawn.localPosition = Vector3.zero;
            var labelPrefab = CreateLabelPrefab();
            var printer = printerGo.AddComponent<LabelPrinter>();
            SetField(printer, "labelSpawnPoint", labelSpawn);
            SetField(printer, "labelPrefab", labelPrefab);

            // Tools
            var toolsGo = new GameObject("Tools");
            var inspection = toolsGo.AddComponent<PillInspectionTool>();
            SetField(inspection, "inspectionCamera", cam);
            var scanLightGo = new GameObject("ScanLight");
            scanLightGo.transform.SetParent(toolsGo.transform);
            var scanLight = scanLightGo.AddComponent<Light>();
            scanLight.type = LightType.Spot;
            scanLight.range = 2f;
            scanLight.intensity = 0f;
            scanLight.enabled = false;
            SetField(inspection, "scanLight", scanLight);
            SetField(inspection, "pillLayer", LayerMask.GetMask("Pill"));
            toolsGo.AddComponent<BarcodeScanner>();

            // Systems root
            var systems = new GameObject("PharmacySystems");
            var gameManager = systems.AddComponent<PharmacyGameManager>();
            var prescriptions = systems.AddComponent<PrescriptionManager>();
            var orders = systems.AddComponent<OrderGenerator>();
            var progression = systems.AddComponent<ProgressionManager>();
            var save = systems.AddComponent<SaveSystem>();
            var audio = systems.AddComponent<PharmacyAudioManager>();
            var ui = systems.AddComponent<PharmacyUIManager>();
            var workflow = systems.AddComponent<PharmacyWorkflowController>();
            var verification = systems.AddComponent<VerificationSystem>();
            var hud = systems.AddComponent<PharmacyGameplayHUD>();
            var interactor = systems.AddComponent<PillClickInteractor>();

            SetField(prescriptions, "database", db);
            SetField(prescriptions, "sortingTray", sorter);
            SetField(prescriptions, "verificationSystem", verification);
            SetField(verification, "database", db);
            SetField(orders, "database", db);
            SetField(orders, "prescriptionManager", prescriptions);
            SetField(gameManager, "prescriptionManager", prescriptions);
            SetField(gameManager, "orderGenerator", orders);
            SetField(gameManager, "progressionManager", progression);
            SetField(gameManager, "uiManager", ui);
            SetField(gameManager, "audioManager", audio);
            SetField(gameManager, "saveSystem", save);
            SetField(gameManager, "workstationCamera", cam);
            SetField(workflow, "gameManager", gameManager);
            SetField(workflow, "prescriptionManager", prescriptions);
            SetField(workflow, "sortingTray", sorter);
            SetField(workflow, "inspectionTool", inspection);
            SetField(workflow, "labelPrinter", printer);
            SetField(workflow, "labelApplicator", applicator);
            SetField(workflow, "bottleFiller", filler);
            SetField(workflow, "barcodeScanner", toolsGo.GetComponent<BarcodeScanner>());
            SetField(hud, "workflow", workflow);
            SetField(hud, "ui", ui);
            SetField(hud, "inspectionTool", inspection);
            SetField(interactor, "workflow", workflow);

            // UI
            var uiRefs = PharmacyUIFactory.BuildCanvas(ui, hud, db);
            WireUIManager(ui, uiRefs);

            // Pill layer on template
            var pillTemplate = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pills/PillTemplate.prefab");
            if (pillTemplate != null)
            {
                int pillLayer = LayerMask.NameToLayer("Pill");
                if (pillLayer >= 0)
                    pillTemplate.layer = pillLayer;
                EditorUtility.SetDirty(pillTemplate);
            }

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void WireUIManager(PharmacyUIManager ui, PharmacyUIRefs refs)
        {
            SetField(ui, "prescriptionQueuePanel", refs.QueuePanel);
            SetField(ui, "identificationPanel", refs.IdentificationPanel);
            SetField(ui, "verificationChecklistPanel", refs.ChecklistPanel);
            SetField(ui, "queueListRoot", refs.QueueListRoot);
            SetField(ui, "queueEntryPrefab", refs.QueueEntryPrefab);
            SetField(ui, "patientNameText", refs.PatientName);
            SetField(ui, "medicationText", refs.Medication);
            SetField(ui, "imprintRequiredText", refs.ImprintRequired);
            SetField(ui, "quantityText", refs.Quantity);
            SetField(ui, "instructionsText", refs.Instructions);
            SetField(ui, "urgencyText", refs.Urgency);
            SetField(ui, "imprintInput", refs.ImprintInput);
            SetField(ui, "identificationResultText", refs.IdentificationResult);
            SetField(ui, "imprintVerifiedToggle", refs.ImprintToggle);
            SetField(ui, "quantityVerifiedToggle", refs.QuantityToggle);
            SetField(ui, "labelAppliedToggle", refs.LabelToggle);
            SetField(ui, "barcodeVerifiedToggle", refs.BarcodeToggle);
            SetField(ui, "doubleVerificationToggle", refs.DoubleVerifyToggle);
            SetField(ui, "alertBannerText", refs.AlertBanner);
            SetField(ui, "trayShakeSlider", refs.TrayShakeSlider);
        }

        private static void PopulateInventory(InventoryShelf shelf, MedicationDatabase db)
        {
            if (shelf == null || db == null) return;
            var so = new SerializedObject(shelf);
            var stock = so.FindProperty("stock");
            stock.ClearArray();
            int i = 0;
            foreach (var med in db.All)
            {
                if (med == null) continue;
                stock.InsertArrayElementAtIndex(i);
                var el = stock.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("medication").objectReferenceValue = med;
                el.FindPropertyRelative("quantityOnHand").intValue = 500;
                var expiryProp = el.FindPropertyRelative("expiryIso");
                if (expiryProp != null)
                    expiryProp.stringValue = System.DateTime.Today.AddYears(1).ToString("o");
                i++;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigurePlayerAndBuildSettings()
        {
            EditorUserBuildSettings.activeBuildTarget = BuildTarget.StandaloneWindows64;
            try
            {
                var playerSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
                var prop = playerSettings.FindProperty("activeInputHandler");
                if (prop != null)
                {
                    prop.intValue = 2; // Both
                    playerSettings.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            catch { /* optional */ }

            var scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorBuildSettings.scenes = scenes;
        }

        private static GameObject CreateLabelPrefab()
        {
            const string path = "Assets/Prefabs/Label/PrescriptionLabel.prefab";
            Directory.CreateDirectory("Assets/Prefabs/Label");
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "PrescriptionLabel";
            go.transform.localScale = new Vector3(0.09f, 0.05f, 1f);
            Object.DestroyImmediate(go.GetComponent<Collider>());

            TextMeshPro CreateLine(string name, float y, int size)
            {
                var tgo = new GameObject(name);
                tgo.transform.SetParent(go.transform, false);
                tgo.transform.localPosition = new Vector3(0f, y, -0.01f);
                var tmp = tgo.AddComponent<TextMeshPro>();
                tmp.fontSize = size;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.rect = new Rect(0, 0, 200, 40);
                tmp.color = Color.black;
                return tmp;
            }

            var view = go.AddComponent<LabelView>();
            view.ConfigureRuntime(
                CreateLine("Patient", 0.02f, 2),
                CreateLine("Drug", 0.01f, 2),
                CreateLine("Dose", 0f, 2),
                CreateLine("Instruct", -0.01f, 1),
                CreateLine("Warn", -0.02f, 1),
                CreateLine("RxId", -0.03f, 1));

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void SetField(Object target, string fieldName, object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogWarning($"[Pharmacy Sim] Missing field {fieldName} on {target.name}");
                return;
            }

            switch (value)
            {
                case Object obj:
                    prop.objectReferenceValue = obj;
                    break;
                case int i:
                    prop.intValue = i;
                    break;
                case float f:
                    prop.floatValue = f;
                    break;
                case bool b:
                    prop.boolValue = b;
                    break;
                case System.Enum e:
                    prop.enumValueIndex = System.Convert.ToInt32(e);
                    break;
                case string s:
                    prop.stringValue = s;
                    break;
                case LayerMask mask:
                    prop.intValue = mask.value;
                    break;
                default:
                    if (value is System.Array arr)
                    {
                        prop.arraySize = arr.Length;
                        for (int i = 0; i < arr.Length; i++)
                        {
                            var el = prop.GetArrayElementAtIndex(i);
                            if (arr.GetValue(i) is PillShape shape)
                                el.enumValueIndex = (int)shape;
                        }
                    }
                    break;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private readonly struct PharmacyMaterials
        {
            public readonly Material Tray, Counter, Bottle, Shelf, Pill;
            public PharmacyMaterials(Material tray, Material counter, Material bottle, Material shelf, Material pill)
            {
                Tray = tray; Counter = counter; Bottle = bottle; Shelf = shelf; Pill = pill;
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using System.IO;
using PharmacySim.Core;
using PharmacySim.Data;
using PharmacySim.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PharmacySim.Editor
{
    internal sealed class PharmacyUIRefs
    {
        public GameObject QueuePanel, IdentificationPanel, ChecklistPanel, QueueEntryPrefab;
        public Transform QueueListRoot;
        public TextMeshProUGUI PatientName, Medication, ImprintRequired, Quantity, Instructions, Urgency;
        public TextMeshProUGUI IdentificationResult, AlertBanner;
        public TMP_InputField ImprintInput;
        public Toggle ImprintToggle, QuantityToggle, LabelToggle, BarcodeToggle, DoubleVerifyToggle;
        public Slider TrayShakeSlider;
    }

    internal static class PharmacyUIFactory
    {
        private static readonly Color PanelBg = new(0.12f, 0.16f, 0.2f, 0.92f);
        private static readonly Color Accent = new(0.2f, 0.55f, 0.58f, 1f);
        private static readonly Color Header = new(0.88f, 0.92f, 0.95f, 1f);
        private static readonly Color Warning = new(0.9f, 0.55f, 0.15f, 1f);

        public static PharmacyUIRefs BuildCanvas(PharmacyUIManager ui, PharmacyGameplayHUD hud, MedicationDatabase db)
        {
            var refs = new PharmacyUIRefs();

            var canvasGo = new GameObject("PharmacyUI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }

            // Alert banner
            refs.AlertBanner = CreateText(canvasGo.transform, "AlertBanner", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(-40, 60), "", 22, Warning, FontStyles.Bold);

            // Left — queue
            refs.QueuePanel = CreatePanel(canvasGo.transform, "PrescriptionQueue", new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(20, 0), new Vector2(360, 520));
            CreateHeader(refs.QueuePanel.transform, "Prescription Queue");
            refs.QueueListRoot = CreateScrollContent(refs.QueuePanel.transform, "QueueList");
            refs.QueueEntryPrefab = CreateQueueEntryPrefab();

            // Center — prescription detail
            refs.IdentificationPanel = CreatePanel(canvasGo.transform, "PrescriptionDetail", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(520, 400));
            CreateHeader(refs.IdentificationPanel.transform, "Active Prescription");
            refs.PatientName = CreateLabeledText(refs.IdentificationPanel.transform, "Patient", 0);
            refs.Medication = CreateLabeledText(refs.IdentificationPanel.transform, "Medication", 1);
            refs.ImprintRequired = CreateLabeledText(refs.IdentificationPanel.transform, "Imprint", 2);
            refs.Quantity = CreateLabeledText(refs.IdentificationPanel.transform, "Quantity", 3);
            refs.Instructions = CreateLabeledText(refs.IdentificationPanel.transform, "Instructions", 4);
            refs.Urgency = CreateLabeledText(refs.IdentificationPanel.transform, "Urgency", 5);

            refs.ImprintInput = CreateInputField(refs.IdentificationPanel.transform, "ImprintInput", new Vector2(-10, -200));
            refs.IdentificationResult = CreateText(refs.IdentificationPanel.transform, "IdResult",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(480, 50),
                "Enter imprint from tablet. Do not identify by color.", 16, Header, FontStyles.Italic);

            var confirmBtn = CreateButton(refs.IdentificationPanel.transform, "Verify Imprint", new Vector2(-120, 60),
                new Vector2(200, 40));
            UnityEventTools.AddPersistentListener(confirmBtn.onClick, hud.OnConfirmImprint);

            // Right — checklist
            refs.ChecklistPanel = CreatePanel(canvasGo.transform, "VerificationChecklist", new Vector2(1, 0.5f),
                new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(340, 520));
            CreateHeader(refs.ChecklistPanel.transform, "Verification Checklist");
            refs.ImprintToggle = CreateToggle(refs.ChecklistPanel.transform, "Imprint verified", -60);
            refs.QuantityToggle = CreateToggle(refs.ChecklistPanel.transform, "Quantity correct", -110);
            refs.LabelToggle = CreateToggle(refs.ChecklistPanel.transform, "Label applied", -160);
            refs.BarcodeToggle = CreateToggle(refs.ChecklistPanel.transform, "Barcode scanned", -210);
            refs.DoubleVerifyToggle = CreateToggle(refs.ChecklistPanel.transform, "Double verification", -260);

            var doubleBtn = CreateButton(refs.ChecklistPanel.transform, "Run Double Verify", new Vector2(0, -310),
                new Vector2(240, 36));
            UnityEventTools.AddPersistentListener(doubleBtn.onClick, hud.OnDoubleVerify);

            var submitBtn = CreateButton(refs.ChecklistPanel.transform, "Submit Order", new Vector2(0, -380),
                new Vector2(240, 44));
            UnityEventTools.AddPersistentListener(submitBtn.onClick, hud.OnSubmitOrder);

            // Bottom — workstation controls
            var controls = CreatePanel(canvasGo.transform, "WorkstationControls", new Vector2(0.5f, 0),
                new Vector2(0.5f, 0), new Vector2(0, 25), new Vector2(900, 120));
            refs.TrayShakeSlider = CreateSlider(controls.transform, "Tray Shake", new Vector2(-300, 0));
            CreateButton(controls.transform, "Magnifier", new Vector2(-80, 0), new Vector2(130, 40), hud.OnToggleMagnifier);
            CreateButton(controls.transform, "Seal Bottle", new Vector2(80, 0), new Vector2(130, 40), hud.OnSealBottle);
            CreateButton(controls.transform, "Apply Label", new Vector2(230, 0), new Vector2(130, 40), hud.OnApplyLabel);
            CreateButton(controls.transform, "Scan Barcode", new Vector2(380, 0), new Vector2(130, 40), hud.OnScanBarcode);

            // Reference DB panel
            var dbPanel = CreatePanel(canvasGo.transform, "MedicationDatabase", new Vector2(0.5f, 0),
                new Vector2(0.5f, 0), new Vector2(0, 170), new Vector2(700, 140));
            CreateHeader(dbPanel.transform, "Medication Reference (Imprint Search)");
            var search = CreateInputField(dbPanel.transform, "DbSearch", new Vector2(-100, -20));
            var dbResults = CreateText(dbPanel.transform, "DbResults", new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0.5f, 0), new Vector2(120, 10), new Vector2(400, 80), "", 14, Header);
            var refPanel = dbPanel.AddComponent<MedicationReferencePanel>();
            var so = new SerializedObject(refPanel);
            so.FindProperty("database").objectReferenceValue = db;
            so.FindProperty("searchField").objectReferenceValue = search;
            so.FindProperty("resultsText").objectReferenceValue = dbResults;
            so.ApplyModifiedPropertiesWithoutUndo();
            var searchBtn = CreateButton(dbPanel.transform, "Search", new Vector2(280, -20), new Vector2(100, 36));
            UnityEventTools.AddPersistentListener(searchBtn.onClick, refPanel.SearchByImprint);

            return refs;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(anchorMin.x, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = PanelBg;
            return go;
        }

        private static void CreateHeader(Transform parent, string text)
        {
            CreateText(parent, "Header", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1f),
                new Vector2(0, -8), new Vector2(-20, 40), text, 20, Accent, FontStyles.Bold);
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 pos, Vector2 size, string text, int fontSize, Color color, FontStyles style = FontStyles.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Left;
            return tmp;
        }

        private static TextMeshProUGUI CreateLabeledText(Transform parent, string label, int row)
        {
            float y = -50 - row * 36;
            CreateText(parent, label + "_Label", new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(20, y), new Vector2(120, 28), label + ":", 16, Accent, FontStyles.Bold);
            return CreateText(parent, label + "_Value", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(150, y), new Vector2(-30, 28), "—", 16, Header);
        }

        private static TMP_InputField CreateInputField(Transform parent, string name, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(280, 36);
            rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.24f, 0.28f, 1f);
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 5);
            textRt.offsetMax = new Vector2(-10, -5);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.fontSize = 18;
            text.color = Header;
            var input = go.AddComponent<TMP_InputField>();
            input.textComponent = text;
            return input;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 pos, Vector2 size,
            UnityEngine.Events.UnityAction onClick = null)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = Accent;
            var btn = go.AddComponent<Button>();
            var text = CreateText(go.transform, "Text", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, label, 16, Color.white, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.Center;
            if (onClick != null)
                UnityEventTools.AddPersistentListener(btn.onClick, onClick);
            return btn;
        }

        private static Toggle CreateToggle(Transform parent, string label, float y)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, y);
            rt.sizeDelta = new Vector2(-30, 32);
            var toggle = go.AddComponent<Toggle>();
            toggle.interactable = false;
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.sizeDelta = new Vector2(24, 24);
            bgRt.anchorMin = new Vector2(0, 0.5f);
            bgRt.anchorMax = new Vector2(0, 0.5f);
            bgRt.anchoredPosition = new Vector2(12, 0);
            bg.AddComponent<Image>().color = new Color(0.25f, 0.3f, 0.35f);
            var check = new GameObject("Check");
            check.transform.SetParent(bg.transform, false);
            var checkImg = check.AddComponent<Image>();
            checkImg.color = Accent;
            var checkRt = check.GetComponent<RectTransform>();
            checkRt.anchorMin = Vector2.zero;
            checkRt.anchorMax = Vector2.one;
            checkRt.offsetMin = new Vector2(4, 4);
            checkRt.offsetMax = new Vector2(-4, -4);
            toggle.graphic = checkImg;
            toggle.targetGraphic = bg.GetComponent<Image>();
            CreateText(go.transform, "Label", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f),
                new Vector2(40, 0), new Vector2(-10, 28), label, 16, Header);
            return toggle;
        }

        private static Slider CreateSlider(Transform parent, string label, Vector2 pos)
        {
            CreateText(parent, label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                pos + new Vector2(0, 30), new Vector2(200, 24), label, 14, Header);
            var go = new GameObject("TrayShakeSlider");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(220, 20);
            var slider = go.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bg.AddComponent<Image>().color = new Color(0.2f, 0.25f, 0.3f);
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRt = fillArea.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.sizeDelta = Vector2.zero;
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = Accent;
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            slider.fillRect = fillRt;
            slider.targetGraphic = fillImg;
            return slider;
        }

        private static Transform CreateScrollContent(Transform parent, string name)
        {
            var scrollGo = new GameObject(name);
            scrollGo.transform.SetParent(parent, false);
            var rt = scrollGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(10, 10);
            rt.offsetMax = new Vector2(-10, -50);
            var content = new GameObject("Content").transform;
            content.SetParent(scrollGo.transform, false);
            var contentRt = content.gameObject.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0, 400);
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            return content;
        }

        private static GameObject CreateQueueEntryPrefab()
        {
            const string path = "Assets/Prefabs/UI/QueueEntry.prefab";
            Directory.CreateDirectory("Assets/Prefabs/UI");
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject("QueueEntry");
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 36);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.22f, 0.26f, 1f);
            go.AddComponent<Button>();
            var text = CreateText(go.transform, "Text", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, "Prescription", 15, Header);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.margin = new Vector4(12, 0, 0, 0);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
#endif

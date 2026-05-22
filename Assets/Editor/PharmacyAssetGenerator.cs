#if UNITY_EDITOR
using PharmacySim.Data;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PharmacySim.Editor
{
    public static class PharmacyAssetGenerator
    {
        private const string DataRoot = "Assets/Data/Medications";
        private const string DbPath = "Assets/Data/MedicationDatabase.asset";

        [MenuItem("Pharmacy Sim/Generate Sample Medications")]
        public static void GenerateSampleMedications()
        {
            Directory.CreateDirectory(DataRoot);

            var meds = new List<PillData>
            {
                CreateMed("Amoxicillin_500mg_Capsule", "Amoxicillin", "500mg", "AMOX500",
                    PillShape.Capsule, 12f, 0.45f, new Color(0.95f, 0.95f, 0.9f), new Color(0.2f, 0.45f, 0.75f)),
                CreateMed("Lisinopril_10mg_Round", "Lisinopril", "10mg", "LISI10",
                    PillShape.RoundTablet, 8f, 0.25f, new Color(0.85f, 0.85f, 0.9f), Color.white),
                CreateMed("Metformin_500mg_Oval", "Metformin", "500mg", "MET500",
                    PillShape.OvalTablet, 14f, 0.55f, new Color(0.9f, 0.9f, 0.85f), Color.white),
                CreateMed("Atorvastatin_20mg_Round", "Atorvastatin", "20mg", "ATOR20",
                    PillShape.RoundTablet, 9f, 0.3f, new Color(0.95f, 0.9f, 0.95f), Color.white),
                CreateMed("Omeprazole_20mg_Capsule", "Omeprazole", "20mg", "OME20",
                    PillShape.Capsule, 11f, 0.4f, new Color(0.9f, 0.88f, 0.8f), new Color(0.6f, 0.5f, 0.35f)),
                CreateMed("Levothyroxine_50mcg_Round", "Levothyroxine", "50mcg", "LEV50",
                    PillShape.RoundTablet, 7f, 0.15f, new Color(0.95f, 0.97f, 0.85f), Color.white),
                CreateMed("Gabapentin_300mg_Capsule", "Gabapentin", "300mg", "GABA300",
                    PillShape.Capsule, 13f, 0.5f, new Color(0.9f, 0.9f, 0.92f), Color.white),
                CreateMed("Warfarin_5mg_Round", "Warfarin", "5mg", "WARF5",
                    PillShape.RoundTablet, 8f, 0.22f, new Color(0.95f, 0.92f, 0.85f), Color.white,
                    ControlledStatus.NonControlled, requiresDoubleVerification: true),
                CreateMed("Prednisone_10mg_Round", "Prednisone", "10mg", "PRED10",
                    PillShape.RoundTablet, 9f, 0.28f, new Color(0.92f, 0.88f, 0.8f), Color.white),
                CreateMed("Duloxetine_30mg_Capsule", "Duloxetine", "30mg", "DULO30",
                    PillShape.Capsule, 12f, 0.42f, new Color(0.9f, 0.9f, 0.95f), new Color(0.4f, 0.4f, 0.5f)),
                CreateMed("Aspirin_81mg_Round", "Aspirin", "81mg", "ASP81",
                    PillShape.RoundTablet, 6f, 0.12f, new Color(0.95f, 0.9f, 0.9f), Color.white),
                CreateMed("Nitroglycerin_Beads", "Nitroglycerin", "0.4mg", "NTG04",
                    PillShape.SmallBead, 2f, 0.02f, new Color(0.9f, 0.85f, 0.85f), Color.white),
                CreateMed("Finasteride_5mg_Round", "Finasteride", "5mg", "FIN5",
                    PillShape.RoundTablet, 8f, 0.2f, new Color(0.85f, 0.9f, 0.95f), Color.white),
                CreateMed("Pregabalin_75mg_Capsule", "Pregabalin", "75mg", "PREG75",
                    PillShape.Capsule, 11f, 0.38f, new Color(0.92f, 0.92f, 0.95f), Color.white),
                CreateMed("Metoprolol_50mg_Hex", "Metoprolol", "50mg", "METO50",
                    PillShape.HexagonalTablet, 10f, 0.35f, new Color(0.88f, 0.9f, 0.95f), Color.white),
                CreateMed("Ibuprofen_200mg_Coated", "Ibuprofen", "200mg", "IBU200",
                    PillShape.LargeCoatedTablet, 16f, 0.85f, new Color(0.95f, 0.95f, 0.95f), new Color(0.7f, 0.1f, 0.1f))
            };

            var db = AssetDatabase.LoadAssetAtPath<MedicationDatabase>(DbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<MedicationDatabase>();
                AssetDatabase.CreateAsset(db, DbPath);
            }

            var so = new SerializedObject(db);
            var list = so.FindProperty("medications");
            list.ClearArray();
            for (int i = 0; i < meds.Count; i++)
            {
                list.InsertArrayElementAtIndex(i);
                list.GetArrayElementAtIndex(i).objectReferenceValue = meds[i];
            }
            so.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Pharmacy Sim] Generated {meds.Count} medications and database at {DbPath}");
        }

        [MenuItem("Pharmacy Sim/Create Pill Prefab Template")]
        public static void CreatePillPrefabTemplate()
        {
            const string prefabDir = "Assets/Prefabs/Pills";
            Directory.CreateDirectory(prefabDir);

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "PillTemplate";
            go.transform.localScale = new Vector3(0.12f, 0.06f, 0.06f);

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            go.AddComponent<PillInstance>();
            go.AddComponent<PharmacySim.Pills.PillPhysicsController>();

            int pillLayer = LayerMask.NameToLayer("Pill");
            if (pillLayer >= 0)
                go.layer = pillLayer;

            var pillMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Pill.mat");
            if (pillMat != null)
                go.GetComponent<Renderer>().sharedMaterial = pillMat;

            var path = $"{prefabDir}/PillTemplate.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[Pharmacy Sim] Pill template prefab saved to {path}. Assign to PillData assets.");
        }

        private static PillData CreateMed(
            string fileName, string name, string dose, string imprint,
            PillShape shape, float sizeMm, float weightG,
            Color primary, Color secondary,
            ControlledStatus controlled = ControlledStatus.NonControlled,
            bool requiresDoubleVerification = false)
        {
            var path = $"{DataRoot}/{fileName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PillData>(path);
            var med = existing != null ? existing : ScriptableObject.CreateInstance<PillData>();

            med.medicationName = name;
            med.dosage = dose;
            med.imprintCode = imprint;
            med.shape = shape;
            med.sizeMillimeters = sizeMm;
            med.weightGrams = weightG;
            med.primaryColor = primary;
            med.secondaryColor = secondary;
            med.controlledStatus = controlled;
            med.requiresDoubleVerification = requiresDoubleVerification;
            med.instructions = $"Take {name} {dose} as directed by prescriber.";
            med.warnings = "Keep out of reach of children. Consult pharmacist with questions.";

            var template = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pills/PillTemplate.prefab");
            if (template != null)
                med.pillPrefab = template;

            if (existing == null)
                AssetDatabase.CreateAsset(med, path);
            else
                EditorUtility.SetDirty(med);

            return med;
        }
    }
}
#endif

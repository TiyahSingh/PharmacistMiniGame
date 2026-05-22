# Pharmacist Mini Game — Pharmacy Simulation (Unity)

Educational 3D pharmacy workflow simulation: imprint verification, physics pill sorting, dispensing, labeling, and safety checks.

## Play in 3 steps

1. **Open** this folder in **Unity 2022.3 LTS** (or newer 2022.3.x).
2. Wait for packages to import. **Full setup runs automatically** on first open (URP, medications, scene, UI).
3. Open **`Assets/Scenes/PharmacyLab.unity`** and press **Play**.

Manual re-run: menu **Pharmacy Sim → Setup Everything (Full Project)**

## Controls

| Action | Input |
|--------|--------|
| Shake tray | Bottom slider, drag tray, mouse delta, or gamepad left stick |
| Inspect pill imprint | Click pill |
| Dispense pill | Hold **E** + click pill (after imprint verified in UI) |
| Verify prescription imprint | Enter code → **Verify Imprint** |
| Reference lookup | Database panel → Search |
| Bottle / label / scan | **Seal Bottle**, **Apply Label**, **Scan Barcode** |
| Controlled meds | **Run Double Verify** then **Submit Order** |

## Workflow

1. Select prescription from queue (first order auto-selected).
2. Read patient/medication/imprint on UI.
3. Enter required imprint → **Verify Imprint**.
4. Shake tray; click pills to confirm imprint on tablets.
5. Hold **E** and click to dispense into bottle (repeat for full quantity).
6. **Seal Bottle** → **Apply Label** → **Scan Barcode** (uses Rx ID).
7. Complete checklist → **Submit Order**.

**Safety:** Identification is by **imprint + dosage + shape**, not color.

## What auto-setup creates

- URP pipeline (`Assets/Settings/PharmacyURP.asset`)
- 16 medications + `MedicationDatabase`
- Pill prefab with physics
- `PharmacyControls` input actions
- Complete `PharmacyLab` scene (tray, bottle, printer, camera, lighting)
- Full professional UI (queue, Rx detail, checklist, database search)
- Build settings → `PharmacyLab` scene

## Architecture

See script folders under `Assets/Scripts/` — Core, Data, Pills, Tray, Prescription, Verification, Labeling, UI, Tools, Audio, Save, Progression.

## Add medications

**Pharmacy Sim → Generate Sample Medications** after creating new `PillData` assets, or duplicate existing assets under `Assets/Data/Medications/`.

## Audio (optional polish)

Assign clips to `PharmacyAudioManager` on `PharmacySystems`: rattle, tray shake, pour, printer, scanner beep, ambient.

---

*Educational simulation — not medical advice.*

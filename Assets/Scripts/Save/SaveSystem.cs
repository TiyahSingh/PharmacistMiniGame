using UnityEngine;

namespace PharmacySim.Save
{
    public class SaveSystem : MonoBehaviour
    {
        private const string SaveKey = "PharmacySim_Save";

        public SaveData Data { get; private set; } = new();

        public void Save()
        {
            Data.lastShiftDate = System.DateTime.UtcNow.ToString("o");
            var json = JsonUtility.ToJson(Data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                Data = new SaveData();
                return;
            }

            var json = PlayerPrefs.GetString(SaveKey);
            Data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }

        public void ResetSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            Data = new SaveData();
        }
    }
}

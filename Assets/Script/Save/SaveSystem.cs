using UnityEngine;

public class SaveSystem
{
    private const string SAVE_KEY = "GameSave";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public static SaveData Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            return new SaveData();
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
    }
}

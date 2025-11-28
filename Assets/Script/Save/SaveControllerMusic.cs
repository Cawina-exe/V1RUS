using UnityEngine;

public class SaveControllerMusic : MonoBehaviour
{
    private SaveData currentData;
    private void Start()
    {
        currentData = SaveSystem.Load();

        AudioListener.volume = currentData.som;
    }

}

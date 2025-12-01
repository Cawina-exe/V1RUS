using UnityEngine;

public class SaveControllerSFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    private SaveData currentData;
    private void Start()
    {
        currentData = SaveSystem.Load();

        audioSource.volume = currentData.sfx;
    }

}

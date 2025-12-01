using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldsFunction : MonoBehaviour
{
    [Header("Data Settings")]
    [SerializeField] private List<WorldData> dataPossiveis;
    [SerializeField] private WorldData data;
    public WorldData Data { get { return data; } }

    [Header("Visual Settings")]
    [SerializeField] private List<GameObject> planetPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject currentPlanetInstance;

    [Header("UI & Stats")]
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text curaText;
    [SerializeField] private Virus statsVirus;

    [Header("Main Scene Management")]
    [Tooltip("Assign the PARENT object of your Main UI Canvas here.")]
    [SerializeField] private GameObject mainUIContainer;
    [SerializeField] private AudioSource mainMusic;
    [SerializeField] private Camera mainCamera;

    // Timers
    private float timer5sec = 0f;
    private float timer10sec = 0f;
    private float timer1min = 0f;

    // Stats
    public float defesaAcumulada = 0;
    public float defesaGain = 0;
    public float curaAcumulada = 0f;
    public float curaGain = 0f;
    public float worldHabitantes;

    public bool statsCura = false;
    public bool statsDefesa = false;

    public int planetaIndexAtual;

    void Start()
    {
       
        if (mainCamera == null) mainCamera = Camera.main;


        SaveData save = SaveSystem.Load();
        int planetaIndex = -1;

        if (save != null)
        {
            for (int i = 0; i < save.PlanetaAtivado.Count; i++)
            {
                if (save.PlanetaAtivado[i])
                {
                    planetaIndex = i;
                    break;
                }
            }
        }

        if (planetaIndex == -1) planetaIndex = 0;

       
        if (planetaIndex >= dataPossiveis.Count)
        {
            Debug.LogError("Error: Index is larger than Data list!");
            return;
        }

      
        planetaIndexAtual = planetaIndex;
        data = dataPossiveis[planetaIndex];

        SpawnPlanet(planetaIndex);

        worldHabitantes = data.habitantes;
        defesaGain = data.defesa;
        curaGain = data.cura;

       
        UpdateStatUI();
    }

    void Update()
    {
        if (worldHabitantes <= 0) WinVirus();

        if (defesaAcumulada < 0)
        {
            statsDefesa = false;
            defesaAcumulada = 0;
            if (defenseText) defenseText.text = "0";
        }

        if (curaAcumulada < 0)
        {
            statsCura = false;
            curaAcumulada = 0;
            if (curaText) curaText.text = "0";
        }

        timer5sec += Time.deltaTime;
        timer10sec += Time.deltaTime;
        timer1min += Time.deltaTime;

        if (timer5sec >= 5f)
        {
            if (statsCura)
            {
                curaAcumulada += curaGain;
                if (curaText) curaText.text = curaAcumulada.ToString();
            }
            timer5sec = 0f;
        }

        if (timer10sec >= 10f)
        {
            if (statsDefesa)
            {
                defesaAcumulada += defesaGain;
                if (defenseText) defenseText.text = defesaAcumulada.ToString();
            }
            timer10sec = 0f;
        }

        if (timer1min >= 60f)
        {
            if (statsCura)
            {
                curaAcumulada *= 2f;
                if (curaText) curaText.text = curaAcumulada.ToString();
            }
            if (statsDefesa)
            {
                defesaAcumulada *= 2f;
                if (defenseText) defenseText.text = defesaAcumulada.ToString();
            }
            timer1min = 0f;
        }
    }

    void SpawnPlanet(int index)
    {
        if (index >= planetPrefabs.Count) return;
        if (currentPlanetInstance != null) Destroy(currentPlanetInstance);

        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : Vector3.zero;
        Quaternion rot = (spawnPoint != null) ? spawnPoint.rotation : Quaternion.identity;

        currentPlanetInstance = Instantiate(planetPrefabs[index], pos, rot);
    }

    public void WinVirus()
    {
        SaveData save = SaveSystem.Load();
        if (statsVirus != null && save != null)
        {
            if (statsVirus.virusPontos > save.Pontos[planetaIndexAtual])
            {
                save.Pontos[planetaIndexAtual] = statsVirus.virusPontos;
            }
            SaveSystem.Save(save);
        }
        SceneManager.LoadScene("Vitoria");
    }

    
    public void change()
    {
        UpdateStatUI();
    }

    private void UpdateStatUI()
    {
        if (curaText) curaText.text = curaAcumulada.ToString();
        if (defenseText) defenseText.text = defesaAcumulada.ToString();
    }

    public void ToggleMainScene(bool isActive)
    {
   
        if (mainUIContainer != null)
            mainUIContainer.SetActive(isActive);
        else
            Debug.LogWarning("Main UI Container is not assigned in WorldsFunction! UI will overlap.");

    
        if (mainCamera != null) mainCamera.enabled = isActive;

 
        if (mainMusic != null)
        {
            if (isActive && !mainMusic.isPlaying) mainMusic.Play();
            else if (!isActive && mainMusic.isPlaying) mainMusic.Pause();
        }
    }

    public void ApplyPacmanWin()
    {
        Debug.Log("Pacman Won! Rewards Applied.");

        if (statsVirus != null)
        {
            float bonus = statsVirus.virusPontos * 0.5f;
            statsVirus.virusPontos += bonus;
            if (statsVirus.VirusPontosText != null)
                statsVirus.VirusPontosText.text = statsVirus.virusPontos.ToString();
        }

        if (statsCura)
        {
            curaAcumulada -= 20f;
            if (curaAcumulada < 0) curaAcumulada = 0;
            if (curaText != null) curaText.text = curaAcumulada.ToString();
        }

        ToggleMainScene(true);
    }

    public void ApplyPacmanLoss()
    {
        Debug.Log("Pacman Lost! Penalty Applied.");

        statsCura = true;
        curaAcumulada += 15f;

        if (curaText != null) curaText.text = curaAcumulada.ToString();

        ToggleMainScene(true);
    }
}
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldsFunction : MonoBehaviour
{
    [Header("Data Settings")]
    [SerializeField] private List<WorldData> dataPossiveis;
    [SerializeField] private WorldData data;

    [Header("Visual Settings")] 
  
    [SerializeField] private List<GameObject> planetPrefabs;

    [SerializeField] private Transform spawnPoint;
    private GameObject currentPlanetInstance; 

    [Header("UI & Stats")]
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text curaText;
    [SerializeField] private Virus statsVirus;

    private float timer5sec = 0f;
    private float timer10sec = 0f;
    private float timer1min = 0f;

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
        SaveData save = SaveSystem.Load();

        int planetaIndex = -1;

 
        for (int i = 0; i < save.PlanetaAtivado.Count; i++)
        {
            if (save.PlanetaAtivado[i])
            {
                planetaIndex = i;
                break;
            }
        }

  
        if (planetaIndex == -1)
            planetaIndex = 0;

   
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

        curaText.text = curaAcumulada.ToString();
        defenseText.text = defesaAcumulada.ToString();
    }

  
    void SpawnPlanet(int index)
    {
    
        if (index >= planetPrefabs.Count)
        {
            Debug.LogError("Error: You forgot to add the Prefab to the list in the Inspector!");
            return;
        }

        if (currentPlanetInstance != null)
        {
            Destroy(currentPlanetInstance);
        }


        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : Vector3.zero;
        Quaternion rot = (spawnPoint != null) ? spawnPoint.rotation : Quaternion.identity;

        currentPlanetInstance = Instantiate(planetPrefabs[index], pos, rot);

     
    }

    void Update()
    {
        if (worldHabitantes <= 0)
        {
            WinVirus();
        }

        if (defesaAcumulada < 0)
        {
            statsDefesa = false;
            defenseText.text = "0";
        }

        if (curaAcumulada < 0)
        {
            statsCura = false;
            curaText.text = "0";
        }

        timer5sec += Time.deltaTime;
        timer10sec += Time.deltaTime;
        timer1min += Time.deltaTime;

        if (timer5sec >= 5f)
        {
            if (statsCura)
            {
                curaAcumulada += curaGain;
                curaText.text = curaAcumulada.ToString();
            }
            timer5sec = 0f;
        }

        if (timer10sec >= 10f)
        {
            if (statsDefesa)
            {
                defesaAcumulada += defesaGain;
                defenseText.text = defesaAcumulada.ToString();
            }
            timer10sec = 0f;
        }

        if (timer1min >= 60f)
        {
            if (statsCura)
            {
                curaAcumulada *= 2f;
                curaText.text = curaAcumulada.ToString();
            }
            if (statsDefesa)
            {
                defesaAcumulada *= 2f;
                defenseText.text = defesaAcumulada.ToString();
            }
            timer1min = 0f;
        }
    }

    public void WinVirus()
    {
        SaveData save = SaveSystem.Load();
        if (statsVirus.virusPontos > save.Pontos[planetaIndexAtual])
        {
            save.Pontos[planetaIndexAtual] = statsVirus.virusPontos;
        }
        SaveSystem.Save(save);
        SceneManager.LoadScene("Vitoria");
    }

    public void change()
    {
        curaText.text = curaAcumulada.ToString();
        defenseText.text = defesaAcumulada.ToString();
    }
}
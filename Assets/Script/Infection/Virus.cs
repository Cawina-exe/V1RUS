using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Virus : MonoBehaviour
{
    public float virusMoney = 0;
    public float virusPower = 0;
    public float virusPontos = 0;
    public float virusGain = 1;
    private float timer1sec = 0f;
    private bool stateVirus = false;
    
    [Header("Virus Shaders")]
    [SerializeField] private Material virusMaterial;
    [SerializeField] private Material techVirusMaterial;
    
    [SerializeField] private TMP_Text HabitantesText;
    [SerializeField] private TMP_Text VirusPowerText;
    [SerializeField] private TMP_Text VirusMoneyText;
    [SerializeField] public TMP_Text VirusPontosText;
    [SerializeField] private WorldsFunction statsWorld;

    void Start()
    {
        VirusMoneyText.text = virusMoney.ToString();
        VirusPowerText.text = virusPower.ToString();
        HabitantesText.text = statsWorld.worldHabitantes.ToString();
        VirusPontosText.text = virusPontos.ToString();
    }

    void Update()
    {
        if (stateVirus)
        {
            Derrota();
            return;
        }

        timer1sec += Time.deltaTime;

        if (timer1sec >= 1f)
        {
            virusPower += virusGain;
            virusPontos += Mathf.Ceil(virusGain * 0.25f);
            virusMoney += 1;
            VirusPontosText.text = virusPontos.ToString();
            VirusMoneyText.text = virusMoney.ToString() + "$";
            attack();
            timer1sec = 0f;
        }
    }

    void attack() 
    {
        virusPower = virusPower - statsWorld.curaAcumulada;    
        
        if (virusPower <= 0) 
        {
            stateVirus = true;
        }
        else if (virusPower - statsWorld.defesaAcumulada > 0)
        {
            statsWorld.worldHabitantes = statsWorld.worldHabitantes + statsWorld.defesaAcumulada - virusPower;
            UpdateVirusShader();
        }
        else
        {
            statsWorld.worldHabitantes = statsWorld.worldHabitantes - 1;
            UpdateVirusShader();
        }

        VirusPowerText.text = virusPower.ToString();
        HabitantesText.text = statsWorld.worldHabitantes.ToString();
    }

    void Derrota()
    {
        SceneManager.LoadScene("Derrota");
    }

    public void PerderDinheiro(float money)
    {
        virusMoney -= money;
        VirusMoneyText.text = virusMoney.ToString() + "$";
    }

    public void change()
    {
        VirusPowerText.text = virusPower.ToString();
        VirusMoneyText.text = virusMoney.ToString() + "$";
    }

    public void UpdateVirusShader()
    {
        float totalPopulation = statsWorld.Data.habitantes;
        float currentPopulation = statsWorld.worldHabitantes; 
        
        float populationPercentage = (totalPopulation / currentPopulation) - 1;
        Mathf.Clamp(populationPercentage, 0, 1);
        Debug.Log($"Current Percentage: {populationPercentage}");
        virusMaterial.SetFloat("_VirusPercentage", populationPercentage);
        techVirusMaterial.SetFloat("_VirusPercentage", populationPercentage);
    }
}


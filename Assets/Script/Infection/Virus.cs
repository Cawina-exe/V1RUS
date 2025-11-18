using TMPro;
using UnityEngine;

public class Virus : MonoBehaviour
{
    private float virusPower = 0;
    private float virusGain = 1;
    private float timer1sec = 0f;
    private bool stateVirus = false;

    [SerializeField] private TMP_Text HabitantesText;
    [SerializeField] private TMP_Text VirusPowerText;
    [SerializeField] private WorldsFunction statsWorld;

    void Start()
    {
        VirusPowerText.text = virusPower.ToString();
        HabitantesText.text = statsWorld.worldHabitantes.ToString();
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
        }
        else
        {
            statsWorld.worldHabitantes = statsWorld.worldHabitantes - 1;
        }

        VirusPowerText.text = virusPower.ToString();
        HabitantesText.text = statsWorld.worldHabitantes.ToString();
    }

    void Derrota()
    {

    }
}


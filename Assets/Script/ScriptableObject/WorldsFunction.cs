using TMPro;
using UnityEditor;
using UnityEngine;

public class WorldsFunction : MonoBehaviour
{
    [SerializeField] private WorldData data;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text curaText;

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


    void Start()
    {
        worldHabitantes = data.habitantes;

        defesaGain = data.defesa;
        curaGain = data.cura;

        defenseText.text = "0";
        curaText.text = "0";
    }

    void Update()
    {
        if (worldHabitantes == 0)
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

    }

    public void change()
    {
        curaText.text = curaAcumulada.ToString();
        defenseText.text = defesaAcumulada.ToString();
    }

}

using System.Collections;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField] private WorldsFunction statsWorld;
    public TMP_Text timerText;

    public Color defaultColor;
    public Color colorAfter1Min;
    public Color colorLast30Sec;

    private float timeRemaining = 300f; 
    private bool isBlinking = false;
    public float blinkSpeed = 0.5f;

    private bool isCuraActive = false;
    private bool isDefesaActive = false;

    void Start()
    {
        timerText.color = defaultColor;
        UpdateTimerText();
    }

    [System.Obsolete]
    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 240f && timeRemaining > 30f)
            {
                if (isDefesaActive == false)
                {
                    ActiveDefesa();
                }
                timerText.color = colorAfter1Min;
            }

            if (timeRemaining <= 180f)
            {
                if (isCuraActive == false)
                {
                    ActiveCura();
                }
            }

            if (timeRemaining <= 30f && timeRemaining > 10f)
            {
                timerText.color = colorLast30Sec;
            }

            if (timeRemaining <= 10f && !isBlinking)
            {
                StartCoroutine(BlinkEffect());
                isBlinking = true;
            }

            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
    private IEnumerator BlinkEffect()
    {
        while (timeRemaining > 0)
        {
            timerText.enabled = !timerText.enabled;
            yield return new WaitForSeconds(blinkSpeed);
        }

        timerText.enabled = true;
    }

    void ActiveDefesa()
    {
        isDefesaActive = true;
        statsWorld.statsDefesa = true;
    }
    void ActiveCura()
    {
        isCuraActive = true;
        statsWorld.statsCura = true;
    }
}
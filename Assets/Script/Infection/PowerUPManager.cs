using System.Collections.Generic;
using UnityEngine;

public class PowerUPManager : MonoBehaviour
{
    [Header("Lista com TODOS os ScriptableObjects")]
    public List<OptionsData> allOptions;

    [Header("Slots onde aparecem os 3 PowerUps")]
    public PowerUPSlots[] slots;

    [Header("UI inteira do PowerUp")]
    public GameObject UIPower;

    private float timer = 0f;

    void Start()
    {
        UIPower.SetActive(false);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 60f)
        {
            timer = 0f;
            EscolherNovos();
        }
    }

    void EscolherNovos()
    {
        UIPower.SetActive(true);

        List<OptionsData> copia = new List<OptionsData>(allOptions);
        OptionsData[] escolhidos = new OptionsData[3];

        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, copia.Count);
            escolhidos[i] = copia[index];
            copia.RemoveAt(index);
        }

        for (int i = 0; i < 3; i++)
        {
            slots[i].Setup(escolhidos[i], this);
        }
    }

    public void EscolhaFeita()
    {
        UIPower.SetActive(false);
    }
}

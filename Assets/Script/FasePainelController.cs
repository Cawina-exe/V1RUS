using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FasePainelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nomePlaneta;
    [SerializeField] private TextMeshProUGUI pontos;
    [SerializeField] private Image planetaFoto;
    [SerializeField] private List<Sprite> planeta;

    public void AtivarPainelFase()
    {
        SaveData data = SaveSystem.Load();

        int planetaIndex = -1;

        for (int i = 0; i < data.PlanetaAtivado.Count; i++)
        {
            if (data.PlanetaAtivado[i])
            {
                planetaIndex = i;
                break;
            }
        }

        if (planetaIndex == -1)
            return;

        if (planetaIndex >= planeta.Count)
            return;

        if (data.Planetas.Count > planetaIndex)
            nomePlaneta.text = data.Planetas[planetaIndex];
        else
            nomePlaneta.text = "???";

        if (data.Pontos.Count > planetaIndex)
            pontos.text = "Pontos: " + data.Pontos[planetaIndex].ToString("0");
        else
            pontos.text = "Pontos: 0";

        planetaFoto.sprite = planeta[planetaIndex];
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUPSlots : MonoBehaviour
{
    [Header("Referências UI")]
    public TMP_Text tituloText;
    public TMP_Text virusText;
    public TMP_Text planetaText;
    public Button botao;

    private OptionsData currentData;
    private PowerUPManager manager;

    private Virus virus;
    private WorldsFunction world;

    void Start()
    {
        virus = GameObject.Find("EventSystem").GetComponent<Virus>();
        world = GameObject.Find("EventSystem").GetComponent<WorldsFunction>();
    }

    public void Setup(OptionsData data, PowerUPManager m)
    {
        currentData = data;
        manager = m;

        tituloText.text = data.Titulo;
        virusText.text = data.descricaoVirus;
        planetaText.text = data.descricaoPlaneta;

        botao.onClick.RemoveAllListeners();
        botao.onClick.AddListener(Aplicar);
    }

    void Aplicar()
    {
        switch (currentData.virusAction)
        {
            case OptionsData.ActionType.AumentarValor:
                virus.virusPower += currentData.valorParaOVirus;
                break;

            case OptionsData.ActionType.DiminuirValor:
                virus.virusPower -= currentData.valorParaOVirus;
                break;

            case OptionsData.ActionType.MultiplicarValor:
                virus.virusPower *= currentData.valorParaOVirus;
                break;

            case OptionsData.ActionType.DividirValor:
                virus.virusPower /= currentData.valorParaOVirus;
                break;

            case OptionsData.ActionType.AumentarGanho:
                virus.virusGain += currentData.valorParaOVirus;
                break;

            case OptionsData.ActionType.DiminuirGanho:
                virus.virusGain -= currentData.valorParaOVirus;
                break;

            case OptionsData.ActionType.MultiplicarGanho:
                virus.virusGain *= currentData.valorParaOVirus;
                break;

            case OptionsData.ActionType.DividirGanho:
                virus.virusGain /= currentData.valorParaOVirus;
                break;
        }

        switch (currentData.planetaTipe)
        {
            case OptionsData.TypePlanet.Defesa:
                AplicarPlaneta(true);
                break;

            case OptionsData.TypePlanet.Cura:
                AplicarPlaneta(false);
                break;
        }

        manager.EscolhaFeita();
    }

    void AplicarPlaneta(bool defesa)
    {
        switch (currentData.planetaAction)
        {
            case OptionsData.ActionType.AumentarValor:
                if (defesa) world.defesaAcumulada += currentData.valorParaOPlaneta;
                else world.curaAcumulada += currentData.valorParaOPlaneta;
                break;

            case OptionsData.ActionType.DiminuirValor:
                if (defesa) world.defesaAcumulada -= currentData.valorParaOPlaneta;
                else world.curaAcumulada -= currentData.valorParaOPlaneta;
                break;

            case OptionsData.ActionType.MultiplicarValor:
                if (defesa) world.defesaAcumulada *= currentData.valorParaOPlaneta;
                else world.curaAcumulada *= currentData.valorParaOPlaneta;
                break;

            case OptionsData.ActionType.DividirValor:
                if (defesa) world.defesaAcumulada /= currentData.valorParaOPlaneta;
                else world.curaAcumulada /= currentData.valorParaOPlaneta;
                break;

            case OptionsData.ActionType.AumentarGanho:
                if (defesa) world.defesaGain += currentData.valorParaOPlaneta;
                else world.curaGain += currentData.valorParaOPlaneta;
                break;

            case OptionsData.ActionType.DiminuirGanho:
                if (defesa) world.defesaGain -= currentData.valorParaOPlaneta;
                else world.curaGain -= currentData.valorParaOPlaneta;
                break;

            case OptionsData.ActionType.MultiplicarGanho:
                if (defesa) world.defesaGain *= currentData.valorParaOPlaneta;
                else world.curaGain *= currentData.valorParaOPlaneta;
                break;

            case OptionsData.ActionType.DividirGanho:
                if (defesa) world.defesaGain /= currentData.valorParaOPlaneta;
                else world.curaGain /= currentData.valorParaOPlaneta;
                break;
        }
    }
}

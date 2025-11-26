using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "OptionsData", menuName = "Scriptable Objects/OptionsData")]
public class OptionsData : ScriptableObject
{
    public enum ActionType
    {
        AumentarValor,
        DiminuirValor,
        MultiplicarValor,
        DividirValor,
        AumentarGanho,
        DiminuirGanho,
        MultiplicarGanho,
        DividirGanho
    }

    public enum TypePlanet
    {
        Defesa,
        Cura
    }

    public string Titulo;

    [Header("Ação deste botão Para o Virus")]
    public float valorParaOVirus;
    public ActionType virusAction;
    public string descricaoVirus;

    [Header("Ação deste botão Para o Planeta")]
    public float valorParaOPlaneta;
    public ActionType planetaAction;
    public TypePlanet planetaTipe;
    public string descricaoPlaneta;
}

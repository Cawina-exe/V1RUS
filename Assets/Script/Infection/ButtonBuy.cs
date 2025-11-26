using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonBuy : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum ActionTypeOne
    {
        AumentarValor,
        DiminuirValor,
        MultiplicarValor,
        Dividir
    }

    public enum ActionTypeTwo
    {
        virus,
        planetaDefesa,
        planetaCura
    }

    [Header("Ação deste botão")]
    [SerializeField] private ActionTypeOne acao;

    [Header("Para quem é direcionado")]
    [SerializeField] private ActionTypeTwo quem;

    [Header("Local da descrição")]
    [SerializeField] private TextMeshProUGUI text;

    [Header("Valores configuráveis")]
    [SerializeField] private float valor;
    [SerializeField] private float preco = 0;
    [SerializeField] private string descricao;

    private bool utilizado = false;
    private WorldsFunction statsWorld;
    private Virus statsVirus;

    void Start()
    {
        statsVirus = GameObject.Find("EventSystem").GetComponent<Virus>();
        statsWorld = GameObject.Find("EventSystem").GetComponent<WorldsFunction>();
        GetComponent<Button>().onClick.AddListener(Executar);
    }

    public void Update()
    {
        if (utilizado == true)
        {
            GetComponent<Button>().interactable = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.text = descricao + "\nPreço: " + preco.ToString("F2");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.text = "";
    }

    public void Executar()
    {
        if (statsVirus.virusMoney >= preco)
        {
            statsVirus.PerderDinheiro(preco);
            switch (quem)
            {
                case ActionTypeTwo.planetaDefesa:
                    switch (acao)
                    {

                        case ActionTypeOne.AumentarValor:
                            statsWorld.defesaGain += valor;
                            utilizado = true;
                            break;

                        case ActionTypeOne.DiminuirValor:
                            statsWorld.defesaGain -= valor;
                            utilizado = true;
                            break;

                        case ActionTypeOne.MultiplicarValor:
                            statsWorld.defesaGain *= valor;
                            utilizado = true;
                            break;

                        case ActionTypeOne.Dividir:
                            statsWorld.defesaGain /= valor;
                            utilizado = true;
                            break;
                    }
                    break;

                case ActionTypeTwo.planetaCura:
                    switch (acao)
                    {

                        case ActionTypeOne.AumentarValor:
                            statsWorld.curaGain += valor;
                            utilizado = true;
                            break;

                        case ActionTypeOne.DiminuirValor:
                            statsWorld.curaGain -= valor;
                            utilizado = true;
                            break;

                        case ActionTypeOne.MultiplicarValor:
                            statsWorld.curaGain *= valor;
                            utilizado = true;
                            break;

                        case ActionTypeOne.Dividir:
                            statsWorld.curaGain /= valor;
                            utilizado = true;
                            break;
                    }
                    break;

                case ActionTypeTwo.virus:
                    switch (acao)
                    {

                        case ActionTypeOne.AumentarValor:
                            statsVirus.virusGain += valor;
                            utilizado = true;
                            break;

                        case ActionTypeOne.DiminuirValor:
                            statsVirus.virusGain -= valor;
                            utilizado = true;
                            break;

                        case ActionTypeOne.MultiplicarValor:
                            statsVirus.virusGain *= valor;
                            utilizado = true;
                            break;

                        case ActionTypeOne.Dividir:
                            statsVirus.virusGain /= valor;
                            utilizado = true;
                            break;
                    }
                    break;
            }
            statsWorld.change();
            statsVirus.change();
        }
    }
}

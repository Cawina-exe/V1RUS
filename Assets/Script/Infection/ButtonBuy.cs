using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonBuy : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum ActionTypeOne { AumentarValor, DiminuirValor, MultiplicarValor, Dividir }
    public enum ActionTypeTwo { virus, planetaDefesa, planetaCura }

    [Header("Configuração de Tipo")]

    [SerializeField] private bool bloquearAposCompra = false;

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
    private Button myButton;

    void Start()
    {
        statsVirus = GameObject.Find("EventSystem").GetComponent<Virus>();
        statsWorld = GameObject.Find("EventSystem").GetComponent<WorldsFunction>();

        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(Executar);

       
        if (bloquearAposCompra && utilizado)
        {
            myButton.interactable = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
        if ((!utilizado || !bloquearAposCompra) && text != null)
        {
            text.text = descricao + "\nCost: " + preco.ToString("F2");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (text != null) text.text = "";
    }

    public void Executar()
    {
       
        if (bloquearAposCompra && utilizado) return;

        if (statsVirus.virusMoney >= preco)
        {
            statsVirus.virusPontos += preco / 3f;
            if (statsVirus.VirusPontosText != null)
                statsVirus.VirusPontosText.text = statsVirus.virusPontos.ToString();

            statsVirus.PerderDinheiro(preco);

           
            switch (quem)
            {
                case ActionTypeTwo.planetaDefesa:
                    ApplyAction(ref statsWorld.defesaGain);
                    break;
                case ActionTypeTwo.planetaCura:
                    ApplyAction(ref statsWorld.curaGain);
                    break;
                case ActionTypeTwo.virus:
                    ApplyAction(ref statsVirus.virusGain);
                    break;
            }

            statsWorld.change();
            statsVirus.change();

          
            if (bloquearAposCompra)
            {
                utilizado = true;
                myButton.interactable = false;
                if (text != null) text.text = "";
            }
        }
    }

  
    private void ApplyAction(ref float targetValue)
    {
        switch (acao)
        {
            case ActionTypeOne.AumentarValor: targetValue += valor; break;
            case ActionTypeOne.DiminuirValor: targetValue -= valor; break;
            case ActionTypeOne.MultiplicarValor: targetValue *= valor; break;
            case ActionTypeOne.Dividir: targetValue /= valor; break;
        }
    }
}
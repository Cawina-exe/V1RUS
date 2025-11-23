using System.Collections.Generic;
using UnityEngine;

public class MiniGamePopUp : MonoBehaviour
{
    public List<GameObject> allPopUps;

    private List<GameObject> allPopUpsAtivos;  

    public GameObject panelDeCriacao;

    void Awake()
    {
        allPopUpsAtivos = new List<GameObject>();
    }

    public void StartGame()
    {
        LimparAtivosExistentes();
        CriarPopUpsIniciais(10);
    }

    private void LimparAtivosExistentes()
    {
        for (int i = allPopUpsAtivos.Count - 1; i >= 0; i--)
        {
            if (allPopUpsAtivos[i] != null)
                Destroy(allPopUpsAtivos[i]);
        }
        allPopUpsAtivos.Clear();
    }

    private void CriarPopUpsIniciais(int quantidade)
    {
        for (int i = 0; i < quantidade; i++)
            CriarPopUpAleatorio();
    }

    public void CriarMaisDoisPopUps()
    {
        CriarPopUpAleatorio();
        CriarPopUpAleatorio();
    }

    private GameObject CriarPopUpAleatorio()
    {
        GameObject prefab = allPopUps[Random.Range(0, allPopUps.Count)];

        GameObject novo = Instantiate(prefab, panelDeCriacao.transform, false);

        RectTransform parentRT = panelDeCriacao.GetComponent<RectTransform>();
        RectTransform rt = novo.GetComponent<RectTransform>();
        if (parentRT != null && rt != null)
        {
            Rect r = parentRT.rect;

            float x = Random.Range(r.xMin + rt.rect.width * 0.5f, r.xMax - rt.rect.width * 0.5f);
            float y = Random.Range(r.yMin + rt.rect.height * 0.5f, r.yMax - rt.rect.height * 0.5f);

            rt.anchoredPosition = new Vector2(x, y);
        }
        else
        {
            novo.transform.localPosition = new Vector3(
                Random.Range(-300f, 300f),
                Random.Range(-200f, 200f),
                0f
            );
        }

        allPopUpsAtivos.Add(novo);
        return novo;
    }

    public void RemoverPopUp(GameObject popup)
    {
        if (popup == null) return;
        if (allPopUpsAtivos.Contains(popup))
            allPopUpsAtivos.Remove(popup);

        Destroy(popup);

        if (allPopUpsAtivos.Count <= 0)
            WinGame();
    }

    private void WinGame()
    {
        Virus pontos = GameObject.Find("EventSystem").GetComponent<Virus>();
        pontos.virusPontos += pontos.virusPontos*0.5f;
        pontos.VirusPontosText.text = pontos.virusPontos.ToString();
    }
}

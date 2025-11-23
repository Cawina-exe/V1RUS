using UnityEngine;
using UnityEngine.LightTransport;

public class PopUpButton : MonoBehaviour
{
    public MiniGamePopUp miniGame;

    private void Start()
    {
        miniGame = GameObject.Find("EventSystem").GetComponent<MiniGamePopUp>();
    }

    public void OnClick_CreateTwo()
    {
        miniGame.CriarMaisDoisPopUps();
    }
}
using UnityEngine;

public class PopUpCloseButton : MonoBehaviour
{
    public MiniGamePopUp miniGame;

    private void Start()
    {
        miniGame = GameObject.Find("EventSystem").GetComponent<MiniGamePopUp>();
    }

    public void OnClick_RemoveParentPopup()
    {
        GameObject popupPai = transform.parent != null ? transform.parent.gameObject : null;

        miniGame.RemoverPopUp(popupPai);
    }
}

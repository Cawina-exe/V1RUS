using UnityEngine;
using TMPro; // Make sure this line is here

public class ClickableCircle : MonoBehaviour
{
    public int circleNumber;

    // THIS IS THE FIX:
    // It should be "TextMeshProUGUI" because you created it from the UI menu.
    public TextMeshProUGUI textDisplay;

    private GameManager gameManager;

    public void Initialize(GameManager manager, int number)
    {
        gameManager = manager;
        circleNumber = number;
        textDisplay.text = circleNumber.ToString();
    }

    private void OnMouseDown()
    {
        gameManager.CircleClicked(circleNumber);
        Destroy(gameObject);
    }
}
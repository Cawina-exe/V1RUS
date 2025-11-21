using System.Collections;
using UnityEngine;

public class ClickableCircle : MonoBehaviour
{
    public int circleNumber; 
    private GameManager gameManager;
    private bool isClicked = false;

    
    private SpriteRenderer spriteRenderer;

   
    void Awake()
    {
       
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Circle prefab is missing a SpriteRenderer!");
        }
    }

    public void Initialize(GameManager manager, int number)
    {
        gameManager = manager;
        circleNumber = number;
    }

    private void OnMouseDown()
    {
        if (isClicked) return;
        isClicked = true;
        
        gameManager.CircleClicked(circleNumber);
        StartCoroutine(PopAndShrinkAnimation());
    }
    
    private IEnumerator PopAndShrinkAnimation()
    {
     
        float popDuration = 0.1f;    
        float shrinkDuration = 0.2f; 
        float popScaleMultiplier = 1.3f;
        
       
        float darkenAmount = 0.7f; 

        
        Vector3 startScale = transform.localScale;
        Vector3 popScale = startScale * popScaleMultiplier;
        Vector3 endScale = Vector3.zero;
        
       
        Color startColor = spriteRenderer.color; 
        
        
        Color popColor = new Color(startColor.r * darkenAmount, 
                                 startColor.g * darkenAmount, 
                                 startColor.b * darkenAmount, 
                                 startColor.a);
                                 
     
        Color endColor_Color = new Color(popColor.r, popColor.g, popColor.b, 0f);


     
        float timeElapsed = 0f;
        while (timeElapsed < popDuration)
        {
            float t = timeElapsed / popDuration; 
            
            transform.localScale = Vector3.Lerp(startScale, popScale, t);
            
            
            spriteRenderer.color = Color.Lerp(startColor, popColor, t);
            
            timeElapsed += Time.deltaTime;
            yield return null; 
        }
        
       
        transform.localScale = popScale;
        spriteRenderer.color = popColor; 

       
        timeElapsed = 0f; 
        while (timeElapsed < shrinkDuration)
        {
            float t = timeElapsed / shrinkDuration;
            
            transform.localScale = Vector3.Lerp(popScale, endScale, t);
            
            
            spriteRenderer.color = Color.Lerp(popColor, endColor_Color, t);
            
            timeElapsed += Time.deltaTime;
            yield return null; 
        }

        
        Destroy(gameObject);
    }
}
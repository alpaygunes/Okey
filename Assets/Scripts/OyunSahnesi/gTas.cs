using UnityEngine;

public class gTas : MonoBehaviour{
    public int rakam;
    public Color renk; 
    private SpriteRenderer zeminSpriteRenderer;

    private void Awake(){ 
        gameObject.SetActive(false);
        zeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();  
    }

    private void Start(){ 
        zeminSpriteRenderer.color = renk; 
    }
}
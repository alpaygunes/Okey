using TMPro;
using UnityEngine;

public class gTas : MonoBehaviour{
    public int meyveID;
    public Color renk;
    private SpriteRenderer zeminSpriteRenderer; 
    public TextMeshPro TextMeyveID;
    public SpriteRenderer meyveResmiSpriteRenderer;
    public int colID;

    private void Awake(){
        gameObject.SetActive(false);
        zeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();
        TextMeyveID = transform.Find("TextMeyveID").GetComponent<TextMeshPro>();
    }

    private void Start(){
        var acikRenk = Color.Lerp(renk, Color.white, 0.5f);
        zeminSpriteRenderer.color = acikRenk; 
        TextMeyveID.text = meyveID.ToString();
        meyveResmiSpriteRenderer = transform.Find("MeyveResmi").GetComponent<SpriteRenderer>();
        var koyuRenk = Color.Lerp(renk, Color.black, 0.2f);
        meyveResmiSpriteRenderer.color = koyuRenk;
        meyveResmiSpriteRenderer.transform.localScale *= 1f;
        Sprite sprite = Resources.Load<Sprite>("Images/Meyveler/" + meyveID);
        meyveResmiSpriteRenderer.sprite = sprite;  
    } 
 
}
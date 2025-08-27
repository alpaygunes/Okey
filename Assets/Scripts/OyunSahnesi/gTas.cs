using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class gTas : MonoBehaviour{
    private Dictionary<int , Sprite[]> RenkliMeyvelerDic = new Dictionary<int , Sprite[]>();
    public int MeyveID;
    public int RenkID;
    private SpriteRenderer zeminSpriteRenderer; 
    public TextMeshPro TextMeyveID;
    [FormerlySerializedAs("meyveResmiSpriteRenderer")] public SpriteRenderer MeyveResmiSpriteRenderer;
    public int colID;

    private void Awake(){
        gameObject.SetActive(false);
        zeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();
        //TextMeyveID = transform.Find("TextMeyveID").GetComponent<TextMeshPro>();
        
        RenkliMeyvelerDic.Add(0,Resources.LoadAll<Sprite>("Images/Meyveler/0_kirmizi"));
        RenkliMeyvelerDic.Add(1,Resources.LoadAll<Sprite>("Images/Meyveler/1_turuncu"));
        RenkliMeyvelerDic.Add(2,Resources.LoadAll<Sprite>("Images/Meyveler/2_mavi"));
        RenkliMeyvelerDic.Add(3,Resources.LoadAll<Sprite>("Images/Meyveler/3_yesil"));
        RenkliMeyvelerDic.Add(4,Resources.LoadAll<Sprite>("Images/Meyveler/4_mor"));
        RenkliMeyvelerDic.Add(5,Resources.LoadAll<Sprite>("Images/Meyveler/5_pembe"));
        RenkliMeyvelerDic.Add(6,Resources.LoadAll<Sprite>("Images/Meyveler/6_kahve"));
        RenkliMeyvelerDic.Add(7,Resources.LoadAll<Sprite>("Images/Meyveler/7_mavimsi"));
        RenkliMeyvelerDic.Add(8,Resources.LoadAll<Sprite>("Images/Meyveler/8_turkuazimsi"));
        RenkliMeyvelerDic.Add(9,Resources.LoadAll<Sprite>("Images/Meyveler/9_yesilimsi"));
    }

    private void Start(){
        // var acikRenk = Color.Lerp(RenkID, Color.white, 0.5f);
        // zeminSpriteRenderer.color = acikRenk; 
        //TextMeyveID.text = meyveID.ToString();
        MeyveResmiSpriteRenderer = transform.Find("MeyveResmi").GetComponent<SpriteRenderer>();
        // var koyuRenk = Color.Lerp(RenkID, Color.black, 0.2f);
        // meyveResmiSpriteRenderer.color = koyuRenk;
        MeyveResmiSpriteRenderer.transform.localScale *= 1f;
        var Meyveler = RenkliMeyvelerDic[RenkID];
        Sprite sprite = Meyveler[MeyveID]; 
        MeyveResmiSpriteRenderer.sprite = sprite;  
    } 
 
}
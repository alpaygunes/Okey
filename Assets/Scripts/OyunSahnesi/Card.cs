using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Card : MonoBehaviour{
    public Vector2 Size;
    public bool TaslarHareketli{ get; set; }

    public static Card Instance{ get; private set; }
    public List<List<GameObject>> _siraliAyniRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> SiraliAyniRenkliGrup = new List<GameObject>();

    public List<List<GameObject>> _siraliFarkliRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> SiraliFarkliRenkliGrup = new List<GameObject>();

    public List<List<GameObject>> _ayniMeyveAyniRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> AyniMeyveAyniRenkliGrup = new List<GameObject>();

    public List<List<GameObject>> _ayniMeyveFarkliRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> AyniMeyveFarkliRenkli = new List<GameObject>();
    private GameObject[] carddakiTaslar;

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;

        Instance = this;
        Size = GetComponent<SpriteRenderer>().bounds.size;
    }

    public void KutulariHazirla(){
        Vector2 cardSize = Instance.Size;
        float colonWidth = (cardSize.x / GameManager.Instance._colonCount);
        GameObject kutu_ = Resources.Load<GameObject>("Prefabs/Kutu");
        float satirSayisi = (cardSize.y / colonWidth);
        for (var satir = 0; satir < satirSayisi; satir++){
            for (int sutun = 0; sutun < GameManager.Instance._colonCount; sutun++){
                float positionX = (colonWidth * .5f) + (sutun * colonWidth) - cardSize.x * .5f;
                float positionY = -(cardSize.y * .5f) + ((satirSayisi - satir) * colonWidth);
                var kutu = Instantiate(kutu_, new Vector3(positionX, positionY, -0.01f), Quaternion.identity);
                kutu.transform.localScale = GameManager.Instance.spawnHolesList[0].transform.localScale;
            }
        }
    }

    public void Sallanma(){
        carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var tas in carddakiTaslar){
            TasManeger.Instance.TasInstances[tas].Sallanma();
        } 
        var ceptekiTaslar = GameObject.FindGameObjectsWithTag("CEPTEKI_TAS");
        foreach (var ptas in ceptekiTaslar){
            TasManeger.Instance.TasInstances[ptas].Sallanma();
        }
    }

    public void GoreveUyumluCtasYoket(){ 
        float beklemeSuresi = .1f;
        foreach (var pTas in Puanlama.Instance.SiralanmisTumPerlerdekiTaslar){ 
            var pTasScript = TasManeger.Instance.TasInstances[pTas.Value];
            foreach (var uTas in pTasScript.AyniKolondakiAltinveAltinTaslar){
                var uTasInstance = TasManeger.Instance.TasInstances[uTas]; 
                uTasInstance.StartCoroutine(uTasInstance.BekleYokol(beklemeSuresi));  
            }
            beklemeSuresi += 0.1f; 
        }
    }

    public void PtasIleUyumluCtaslariYoket(){
        float beklemeSuresi = .1f;
        foreach (var pTas in Puanlama.Instance.SiralanmisTumPerlerdekiTaslar){
            var pTasScript = TasManeger.Instance.TasInstances[pTas.Value];
            foreach (var bonusTaslari in pTasScript.BonusOlarakEslesenTaslar){
               bonusTaslari.Value.StartCoroutine(bonusTaslari.Value.BekleYokol(beklemeSuresi)); 
               beklemeSuresi += .1f;
            }
             
        } 
    }
    
    public void PtaslariYoket(){
        float beklemeSuresi = .1f;
        foreach (var pTas in Puanlama.Instance.SiralanmisTumPerlerdekiTaslar){
            var pTasScript = TasManeger.Instance.TasInstances[pTas.Value];
            beklemeSuresi += .1f;
            pTasScript.StartCoroutine(pTasScript.BekleYokol(beklemeSuresi)); 
        } 
    }
}
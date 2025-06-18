using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Card : MonoBehaviour{
    public Vector2 Size;
    public bool TaslarHareketli{ get; set; }
    public List<GameObject> spawnHolesList = new List<GameObject>();

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
    
    public void CreateSpawnHoles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / GameManager.Instance._colonCount;
        for (int i = 0; i < GameManager.Instance._colonCount; i++){
            float holePositionX = i * colonWidth - cardSize.x * .5f;
            float holePositionY = cardSize.y * .5f + colonWidth;
            holePositionX += colonWidth * .5f;
            GameObject SpawnHole = Resources.Load<GameObject>("Prefabs/SpawnHole");
            SpawnHole.GetComponent<SpawnHole>().colID = i;
            var sh = Instantiate(SpawnHole, new Vector3(holePositionX, holePositionY, -0.2f), Quaternion.identity);
            sh.transform.localScale = new Vector2(colonWidth, colonWidth);
            spawnHolesList.Add(sh);
        }
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
                kutu.transform.localScale = spawnHolesList[0].transform.localScale;
            }
        }
    }

    public void Sallanma(){
        carddakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var tas in carddakiTaslar){
            TasManeger.Instance.TasInstances[tas].sallanmaDurumu=false; 
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
            pTasScript.cepInstance?.YildiziYak(0);
            beklemeSuresi += .1f;
            pTasScript.StartCoroutine(pTasScript.BekleYokol(beklemeSuresi)); 
        } 
    }
}
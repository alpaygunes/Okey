using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Card : MonoBehaviour{
    public Vector2 Size; 
    public bool TaslarHareketli{ get; set; }
    
    public static Card Instance { get; private set; }
    public List<List<GameObject>> _siraliAyniRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> SiraliAyniRenkliGrup = new List<GameObject>();

    public List<List<GameObject>> _siraliFarkliRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> SiraliFarkliRenkliGrup = new List<GameObject>();

    public List<List<GameObject>> _ayniRakamAyniRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> AyniRakamAyniRenkliGrup = new List<GameObject>();

    public List<List<GameObject>> _ayniRakamFarkliRenkliGruplar = new List<List<GameObject>>();
    public List<GameObject> AyniRakamFarkliRenkli = new List<GameObject>();

      
    void Awake(){
        
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this; 

        Instance = this; 
        Size = GetComponent<SpriteRenderer>().bounds.size; 
    }
    
    // Karttaki taşları kontrol edip yan yana  perler varmı bakalım 
    public async Task KarttakiPerleriBul(){
        _siraliAyniRenkliGruplar.Clear();
        _siraliFarkliRenkliGruplar.Clear();
        _ayniRakamAyniRenkliGruplar.Clear();
        _ayniRakamFarkliRenkliGruplar.Clear();

        GameObject[] taslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");

        // önceki işaretlemeleri iptal edelim.
        foreach (var tas in taslar) {
            TasManeger.Instance.TasInstances[tas].birCardPerineDahil = false;
        }

        for (int i = 0; i < taslar.Length; i++) {
            var tas = taslar[i];

            SiraliAyniRenkliGrup.Clear();
            await TasManeger.Instance.TasInstances[tas].SiraliAyniRenkGrubunaDahilOl();
            if (SiraliAyniRenkliGrup.Count > 2) {
                _siraliAyniRenkliGruplar.Add(new List<GameObject>(SiraliAyniRenkliGrup));
                foreach (var item in SiraliAyniRenkliGrup) {
                    TasManeger.Instance.TasInstances[item].birCardPerineDahil = true;
                }
            }

            SiraliFarkliRenkliGrup.Clear();
            await TasManeger.Instance.TasInstances[tas].SiraliFarkliRenkGrubunaDahilOl();
            if (SiraliFarkliRenkliGrup.Count > 2) {
                _siraliFarkliRenkliGruplar.Add(new List<GameObject>(SiraliFarkliRenkliGrup));
                foreach (var item in SiraliFarkliRenkliGrup) {
                    TasManeger.Instance.TasInstances[item].birCardPerineDahil = true;
                }
            }

            AyniRakamAyniRenkliGrup.Clear();
            await TasManeger.Instance.TasInstances[tas].AyniRakamAyniRenkGrubunaDahilOl();
            if (AyniRakamAyniRenkliGrup.Count > 2) {
                _ayniRakamAyniRenkliGruplar.Add(new List<GameObject>(AyniRakamAyniRenkliGrup));
                foreach (var item in AyniRakamAyniRenkliGrup) {
                    TasManeger.Instance.TasInstances[item].birCardPerineDahil = true;
                }
            }

            AyniRakamFarkliRenkli.Clear();
            await TasManeger.Instance.TasInstances[tas].AyniRakamFarkliRenkGrubunaDahilOl();
            if (AyniRakamFarkliRenkli.Count > 2) {
                _ayniRakamFarkliRenkliGruplar.Add(new List<GameObject>(AyniRakamFarkliRenkli));
                foreach (var item in AyniRakamFarkliRenkli) {
                    TasManeger.Instance.TasInstances[item].birCardPerineDahil = true;
                }
            }
        }

        

        
    }

    public void KutulariHazirla(){
        Vector2 cardSize = Instance.Size;
        float colonWidth = (cardSize.x / GameManager.Instance._colonCount);
        GameObject kutu_ = Resources.Load<GameObject>("Prefabs/Kutu");
        float satirSayisi = (cardSize.y / colonWidth);
        for (var satir = satirSayisi; satir > 0; satir--){
            for (int sutun = 0; sutun < GameManager.Instance._colonCount; sutun++){
                float positionX = (colonWidth * .5f) + (sutun * colonWidth) - cardSize.x * .5f;
                float positionY = -(cardSize.y * .5f) + (colonWidth * 0.5f) + ((satirSayisi - satir) * colonWidth);
                positionY += 0.2f;
                var kutu = Instantiate(kutu_, new Vector3(positionX, positionY, -0.01f), Quaternion.identity);
                kutu.transform.localScale *= .6f;
                //kutu.transform.localScale = new Vector2(colonWidth, colonWidth);
            }
        }
    }
    
}
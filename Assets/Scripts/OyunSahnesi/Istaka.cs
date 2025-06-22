using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Istaka : MonoBehaviour{
    public List<Cep> CepList = new List<Cep>();
    public static Istaka Instance;
    public List<Dictionary<int, GameObject>> FarkliMeyvePerleri = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniMeyvePerleri = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> FarkliMeyveAyniRenkPerleri = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniMeyveAyniRenkPerleri = new List<Dictionary<int, GameObject>>();
    public List<Dictionary<int, GameObject>> AyniMeyveFarkliRenkPerleri = new List<Dictionary<int, GameObject>>();
    public GameObject Body;
    private float posYrate = 0.66f;

    private bool _yeniGrupOlustur;

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;
        Body = transform.Find("Body").gameObject;
    }

    private void Start(){
        float y = Card.Instance.Size.y;
        transform.position = new Vector3(0, -y * posYrate, 0);
        CepleriOlustur();
    }

    public int DoluCepSayisi(){
        int doluCepSayisi = 0;
        foreach (var cepIns in CepList){
            //var cepScript = cep.GetComponent<IstakaCebi>();
            if (cepIns.Dolu){
                doluCepSayisi++;
            }
        }

        return doluCepSayisi;
    }

    public void CepleriOlustur(){
        float istakaGenisligi = Body.GetComponent<SpriteRenderer>().bounds.size.x;
        float aralikMesafesi = istakaGenisligi / GameManager.Instance.CepSayisi;
        for (int i = 0; i < GameManager.Instance.CepSayisi; i++){
            float x = (i * aralikMesafesi) + aralikMesafesi * .5f - istakaGenisligi * .5f;
            GameObject Cep = Resources.Load<GameObject>("Prefabs/IstakaCebi");
            var cep = Instantiate(Cep, new Vector3(x, Body.transform.position.y, -2), Quaternion.identity);
            cep.transform.localScale = new Vector3(aralikMesafesi, aralikMesafesi, -5);
            cep.transform.localScale *= .2f;
            cep.GetComponent<Cep>().colID = i;
            cep.transform.SetParent(Body.transform);
            CepList.Add(cep.GetComponent<Cep>());
        }
    }
 
    public void IstakayiVeCartiTemizle(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        float beklemeSuresi = 0.05f;

        foreach (var cardtakiTas in cardtakiTaslar){
            if (cardtakiTas is null) break;
            var CardtakiTas = TasManeger.Instance.TasInstances[cardtakiTas];
            StartCoroutine(CardtakiTas.BekleYokol(beklemeSuresi));
            beklemeSuresi += 0.05f;
        }

        foreach (var cepInstance in CepList){
            var CeptekiTas = cepInstance?.TasInstance;
            if (CeptekiTas is null) break;
            StartCoroutine(CeptekiTas.BekleYokol(beklemeSuresi));
            cepInstance.TasInstance = null;
            beklemeSuresi += 0.05f;
        }

         
    }

    public void TumTaslarinGostergesiniAc(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var cardtakiTas in cardtakiTaslar){
            if (cardtakiTas is null) break;
            var CardtakiTas = TasManeger.Instance.TasInstances[cardtakiTas];
            CardtakiTas.PersizIstakaTaslariGostergesi.SetActive(true);
            CardtakiTas.TiklanaBilir = false;
        }

        foreach (var cepInstance in CepList){
            if (cepInstance is null) break;
            var CeptekiTas = cepInstance.TasInstance;
            CeptekiTas?.PersizIstakaTaslariGostergesi.SetActive(true);
        }
    }
 
    public void PerlerListObjeleriTemizle(){
        // SiraliRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < FarkliMeyveAyniRenkPerleri.Count; i++){
            var grup = FarkliMeyveAyniRenkPerleri[i];
            foreach (var item in grup){
                var cepins = CepList[item.Key];
                //cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }

        // AyniRakamAyniRenkGruplari grupları temizle
        for (int i = 0; i < AyniMeyveAyniRenkPerleri.Count; i++){
            var grup = AyniMeyveAyniRenkPerleri[i];
            foreach (var item in grup){
                var cepins = CepList[item.Key];
                //cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }


        // AyniRakamHepsiFarkliRenkGruplari grupları temizle
        for (int i = 0; i < AyniMeyveFarkliRenkPerleri.Count; i++){
            var grup = AyniMeyveFarkliRenkPerleri[i];
            foreach (var item in grup){
                var cepins = CepList[item.Key];
                //cepins.Dolu = false;
                cepins.TasInstance = null;
            }
        }


        FarkliMeyveAyniRenkPerleri.Clear();
        AyniMeyveAyniRenkPerleri.Clear();
        AyniMeyveFarkliRenkPerleri.Clear();
        AyniMeyvePerleri.Clear();
        FarkliMeyvePerleri.Clear();
    }

    public void CeptekiYildizleriGizle(){
        foreach (var cep in CepList){
            cep.YildiziYak(0);
        }
    }

    public void PtaslariYoket(){ 
        float beklemeSuresi = .1f;
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar){
            foreach (var pTas in grup.Value.Taslar){ 
                pTas.cepInstance?.YildiziYak(0);
                beklemeSuresi += .1f;
                pTas.StartCoroutine(pTas.BekleYokol(beklemeSuresi));
            } 
        }
    }
}
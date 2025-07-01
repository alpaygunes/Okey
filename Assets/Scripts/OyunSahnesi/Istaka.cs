using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Istaka : MonoBehaviour{
    public List<Cep> CepList = new List<Cep>();
    public static Istaka Instance;
    //public bool CeptekiYildiziKontrolEtBayragi = false;
 
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
            if (cepIns.Dolu){
                doluCepSayisi++;
            }
        }

        return doluCepSayisi;
    }

    public void CepleriOlustur(){
        float istakaGenisligi = Body.GetComponent<SpriteRenderer>().bounds.size.x;
        float aralikMesafesi = istakaGenisligi / GameManager.Instance.CepSayisi;
        
        if (aralikMesafesi > istakaGenisligi / 6) {
            aralikMesafesi = istakaGenisligi / 6;
        }
        var toplamgenislik = GameManager.Instance.CepSayisi * aralikMesafesi;
        var fark = istakaGenisligi - toplamgenislik;
        for (int i = 0; i < GameManager.Instance.CepSayisi; i++){
            float x = (i * aralikMesafesi) + aralikMesafesi * .5f - istakaGenisligi * .5f;
            x += fark * .5f; // ortalama için farkın yarısıı ekle
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
            CardtakiTas.TiklanaBilir = false;
            StartCoroutine(CardtakiTas.BekleYokol(beklemeSuresi));
            beklemeSuresi += 0.1f;
        }

        foreach (var cepInstance in CepList){
            var CeptekiTas = cepInstance?.TasInstance;
            if (CeptekiTas is null) break;
            CeptekiTas.TiklanaBilir = false;
            StartCoroutine(CeptekiTas.BekleYokol(beklemeSuresi));
            cepInstance.TasInstance = null;
            beklemeSuresi += 0.1f;
        }

         
    }

    public void TumTaslarinGostergesiniAc(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS");
        foreach (var cardtakiTas in cardtakiTaslar){
            if (cardtakiTas is null) break;
            var CardtakiTas = TasManeger.Instance.TasInstances[cardtakiTas];
            CardtakiTas.PersizIstakaTaslariGostergesi.SetActive(true);
            //CardtakiTas.TiklanaBilir = false;
        }

        foreach (var cepInstance in CepList){
            if (cepInstance is null) break;
            var CeptekiTas = cepInstance.TasInstance;
            CeptekiTas?.PersizIstakaTaslariGostergesi.SetActive(true);
        }
    }
    
    public void PtaslariYoket(){ 
        float beklemeSuresi = .1f;
        foreach (var grup in PerKontrolBirimi.Instance.Gruplar){
            foreach (var pTas in grup.Value.Taslar){ 
                pTas.cepInstance?.YildiziYak(0);
                beklemeSuresi += .1f;
                pTas.TiklanaBilir = false;
                pTas.StartCoroutine(pTas.BekleYokol(beklemeSuresi));
            } 
        }
    }
    
    public void IlkBosCebiBelirt(){
        Cep ilkBosCep = null;
        foreach (var cep in CepList){
            cep.BosBelirteci.SetActive(false); 
            if (cep.Dolu == false && ilkBosCep == null){
                ilkBosCep = cep; 
            }
        } 
        if (ilkBosCep != null) ilkBosCep.BosBelirteci.SetActive(true);
    }

 


}
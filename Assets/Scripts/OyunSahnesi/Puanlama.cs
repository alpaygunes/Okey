using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Puanlama : MonoBehaviour{
    public static Puanlama Instance { get; private set; } 
    private Camera uiCamera;  
    private AudioSource _audioSource_puan_sayac; 
    List<Tas> BonusAyniRenkAyniMeyve = new List<Tas>();
    List<Tas> BonusFarkliRenkAyniMeyve = new List<Tas>();
    List<Tas> BonusAyniRenkArdisikMeyve = new List<Tas>();
    List<Tas> BonusFarkliRenkArdisikMeyve = new List<Tas>();
    public int HamleSayisi;
    public event Action<int> OnNetworkDataIicinPuanChanged;
    private int _toplamPuan;
    
    public int ToplamPuan
    {
        get => _toplamPuan;
        set
        {
            if (_toplamPuan != value)
            {
                _toplamPuan = value; 
                OnNetworkDataIicinPuanChanged?.Invoke(_toplamPuan); // Event tetikleniyor
            }
        }
    } 
    
    public SortedDictionary<int, GameObject> SiralanmisTumPerTaslari;
    
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;  
        uiCamera = Camera.main; 
        
        _audioSource_puan_sayac = gameObject.AddComponent<AudioSource>();
        _audioSource_puan_sayac.playOnAwake = false;
        _audioSource_puan_sayac.clip = Resources.Load<AudioClip>("Sounds/puan_sayac");
    }

    private void Start(){
        OnNetworkDataIicinPuanChanged += (_netwokrDataToplamPuan) => { 
            HamleSayisi++;
            var skor = ToplamPuan;
            var _hameleSayisi = HamleSayisi;
            string clientName = "Bos_clientName";
            if (LobbyManager.Instance){
                clientName = LobbyManager.Instance.myDisplayName; 
            } 
            SkorBoardiGuncelle();
            if (OyunKurallari.Instance){ 
                LimitleriKontrolEt();
                NetworkDataManager.Instance?.SkorVeHamleGuncelleServerRpc(skor,_hameleSayisi,clientName); 
            } 
            
        };
        // boş bir değişim yaratalım. skorlsitesine eklensin. Host un skor listesinde tekrar eden host clientName i çözmek için.
        NetworkDataManager.Instance?.SkorVeHamleGuncelleServerRpc(1,0,LobbyManager.Instance.myDisplayName);
        OyunSahnesiUI.Instance.KalanTasSayisi.text = GameManager.Instance.TasCount.ToString();
    }

    public void LimitleriKontrolEt(){
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
            if (HamleSayisi >= OyunKurallari.Instance.HamleLimit){
                GameManager.Instance.oyunDurumu = GameManager.OynanmaDurumu.bitti; 
                SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
            }
        }
        
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            if (GameManager.Instance.oyununBitimineKalanZaman<=0){
                GameManager.Instance.oyunDurumu = GameManager.OynanmaDurumu.bitti; 
                SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                if (NetworkDataManager.Instance.oyuncuListesi != null){
                    IEnumerator enumerator;
                    try{
                        enumerator = NetworkDataManager.Instance.SkorListesiniYavasGuncelle();
                        NetworkDataManager.Instance.skorListesiniYavasGuncelleCoroutine  
                            = NetworkDataManager.Instance.StartCoroutine(enumerator);
                    }
                    catch (Exception e){
                        Debug.Log($"Host ayrışmış olabilir . LimitleriKontrolEt HATA : {e.Message}"); 
                    }
                }

            }
        }
        
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap && SiralanmisTumPerTaslari != null){ 
            GorevYoneticisi.Instance.GorevYapildimiKontrolEt();
        }
    }
 
    public void IstakadakiPerdekiTaslariToparla(){
        
        BonusAyniRenkArdisikMeyve.Clear();
        BonusAyniRenkAyniMeyve.Clear();
        BonusFarkliRenkAyniMeyve.Clear();
        BonusFarkliRenkArdisikMeyve.Clear();
        
        SiralanmisTumPerTaslari = new SortedDictionary<int, GameObject>();
        Dictionary<int, GameObject> perdekiTumTaslarDic = new Dictionary<int, GameObject>(); 
        //SiraliRakamRenkGruplari sahneye diz 
        foreach (var grupList in Istaka.Instance.FarkliMeyveAyniRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusAyniRenkArdisikMeyve.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        }

        //AyniRakamAyniRenkGruplari sahneye diz 
        foreach (var grupList in Istaka.Instance.AyniMeyveAyniRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusAyniRenkAyniMeyve.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        }

        //AyniRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in Istaka.Instance.AyniMeyveFarkliRenkGruplari) {
            foreach (var item in grupList) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (grupList.Count > 2) {
                    BonusFarkliRenkAyniMeyve.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        } 
        
        SiralanmisTumPerTaslari = new SortedDictionary<int, GameObject>(perdekiTumTaslarDic); 
    }
    
    public void BonuslariVer(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS"); 
        if (BonusAyniRenkArdisikMeyve.Count > 2) {
            foreach (var tasInstance in BonusAyniRenkArdisikMeyve) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusAyniRenkArdisikMeyve.Count > 3) {
                        if (tasInstance.MeyveID == itemIstance.MeyveID || tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    else {
                        if (tasInstance.MeyveID == itemIstance.MeyveID) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                }
            }
        } 

        if (BonusAyniRenkAyniMeyve.Count > 2) {
            foreach (var tasInstance in BonusAyniRenkAyniMeyve) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (tasInstance.MeyveID == itemIstance.MeyveID || tasInstance.renk == itemIstance.renk) { 
                        tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                    }
                }
            }
        } 

        if (BonusFarkliRenkAyniMeyve.Count > 2) {
            foreach (var tasInstance in BonusFarkliRenkAyniMeyve) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusFarkliRenkAyniMeyve.Count > 3) {
                        if (tasInstance.MeyveID == itemIstance.MeyveID || tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    else {
                        if (tasInstance.MeyveID == itemIstance.MeyveID) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                }
            }
        }


        if (BonusFarkliRenkArdisikMeyve.Count > 2) {
            foreach (var tasInstance in BonusFarkliRenkArdisikMeyve) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusFarkliRenkArdisikMeyve.Count > 3) {
                        if (tasInstance.MeyveID == itemIstance.MeyveID || tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    else {
                        if (tasInstance.MeyveID == itemIstance.MeyveID) {
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                }
            }
        }
        
        // networke göndermek için animasyon vb gecikme leri beklemeden kaznılan net puan 
        int nwrkPuan = 0;
        foreach (var tas in SiralanmisTumPerTaslari){ 
            nwrkPuan += TasManeger.Instance.TasInstances[tas.Value].MeyveID; 
            foreach (var bonusTaslari in TasManeger.Instance.TasInstances[tas.Value].BonusOlarakEslesenTaslar){
                if (bonusTaslari.Value && !bonusTaslari.Value.NetworkDatayaEklendi){
                    bonusTaslari.Value.NetworkDatayaEklendi = true;
                    nwrkPuan += bonusTaslari.Value.MeyveID;
                }
            }
        } 
        ToplamPuan += nwrkPuan; 

        float gecikme = 0f;
        foreach (var tas in SiralanmisTumPerTaslari){ 
            StartCoroutine(TasManeger.Instance.TasInstances[tas.Value].TaslarinRakaminiPuanaEkle(gecikme));
            gecikme++;
        } 
        
        Istaka.Instance.GruplariTemizle(); 
    }
    
    public void Puanla(){ 
        if (      Istaka.Instance.FarkliMeyveAyniRenkGruplari.Count>0 
               || Istaka.Instance.AyniMeyveAyniRenkGruplari.Count>0
               || Istaka.Instance.AyniMeyveFarkliRenkGruplari.Count>0) {
                IstakadakiPerdekiTaslariToparla();
                BonuslariVer(); 
                PerIcinTasTavsiye.Instance.Sallanma(); 
        } 
        _ = Card.Instance.KarttakiPerleriBul(); 
    }

    public void SkorBoardiGuncelle(){ 
        OyunSahnesiUI.Instance.Skor.text = ToplamPuan.ToString();
        OyunSahnesiUI.Instance.HamleSayisi.text = HamleSayisi.ToString();
    }

    public void ButtonlaPuanlamaYap(){ 
        if (PuanlamaCounter.Instance.CountdownCoroutine != null){
            PuanlamaCounter.Instance.StopCoroutine(PuanlamaCounter.Instance.CountdownCoroutine);
        }
        
        if (   Istaka.Instance.FarkliMeyveAyniRenkGruplari.Count>0 
               || Istaka.Instance.AyniMeyveAyniRenkGruplari.Count>0
               || Istaka.Instance.AyniMeyveFarkliRenkGruplari.Count>0){ 
            Puanla();
        }
        else{
            Istaka.Instance.PersizFullIstakayiBosalt();
        }
        
    }
}
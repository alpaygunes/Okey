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
    List<Tas> BonusMeyveAyniRenkAyni = new List<Tas>();
    List<Tas> BonusMeyveAyniFarkliRenk = new List<Tas>();
    List<Tas> BonusMeyveFarkliRenkAyni = new List<Tas>();
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
                GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.bitti; 
                SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
            }
        }
        
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            if (GameManager.Instance.oyununBitimineKalanZaman<=0){
                GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.bitti; 
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
            GorevYoneticisi.Instance.GoreveUygunCepleriIsaretle(); 
            GorevYoneticisi.Instance.SiradakiGoreviSahnedeGoster();
        }
    }
 
    public void PerlerleriBelirginlestir(){
        
        BonusMeyveFarkliRenkAyni.Clear();
        BonusMeyveAyniRenkAyni.Clear();
        BonusMeyveAyniFarkliRenk.Clear(); 
        
        SiralanmisTumPerTaslari = new SortedDictionary<int, GameObject>();
        Dictionary<int, GameObject> perdekiTumTaslarDic = new Dictionary<int, GameObject>(); 
          
        foreach (var MfRa_Peri in Istaka.Instance.FarkliMeyveAyniRenkPerleri) {
            foreach (var item in MfRa_Peri) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (MfRa_Peri.Count > 2) {
                    BonusMeyveFarkliRenkAyni.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        }

         
        foreach (var MaRa_Peri in Istaka.Instance.AyniMeyveAyniRenkPerleri) {
            foreach (var item in MaRa_Peri) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (MaRa_Peri.Count > 2) {
                    BonusMeyveAyniRenkAyni.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        }

          
        foreach (var MaRf_Peri in Istaka.Instance.AyniMeyveFarkliRenkPerleri) {
            foreach (var item in MaRf_Peri) {
                if (!perdekiTumTaslarDic.ContainsKey(item.Key)) {
                    perdekiTumTaslarDic.Add(item.Key, item.Value);
                }

                if (MaRf_Peri.Count > 2) {
                    BonusMeyveAyniFarkliRenk.Add(TasManeger.Instance.TasInstances[item.Value]);
                }
            }
        } 
        
        SiralanmisTumPerTaslari = new SortedDictionary<int, GameObject>(perdekiTumTaslarDic); 
    }
    
    public void RengiVeMeyvesiUyumluKarttakiTaslariPuanla(){
        var cardtakiTaslar = GameObject.FindGameObjectsWithTag("CARDTAKI_TAS"); 
        
        // Meyveler Farklı Renkler Aynı   A  B  C  D  E 
        if (BonusMeyveFarkliRenkAyni.Count > 2) {
            foreach (var tasInstance in BonusMeyveFarkliRenkAyni) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusMeyveFarkliRenkAyni.Count == 3){ 
                        // bonus yok
                    }  
                    
                    if (BonusMeyveFarkliRenkAyni.Count == 4){ // R a M a 
                        if (tasInstance.MeyveID == itemIstance.MeyveID && tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    if (BonusMeyveFarkliRenkAyni.Count == 5){ // R a M * 
                        if (tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    if (BonusMeyveFarkliRenkAyni.Count >= 6){ // R * M *  
                        tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance); 
                    } 
                }
            }
        } 

        //  Meyve Aynı Renk Aynı  M M M M M (Aynı renk)
        if (BonusMeyveAyniRenkAyni.Count > 2) {
            foreach (var tasInstance in BonusMeyveAyniRenkAyni) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusMeyveAyniRenkAyni.Count == 3){ // R a M a 
                        if (tasInstance.MeyveID == itemIstance.MeyveID && tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    if (BonusMeyveAyniRenkAyni.Count == 4){ // R *  M a 
                        if (tasInstance.MeyveID == itemIstance.MeyveID) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    if (BonusMeyveAyniRenkAyni.Count == 5){ // R a M * 
                        if (tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    if (BonusMeyveAyniRenkAyni.Count >= 6){ // R *  M * 
                        tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance); 
                    }
                }
            }
        } 

        // Meyve Aynı Renk Farklı  M M M M M (farklı renk)
        if (BonusMeyveAyniFarkliRenk.Count > 2) {
            foreach (var tasInstance in BonusMeyveAyniFarkliRenk) {
                foreach (var item in cardtakiTaslar) {
                    var itemIstance = TasManeger.Instance.TasInstances[item];
                    if (BonusMeyveAyniFarkliRenk.Count == 3){ // R a M a 
                        if (tasInstance.MeyveID == itemIstance.MeyveID && tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    if (BonusMeyveAyniFarkliRenk.Count == 4){ // R * M a 
                        if (tasInstance.MeyveID == itemIstance.MeyveID) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    if (BonusMeyveAyniFarkliRenk.Count == 5){ // R a M * 
                        if (tasInstance.renk == itemIstance.renk) { 
                            tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance);
                        }
                    }
                    if (BonusMeyveAyniFarkliRenk.Count >= 6){ // R * M *  
                        tasInstance.BonusOlarakEslesenTaslar.Add(tasInstance.BonusOlarakEslesenTaslar.Count,itemIstance); 
                    }
                }
            }
        }

  
        // networke göndermek için animasyon vb gecikme leri beklemeden kaznılan net puan 
        int nwrkPuan = 0;
        foreach (var tas in SiralanmisTumPerTaslari){  
            nwrkPuan ++; 
            foreach (var bonusTaslari in TasManeger.Instance.TasInstances[tas.Value].BonusOlarakEslesenTaslar){
                if (bonusTaslari.Value && !bonusTaslari.Value.NetworkDatayaEklendi){
                    bonusTaslari.Value.NetworkDatayaEklendi = true; 
                    nwrkPuan ++;
                }
            }
        }
        ToplamPuan += nwrkPuan; 

        float gecikme = 0f;
        foreach (var tas in SiralanmisTumPerTaslari){
            var tasScript = TasManeger.Instance.TasInstances[tas.Value];
            tasScript.PereUyumluGostergesi.gameObject.SetActive(true);
            StartCoroutine(tasScript.TaslarinRakaminiPuanaEkle(gecikme));
            TasManeger.Instance.TasInstances[tas.Value].GoreveUygunsaKolonuIsaretle();
            gecikme++;
        } 
         
        Invoke(nameof(Deneme),gecikme); 
        Istaka.Instance.PerleriTemizle(); 
    }
    
    void Deneme(){
        GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.oynaniyor; 
    }
    
    public void Puanla(){ 
        if (Istaka.Instance.FarkliMeyveAyniRenkPerleri.Count>0 
               || Istaka.Instance.AyniMeyveAyniRenkPerleri.Count>0
               || Istaka.Instance.AyniMeyveFarkliRenkPerleri.Count>0) {
                GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.puanlamaYapiliyor;  
                PerlerleriBelirginlestir();
                RengiVeMeyvesiUyumluKarttakiTaslariPuanla(); 
                Card.Instance.Sallanma(); 
        } 
 
    }

    public void SkorBoardiGuncelle(){ 
        OyunSahnesiUI.Instance.Skor.text = ToplamPuan.ToString();
        OyunSahnesiUI.Instance.HamleSayisi.text = HamleSayisi.ToString();
    }

    public void ButtonlaPuanlamaYap(){ 
        if (PuanlamaCounter.Instance.CountdownCoroutine != null){
            PuanlamaCounter.Instance.StopCoroutine(PuanlamaCounter.Instance.CountdownCoroutine);
        }
        
        if (Istaka.Instance.FarkliMeyveAyniRenkPerleri.Count>0 
            || Istaka.Instance.AyniMeyveAyniRenkPerleri.Count>0
            || Istaka.Instance.AyniMeyveFarkliRenkPerleri.Count>0){ 
            Puanla();
        } else {
            Istaka.Instance.PersizFullIstakayiBosalt();
        }
        
    }
}
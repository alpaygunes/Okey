using Unity.Netcode;
using Unity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class GorevYoneticisi : NetworkBehaviour{ 
    public static GorevYoneticisi Instance{ get; private set; }
    private int SiradakiGorevSirasNosu = 0;
    private float posYrate = 0.7f;
    
    public struct TasData : INetworkSerializable, IEquatable<TasData>{
        public int Rakam;
        public Color32 Renk;

        public void NetworkSerialize<T>(BufferSerializer<T> ser) where T : IReaderWriter{
            ser.SerializeValue(ref Rakam);
            ser.SerializeValue(ref Renk);
        }

        public bool Equals(TasData other) =>
            Rakam == other.Rakam && Renk.Equals(other.Renk);

        public override bool Equals(object obj) =>
            obj is TasData other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Rakam, Renk);
    }

    public struct GorevData : INetworkSerializable, IEquatable<GorevData>{
        public byte TasSayisi;
        public FixedList128Bytes<TasData> Taslar; // 16 taş için kapasiteyi büyüttük

        public void NetworkSerialize<T>(BufferSerializer<T> ser) where T : IReaderWriter{
            ser.SerializeValue(ref TasSayisi);

            if (ser.IsReader)
                Taslar = new FixedList128Bytes<TasData>();

            for (int i = 0; i < TasSayisi; i++){
                if (ser.IsReader){
                    TasData td = default;
                    ser.SerializeValue(ref td);
                    Taslar.Add(td);
                }
                else{
                    var td = Taslar[i];
                    ser.SerializeValue(ref td);
                }
            }
        }

        // IEquatable<GorevData> uygulaması
        public bool Equals(GorevData other){
            if (TasSayisi != other.TasSayisi)
                return false;

            for (int i = 0; i < TasSayisi; i++){
                if (!Taslar[i].Equals(other.Taslar[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj) =>
            obj is GorevData other && Equals(other);

        public override int GetHashCode(){
            int hash = TasSayisi;
            for (int i = 0; i < TasSayisi; i++)
                hash = HashCode.Combine(hash, Taslar[i].GetHashCode());
            return hash;
        }
    }
    
    private NetworkList<GorevData> gorevlerNetList;
    
    private static readonly int GorevSayisi = 10;

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        } 
        Instance = this; 
        
        gorevlerNetList = new NetworkList<GorevData>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);
    }

    private void Start(){
        float y = Card.Instance.Size.y;
        transform.position = new Vector3(0, -y * posYrate, 0);
    }

    public override void OnNetworkSpawn(){
        
        if (OyunKurallari.Instance.GuncelOyunTipi != OyunKurallari.OyunTipleri.GorevYap){
            return;
        }
 
        if (IsServer){
            gorevlerNetList.Clear();
            GorevHazirla();
        }

        if (IsClient){
            SiradakiGoreviSahnedeGoster();
        }
    }
 
    //----------------------------- GÖREVLERİ OLUŞTURMA --------------------------------- 
    private enum PerTurleri{
        ArdisikRakamAyniRenk,
        ArdisikRakamFarkliRenk,
        AyniRakamFarkliRenkPerleri,
        AyniRakamAyniRenkPerleri,
    }

    private static void GorevHazirla(){
        for (int i = 0; i < GorevSayisi; i++) 
            switch (RastgelePerTuruSec())
            {
                case PerTurleri.ArdisikRakamAyniRenk:
                    ArdisikRakamAyniRenkPeriOlustur();
                    break;

                case PerTurleri.ArdisikRakamFarkliRenk:
                    ArdisikRakamFarkliRenkPeriOlustur();
                    break;

                case PerTurleri.AyniRakamFarkliRenkPerleri:
                    AyniRakamFarkliRenkPerleriOlustur();
                    break;

                case PerTurleri.AyniRakamAyniRenkPerleri:
                    AyniRakamAyniRenkPerleriOlustur();
                    break;

                default:
                    // Beklenmeyen durum (opsiyonel)
                    break;
            } 
    }
    
    private static PerTurleri RastgelePerTuruSec()
    {
        int elemanSayisi = Enum.GetValues(typeof(PerTurleri)).Length;
        int rastgeleIndex = UnityEngine.Random.Range(0, elemanSayisi);
        return (PerTurleri)rastgeleIndex;
    }
    
    private static void ArdisikRakamAyniRenkPeriOlustur(){
        GorevData gorev = new GorevData
        {
            Taslar = new FixedList128Bytes<TasData>()
        };

        int start = GameManager.Instance.RakamAraligi.start;
        int end = GameManager.Instance.RakamAraligi.end;
        //int araliktakiElemanSayisi = end - start + 1;
        int secilecekRakamSayisi = GameManager.Instance.CepSayisi;
        int rasgeleBaslangic = UnityEngine.Random.Range(start, end - secilecekRakamSayisi);
        var secilenSayilar = Enumerable.Range(rasgeleBaslangic, secilecekRakamSayisi).ToList();
        int renkStart = GameManager.Instance.RenkAraligi.start;
        int renkEnd = GameManager.Instance.RenkAraligi.end;
        Color32 color = Renkler.RenkSozlugu[UnityEngine.Random.Range(renkStart, renkEnd + 1)];
        foreach (int sayi in secilenSayilar){
            gorev.Taslar.Add(new TasData { Rakam = sayi, Renk = color });
        }

        gorev.TasSayisi = (byte)gorev.Taslar.Length;
        // Sadece sunucu yazabilir
        if (Instance != null && Instance.IsServer)
            Instance.gorevlerNetList.Add(gorev);
    }

    private static void ArdisikRakamFarkliRenkPeriOlustur(){
        GorevData gorev = new GorevData
        {
            Taslar = new FixedList128Bytes<TasData>()
        };

        // --- Ardışık rakamlar -------------------------------------------------
        int start = GameManager.Instance.RakamAraligi.start;
        int end = GameManager.Instance.RakamAraligi.end;
        //int aralik = end - start + 1;

        int secilecekRakamSayisi = GameManager.Instance.CepSayisi;
        int rasgeleBaslangic = UnityEngine.Random.Range(start, end - secilecekRakamSayisi);
        List<int> secilenSayilar = Enumerable.Range(rasgeleBaslangic, secilecekRakamSayisi).ToList();

        // --- Benzersiz renkler ------------------------------------------------
        int renkStart = GameManager.Instance.RenkAraligi.start;
        int renkEnd = GameManager.Instance.RenkAraligi.end;

        // 1) Aralıktaki tüm indeksleri havuza koy
        List<int> renkHavuzu = Enumerable.Range(renkStart, renkEnd - renkStart + 1).ToList();

        // 2) Fisher-Yates ile karıştır
        for (int i = renkHavuzu.Count - 1; i > 0; i--){
            int j = UnityEngine.Random.Range(0, i + 1);
            (renkHavuzu[i], renkHavuzu[j]) = (renkHavuzu[j], renkHavuzu[i]);
        }

        // 3) İstenen kadar rengi seç
        List<Color32> secilenRenkler = renkHavuzu
            .Take(secilecekRakamSayisi)
            .Select(idx => Renkler.RenkSozlugu[idx])
            .ToList();

        // --- Taşları oluştur --------------------------------------------------
        for (int i = 0; i < secilecekRakamSayisi; i++){
            gorev.Taslar.Add(new TasData
            {
                Rakam = secilenSayilar[i],
                Renk = secilenRenkler[i]
            });
        }

        gorev.TasSayisi = (byte)gorev.Taslar.Length;

        if (Instance != null && Instance.IsServer)
            Instance.gorevlerNetList.Add(gorev);
    }

    private static void AyniRakamFarkliRenkPerleriOlustur(){
        GorevData gorev = new GorevData
        {
            Taslar = new FixedList128Bytes<TasData>()
        };

        // --- Seçilecek taş sayısı ------------------------------------------
        int maxTasSayisi = GameManager.Instance.CepSayisi; // Üst sınır
        int tasSayisi = maxTasSayisi;

        // --- Rakam (hepsi aynı olacak) --------------------------------------
        int rakamStart = GameManager.Instance.RakamAraligi.start;
        int rakamEnd = GameManager.Instance.RakamAraligi.end;
        int secilenRakam = UnityEngine.Random.Range(rakamStart, rakamEnd + 1);

        // --- Benzersiz renkler ---------------------------------------------
        int renkStart = GameManager.Instance.RenkAraligi.start;
        int renkEnd = GameManager.Instance.RenkAraligi.end;

        // Aralıktaki tüm renk indekslerini havuza al
        List<int> renkHavuzu = Enumerable
            .Range(renkStart, renkEnd - renkStart + 1)
            .ToList();

        // Fisher–Yates karıştırması
        for (int i = renkHavuzu.Count - 1; i > 0; i--){
            int j = UnityEngine.Random.Range(0, i + 1);
            (renkHavuzu[i], renkHavuzu[j]) = (renkHavuzu[j], renkHavuzu[i]);
        }

        // Havuz yeterli değilse üst sınırı düşür
        tasSayisi = Mathf.Min(tasSayisi, renkHavuzu.Count);

        // --- Taşları oluştur -------------------------------------------------
        for (int i = 0; i < tasSayisi; i++){
            gorev.Taslar.Add(new TasData
            {
                Rakam = secilenRakam,
                Renk = Renkler.RenkSozlugu[renkHavuzu[i]]
            });
        }
        gorev.TasSayisi = (byte)gorev.Taslar.Length;

        // Sunucudaysak görevi listeye ekle
        if (Instance != null && Instance.IsServer)
            Instance.gorevlerNetList.Add(gorev);
    }

    private static void AyniRakamAyniRenkPerleriOlustur(){
        GorevData gorev = new GorevData
        {
            Taslar = new FixedList128Bytes<TasData>()
        };

        // --- Taş adedi ------------------------------------------------------
        int maxTasSayisi = GameManager.Instance.CepSayisi;
        int tasSayisi = maxTasSayisi;

        // --- Ortak rakam -----------------------------------------------------
        int rakamStart   = GameManager.Instance.RakamAraligi.start;
        int rakamEnd     = GameManager.Instance.RakamAraligi.end;
        int secilenRakam = UnityEngine.Random.Range(rakamStart, rakamEnd + 1);

        // --- Benzersiz renkler ---------------------------------------------
        int renkStart = GameManager.Instance.RenkAraligi.start;
        int renkEnd   = GameManager.Instance.RenkAraligi.end;

        // Havuz: seçilebilir tüm renk indeksleri
        List<int> renkHavuzu = Enumerable
            .Range(renkStart, renkEnd - renkStart + 1)
            .ToList();

        // Fisher-Yates ile karıştır
        for (int i = renkHavuzu.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (renkHavuzu[i], renkHavuzu[j]) = (renkHavuzu[j], renkHavuzu[i]);
        }

        // Havuzdaki renk sayısı yetersizse, taş sayısını düşür
        tasSayisi = Mathf.Min(tasSayisi, renkHavuzu.Count);

        // --- Taşları oluştur -------------------------------------------------
        for (int i = 0; i < tasSayisi; i++)
        {
            gorev.Taslar.Add(new TasData
            {
                Rakam = secilenRakam,
                Renk  = Renkler.RenkSozlugu[renkHavuzu[i]]
            });
        }

        gorev.TasSayisi = (byte)gorev.Taslar.Length;

        // Yalnızca sunucu ekler
        if (Instance != null && Instance.IsServer)
            Instance.gorevlerNetList.Add(gorev);
    } 
    // -----------------------------------  SON ---------------------------------------
 
    
    private void SiradakiGoreviSahnedeGoster(){
        var body = Instance.transform.Find("Body");
        if (body == null) return;
        // öncekileri temizle 
        for (int j = body.childCount - 1; j >= 0; j--) {
            var child = body.GetChild(j); 
            Destroy(child.gameObject);
        }
 
        
        GorevData gorev = gorevlerNetList[SiradakiGorevSirasNosu];
        FixedList128Bytes<TasData> taslar = gorev.Taslar;
        // tas nesnelerini tasList e ekle 
        int i = 0;
        foreach (var gorevTasi in taslar){ 
            // gorev gameobjectini kooordinatlarına göre sahneye yerleştir 
            float gorvePaneliGenisligi = body.GetComponent<SpriteRenderer>().bounds.size.x;
            float aralikMesafesi = gorvePaneliGenisligi / GameManager.Instance.CepSayisi;
            float x = (i * aralikMesafesi) + aralikMesafesi * .5f - gorvePaneliGenisligi * .5f;
            GameObject gTasPref = Resources.Load<GameObject>("Prefabs/gTas");
            var gTas = Instantiate(gTasPref, new Vector3(x, body.transform.position.y, -2), Quaternion.identity);
            int rakam = gorevTasi.Rakam;
            Color color = gorevTasi.Renk; 
            Sprite sprite = Resources.Load<Sprite>("Images/Rakamlar/" + rakam);
            gTas.transform.Find("RakamResmi").GetComponent<SpriteRenderer>().sprite = sprite; 
            gTas.transform.localScale = new Vector3(aralikMesafesi, aralikMesafesi, -1);
            gTas.transform.localScale *= .13f; 
            gTas.GetComponent<gTas>().rakam = rakam;
            gTas.GetComponent<gTas>().renk = color;
            gTas.transform.SetParent(body.transform);
            gTas.SetActive(true);
            gTas.tag = "gTas";
            gTas.name = "gTas" + i;  
            i++;
        }
    }

    public void GorevYapildimiKontrolEt(){
        var gorevTaslari = gorevlerNetList[SiradakiGorevSirasNosu].Taslar;
        var per = Puanlama.Instance.siralanmisTumPerTaslari;

        int i = 0;
        foreach (var pTs in per){
            if (gorevTaslari.Length==i) break;
            if (pTs.Value==null) break;
            var pTas = pTs.Value.GetComponent<Tas>();
            var gTas = gorevTaslari[i];
            if (pTas.rakam != gTas.Rakam && pTas.renk != gTas.Renk){
                Debug.Log($"pTas.rakam {pTas.rakam}  pTas.renk {pTas.renk}");
            }
            i++;
        }
        
        SiradakiGorevSirasNosu++;
        SiradakiGoreviSahnedeGoster();
        OyunSahnesiUI.Instance.GorevSayisiLbl.text = SiradakiGorevSirasNosu +"/"+OyunKurallari.Instance.GorevYap.ToString();  
        if (SiradakiGorevSirasNosu >= OyunKurallari.Instance.GorevYap){
            GameManager.Instance.oyunDurumu = GameManager.OynanmaDurumu.bitti; 
            SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
        }
    }
    
}
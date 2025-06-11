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
    private float posYrate = 0.74f;
    
    public struct TasData : INetworkSerializable, IEquatable<TasData>{
        public int MeyveID;
        public Color32 Renk;

        public void NetworkSerialize<T>(BufferSerializer<T> ser) where T : IReaderWriter{
            ser.SerializeValue(ref MeyveID);
            ser.SerializeValue(ref Renk);
        }

        public bool Equals(TasData other) =>
            MeyveID == other.MeyveID && Renk.Equals(other.Renk);

        public override bool Equals(object obj) =>
            obj is TasData other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(MeyveID, Renk);
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
        FarkliMeyveAyniRenk, 
        AyniMeyveFarkliRenkPerleri,
        AyniMeyveAyniRenkPerleri,
    }

    private static void GorevHazirla(){
        int renkStart = GameManager.Instance.RenkAraligi.start;
        int renkEnd = GameManager.Instance.RenkAraligi.end;
        int renkSayisi = renkEnd - renkStart + 1;
        
        int meyveStart = GameManager.Instance.MeyveIDAraligi.start;
        int meyveEnd = GameManager.Instance.MeyveIDAraligi.end;
        int meyveSayisi = meyveEnd - meyveStart + 1;
        for (int i = 0; i < GorevSayisi; i++) 
            switch (RastgelePerTuruSec())
            {
                case PerTurleri.FarkliMeyveAyniRenk:
                    if (GameManager.Instance.CepSayisi <= meyveSayisi){
                        FarkliMeyveAyniRenkPeriOlustur();
                    }
                    else{
                        AyniMeyveAyniRenkPerleriOlustur();
                    }
                    break;  
                case PerTurleri.AyniMeyveFarkliRenkPerleri:
                    if (GameManager.Instance.CepSayisi <= renkSayisi){
                        AyniMeyveFarkliRenkPerleriOlustur();
                    } else{
                        AyniMeyveAyniRenkPerleriOlustur(); 
                    }
                    break; 
                case PerTurleri.AyniMeyveAyniRenkPerleri:
                    AyniMeyveAyniRenkPerleriOlustur();
                    break; 
            } 
    }
    
    private static PerTurleri RastgelePerTuruSec()
    {
        int elemanSayisi = Enum.GetValues(typeof(PerTurleri)).Length;
        int rastgeleIndex = UnityEngine.Random.Range(0, elemanSayisi);
        return (PerTurleri)rastgeleIndex;
    }
    
    private static void FarkliMeyveAyniRenkPeriOlustur(){
        GorevData gorev = new GorevData
        {
            Taslar = new FixedList128Bytes<TasData>()
        };

        int start = GameManager.Instance.MeyveIDAraligi.start;
        int end = GameManager.Instance.MeyveIDAraligi.end - GameManager.Instance.CepSayisi; 
        int rasgeleBaslangic = UnityEngine.Random.Range(start, end);
        var secilenSayilar = Enumerable.Range(rasgeleBaslangic, GameManager.Instance.CepSayisi).ToList();
        
        int renkStart = GameManager.Instance.RenkAraligi.start;
        int renkEnd = GameManager.Instance.RenkAraligi.end;
        Color32 color = Renkler.RenkSozlugu[UnityEngine.Random.Range(renkStart, renkEnd + 1)];
        foreach (int sayi in secilenSayilar){
            gorev.Taslar.Add(new TasData { MeyveID = sayi, Renk = color });
        }

        gorev.TasSayisi = (byte)gorev.Taslar.Length;
        // Sadece sunucu yazabilir
        if (Instance != null && Instance.IsServer)
            Instance.gorevlerNetList.Add(gorev);
    }
    
    private static void AyniMeyveFarkliRenkPerleriOlustur(){
        GorevData gorev = new GorevData
        {
            Taslar = new FixedList128Bytes<TasData>()
        };

        // --- Seçilecek taş sayısı ------------------------------------------
        int maxTasSayisi = GameManager.Instance.CepSayisi; // Üst sınır
        int tasSayisi = maxTasSayisi;

        // --- Rakam (hepsi aynı olacak) --------------------------------------
        int meyveStart = GameManager.Instance.MeyveIDAraligi.start;
        int meyveEnd = GameManager.Instance.MeyveIDAraligi.end;
        int secilenRakam = UnityEngine.Random.Range(meyveStart, meyveEnd + 1);

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
                MeyveID = secilenRakam,
                Renk = Renkler.RenkSozlugu[renkHavuzu[i]]
            });
        }
        gorev.TasSayisi = (byte)gorev.Taslar.Length;

        // Sunucudaysak görevi listeye ekle
        if (Instance != null && Instance.IsServer)
            Instance.gorevlerNetList.Add(gorev);
    }

    private static void AyniMeyveAyniRenkPerleriOlustur(){
        GorevData gorev = new GorevData
        {
            Taslar = new FixedList128Bytes<TasData>()
        };

        // --- Taş adedi ------------------------------------------------------
        int maxTasSayisi = GameManager.Instance.CepSayisi;
        int tasSayisi = maxTasSayisi;

        // --- Ortak rakam -----------------------------------------------------
        int rakamStart   = GameManager.Instance.MeyveIDAraligi.start;
        int rakamEnd     = GameManager.Instance.MeyveIDAraligi.end;
        int secilenMeyve = UnityEngine.Random.Range(rakamStart, rakamEnd + 1);

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

        // --- Taşları oluştur -------------------------------------------------
        for (int i = 0; i < tasSayisi; i++)
        {
            gorev.Taslar.Add(new TasData
            {
                MeyveID = secilenMeyve,
                Renk  = Renkler.RenkSozlugu[renkHavuzu[0]]
            });
        }

        gorev.TasSayisi = (byte)gorev.Taslar.Length;

        // Yalnızca sunucu ekler
        if (Instance != null && Instance.IsServer)
            Instance.gorevlerNetList.Add(gorev);
    } 
    // -----------------------------------  SON ---------------------------------------
    
    public void SiradakiGoreviSahnedeGoster(){
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
            gTas.transform.localScale = new Vector3(aralikMesafesi, aralikMesafesi, -1); 
            gTas.GetComponent<gTas>().meyveID = gorevTasi.MeyveID;
            gTas.GetComponent<gTas>().renk = gorevTasi.Renk;
            gTas.transform.SetParent(body.transform);
            gTas.SetActive(true);
            gTas.tag  = "gTas";
            gTas.name = "gTas" + i;
            gTas.GetComponent<gTas>().colID = i;
            i++;
        }
    }
    
    public void GoreveUygunCepleriIsaretle(){
        var gorevTaslari = gorevlerNetList[SiradakiGorevSirasNosu].Taslar;
        var per = Puanlama.Instance.SiralanmisTumPerTaslari;

        int i = 0;
        foreach (var pTs in per){
            if (gorevTaslari.Length==i) break;
            if (pTs.Value==null) break;
            var pTas = pTs.Value.GetComponent<Tas>();
            var gTas = gorevTaslari[i];
            if (pTas.MeyveID == gTas.MeyveID && pTas.renk == gTas.Renk){
                pTas.GorevleUyum = 2;
            }else if (pTas.MeyveID == gTas.MeyveID || pTas.renk == gTas.Renk){ 
                pTas.GorevleUyum = 1;
            }
            i++;
        }
        
        SiradakiGorevSirasNosu++;
        OyunSahnesiUI.Instance.GorevSayisiLbl.text = SiradakiGorevSirasNosu +"/"+OyunKurallari.Instance.GorevYap.ToString();  
        if (SiradakiGorevSirasNosu >= OyunKurallari.Instance.GorevYap){
            GameManager.Instance.OyunDurumu = GameManager.OynanmaDurumu.bitti; 
            SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
        }
    }

    public void TasGoreveUygunsaYildiziYak(Tas tas){
        var cepScript = tas.cepInstance;
        if (OyunKurallari.Instance.GuncelOyunTipi != OyunKurallari.OyunTipleri.GorevYap) return;
        GameObject[] gorevTaslari = GameObject.FindGameObjectsWithTag("gTas");
        var gTas = gorevTaslari[cepScript.colID];
        int uyumSayisi = 0;
        if (gTas.GetComponent<gTas>().renk == tas.renk){ 
            uyumSayisi++;
        }
        if (gTas.GetComponent<gTas>().meyveID == tas.MeyveID){ 
            uyumSayisi++;
        }
        cepScript.YildiziYak(uyumSayisi); 
    }
}
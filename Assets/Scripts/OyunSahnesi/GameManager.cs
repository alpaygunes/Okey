using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour{
    public readonly int _colonCount = 6;
    public readonly int BaslangicTasSayisi = 100;
    public readonly int CepSayisi = 6;
    public readonly RangeInt RenkAraligi = new RangeInt(0, 6);
    public readonly RangeInt MeyveIDAraligi = new RangeInt(0, 8);
    public string seed;
    public static GameManager Instance{ get; private set; }
    public int oyununBitimineKalanZaman = 0; // OyunKurallari.Instance.ZamanLimitin den alacak
    public OyunDurumlari oyunDurumu;
    public Coroutine OyununBitimiIcinGeriSayRoutineCoroutin = null;
    public int YeniTasEklendiSayisi = 0;
    public bool OyunSahnesiKapaniyor{ get; set; } = false;
    public int CanSayisi{ get; set; } = 10;

    public enum OyunDurumlari{
        LimitDoldu,
        DevamEdiyor,
        DegerlendirmeYapiliyor,
    }

    void Awake(){
        if (MainMenu.isSoloGame){
            OyunKurallari.Instance.InitializeSettings();
        }

        oyunDurumu = OyunDurumlari.DevamEdiyor;
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start(){
        seed = "A";
        // solo
        if (MainMenu.isSoloGame){
            seed = MainMenu.GetRandomSeed();
        }
        // multy
        else if (LobbyManager.Instance){ 
            seed = LobbyManager.Instance.gameSeed;
        }
 
        Card.Instance.CreateSpawnHoles();
        TasManeger.Instance.TaslariOlustur();
        Card.Instance.KutulariHazirla();

        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            OyununBitimiIcinGeriSayRoutineCoroutin = StartCoroutine(OyununBitimiIcinGeriSayRoutine());
        }
        else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
            // solo ise
            if (MainMenu.isSoloGame){
                GorevYoneticisi.GorevHazirla();
                GorevYoneticisi.Instance.SiradakiGoreviIstakadaGoster();
            }
            // Multi ise GorevYoneticisi OnNetworkSpawn() olunca tetiklenir.
        }
        else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
            // puanlama sırasında gereken kotrol yapılıyor.
        }

        OyunSahnesiUI.Instance.KalanTasSayisi.text = TasManeger.Instance.TasList.Count.ToString();
        OyunSahnesiUI.Instance.CanSayisi.text = CanSayisi.ToString();
        PuanlamaIStatistikleri.Sifirla();
    }

    private IEnumerator OyununBitimiIcinGeriSayRoutine(){
        if (OyunKurallari.Instance){
            oyununBitimineKalanZaman = OyunKurallari.Instance.ZamanLimiti;
        }

        while (oyununBitimineKalanZaman > 0){
            OyunSahnesiUI.Instance.GeriSayim.text = oyununBitimineKalanZaman.ToString();
            yield return new WaitForSeconds(1f);
            oyununBitimineKalanZaman--;
        }

        OyunSahnesiUI.Instance.GeriSayim.text = "0";
        if (OyununBitimiIcinGeriSayRoutineCoroutin != null){
            StopCoroutine(OyununBitimiIcinGeriSayRoutineCoroutin);
            OyununBitimiIcinGeriSayRoutineCoroutin = null;
        }

        IsaretleBelirtYoket.Instance.LimitleriKontrolEt();
    }

    public void DugmeYadaOtomatikDegerlendirme(){
        // per VAR sa
        if (PerKontrolBirimi.Instance.Gruplar.Count > 0){
            if (Istaka.Instance.DoluCepSayisi() == CepSayisi){
                IsaretleBelirtYoket.Instance.Degerlendir();
            }
            else{
                OyunSahnesiUI.Instance.degerlendirmeYap.style.display = DisplayStyle.Flex;
            }
        }
        // per YOK ama istaka full.
        else if (Istaka.Instance.DoluCepSayisi() == CepSayisi){
            CanSayisi--; 
            IsaretleBelirtYoket.Instance.HamleSayisi++;  
            OyunSahnesiUI.Instance.CanSayisi.text = CanSayisi.ToString();
            Istaka.Instance.TumTaslarinGostergesiniAc();
            Istaka.Instance.IstakayiVeCartiTemizle();
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){ 
                GorevYoneticisi.Instance.SiradakiGorevSiraNosu++;
                GorevYoneticisi.Instance.SiradakiGoreviIstakadaGoster();
                if (GorevYoneticisi.Instance.SiradakiGorevSiraNosu >= OyunKurallari.Instance.GorevLimit){
                    oyunDurumu = OyunDurumlari.LimitDoldu;
                    SceneManager.LoadScene("OyunSonu", LoadSceneMode.Additive);
                    OyunSahnesiKapaniyor = true;
                }
            }
        } 
        PuanlamaIStatistikleri.SayilariGuncelle(); 
        PuanlamaIStatistikleri.UIgucelle(); 
        IsaretleBelirtYoket.Instance.LimitleriKontrolEt();
    }

    void Update(){
        if (oyunDurumu == OyunDurumlari.LimitDoldu) return;
        // tıklama algılama
        #if UNITY_ANDROID || UNITY_IOS
                                    if (Input.touchCount > 0)
                                    {
                                        Touch touch = Input.GetTouch(0);
                                        if (touch.phase == TouchPhase.Began)
                                        {
                                            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touch.position);
                                            TiklamaTuslamaKontrol(worldPoint);
                                        }
                                    }
        #else
                if (Input.GetMouseButtonDown(0)){
                    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    TiklamaTuslamaKontrol(worldPoint);
                }
        #endif
 
        oyunDurumu = Card.Instance.TiklanamazTasVar()
                    ? OyunDurumlari.DegerlendirmeYapiliyor
                    : OyunDurumlari.DevamEdiyor;
        
        Istaka.Instance.CeptekiYildiziKontrolEtBayragi = Card.Instance.TiklanamazTasVar();
         
    }

    void TiklamaTuslamaKontrol(Vector2 worldPoint){
        if (oyunDurumu != OyunDurumlari.DevamEdiyor) return;
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.collider != null){
            if (hit.collider.gameObject.CompareTag("CARDTAKI_TAS")){
                var tasInstance = TasManeger.Instance.TasInstances[hit.collider.gameObject];
                if (!tasInstance.TiklanaBilir) return;
                var yerlestimi = tasInstance.BosCebeYerles();
                if (yerlestimi){
                    if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
                        GorevYoneticisi.Instance.CeplerinYidiziniGuncelle();
                    } 
                    PerKontrolBirimi.Instance.ParseEt(Istaka.Instance.CepList);
                    PerKontrolBirimi.Instance.PerdekiTaslariBelirt();
                    DugmeYadaOtomatikDegerlendirme(); 
                } 
            }
        }
    }
}


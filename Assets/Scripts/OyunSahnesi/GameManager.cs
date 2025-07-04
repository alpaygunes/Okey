using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour {
    public int ColonCount = 6;
    public readonly int BaslangicTasSayisi = 200;
    public int CepSayisi = 6;
    public RangeInt RenkAraligi = new RangeInt(0, 10);
    public RangeInt MeyveAraligi = new RangeInt(0, 14);
    public string Seed;
    public static GameManager Instance { get; private set; }
    public int oyununBitimineKalanZaman = 0; // OyunKurallari.Instance.ZamanLimitin den alacak
    private OyunDurumlari _oyunDurumu;

    public OyunDurumlari OyunDurumu {
        get => _oyunDurumu;
        set {
            if (_oyunDurumu != value) {
                _oyunDurumu = value;
                OyunDurumuDegisti();
                Istaka.Instance.IlkBosCebiBelirt();
            }
        }
    }

    public Coroutine OyununBitimiIcinGeriSayRoutineCoroutin = null;
    public int YeniTasEklendiSayisi = 0;
    public bool OyunSahnesiKapaniyor { get; set; } = false;
    public int CanSayisi { get; set; } = 10;

    public enum OyunDurumlari {
        LimitDoldu,
        DevamEdiyor,
        DegerlendirmeYapiliyor,
    }

    void Awake() {
        if (MainMenu.isSoloGame) {
            OyunKurallari.Instance.InitializeSettings();
        }

        _oyunDurumu = OyunDurumlari.DevamEdiyor;
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        } 
        Instance = this;

        if (MainMenu.isSoloGame) {
            LevelManager.Init();
        }
    }

    private void Start() {
        Seed = "A";
        // solo
        if (MainMenu.isSoloGame) {
            Seed = MainMenu.GetRandomSeed(); 
        }
        // multy
        else if (LobbyManager.Instance) {
            Seed = LobbyManager.Instance.gameSeed;
        }

        Card.Instance.CreateSpawnHoles();
        TasManeger.Instance.TaslariOlustur();
        Card.Instance.KutulariHazirla();

        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli) {
            OyununBitimiIcinGeriSayRoutineCoroutin = StartCoroutine(OyununBitimiIcinGeriSayRoutine());
        }
        else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap) {
            // solo ise
            if (MainMenu.isSoloGame) {
                GorevYoneticisi.GorevHazirla();
                GorevYoneticisi.Instance.SiradakiGoreviIstakadaGoster();
            }

            OyununBitimiIcinGeriSayRoutineCoroutin = StartCoroutine(OyununBitimiIcinGeriSayRoutine());
        }
        else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli) {
            // puanlama sırasında gereken kotrol yapılıyor.
        }

        OyunSahnesiUI.Instance.KalanTasSayisi.text = TasManeger.Instance.TasList.Count.ToString();
        OyunSahnesiUI.Instance.CanSayisi.text = CanSayisi.ToString();
        PuanlamaIStatistikleri.Sifirla();
    }

    private void OyunDurumuDegisti() {
        if (OyunDurumu == OyunDurumlari.DevamEdiyor) {
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap) {
                GorevYoneticisi.Instance.CeplerinYidiziniGuncelle();
            }
        }
    }

    private IEnumerator OyununBitimiIcinGeriSayRoutine() {
        if (OyunKurallari.Instance) {
            oyununBitimineKalanZaman = OyunKurallari.Instance.ZamanLimiti;
        }

        while (oyununBitimineKalanZaman > 0) {
            OyunSahnesiUI.Instance.GeriSayim.text = oyununBitimineKalanZaman.ToString();
            yield return new WaitForSeconds(1f);
            oyununBitimineKalanZaman--;
        }

        OyunSahnesiUI.Instance.GeriSayim.text = "0";
        if (OyununBitimiIcinGeriSayRoutineCoroutin != null) {
            StopCoroutine(OyununBitimiIcinGeriSayRoutineCoroutin);
            OyununBitimiIcinGeriSayRoutineCoroutin = null;
        }

        IsaretleBelirtYoket.Instance.LimitleriKontrolEt();
    }

    public void DugmeYadaOtomatikDegerlendirme() {
        // per VAR sa
        if (PerKontrolBirimi.Instance.Gruplar.Count > 0) {
            if (Istaka.Instance.DoluCepSayisi() == CepSayisi) {
                IsaretleBelirtYoket.Instance.Degerlendir();
            }
            else {
                OyunSahnesiUI.Instance.DegerlendirmeYap.style.display = DisplayStyle.Flex;
            }
        }
        // per YOK ama istaka full.
        else if (Istaka.Instance.DoluCepSayisi() == CepSayisi) {
            CanSayisi--;
            IsaretleBelirtYoket.Instance.HamleSayisi++;
            OyunSahnesiUI.Instance.CanSayisi.text = CanSayisi.ToString();
            Istaka.Instance.TumTaslarinGostergesiniAc();
            Istaka.Instance.IstakayiVeCartiTemizle();
            if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap) {
                GorevYoneticisi.Instance.SiradakiGorevSiraNosu++;
                GorevYoneticisi.Instance.SiradakiGoreviIstakadaGoster();
                GorevYoneticisi.Instance.GorevLimitiKontrolu();
            }
        }

        PuanlamaIStatistikleri.SayilariGuncelle();
        PuanlamaIStatistikleri.UIgucelle();
        IsaretleBelirtYoket.Instance.LimitleriKontrolEt();
    }

    void Update() {
        if (OyunDurumu == OyunDurumlari.LimitDoldu) return;
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
        if (Input.GetMouseButtonDown(0)) {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TiklamaTuslamaKontrol(worldPoint);
        }
#endif

        OyunDurumu = Card.Instance.TiklanamazTasVar()
            ? OyunDurumlari.DegerlendirmeYapiliyor
            : OyunDurumlari.DevamEdiyor;
    }

    void TiklamaTuslamaKontrol(Vector2 worldPoint) {
        if (OyunDurumu != OyunDurumlari.DevamEdiyor) return;
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.collider != null) {
            if (hit.collider.gameObject.CompareTag("CARDTAKI_TAS")) {
                var tasInstance = TasManeger.Instance.TasInstances[hit.collider.gameObject];
                if (!tasInstance.TiklanaBilir) return;
                var yerlestimi = tasInstance.BosCebeYerles();
                if (yerlestimi) {
                    if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap) {
                        GorevYoneticisi.Instance.CeplerinYidiziniGuncelle();
                    }
                    Card.Instance.Sallanma();
                    PerKontrolBirimi.Instance.ParseEt(Istaka.Instance.CepList);
                    PerKontrolBirimi.Instance.PerdekiTaslariBelirt();
                    DugmeYadaOtomatikDegerlendirme();
                }
            }
        }

        Istaka.Instance.IlkBosCebiBelirt();
    }
}
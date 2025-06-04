using System.Collections;
using System.Collections.Generic; 
using UnityEngine; 

public class GameManager : MonoBehaviour{
    public readonly float PuanlamaIcinGeriSayimSuresi = 100f; // sadece bir rakam. sn değil.
    public readonly int _colonCount = 5;
    public readonly int TasCount = 27;
    public readonly int CepSayisi = 5;
    public readonly RangeInt RenkAraligi = new RangeInt(1, 10);
    public readonly RangeInt RakamAraligi = new RangeInt(1, 4);
    public List<GameObject> spawnHolesList = new List<GameObject>();
    public string seed;
    public bool PerKontrolDugmesiOlsun ;
    public bool OtomatikPerkontrolu ;
    public static GameManager Instance{ get; private set; }
    public int oyununBitimineKalanZaman=0;  
    public OynanmaDurumu oyunDurumu;
    public Coroutine OyununBitimiIcinGeriSayRoutineCoroutin = null;
    public enum OynanmaDurumu{
        bitti,
        oynaniyor,
    }
    
    void Awake(){ 
        oyunDurumu = OynanmaDurumu.oynaniyor; 
        if (Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start(){
        if (LobbyManager.Instance){
            seed = LobbyManager.Instance.gameSeed;
        } else{ 
            seed = "1234";
        }

        CreateSpawnHoles();
        TasManeger.Instance.TaslariHazirla();
        Card.Instance.KutulariHazirla();
        
        ///////////////// younun türüne göre sürecin devamına karar veriliyor. ///////////////////////
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            OyununBitimiIcinGeriSayRoutineCoroutin = StartCoroutine(OyununBitimiIcinGeriSayRoutine());
        }else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){ 
            //GorevYoneticisi OnNetworkSpawn() olunca tetiklenir.
        }else if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.HamleLimitli){
            // puanlama sırasında gereken kotrol yapılıyor.
        } 
    }
    
    private void CreateSpawnHoles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / _colonCount;
        for (int i = 0; i < _colonCount; i++){
            float holePositionX = i * colonWidth - cardSize.x * .5f;
            float holePositionY = cardSize.y * .5f + colonWidth;
            holePositionX += colonWidth * .5f;
            GameObject SpawnHole = Resources.Load<GameObject>("Prefabs/SpawnHole");
            var sh = Instantiate(SpawnHole, new Vector3(holePositionX, holePositionY, -0.2f), Quaternion.identity);
            sh.transform.localScale = new Vector2(colonWidth, colonWidth);
            spawnHolesList.Add(sh);
        }
    }
    
    private IEnumerator OyununBitimiIcinGeriSayRoutine(){
        oyununBitimineKalanZaman = 120;
        if (OyunKurallari.Instance){
            oyununBitimineKalanZaman = OyunKurallari.Instance.ZamanLimiti; 
        }
        while (oyununBitimineKalanZaman > 0){
            OyunSahnesiUI.Instance.GeriSayim.text = oyununBitimineKalanZaman.ToString();
            yield return new WaitForSeconds(1f);
            oyununBitimineKalanZaman--;
        } 
        OyunSahnesiUI.Instance.GeriSayim.text = "0";
        if (OyununBitimiIcinGeriSayRoutineCoroutin!=null){
            StopCoroutine(OyununBitimiIcinGeriSayRoutineCoroutin);
            OyununBitimiIcinGeriSayRoutineCoroutin = null;
        } 
        Puanlama.Instance.LimitleriKontrolEt();
    }

    void Update(){
        if (oyunDurumu == OynanmaDurumu.bitti) return;
        
        #if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touch.position);
                    RaycastVeIslem(worldPoint);
                }
            }
        #else
                if (Input.GetMouseButtonDown(0)){
                    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastVeIslem(worldPoint);
                }
        #endif 
                void RaycastVeIslem(Vector2 worldPoint){
                    RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                    if (hit.collider != null){
                        if (hit.collider.gameObject.CompareTag("CARDTAKI_TAS")){
                            TasManeger.Instance.TasInstances[hit.collider.gameObject].BosCebeYerles();
                            PerIcinTasTavsiye.Instance.Basla();
                        }
                    }
                }
    }
    
}
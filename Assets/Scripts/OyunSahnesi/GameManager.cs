using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour{
    public readonly float PuanlamaIcinGeriSayimSuresi = 100f; // sadece bir rakam. sn değil.
    private readonly int _colonCount = 5;
    public readonly int TasCount = 100;
    public readonly int CepSayisi = 5;
    public readonly RangeInt RenkAraligi = new RangeInt(1, 20);
    public readonly RangeInt RakamAraligi = new RangeInt(1, 4);
    public List<GameObject> spawnHolesList = new List<GameObject>();
    public string seed;
    [NonSerialized] public bool PerKontrolDugmesiOlsun = true;
    [NonSerialized] public bool OtomatikPerkontrolu = true;
    public static GameManager Instance{ get; private set; }
    public int OyununBitimineKalanZaman=0;  
    public OynanmaDurumu oyunDurumu;
    public Coroutine OyununBitimiIcinGeriSayRoutineCoroutin = null;
    public enum OynanmaDurumu{
        bitti,
        oynaniyor,
    }
    
    void Awake(){
        oyunDurumu = OynanmaDurumu.oynaniyor; 
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this;
    }

    private void Start(){
        if (LobbyManager.Instance){
            seed = LobbyManager.Instance.gameSeed;
        }
        else{ 
            seed = "1234";
        }

        CreateSpawnHoles();
        TasManeger.Instance.TaslariHazirla();
        KutulariHazirla();
        if (OyunKurallari.Instance.GuncelOyunTipi==OyunKurallari.OyunTipleri.ZamanLimitli){
            OyununBitimiIcinGeriSayRoutineCoroutin = StartCoroutine(OyununBitimiIcinGeriSayRoutine());
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

    private void KutulariHazirla(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = (cardSize.x / _colonCount);
        GameObject kutu_ = Resources.Load<GameObject>("Prefabs/Kutu");
        float satirSayisi = (cardSize.y / colonWidth);
        for (var satir = satirSayisi; satir > 0; satir--){
            for (int sutun = 0; sutun < _colonCount; sutun++){
                float positionX = (colonWidth * .5f) + (sutun * colonWidth) - cardSize.x * .5f;
                float positionY = -(cardSize.y * .5f) + colonWidth * 0.5f + ((satirSayisi - satir) * colonWidth);
                var kutu = Instantiate(kutu_, new Vector3(positionX, positionY, -0.01f), Quaternion.identity);
                kutu.transform.localScale = new Vector2(colonWidth, colonWidth);
            }
        }
    }
    
    private IEnumerator OyununBitimiIcinGeriSayRoutine(){
        OyununBitimineKalanZaman = 120;
        if (OyunKurallari.Instance){
            OyununBitimineKalanZaman = OyunKurallari.Instance.ZamanLimiti; 
        }
        while (OyununBitimineKalanZaman > 0)
        {
            OyunSahnesiUI.Instance.GeriSayim.text = OyununBitimineKalanZaman.ToString();
            yield return new WaitForSeconds(1f);
            OyununBitimineKalanZaman--;
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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlatformManager : MonoBehaviour{
    public GameObject card;
    private readonly int _colonCount = 4;
    internal readonly int TasCount = 50;
    public List<GameObject> spawnHolesList = new List<GameObject>();

    public static PlatformManager Instance{ get; private set; }

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start(){
        CreateSpawnHoles();
        TasManeger.Instance.TaslariHazirla(); 
        KutulariHazirla();
    }

 

    private void CreateSpawnHoles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / _colonCount;
        for (int i = 0; i < _colonCount; i++)
        {
            float holePositionX = i * colonWidth - cardSize.x * .5f;
            float holePositionY = cardSize.y * .5f + colonWidth;
            holePositionX += colonWidth * .5f;
            GameObject SpawnHole = Resources.Load<GameObject>("Prefabs/SpawnHole");
            var sh = Instantiate(SpawnHole, new Vector3(holePositionX, holePositionY, -0.2f), Quaternion.identity);
            sh.transform.localScale = new Vector2(colonWidth,colonWidth);
            spawnHolesList.Add(sh);
        }
    }
    
    
    private void KutulariHazirla(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / _colonCount; 
        GameObject Kutu = Resources.Load<GameObject>("Prefabs/Kutu");
        float satirSayisi = cardSize.y / colonWidth;
        for (int satir = 0; satir < satirSayisi; satir++){
            for (int sutun = 0; sutun < _colonCount; sutun++){
                float positionX = (colonWidth * .5f) + (sutun * colonWidth) - cardSize.x * .5f;
                float positionY = (cardSize.y * .5f) - (satir * cardSize.y/satirSayisi) - (cardSize.y/satirSayisi)*.5f;  
                var kutu = Instantiate(Kutu, new Vector3(positionX, positionY, -0.01f), Quaternion.identity);
                kutu.transform.localScale = new Vector2(colonWidth,colonWidth); 
            }
        } 
    }

    void Update(){
        if (Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began){
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero); 
                if (hit.collider != null){
                    if (hit.collider.gameObject.CompareTag("TAS")){
                        TasManeger.Instance.TasIstances[hit.collider.gameObject].BosCebeYerles();
                    }
                }
            }
        }
    }

 
}
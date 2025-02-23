using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlatformManager : MonoBehaviour{
    public GameObject card;
    private int colonCount = 5;
    internal int kareCount = 50;
    public List<GameObject> SpawnHolesList = new List<GameObject>();

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
        IstakayiHazirla();
        KareManeger.Instance.KareleriHazirla();
    }

    private void IstakayiHazirla(){
        
    }

    private void CreateSpawnHoles(){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / colonCount;
        for (int i = 0; i < colonCount; i++){
            float holePositionX = i * colonWidth - cardSize.x * .5f;
            float holePositionY = cardSize.y * .5f;
            holePositionX += colonWidth * .5f;
            GameObject SpawnHole = Resources.Load<GameObject>("Prefabs/SpawnHole");
            var sh = Instantiate(SpawnHole, new Vector3(holePositionX, holePositionY, -0.01f), Quaternion.identity);
            sh.transform.localScale = new Vector3(colonWidth,colonWidth)*0.95f;
            SpawnHolesList.Add(sh);
        }
    }

    void Update(){
        if (Input.touchCount > 0){
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began){
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero); 
                if (hit.collider != null){
                    if (hit.collider.gameObject.CompareTag("KARE")){
                        KareyeDokunuldu(hit.collider.gameObject);
                    }
                }
            }
        }
    }

    private void KareyeDokunuldu(GameObject dokunulanKare){
       Istaka.Instance.BosCebeYerleştir(dokunulanKare);
    }
}
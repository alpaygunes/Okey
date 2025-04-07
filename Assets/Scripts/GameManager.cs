using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour{ 
   
     public readonly float GeriSayimSuresi = 5f;
     private readonly int _colonCount = 5;
     public readonly int TasCount = 100;
     public readonly int CepSayisi = 5;
     public readonly RangeInt RenkAraligi = new RangeInt(1, 20);
     public readonly RangeInt RakamAraligi = new RangeInt(1, 10);
     public List<GameObject> spawnHolesList = new List<GameObject>();
     public string Seed;
     public int HamleSayisi = 0;
     [NonSerialized]  public bool PerKontrolDugmesiOlsun = true;
     [NonSerialized]  public bool OtomatikPerkontrolu = false;

    public static GameManager Instance{ get; private set; }

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        } 
        Instance = this; 
    }

    private void Start(){
        Seed  = PlayerPrefs.GetString("Seed"); 
        CreateSpawnHoles();
        TasManeger.Instance.TaslariHazirla();
        KutulariHazirla();
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
        GameObject Kutu = Resources.Load<GameObject>("Prefabs/Kutu");
        float satirSayisi = (cardSize.y / colonWidth);
        for (var satir = satirSayisi; satir > 0; satir--){
            for (int sutun = 0; sutun < _colonCount; sutun++){
                float positionX = (colonWidth * .5f) + (sutun * colonWidth) - cardSize.x * .5f;
                float positionY = -(cardSize.y * .5f) + colonWidth * 0.5f + ((satirSayisi - satir) * colonWidth);
                var kutu = Instantiate(Kutu, new Vector3(positionX, positionY, -0.01f), Quaternion.identity);
                kutu.transform.localScale = new Vector2(colonWidth, colonWidth);
            }
        }
    }
}
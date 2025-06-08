using System;
using System.Collections.Generic;
using UnityEngine; 

public class TasManeger : MonoBehaviour{
    public List<GameObject> TasList = new List<GameObject>();
    public Dictionary<GameObject, Tas> TasInstances = new Dictionary<GameObject, Tas>();
    public static TasManeger Instance{ get; private set; }
    
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        }

        Instance = this; 
    }
  
    public void TaslariHazirla(){
        List<Data> generatedData = GenerateDataList(GameManager.Instance.seed); 
        foreach (var data in generatedData){
            GameObject tare = Resources.Load<GameObject>("Prefabs/Tas");
            var Tas = Instantiate(tare, new Vector3(0, 0, 5), Quaternion.identity); 
            var tasScribe = Tas.GetComponentInChildren<Tas>(); 
            tasScribe.MeyveID = data.number;
            tasScribe.renk = Renkler.RenkSozlugu[data.color];
            TasList.Add(Tas);
        } 
    }
 
    public void PerleriKontrolEt(){
        Istaka.Instance.FarkliMeyveGruplariniBelirle();
        Istaka.Instance.AyniMeyveGruplariniBelirle();
        Istaka.Instance.FarkliMeyveGruplriIcindeAyniRenkGruplariniBelirle();
        Istaka.Instance.AyniMeyveGruplarinIcindekiAyniRenkGruplariniBelirle();
        Istaka.Instance.AyniMeyveGruplarinIcindekiFarkliRenkGruplariniBelirle(); 
        PuanlamaCounter.Instance.PuanlamaIcinGeriSayimAnimasyonunuBaslat(); 
    }
    
    public class Data{
        public int number;
        public int color;

        public Data(int number, int color){
            this.number = number;
            this.color = color;
        }
    }

    public static List<Data> GenerateDataList(string seed){
        System.Random random = new System.Random(seed.GetHashCode());
        List<Data> dataList = new List<Data>();

        for (int i = 0; i < GameManager.Instance.TasCount; i++){
            int number = random.Next(GameManager.Instance.MeyveIDAraligi.start,  GameManager.Instance.MeyveIDAraligi.end );
            int color = random.Next(GameManager.Instance.RenkAraligi.start, GameManager.Instance.RenkAraligi.end);
            dataList.Add(new Data(number, color));
        } 
        return dataList;
    }
    

}
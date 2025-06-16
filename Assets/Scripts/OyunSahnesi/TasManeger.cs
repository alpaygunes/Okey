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
  
    public void TaslariOlustur(){
        List<Data> generatedData = GenerateDataList(); 
        foreach (var data in generatedData){
            GameObject tare = Resources.Load<GameObject>("Prefabs/Tas");
            var Tas = Instantiate(tare, new Vector3(0, 0, 5), Quaternion.identity); 
            var tasScribe = Tas.GetComponentInChildren<Tas>(); 
            tasScribe.MeyveID = data.number;
            tasScribe.Renk = Renkler.RenkSozlugu[data.color];
            TasList.Add(Tas);
        }
    }
    
    public class Data{
        public int number;
        public int color;

        public Data(int number, int color){
            this.number = number;
            this.color = color;
        }
    }

    public static List<Data> GenerateDataList(){
        var seed = GameManager.Instance.seed;
        string SubSeed = seed.Substring(GameManager.Instance.YeniTasEklendiSayisi, 1);
        GameManager.Instance.YeniTasEklendiSayisi++; 
        if (GameManager.Instance.YeniTasEklendiSayisi == seed.Length){
            GameManager.Instance.YeniTasEklendiSayisi = 0;
        }
        Debug.Log($"seed {seed} SubSeed {SubSeed}");
        
        System.Random random = new System.Random(SubSeed.GetHashCode());
        List<Data> dataList = new List<Data>();

        for (int i = 0; i < GameManager.Instance.BaslangicTasSayisi; i++){
            int number = random.Next(GameManager.Instance.MeyveIDAraligi.start,  GameManager.Instance.MeyveIDAraligi.end );
            int color = random.Next(GameManager.Instance.RenkAraligi.start, GameManager.Instance.RenkAraligi.end);
            dataList.Add(new Data(number, color));
        }
        return dataList;
    }
    
}
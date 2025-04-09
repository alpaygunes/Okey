using System;
using System.Collections.Generic;
using UnityEngine; 

public class TasManeger : MonoBehaviour{
    public List<GameObject> TasList = new List<GameObject>();
    public Dictionary<GameObject, Tas> TasInstances = new Dictionary<GameObject, Tas>();
    public static TasManeger Instance{ get; private set; }
    

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }

        Instance = this;
        
       
    }
 

    public void TaslariHazirla(){
        List<Data> generatedData = GenerateDataList(GameManager.Instance.Seed);

        foreach (var data in generatedData){ 
            GameObject tare = Resources.Load<GameObject>("Prefabs/Tas");
            var Tas = Instantiate(tare, new Vector3(0, 0, -1), Quaternion.identity);
            var rakam = data.number;
            Color color = Renkler.RenkSozlugu[data.color];
            Sprite sprite = Resources.Load<Sprite>("Images/Rakamlar/" + rakam);
            Tas.transform.Find("RakamResmi").GetComponent<SpriteRenderer>().sprite = sprite;
            Tas.GetComponentInChildren<Tas>().rakam = rakam;
            Tas.GetComponentInChildren<Tas>().renk = color;
            TasList.Add(Tas);
        } 
    }



    public void PerleriKontrolEt(){
        Istaka.Instance.SiraliGruplariBelirle();
        Istaka.Instance.BenzerRakamGruplariniBelirle();
        Istaka.Instance.SiraliGruplarinIcindekiAyniRenkGruplariniBelirle();
        Istaka.Instance.AyniRakamGruplarinIcindekiAyniRenkGruplariniBelirle();
        Istaka.Instance.AyniRakamGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle();
        Istaka.Instance.SiraliGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle();
        Counter.Instance.GeriSayimAnimasyonunuBaslat(); 
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
            int number = random.Next(GameManager.Instance.RakamAraligi.start,  GameManager.Instance.RakamAraligi.end );
            int color = random.Next(GameManager.Instance.RenkAraligi.start, GameManager.Instance.RenkAraligi.end);
            dataList.Add(new Data(number, color));
        }

        return dataList;
    }
}
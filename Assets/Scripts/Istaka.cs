using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Istaka : MonoBehaviour
{ 
    public int CepSayisi = 10;
    public List<GameObject> CepList = new List<GameObject>();
    public Dictionary<int,GameObject> Taslar = new Dictionary<int,GameObject>(); 
    public Dictionary<int,int> TasinRakami = new Dictionary<int,int>(); 
    public static Istaka Instance{ get; private set; }
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }
        Instance = this; 
    }

    private void Start(){
        float istakaGenisligi = transform.GetComponent<SpriteRenderer>().bounds.size.x; 
        float aralikMesafesi = istakaGenisligi / CepSayisi;
        for (int i = 0; i < CepSayisi; i++){
            float x = (i * aralikMesafesi) + aralikMesafesi*.5f - istakaGenisligi * .5f;
            GameObject Cep = Resources.Load<GameObject>("Prefabs/IstakaCebi");
            var cep = Instantiate(Cep, new Vector3(x, transform.position.y,-2), Quaternion.identity); 
            Vector2 cepSize = cep.GetComponent<SpriteRenderer>().bounds.size;
            cep.transform.SetParent(PlatformManager.Instance.transform);
            CepList.Add(cep);
        }
    }

    public void BosCebeYerleştir(GameObject dokunulanTas){
        Vector2 cardSize = Card.Instance.Size;
        float colonWidth = cardSize.x / CepSayisi;
        for (var i = 0; i < CepList.Count; i++){
            var cep = CepList[i];
            var cepScript = cep.GetComponent<IstakaCebi>();
            if (cepScript.Dolu == false){
                dokunulanTas.transform.position = cep.transform.position;
                dokunulanTas.transform.position += new Vector3(0, 0, -1);
                Destroy(dokunulanTas.GetComponent<Rigidbody2D>());
                Destroy(dokunulanTas.GetComponent<Collider2D>());
                dokunulanTas.transform.localScale = new Vector3(colonWidth,colonWidth)*0.9f;
                cepScript.Dolu = true;
                Taslar.Add(i,dokunulanTas);
                TasinRakami.Add(i,dokunulanTas.GetComponent<Tas>().rakam);
                Counter.Instance.StartCountdown();
                break;
            }
        }
    }


}

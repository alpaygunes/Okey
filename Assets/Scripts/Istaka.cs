using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Istaka : MonoBehaviour
{
    public int CepSayisi { get; set; } = 10;
    public Dictionary<int,IstakaCebi> CepList = new Dictionary<int,IstakaCebi>();
    public Dictionary<GameObject, GameObject> CepVeTas = new Dictionary<GameObject, GameObject>();
    public Dictionary<GameObject, Tas> TasInstances = new Dictionary<GameObject, Tas>();
     
    public static Istaka Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        cepleriOlustur();
    }

    // Cepleri oluştur
    void cepleriOlustur()
    {
        float istakaGenisligi = transform.GetComponent<SpriteRenderer>().bounds.size.x;
        float aralikMesafesi = istakaGenisligi / CepSayisi; 
        for (int i = 0; i < CepSayisi; i++)
        {
            float x = (i * aralikMesafesi) + aralikMesafesi * .5f - istakaGenisligi * .5f;
            GameObject Cep = Resources.Load<GameObject>("Prefabs/IstakaCebi");
            var cep = Instantiate(Cep, new Vector3(x, transform.position.y, -2), Quaternion.identity); 
            cep.transform.localScale = new Vector3(aralikMesafesi, aralikMesafesi, -1);
            cep.transform.localScale *= .7f;
            cep.transform.SetParent(PlatformManager.Instance.transform); 
            CepList.Add(i,cep.GetComponent<IstakaCebi>());
        }
    }
}
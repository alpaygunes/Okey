using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Istaka : MonoBehaviour
{
    public int CepSayisi { get; set; } = 4;
    public List<GameObject> CepList = new List<GameObject>();
    public Dictionary<int, GameObject> Taslar = new Dictionary<int, GameObject>();
    public Dictionary<int, int> TasinRakami = new Dictionary<int, int>();
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
            CepList.Add(cep);
        }
    }
}
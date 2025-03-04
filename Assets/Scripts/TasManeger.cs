using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TasManeger : MonoBehaviour{
    public List<GameObject> TasList = new List<GameObject>();
    private RangeInt _renkAraligi = new RangeInt(1, 20);
    private RangeInt _rakamAraligi = new RangeInt(1, 10);
    public Dictionary<GameObject, Tas> TasIstances = new Dictionary<GameObject, Tas>();
    public static TasManeger Instance { get; private set; }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void TaslariHazirla() {
        for (int i = 0; i < PlatformManager.Instance.TasCount; i++) {
            GameObject tare = Resources.Load<GameObject>("Prefabs/Tas");
            var Tas = Instantiate(tare, new Vector3(0, 0, -1), Quaternion.identity);
            var rakam = Random.Range(_rakamAraligi.start, _rakamAraligi.end);
            Color color = RenklerYonetimi.RenkSozlugu[Random.Range(_renkAraligi.start, _renkAraligi.end + 1)];
            //Tas.transform.Find("Zemin").transform.GetComponent<Renderer>().material.color = color;
            Tas.GetComponentInChildren<TextMeshPro>().text = rakam.ToString();
            Tas.GetComponentInChildren<Tas>().rakam = rakam;
            Tas.GetComponentInChildren<Tas>().renk = color;
            TasList.Add(Tas);
        }
    }
}
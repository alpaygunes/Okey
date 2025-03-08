using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TasManeger : MonoBehaviour{
    public List<GameObject> TasList = new List<GameObject>();
    private RangeInt _renkAraligi = new RangeInt(1, 37);
    private RangeInt _rakamAraligi = new RangeInt(1, 10);
    public Dictionary<GameObject, Tas> TasIstances = new Dictionary<GameObject, Tas>();
    public static TasManeger Instance { get; private set; }
    

    void Awake(){
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void TaslariHazirla(){
        for (int i = 0; i < PlatformManager.Instance._tasCount; i++) {
            GameObject tare = Resources.Load<GameObject>("Prefabs/Tas");
            var Tas = Instantiate(tare, new Vector3(0, 0, -1), Quaternion.identity);
            var rakam = Random.Range(_rakamAraligi.start, _rakamAraligi.end);
            Color color = RenklerYonetimi.RenkSozlugu[Random.Range(_renkAraligi.start, _renkAraligi.end + 1)];
            Sprite sprite = Resources.Load<Sprite>("Images/Rakamlar/" + rakam);
            Tas.transform.Find("RakamResmi").GetComponent<SpriteRenderer>().sprite = sprite;
            Tas.GetComponentInChildren<Tas>().rakam = rakam;
            Tas.GetComponentInChildren<Tas>().renk = color;
            TasList.Add(Tas);
        }
    }
    
    void Update() {
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touch.position);
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                if (hit.collider != null) {
                    if (hit.collider.gameObject.CompareTag("CARDTAKI_TAS")) {
                        TasIstances[hit.collider.gameObject].BosCebeYerles();
                        PerleriKontrolEt(); 
                    }
                }
            }
        }
    }
    
    private void PerleriKontrolEt()
    {
        IstakaKontrolcu.Instance.SiraliGruplariBelirle();
        IstakaKontrolcu.Instance.BenzerRakamGruplariniBelirle();
        IstakaKontrolcu.Instance.SiraliGruplarinIcindekiRenkGruplariniBelirle();
        IstakaKontrolcu.Instance.AyniRakamGruplarinIcindekiRenkGruplariniBelirle();
        IstakaKontrolcu.Instance.AyniRakamGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle();
        IstakaKontrolcu.Instance.SiraliGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle();
    }
}
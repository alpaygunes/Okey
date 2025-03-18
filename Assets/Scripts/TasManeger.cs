using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TasManeger : MonoBehaviour{
    public List<GameObject> TasList = new List<GameObject>(); 
    public Dictionary<GameObject, Tas> TasInstances = new Dictionary<GameObject, Tas>();
    public static TasManeger Instance { get; private set; }
    

    void Awake(){
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void TaslariHazirla(){
        for (int i = 0; i < GameManager.Instance.TasCount; i++) {
            GameObject tare = Resources.Load<GameObject>("Prefabs/Tas");
            var Tas = Instantiate(tare, new Vector3(0, 0, -1), Quaternion.identity);
            var rakam = Random.Range(GameManager.Instance.RakamAraligi.start, GameManager.Instance.RakamAraligi.end);
            Color color = Renkler.RenkSozlugu[Random.Range(GameManager.Instance.RenkAraligi.start, GameManager.Instance.RenkAraligi.end + 1)];
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
                if (hit.collider) {
                    if (hit.collider.gameObject.CompareTag("CARDTAKI_TAS")) {
                        TasInstances[hit.collider.gameObject].BosCebeYerles();
                        PerleriKontrolEt(); 
                        PerKontrol.IstakaKontrol();
                    }
                }
            }
        }
    }
    
    private void PerleriKontrolEt()
    {
        Istaka.Instance.SiraliGruplariBelirle();
        Istaka.Instance.BenzerRakamGruplariniBelirle();
        Istaka.Instance.SiraliGruplarinIcindekiAyniRenkGruplariniBelirle();
        Istaka.Instance.AyniRakamGruplarinIcindekiAyniRenkGruplariniBelirle();
        Istaka.Instance.AyniRakamGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle();
        Istaka.Instance.SiraliGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle();
    }
}
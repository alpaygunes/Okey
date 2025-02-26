using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PuanlamaKontrolcu : MonoBehaviour{
    public static PuanlamaKontrolcu Instance{ get; private set; }
    private Vector2 size;
    public int Puan = 0;
    private TextMeshProUGUI Skor;

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start(){
        gameObject.SetActive(false);
        transform.Find("Zemin").transform.localScale = Card.Instance.transform.localScale;
        transform.Find("Zemin").transform.position = Card.Instance.transform.position;
        transform.Find("Zemin").transform.position += new Vector3(0, 0, -0.1f);
        Skor = GameObject.Find("Skor").GetComponent<TextMeshProUGUI>();
    }

    public void PuanlamaYap(){
        Puan = 1;
        gameObject.SetActive(true);
        var satir = 0;
        // önceki puanlamadan kalan ATIL_TAS temizle  
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ATIL_TAS")) {
            Destroy(obj); 
        }
        
        //SiraliGruplar sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliGruplar){
            break; // puanlamaya dahil etme
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            float ortayCekmePayi = tasSize.x * grupList.Count;
            foreach (var item in grupList){
                float x = tasSize.x * sayac + tasSize.x * .5f - ortayCekmePayi * .5f;
                float y = tasSize.y * satir;
                item.Value.tag = "ATIL_TAS";
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                Puan *= Istaka.Instance.TasinRakami[item.Key];
                sayac++;
            }
        }

        //BenzerRakamGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.BenzerRakamGruplari){
            break; // puanlamaya dahil etme
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            float ortayCekmePayi = tasSize.x * grupList.Count;
            foreach (var item in grupList){
                float x = tasSize.x * sayac + tasSize.x * .5f - ortayCekmePayi * .5f;
                float y = tasSize.y * satir;
                item.Value.tag = "ATIL_TAS";
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                Puan *= Istaka.Instance.TasinRakami[item.Key];
                sayac++;
            }
        }

        //SiraliRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamAyniRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            float ortayCekmePayi = tasSize.x * grupList.Count;
            foreach (var item in grupList){
                float x = tasSize.x * sayac + tasSize.x * .5f - ortayCekmePayi * .5f;
                float y = tasSize.y * satir;
                item.Value.tag = "ATIL_TAS";
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                Puan *= Istaka.Instance.TasinRakami[item.Key];
                sayac++;
            }
        }

        //AyniRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamAyniRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            float ortayCekmePayi = tasSize.x * grupList.Count;
            foreach (var item in grupList){
                float x = tasSize.x * sayac + tasSize.x * .5f - ortayCekmePayi * .5f;
                float y = tasSize.y * satir;
                item.Value.tag = "ATIL_TAS";
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                Puan *= Istaka.Instance.TasinRakami[item.Key];
                sayac++;
            }
        }

        //AyniRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamHepsiFarkliRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            float ortayCekmePayi = tasSize.x * grupList.Count;
            foreach (var item in grupList){
                float x = tasSize.x * sayac + tasSize.x * .5f - ortayCekmePayi * .5f;
                float y = tasSize.y * satir;
                item.Value.tag = "ATIL_TAS";
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                Puan *= Istaka.Instance.TasinRakami[item.Key];
                sayac++;
            }
        }

        //SiraliRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamHepsiFarkliRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            float ortayCekmePayi = tasSize.x * grupList.Count;
            foreach (var item in grupList){
                float x = tasSize.x * sayac + tasSize.x * .5f - ortayCekmePayi * .5f;
                float y = tasSize.y * satir; 
                item.Value.tag = "ATIL_TAS";
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                Puan *= Istaka.Instance.TasinRakami[item.Key];
                sayac++;
            }
        }

        IstakaKontrolcu.Instance.GruplariTemizle();
        StartCoroutine(WaitAndExecute());
        Skor.text = Puan.ToString();
    }

    IEnumerator WaitAndExecute(){
        yield return new WaitForSeconds(7f);
        gameObject.SetActive(false);
    }
}
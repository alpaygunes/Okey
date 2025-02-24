using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PuanlamaKontrolcu : MonoBehaviour{
    public static PuanlamaKontrolcu Instance{ get; private set; }
    private Vector2 size;

    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start(){
        //size = GameObject.Find("Zemin").GetComponent<SpriteRenderer>().bounds.size; 
        gameObject.SetActive(false);
        transform.Find("Zemin").transform.localScale = Card.Instance.transform.localScale;
        transform.Find("Zemin").transform.position = Card.Instance.transform.position;
        transform.Find("Zemin").transform.position += new Vector3(0, 0, -0.1f);
    }

    public void PuanlamaYap(){
        gameObject.SetActive(true);
        var satir = 0;
        //SiraliGruplar sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliGruplar){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            foreach (var item in grupList){
                float x = tasSize.x * sayac;
                float y = tasSize.y * satir;
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                sayac++;
            }
        }

        //BenzerRakamGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.BenzerRakamGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            foreach (var item in grupList){
                float x = tasSize.x * sayac;
                float y = tasSize.y * satir;
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                sayac++;
            } 
        }

        //SiraliRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.SiraliRakamRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            foreach (var item in grupList){
                float x = tasSize.x * sayac;
                float y = tasSize.y * satir;
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                sayac++;
            }
        }

        //AyniRakamRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            foreach (var item in grupList){
                float x = tasSize.x * sayac;
                float y = tasSize.y * satir;
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                sayac++;
            }
        }

        //AyniRakamHepsiFarkliRenkGruplari sahneye diz 
        foreach (var grupList in IstakaKontrolcu.Instance.AyniRakamHepsiFarkliRenkGruplari){
            var sayac = 0;
            satir++;
            GameObject tas = grupList.First().Value;
            Vector2 tasSize = tas.GetComponent<SpriteRenderer>().bounds.size;
            foreach (var item in grupList){
                float x = tasSize.x * sayac;
                float y = tasSize.y * satir;
                Instantiate(item.Value, new Vector3(x, y, -2), Quaternion.identity, transform);
                sayac++;
            }
        }

        //IstakaKontrolcu.Instance.GruplariTemizle();
        StartCoroutine(WaitAndExecute());
    }

    IEnumerator WaitAndExecute(){
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
}
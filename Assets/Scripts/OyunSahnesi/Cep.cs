using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Cep : MonoBehaviour{
    public bool Dolu = false;
    public Tas TasInstance = null;
    public int colID;
    private GameObject yildiz1;
    private GameObject yildiz2;
    public GameObject BosBelirteci;

    private void Start(){
        yildiz1 = transform.Find("yildiz1").gameObject;
        yildiz2 = transform.Find("yildiz2").gameObject;
        BosBelirteci = transform.Find("BosBelirteci").gameObject;
        yildiz1.SetActive(false);
        yildiz2.SetActive(false);
        BosBelirteci.SetActive(false);
    }

    public void YildiziYak(int uyumSayisi){
        if (uyumSayisi == 1){
            yildiz1.SetActive(true);
        }else if (uyumSayisi == 2){
            yildiz1.SetActive(true);
            yildiz2.SetActive(true);
        }
        else{
            yildiz1.SetActive(false);
            yildiz2.SetActive(false);
        }
    } 
}

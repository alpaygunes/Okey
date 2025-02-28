using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Tas : MonoBehaviour{
    public int Rakam = 0;
    public Color Renk ;
    public float animasyonSuresi = .5f;


    private void Awake(){ 
        gameObject.SetActive(false); 
    }

    private void Start(){ 
        GetComponentInChildren<TextMeshPro>().color = Renk;
    }

    public void merkezeKay(float gecikme){
        StartCoroutine(WaitAndExecute(gecikme));
        
    }

    IEnumerator WaitAndExecute(float gecikme){
        yield return new WaitForSeconds(gecikme);
        Vector3 ilkScale = transform.localScale;
        transform.DOMove(new Vector3(0, 0, 0), animasyonSuresi);
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(transform.DOScale(transform.localScale * 4, animasyonSuresi * .5f))
            .Append(transform.DOScale(ilkScale*2, animasyonSuresi * .5f));
        StartCoroutine(KillSelf());
        PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan *= Rakam;
        PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuanTMP.text = PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan.ToString();
        
        
        
    }

    IEnumerator KillSelf(){
        yield return new WaitForSeconds(animasyonSuresi);
        transform.DOKill();
        Destroy(this.gameObject);
    }
}
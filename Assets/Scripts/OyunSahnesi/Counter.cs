using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Counter : MonoBehaviour{
    public static Counter Instance;
    private float puanlamaIcinGeriSayimSuresi; 
    public Coroutine CountdownCoroutine; 

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        } 
        Instance = this; 
    }

    private void Start(){ 
        puanlamaIcinGeriSayimSuresi = GameManager.Instance.GeriSayimSuresi; 
    }

    private void OnDestroy(){
        if (CountdownCoroutine!=null){
            StopCoroutine(CountdownCoroutine);
        }
    }

    public void PuanlamaIcinGeriSayimAnimasyonunuBaslat(){ 
        if (!GameManager.Instance.OtomatikPerkontrolu) return;
        
        if (   Istaka.Instance.SiraliRakamAyniRenkGruplari.Count>0 
               || Istaka.Instance.AyniRakamAyniRenkGruplari.Count>0
               || Istaka.Instance.AyniRakamHepsiFarkliRenkGruplari.Count>0
               || Istaka.Instance.SiraliRakamHepsiFarkliRenkGruplari.Count>0){ 
                if (CountdownCoroutine != null){
                    StopCoroutine(CountdownCoroutine);
                }
                CountdownCoroutine = StartCoroutine(PuanlamaGerisayimProgresBariniIslet());
        }
        else{
            Istaka.Instance.PersizFullIstakayiBosalt();
        }
    }

    private IEnumerator PuanlamaGerisayimProgresBariniIslet(){ 
        float _timeLeft = puanlamaIcinGeriSayimSuresi;
        while (_timeLeft > 0){
            OyunSahnesiUI.Instance.GeriSayimBari.value = (puanlamaIcinGeriSayimSuresi-_timeLeft)/puanlamaIcinGeriSayimSuresi * 100;
            yield return new WaitForSeconds(.05f); // 0.05 sn beklerse 100 birimi 5 sn de tamamlar.
            _timeLeft--;
        }
        Puanlama.Instance.Puanla();
        OyunSahnesiUI.Instance.GeriSayimBari.value = 0;

        StopCoroutine(CountdownCoroutine);
        CountdownCoroutine = null;      // sadece referansı temizle
        yield break;                     // kendiliğinden sonlanır
    }
 
}
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Counter : MonoBehaviour{
    public static Counter Instance;
    public float GeriSayimSuresi;
    private Vector3 _startPos;
    private Coroutine _countdownCoroutine;
    private GameObject progress;
    public GameObject Button; 
    private Tweener tweener = null;

    private void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    private void Start(){
        Button = transform.Find("Button").gameObject;
        Button.SetActive(GameManager.Instance.PerKontrolDugmesiOlsun);
        GeriSayimSuresi = GameManager.Instance.GeriSayimSuresi;
        progress = GameObject.Find("Progress");
        _startPos = progress.transform.position;
    }

    public void GeriSayimAnimasyonunuBaslat(){ 
        if (!GameManager.Instance.OtomatikPerkontrolu) return;
        
        if (   Istaka.Instance.SiraliRakamAyniRenkGruplari.Count>0 
               || Istaka.Instance.AyniRakamAyniRenkGruplari.Count>0
               || Istaka.Instance.AyniRakamHepsiFarkliRenkGruplari.Count>0
               || Istaka.Instance.SiraliRakamHepsiFarkliRenkGruplari.Count>0){
                CountdownRoutine();
        }
        else{
            Istaka.Instance.PersizFullIstakayiBosalt();
        }
    }


    public void CountdownRoutine(){  
        PerIcinTasTavsiye.Instance.Sallanma();
        if (tweener == null){
            tweener = progress.transform.DOMoveX(8, GeriSayimSuresi)
                .SetEase(Ease.Linear).OnComplete(() => {
                    tweener.Kill();
                    tweener = null;
                    progress.transform.position = _startPos;
                    Puanlama.Instance.PuanlamaYap(); 
                });;
        }
        else{
            TweenReset();
            CountdownRoutine();
        } 
    }

    public void TweenReset(){
        if (tweener != null){
            tweener.Kill();
            tweener = null;
            progress.transform.position = _startPos;
        }  
    }
}
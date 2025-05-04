using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Counter : MonoBehaviour{
    public static Counter Instance;
    private float GeriSayimSuresi;
    //private Vector3 _startPos;
    public Coroutine _countdownCoroutine;
    //private GameObject progress;
    //public GameObject Button; 
    //private Tweener tweener = null;

    private void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    private void Start(){
        //Button = transform.Find("Button").gameObject;
        //Button.SetActive(GameManager.Instance.PerKontrolDugmesiOlsun);
        GeriSayimSuresi = GameManager.Instance.GeriSayimSuresi;
        //progress = GameObject.Find("Progress");
        //_startPos = progress.transform.position;
    }

    public void GeriSayimAnimasyonunuBaslat(){ 
        if (!GameManager.Instance.OtomatikPerkontrolu) return;
        
        if (   Istaka.Instance.SiraliRakamAyniRenkGruplari.Count>0 
               || Istaka.Instance.AyniRakamAyniRenkGruplari.Count>0
               || Istaka.Instance.AyniRakamHepsiFarkliRenkGruplari.Count>0
               || Istaka.Instance.SiraliRakamHepsiFarkliRenkGruplari.Count>0){ 
                if (_countdownCoroutine != null){
                    StopCoroutine(_countdownCoroutine);
                }
                _countdownCoroutine = StartCoroutine(GerisayimProgresBariniIslet());
        }
        else{
            Istaka.Instance.PersizFullIstakayiBosalt();
        }
    }

    private IEnumerator GerisayimProgresBariniIslet(){ 
        float _timeLeft = GeriSayimSuresi;
        while (_timeLeft > 0){
            OyunSahnesiUI.Instance.GeriSayimBari.value = (GeriSayimSuresi-_timeLeft)/GeriSayimSuresi * 100;
            yield return new WaitForSeconds(.05f); // 0.05 sn beklerse 100 birimi 5 sn de tamamlar.
            _timeLeft--;
        }
        Puanlama.Instance.Puanla();
        OyunSahnesiUI.Instance.GeriSayimBari.value = 0;

        _countdownCoroutine = null;      // sadece referansı temizle
        yield break;                     // kendiliğinden sonlanır
    }
 
}
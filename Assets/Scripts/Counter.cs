using System.Collections;
using UnityEngine;

public class Counter : MonoBehaviour{
    public static Counter Instance;
    public float GeriSayimSuresi{ get; set; } = 5.0f;
    private Vector3 _startPos;
    private Coroutine _countdownCoroutine;

    private void Awake(){
        if (Instance == null){
            Instance = this;
        }else{
            Destroy(gameObject);
        }
    }

    private void Start(){
        _startPos = transform.position; 
    }

    public void KontrolIcinGeriSaymayaBasla(){
        if (Istaka.Instance.DoluCepSayisi() < 3) {
            return;
        } 
        
        if (_countdownCoroutine != null){
            transform.position = _startPos;
            StopCoroutine(_countdownCoroutine);
        } 
        _countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine(){ 
        float _timeLeft = GeriSayimSuresi;
        while (_timeLeft > 0){
            yield return new WaitForSeconds(1f);
            _timeLeft--;
            transform.position += new Vector3(Card.Instance.transform.localScale.x / GeriSayimSuresi, 0, 0);
        } 
        transform.position = _startPos; 
        if (   Istaka.Instance.SiraliRakamAyniRenkGruplari.Count>0 
            || Istaka.Instance.AyniRakamAyniRenkGruplari.Count>0
            || Istaka.Instance.AyniRakamHepsiFarkliRenkGruplari.Count>0
            || Istaka.Instance.SiraliRakamHepsiFarkliRenkGruplari.Count>0){
            PuanlamaKontrolcu.Instance.PerdekiTaslariToparla();
            PuanlamaKontrolcu.Instance.BonuslariVer();
        }
        else {
           Istaka.Instance.PersizFullIstakayiBosalt();
        }
        CardKontrolcu.Instance.KarttakiPerleriBul();
    }
}
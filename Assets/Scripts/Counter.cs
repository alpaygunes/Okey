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

    public void StartCountdown(){ 
        if (_countdownCoroutine != null){
            transform.position = _startPos;
            StopCoroutine(_countdownCoroutine);
        } 
        _countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine(){
        bool herhangiBirGrupVar = true;
        float _timeLeft = GeriSayimSuresi;
        while (_timeLeft > 0){
            yield return new WaitForSeconds(1f);
            _timeLeft--;
            transform.position += new Vector3(Card.Instance.transform.localScale.x / GeriSayimSuresi, 0, 0);
        } 
        transform.position = _startPos;

        IstakaKontrolcu.Instance.SiraliGruplariBelirle();
        IstakaKontrolcu.Instance.BenzerRakamGruplariniBelirle();
        IstakaKontrolcu.Instance.SiraliGruplarinIcindekiRenkGruplariniBelirle();
        IstakaKontrolcu.Instance.AyniRakamGruplarinIcindekiRenkGruplariniBelirle();
        IstakaKontrolcu.Instance.AyniRakamGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle();
        IstakaKontrolcu.Instance.SiraliGruplarinIcindekiHepsiFarkliRenkGruplariniBelirle(); 
        
        if (   IstakaKontrolcu.Instance.SiraliRakamAyniRenkGruplari.Count>0 
            || IstakaKontrolcu.Instance.AyniRakamAyniRenkGruplari.Count>0
            || IstakaKontrolcu.Instance.AyniRakamHepsiFarkliRenkGruplari.Count>0
            || IstakaKontrolcu.Instance.SiraliRakamHepsiFarkliRenkGruplari.Count>0){
            PuanlamaKontrolcu.Instance.PuanlamaYap();
        }
        
    }
}
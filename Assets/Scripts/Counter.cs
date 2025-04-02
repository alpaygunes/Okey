using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Counter : MonoBehaviour{
    public static Counter Instance;
    public float GeriSayimSuresi;
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
        GeriSayimSuresi = GameManager.Instance.GeriSayimSuresi;
    }

    public void KontrolIcinGeriSaymayaBasla(){
        if (Istaka.Instance.DoluCepSayisi() < 3) {
            return;
        } 
        
        if (_countdownCoroutine != null){
            transform.position = _startPos;
            StopCoroutine(_countdownCoroutine);
            if (tweener!=null){
                tweener.Kill();
                tweener = null;
            } 
        } 
        _countdownCoroutine = StartCoroutine(CountdownRoutine());
    }

    private Tweener tweener = null;
    private IEnumerator CountdownRoutine(){
        
        float _timeLeft = GeriSayimSuresi;
        
        if (tweener==null){
            tweener = transform.DOMoveX(Card.Instance.transform.localScale.x, GeriSayimSuresi).SetEase(Ease.Linear);
        } 
        
        while (_timeLeft > 0){
            yield return new WaitForSeconds(1f);
            _timeLeft--; 
        } 
        
        tweener.Kill();
        tweener = null;
        transform.position = _startPos;  
        Puanlama.Instance.PuanlamaYap();
        GameManager.Instance.HamleSayisi++;
    }
}
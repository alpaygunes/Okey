using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine; 

public class Tas : MonoBehaviour{
    public int rakam;
    public Color renk ;
    private readonly float _animasyonSuresi = .2f;


    private void Awake(){ 
        gameObject.SetActive(false); 
    }

    private void Start(){ 
        GetComponentInChildren<TextMeshPro>().color = renk;
    }

    public void MerkezeKay(float gecikme){
        StartCoroutine(WaitAndExecute(gecikme));
        
    }

    IEnumerator WaitAndExecute(float gecikme){
        yield return new WaitForSeconds(gecikme);
        Vector3 ilkScale = transform.localScale;
        transform.DOMove(new Vector3(0, 0, 0), _animasyonSuresi);
        Sequence mySequence = DOTween.Sequence();
        mySequence
            .Append(transform.DOScale(transform.localScale * 4, _animasyonSuresi * .5f))
            .Append(transform.DOScale(ilkScale*2, _animasyonSuresi * .5f));
        StartCoroutine(KillSelf());
        PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan *= rakam;
        PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuanTMP.text = PuanlamaKontrolcu.Instance.PerlerdenKazanilanPuan.ToString();
        
        
        
    }

    IEnumerator KillSelf(){
        yield return new WaitForSeconds(_animasyonSuresi);
        transform.DOKill();
        Destroy(this.gameObject);
    }
}
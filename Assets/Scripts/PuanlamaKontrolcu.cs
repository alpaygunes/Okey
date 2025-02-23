using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PuanlamaKontrolcu : MonoBehaviour
{
    public static PuanlamaKontrolcu Instance{ get; private set; }
    void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }
        Instance = this; 
    }

    private void Start(){
        gameObject.SetActive(false);
        transform.Find("Zemin").transform.localScale = Card.Instance.transform.localScale;
        transform.Find("Zemin").transform.position = Card.Instance.transform.position;
        transform.Find("Zemin").transform.position += new Vector3(0,0,-0.1f); 
    }

    public void PuanlamaYap(){
        gameObject.SetActive(true);
        IstakaKontrolcu.Instance.GruplariTemizle();
        StartCoroutine(WaitAndExecute());
        
    }
    
    IEnumerator WaitAndExecute()
    { 
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
}

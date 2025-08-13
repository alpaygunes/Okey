using System;
using UnityEngine;

public class SpawnHole : MonoBehaviour
{
    public bool musait = true;
    public int colID;
    private bool _temastaBekleyenVar = false;
    private bool _cikanVar = false;

    private void OnCollisionStay(Collision other)
    {
        if (other.collider.CompareTag("CARDTAKI_TAS"))
        {
            _temastaBekleyenVar = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("CARDTAKI_TAS"))
        {
            _cikanVar = true;
        }
    }


    private void Update()
    {
        if (!_temastaBekleyenVar && _cikanVar)
        {
            musait = true;    
            _temastaBekleyenVar = false;
            _cikanVar = false;
        }
        
        if (musait && TasManeger.Instance.TasList.Count > 0)
        {
            musait = false;
            var siradakiTas = TasManeger.Instance.TasList[0];
            siradakiTas.gameObject.transform.position = transform.position;
            siradakiTas.gameObject.transform.localScale = transform.localScale;
            siradakiTas.transform.SetParent(GameManager.Instance.transform);
            siradakiTas.tag = "CARDTAKI_TAS";
            var siradakiTasInstance = siradakiTas.gameObject.GetComponent<Tas>();
            siradakiTasInstance.colID = colID;
            TasManeger.Instance.TasInstances.Add(siradakiTas, siradakiTasInstance);
            siradakiTas.SetActive(true);
            TasManeger.Instance.TasList.RemoveAt(0); 
        } 
    }
}
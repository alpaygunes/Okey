using UnityEngine;

public class SpawnHole : MonoBehaviour{
    public bool musait = true;
    public int colID;

    private void OnTriggerStay2D(Collider2D other){
        if (other.CompareTag("CARDTAKI_TAS")) {
            musait = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other){
        if (other.CompareTag("CARDTAKI_TAS")) {
            musait = true; 
        }
    }


    private void Update(){ 
        if (musait && TasManeger.Instance.TasList.Count > 0) {
            musait = false;
            var siradakiTas = TasManeger.Instance.TasList[0];
            TasManeger.Instance.TasList.RemoveAt(0);
            siradakiTas.gameObject.transform.position = transform.position;
            siradakiTas.gameObject.transform.localScale = transform.localScale;
            siradakiTas.gameObject.transform.localScale *= .195f ;
            siradakiTas.transform.SetParent(GameManager.Instance.transform);
            siradakiTas.tag = "CARDTAKI_TAS";
            siradakiTas.gameObject.GetComponent<Tas>().colID = colID;
            TasManeger.Instance.TasInstances.Add(siradakiTas, siradakiTas.gameObject.GetComponent<Tas>());
            siradakiTas.SetActive(true);
            
        }
    }
}
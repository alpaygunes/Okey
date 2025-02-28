using UnityEngine;

public class SpawnHole : MonoBehaviour{
    private bool musait = true;

    private void OnTriggerStay2D(Collider2D other){
        if (other.CompareTag("TAS")){
            musait = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other){
        if (other.CompareTag("TAS")){
            musait = true;
        }
    }

    private void Update(){
        if (musait && TasManeger.Instance.TasList.Count > 0){
            musait = false;
            var siradakiTas = TasManeger.Instance.TasList[0];
            TasManeger.Instance.TasList.RemoveAt(0);
            siradakiTas.gameObject.transform.position = transform.position;
            siradakiTas.gameObject.transform.localScale = transform.localScale * .85f;
            siradakiTas.transform.SetParent(PlatformManager.Instance.transform);
            siradakiTas.SetActive(true);
        }
    }
}
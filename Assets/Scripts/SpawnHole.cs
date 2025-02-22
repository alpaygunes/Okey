using UnityEngine;

public class SpawnHole : MonoBehaviour{
    private bool musait = true;
    
    private void OnTriggerStay2D(Collider2D other){
        if (other.CompareTag("KARE")){
            musait = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other){
        if (other.CompareTag("KARE")){
            musait = true;
        }
    }

    private void Update(){
        if (musait && KareManeger.Instance.KarelerList.Count>0){
            musait = false;
            var siradakiKare = KareManeger.Instance.KarelerList[0];
            KareManeger.Instance.KarelerList.RemoveAt(0); 
            siradakiKare.gameObject.transform.position = transform.position;
            siradakiKare.gameObject.transform.localScale = transform.localScale;
            siradakiKare.transform.SetParent(PlatformManager.Instance.transform); 
            siradakiKare.SetActive(true);
        }
    }
}
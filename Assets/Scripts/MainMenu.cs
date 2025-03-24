using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour{



    void Update(){
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0)){
            Vector2 inputPosition = Input.touchCount > 0
                ? Input.GetTouch(0).position
                : Input.mousePosition;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = inputPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0){
                GameObject clickedObject = results[0].gameObject;
                if (clickedObject.CompareTag("MULTYPLAYER")){ 
                    SceneManager.LoadScene("MultyPlayer");
                }
            }
        }
    }
}
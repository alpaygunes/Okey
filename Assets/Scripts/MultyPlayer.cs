using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; 

public class MultyPlayer : MonoBehaviour{
    public string Seed;
    public TMP_InputField inputField; 

    private void Start(){
        inputField = GameObject.Find("Seed").GetComponent<TMP_InputField>();
        inputField.characterLimit = 10;
        SeedBelirle();
    }

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
                PlayerPrefs.SetString("Seed", inputField.text);  
                PlayerPrefs.Save();  
                GameObject clickedObject = results[0].gameObject;
                if (clickedObject.CompareTag("PLAY")){
                    SceneManager.LoadScene("OyunSahnesi"); 
                }

                if (clickedObject.CompareTag("ZAR")){
                    SeedBelirle();
                }
            }
        }
    }

    private void SeedBelirle(){
        int length = 4;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        System.Random random = new System.Random();
        Seed = new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());

        inputField.text = Seed;
    }
}
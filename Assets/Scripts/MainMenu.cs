using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        SceneManager.UnloadSceneAsync("OyunSahnesi");
    }

    void Update()
    {
        // Dokunmatik ekran ve fare girişini destekle
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            // Dokunmatik ekran veya fare konumunu al
            Vector2 inputPosition = Input.touchCount > 0 
                ? Input.GetTouch(0).position 
                : Input.mousePosition;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = inputPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                GameObject clickedObject = results[0].gameObject;
                if (clickedObject.CompareTag("PLAY"))
                {
                    SceneManager.LoadScene("OyunSahnesi");
                    SceneManager.UnloadSceneAsync("MainMenu");
                }
            }
        }
    }
}

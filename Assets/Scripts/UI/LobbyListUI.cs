using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyListUI : MonoBehaviour
{
    private void OnEnable(){
        VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
        
        Button CreateLobbyBtn = rootElement.Q<Button>("CreateLobbyBtn");
        
        CreateLobbyBtn.clicked += () => {
            LobbyManager.Instance.CreateLobi();
        };
    }

 
}

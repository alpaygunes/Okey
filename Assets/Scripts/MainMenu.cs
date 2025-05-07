using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button; 

public class MainMenu : MonoBehaviour{
    private Button HamleLimitliBtn; 
    private Button ZamanLimitliBtn; 

    private void OnEnable(){ 
        VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
        HamleLimitliBtn = rootElement.Q<Button>("HamleLimitliBtn");
        HamleLimitliBtn.clicked += SearchHamleLimitliGame;
        
        ZamanLimitliBtn = rootElement.Q<Button>("ZamanLimitliBtn");
        ZamanLimitliBtn.clicked += SearchZamanLimitliGame;
    }

    private void SearchHamleLimitliGame(){ 
        OyunKurallari.Instance.GuncelOyunTipi = OyunKurallari.OyunTipleri.HamleLimitli;
        SceneManager.LoadScene("LobbyManager");
    }

    private void SearchZamanLimitliGame(){
        OyunKurallari.Instance.GuncelOyunTipi = OyunKurallari.OyunTipleri.ZamanLimitli;
        SceneManager.LoadScene("LobbyManager");
    }

    private void OnDisable(){
        HamleLimitliBtn.clicked -= SearchHamleLimitliGame;
        ZamanLimitliBtn.clicked -= SearchZamanLimitliGame; 
    }
}
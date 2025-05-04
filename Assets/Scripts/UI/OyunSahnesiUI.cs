using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class OyunSahnesiUI : MonoBehaviour
{
    private VisualElement rootElement;
    public static OyunSahnesiUI Instance;
    public Label Skor;
    public Label KalanTasSayisi;
    public Label HamleSayisi;
    public Button Exit;
    public Button KontrolEt;
    public ProgressBar GeriSayimBari;
    
    private void Awake(){
        if (Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        Skor = rootElement.Q<Label>("Skor");
        KalanTasSayisi = rootElement.Q<Label>("KalanTasSayisi");
        HamleSayisi = rootElement.Q<Label>("HamleSayisi");
        KontrolEt = rootElement.Q<Button>("KontrolEt");
        Exit = rootElement.Q<Button>("Exit");
        GeriSayimBari = rootElement.Q<ProgressBar>("GeriSayimBari");
        
        // Lobby Create Penceresi
        KontrolEt.clicked += ButtonlaPuanlamaYap;
        
        // Lobby Create Penceresi
        Exit.clicked += async () => {
            SceneManager.LoadScene("LobbyManager");
            await LobbyListUI.Instance.LobidenAyril();
        };

        KontrolEt.visible = GameManager.Instance.PerKontrolDugmesiOlsun;
    }
    


    public void ButtonlaPuanlamaYap(){ 
        GeriSayimBari.value = 0;
        Puanlama.Instance.ButtonlaPuanlamaYap();
    }
}

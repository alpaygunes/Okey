using System;
using System.Collections;
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
    public Label GeriSayim;
    public Button Exit;
    public Button KontrolEt;
    public ProgressBar GeriSayimBari;
    public Label GorevSayisiLbl;
    
    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(gameObject); // Bu nesneden başka bir tane varsa, yenisini yok et
            return;
        } 
        Instance = this; 
    }

    private void OnEnable(){
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        Skor = rootElement.Q<Label>("Skor");
        KalanTasSayisi = rootElement.Q<Label>("KalanTasSayisi");
        HamleSayisi = rootElement.Q<Label>("HamleSayisi");
        GeriSayim = rootElement.Q<Label>("GeriSayim"); 
        GorevSayisiLbl = rootElement.Q<Label>("GorevSayisi");
        KontrolEt = rootElement.Q<Button>("KontrolEt");
        Exit = rootElement.Q<Button>("Exit");
        GeriSayimBari = rootElement.Q<ProgressBar>("GeriSayimBari"); 
        KontrolEt.clicked += ButtonlaPuanlamaYap; 
        KontrolEt.visible = GameManager.Instance.PerKontrolDugmesiOlsun;
        GeriSayim.style.display =    DisplayStyle.None;
        GorevSayisiLbl.style.display =    DisplayStyle.None;
        GeriSayim.text = null;
        GorevSayisiLbl.text = null;
        HamleSayisi.text = "0"; 
        
        Exit.clicked += () => {
            _ = LobbyManager.Instance.CikmakIisteginiGonder();
        }; 
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.ZamanLimitli){
            GeriSayim.text = OyunKurallari.Instance.ZamanLimiti.ToString();  
            GeriSayim.style.display =    DisplayStyle.Flex;
        }
        if (OyunKurallari.Instance.GuncelOyunTipi == OyunKurallari.OyunTipleri.GorevYap){
            GorevSayisiLbl.style.display =    DisplayStyle.Flex;
            GorevSayisiLbl.text  = "1/"+OyunKurallari.Instance.GorevYap.ToString();  
        }
    }
     
    public void ButtonlaPuanlamaYap(){
        GeriSayimBari.value = 0;
        Puanlama.Instance.ButtonlaPuanlamaYap();
    } 
    
}

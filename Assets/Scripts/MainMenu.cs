using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button; 

public class MainMenu : MonoBehaviour{
    private Button HamleLimitliBtn; 
    private Button ZamanLimitliBtn;  
    private Button SPlayerBtn; 
    private Button MPlayerBtn;
    public Button Settings{ get; set; }
    private Button GorevYap; 
    private VisualElement MenuBox; 
    static public bool isSoloGame = true;
    
    async void Awake() {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized)) {
            await UnityServices.InitializeAsync();
        } 
        await AnonimGiris();
        
        string nick = PlayerPrefs.GetString("NickName");
        if (!PlayerPrefs.HasKey("NickName") || string.IsNullOrEmpty(nick))
        {
            SceneManager.LoadScene("Settings");
        } 
    }
    
    async Task AnonimGiris(){
        try{
            AuthenticationService.Instance.ClearSessionToken();
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); 
        }
        catch (AuthenticationException ex){
            Debug.Log($"AnonimGiris AuthenticationException HATA :{ex.Message}" );
        }
        catch (RequestFailedException ex){
            Debug.Log($"AnonimGiris RequestFailedException HATA :{ex.Message}" );
        }
    }

 

    private void OnEnable(){ 
        VisualElement rootElement = GetComponent<UIDocument>().rootVisualElement;
        HamleLimitliBtn = rootElement.Q<Button>("HamleLimitliBtn");
        HamleLimitliBtn.clicked += SearchHamleLimitliGame; 
        ZamanLimitliBtn = rootElement.Q<Button>("ZamanLimitliBtn");
        ZamanLimitliBtn.clicked += SearchZamanLimitliGame; 
        GorevYap = rootElement.Q<Button>("GorevYap");
        GorevYap.clicked += SearchGorevYapGame; 
        MenuBox = rootElement.Q<VisualElement>("MenuBox");
        
        SPlayerBtn = rootElement.Q<Button>("SoloPlayer");
        MPlayerBtn = rootElement.Q<Button>("Multyplayer");
        Settings = rootElement.Q<Button>("Settings");
        SPlayerBtn.clicked += ChangeSoloMultyMode; 
        MPlayerBtn.clicked += ChangeSoloMultyMode; 
        Settings.clicked += OpenSettings; 
        SPlayerBtn.enabledSelf = (!isSoloGame);
        MPlayerBtn.enabledSelf = (isSoloGame);
    }

    private void OpenSettings(){ 
        SceneManager.LoadScene("Settings");
    }


    private void SearchHamleLimitliGame(){ 
        OyunKurallari.Instance.GuncelOyunTipi = OyunKurallari.OyunTipleri.HamleLimitli;
        SceneManager.LoadScene("LobbyManager");
    }

    private void SearchZamanLimitliGame(){
        OyunKurallari.Instance.GuncelOyunTipi = OyunKurallari.OyunTipleri.ZamanLimitli;
        SceneManager.LoadScene("LobbyManager");
    }
    
    private void SearchGorevYapGame(){ 
        OyunKurallari.Instance.GuncelOyunTipi = OyunKurallari.OyunTipleri.GorevYap;
        SceneManager.LoadScene("LobbyManager");
    }
    
    private void ChangeSoloMultyMode(){
        isSoloGame = !isSoloGame;
        MPlayerBtn.enabledSelf = isSoloGame;
        SPlayerBtn.enabledSelf = !isSoloGame;
    }

    private void OnDisable(){
        HamleLimitliBtn.clicked -= SearchHamleLimitliGame;
        ZamanLimitliBtn.clicked -= SearchZamanLimitliGame;
        GorevYap.clicked -= SearchGorevYapGame;
    }
    
    
    public static string GetRandomSeed(){
        int length = 20;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; 
        System.Random random = new System.Random();
        var seed = new string(Enumerable.Range(0, length)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
        return seed;
    }
}
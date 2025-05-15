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
    private Button GorevYap; 
    
    async void Awake() {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized)) {
            await UnityServices.InitializeAsync();
        } 
        await AnonimGiris();
    }
    
    async Task AnonimGiris(){
        try{
            AuthenticationService.Instance.ClearSessionToken();
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); 
        }
        catch (AuthenticationException ex){
            Debug.Log($"AnonimGiris AuthenticationException {ex.Message}" );
        }
        catch (RequestFailedException ex){
            Debug.Log($"AnonimGiris RequestFailedException {ex.Message}" );
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

    private void OnDisable(){
        HamleLimitliBtn.clicked -= SearchHamleLimitliGame;
        ZamanLimitliBtn.clicked -= SearchZamanLimitliGame;
        GorevYap.clicked -= SearchGorevYapGame;
    }
}
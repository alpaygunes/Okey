using Unity.Netcode; 

public class OyunKurallari:NetworkBehaviour
{
    public static OyunKurallari Instance;
    public OyunTipleri GuncelOyunTipi { get; set; }
    public int HamleLimit { get; private set; }
    public int SkorLimiti { get; private set; }
    public int ZamanLimiti { get; private set; }
    public int GorevLimit { get; private set; } 
    
    public enum OyunTipleri
    {
        HamleLimitli,
        ZamanLimitli,
        GorevYap,
        RakibeGonder,
    }

    private void Awake(){
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void InitializeSettings()
    {
        switch (GuncelOyunTipi)
        {
            case OyunTipleri.HamleLimitli:
                HamleLimit = 5;
                SkorLimiti = 0;
                ZamanLimiti = 0;
                GorevLimit = 0;
                break;

            case OyunTipleri.GorevYap:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 25;
                GorevLimit = 2;
                break;

            case OyunTipleri.ZamanLimitli:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 20;
                GorevLimit = 0;
                break;

            case OyunTipleri.RakibeGonder:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 0;
                GorevLimit = 0;
                break;

            default:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 0;
                break;
        }
    }
 
}
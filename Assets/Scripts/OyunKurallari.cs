using Unity.Netcode; 

public class OyunKurallari:NetworkBehaviour
{
    public static OyunKurallari Instance;
    public OyunTipleri GuncelOyunTipi { get; set; }
    public int HamleLimit { get; private set; }
    public int SkorLimiti { get; private set; }
    public int ZamanLimiti { get; private set; }
    public int GorevYap { get; private set; } 
    
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
                GorevYap = 0;
                break;

            case OyunTipleri.GorevYap:
                HamleLimit = 0;
                SkorLimiti = 20;
                ZamanLimiti = 0;
                GorevYap = 5;
                break;

            case OyunTipleri.ZamanLimitli:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 60;
                GorevYap = 0;
                break;

            case OyunTipleri.RakibeGonder:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 0;
                GorevYap = 0;
                break;

            default:
                HamleLimit = 0;
                SkorLimiti = 0;
                ZamanLimiti = 0;
                break;
        }
    }
 
}
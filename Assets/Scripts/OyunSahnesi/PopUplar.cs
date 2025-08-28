using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopUplar : MonoBehaviour {
    public GameObject meyvePopUp;
    public GameObject renkPopUp;
    public GameObject kalipPopUp;
    public GameObject HamleLimitiPopUp;
    public GameObject PuanLimitiPopUp;
    public GameObject SeviyeBilgisiPopUp;
    void Start() {
        meyvePopUp = transform.Find("Meyve").gameObject;
        renkPopUp = transform.Find("Renk").gameObject;
        kalipPopUp = transform.Find("Kalip").gameObject;
        HamleLimitiPopUp = transform.Find("HamleLimiti").gameObject;
        PuanLimitiPopUp = transform.Find("PuanLimiti").gameObject;
        SeviyeBilgisiPopUp = transform.Find("SeviyeBilgisi").gameObject;
        Gizle();
    }

    public void YeniBasariGoster(string asilanLimitAdi) {
        if (asilanLimitAdi != "")
        {
            GameManager.Instance.OyunDurumu = GameManager.OyunDurumlari.OyunDurdu;
            transform.gameObject.SetActive(true);
            Invoke("Gizle", 5f);
            Invoke("toLobbyManager",6f);
        } 
        
        var kayitliOyuncuVerisi = SoloLevelManager.Instance.GetLevelVerisi(PlayerPrefs.GetInt("OynananLevelID"));

        if (asilanLimitAdi == "Meyve") {
            meyvePopUp.SetActive(true);
            meyvePopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "OynananMeyveNo" + kayitliOyuncuVerisi.MeyveSirasi.ToString();
        }else if (asilanLimitAdi == "Renk") {
            renkPopUp.SetActive(true);
            renkPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "OynananRenkNo" + kayitliOyuncuVerisi.RenkSirasi.ToString();
        }else if (asilanLimitAdi == "Kalip") {
            kalipPopUp.SetActive(true);
            kalipPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "OynananKalipNo" + kayitliOyuncuVerisi.KalipSirasi.ToString();
        }else if (asilanLimitAdi == "HamleLimiti") {
            HamleLimitiPopUp.SetActive(true);
            HamleLimitiPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "HamleLimiti " + IsaretleBelirtYoket.Instance.HamleSayisi.ToString();
        }else if (asilanLimitAdi == "PuanLimiti") {
            PuanLimitiPopUp.SetActive(true);
            PuanLimitiPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "Yeni Seviye " + PlayerPrefs.GetInt("OynananLevelID").ToString();
        } 
    }
    
    public void SeviyeBilgisiGoster() {
        transform.gameObject.SetActive(true);
        SeviyeBilgisiPopUp.SetActive(true);
        SeviyeBilgisiPopUp.transform.Find("MeyveSirasiTxt").GetComponent<TextMeshPro>().text = "MeyveSirasi : " + GameManager.Instance.kayitliOyuncuVerisi.MeyveSirasi.ToString();
        SeviyeBilgisiPopUp.transform.Find("RenkSirasiTxt").GetComponent<TextMeshPro>().text  = "RenkSirasi : " + GameManager.Instance.kayitliOyuncuVerisi.RenkSirasi.ToString();
        SeviyeBilgisiPopUp.transform.Find("LevelTxt").GetComponent<TextMeshPro>().text = "Level : " + GameManager.Instance.kayitliOyuncuVerisi.Level.ToString();
        SeviyeBilgisiPopUp.transform.Find("KalipTxt").GetComponent<TextMeshPro>().text = "Kalip : " + GameManager.Instance.kayitliOyuncuVerisi.KalipSirasi.ToString();
        //Invoke("Gizle", 1f);
    }

    void Gizle() {
        meyvePopUp.SetActive(false);
        renkPopUp.SetActive(false);
        kalipPopUp.SetActive(false); 
        HamleLimitiPopUp.SetActive(false); 
        PuanLimitiPopUp.SetActive(false); 
        SeviyeBilgisiPopUp.SetActive(false); 
        transform.gameObject.SetActive(false); 
    }
    
    void toLobbyManager() {
        SceneManager.LoadScene("LobbyManager", LoadSceneMode.Single);
    }
    
 
}
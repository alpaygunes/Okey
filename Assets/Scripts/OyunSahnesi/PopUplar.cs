using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PopUplar : MonoBehaviour {
    public GameObject meyvePopUp;
    public GameObject renkPopUp;
    public GameObject kalipPopUp;
    public GameObject HamleLimitiPopUp;
    public GameObject PuanLimitiPopUp;

    void Start() {
        meyvePopUp = transform.Find("Meyve").gameObject;
        renkPopUp = transform.Find("Renk").gameObject;
        kalipPopUp = transform.Find("Kalip").gameObject;
        HamleLimitiPopUp = transform.Find("HamleLimiti").gameObject;
        PuanLimitiPopUp = transform.Find("PuanLimiti").gameObject;
        Gizle();
    }

    public void Goster(string asilanLimitAdi) {
        if (asilanLimitAdi != "")
        {
            GameManager.Instance.OyunDurumu = GameManager.OyunDurumlari.OyunDurdu;
            transform.gameObject.SetActive(true);
            Invoke("Gizle", 5f);
            Invoke("toLobbyManager",6f);
        } 

        if (asilanLimitAdi == "Meyve") {
            meyvePopUp.SetActive(true);
            meyvePopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "OynananMeyveNo" + PlayerPrefs.GetInt("OynananMeyveNo").ToString();
        }else if (asilanLimitAdi == "Renk") {
            renkPopUp.SetActive(true);
            renkPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "OynananRenkNo" + PlayerPrefs.GetInt("OynananRenkNo").ToString();
        }else if (asilanLimitAdi == "Kalip") {
            kalipPopUp.SetActive(true);
            kalipPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "OynananKalipNo" + PlayerPrefs.GetInt("OynananKalipNo").ToString();
        }else if (asilanLimitAdi == "HamleLimiti") {
            HamleLimitiPopUp.SetActive(true);
            HamleLimitiPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "HamleLimiti " + IsaretleBelirtYoket.Instance.HamleSayisi.ToString();
        }else if (asilanLimitAdi == "PuanLimiti") {
            PuanLimitiPopUp.SetActive(true);
            PuanLimitiPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "Yeni Seviye ";
        } 
    }

    void Gizle() {
        meyvePopUp.SetActive(false);
        renkPopUp.SetActive(false);
        kalipPopUp.SetActive(false);
        transform.gameObject.SetActive(false); 
    }
    
    void toLobbyManager() {
        SceneManager.LoadScene("LobbyManager", LoadSceneMode.Single);
    }
}
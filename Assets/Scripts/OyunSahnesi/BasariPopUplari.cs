using TMPro;
using UnityEngine;

public class BasariPopUplari : MonoBehaviour {
    public GameObject meyvePopUp;
    public GameObject renkPopUp;
    public GameObject kalipPopUp;

    void Start() {
        meyvePopUp = transform.Find("Meyve").gameObject;
        renkPopUp = transform.Find("Renk").gameObject;
        kalipPopUp = transform.Find("Kalip").gameObject;
        Gizle();
    }

    public void Goster(string asilanLimitAdi) {
        if (asilanLimitAdi != "") transform.gameObject.SetActive(true);

        if (asilanLimitAdi == "Meyve") {
            meyvePopUp.SetActive(true);
            meyvePopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "OynananMeyveNo" + PlayerPrefs.GetInt("OynananMeyveNo").ToString();
        }
        else if (asilanLimitAdi == "Renk") {
            renkPopUp.SetActive(true);
            renkPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "OynananRenkNo" + PlayerPrefs.GetInt("OynananRenkNo").ToString();
        }
        else if (asilanLimitAdi == "Kalip") {
            kalipPopUp.SetActive(true);
            kalipPopUp.transform.Find("Txt").GetComponent<TextMeshPro>().text = "OynananKalipNo" + PlayerPrefs.GetInt("OynananKalipNo").ToString();
        } 
        Invoke("Gizle", 5f);
    }

    void Gizle() {
        meyvePopUp.SetActive(false);
        renkPopUp.SetActive(false);
        kalipPopUp.SetActive(false);
        transform.gameObject.SetActive(false);
    }
}
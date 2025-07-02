using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Settings : MonoBehaviour {
    private VisualElement rootElement;
    public VisualElement Satir1 { get; set; }
    public VisualElement Satir2 { get; set; }
    public VisualElement Satir3 { get; set; }
    private ResourceRequest resourceRequest;
    public TextField NickName { get; set; }
    public Button CurrentAvatar { get; set; }
    public Button Quit { get; set; }

    private void Start() {
        AvatarIconlariniListele();
        AyarlariYukle();

        string nick = PlayerPrefs.GetString("NickName");
        if (!PlayerPrefs.HasKey("NickName") || string.IsNullOrEmpty(nick)) {
            NickName.Q("unity-text-input").style.backgroundColor = new StyleColor(Color.red);
        }
    }

    private void OnEnable() {
        rootElement = GetComponent<UIDocument>().rootVisualElement;
        Satir1 = rootElement.Q<VisualElement>("Satir1");
        Satir2 = rootElement.Q<VisualElement>("Satir2");
        Satir3 = rootElement.Q<VisualElement>("Satir3");
        CurrentAvatar = Satir1.Q<Button>("CurrentAvatar");
        Quit = Satir3.Q<Button>("Quit");
        Quit.RegisterCallback<ClickEvent>(OnQuitButtonClicked);
        NickName = Satir1.Q<TextField>("NickName");
        NickName.RegisterCallback<ChangeEvent<string>>(OnNickNameChanged);
    }

    private void AyarlariYukle() {
        NickName.value = PlayerPrefs.GetString("NickName");
        string avatarName = PlayerPrefs.GetString("AvatarName");
        if (!string.IsNullOrEmpty(avatarName)) {
            Sprite avatarSprite = Resources.Load<Sprite>($"avatars/{avatarName}");
            if (avatarSprite != null) {
                CurrentAvatar.style.backgroundImage = new StyleBackground(avatarSprite);
            }
        }
    }

    private void AvatarIconlariniListele() {
        Satir2.Clear();
        Sprite[] avatarSprites = Resources.LoadAll<Sprite>("avatars");
        if (avatarSprites != null) {
            foreach (Sprite sprite in avatarSprites) {
                Button avatarButton = new Button();
                var backgroundImage = new StyleBackground(sprite);
                avatarButton.style.backgroundImage = backgroundImage;
                avatarButton.AddToClassList("avatar_item");
                avatarButton.RegisterCallback<ClickEvent>(e => {
                    ChangeAvatar(sprite);
                    CurrentAvatar.style.backgroundImage = new StyleBackground(sprite);
                });
                Satir2?.Add(avatarButton);
            }
        }
    }

    private void ChangeAvatar(Sprite sprite) {
        PlayerPrefs.SetString("AvatarName", sprite.texture.name);
    }


    private void OnNickNameChanged(ChangeEvent<string> evt) {
        PlayerPrefs.SetString("NickName", evt.newValue);
        AyarlariYukle();
    }

    private void OnQuitButtonClicked(ClickEvent evt) {
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
using TMPro;
using UnityEngine;

public class gTas : MonoBehaviour{
    public int rakam;
    public Color renk;
    private SpriteRenderer zeminSpriteRenderer; 
    public TextMeshPro TextRakam;

    private void Awake(){
        gameObject.SetActive(false);
        zeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();
    }

    private void Start(){
        zeminSpriteRenderer.color = renk;
        TextRakam.text = rakam.ToString();
        CreateBg();
    }

    // kendi zemini hazirla
    public void CreateBg(){
        // Doku boyutu
        int width  = 96 * 4;
        int height = 96 * 4;

        // Dairenin yarıçapı ve kenardaki yumuşak gölge kalınlığı
        float radius          = width * 0.5f; // tam daire
        float shadowThickness = 20f;          // kenar yumuşatma / gölge
        float shadowAlpha     = 0.1f;         // gölge alfa (0-1)

        // Renk tanımları
        Color baseColor   = renk;
        Color shadowColor = new Color(1, 1, 1, shadowAlpha);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[]  pixels  = new Color[width * height];

        // Merkez koordinatları
        Vector2 center = new Vector2(width * 0.5f, height * 0.5f);

        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                int index = x + y * width;

                // Merkeze uzaklık
                float dist = Vector2.Distance(new Vector2(x, y), center);

                // Dairenin dışında kalan pikselleri saydam yap
                if (dist > radius){
                    pixels[index] = new Color(0, 0, 0, 0);
                    continue;
                }

                // Kenara yakın bölge için gölge / yumuşatma karışımı
                float edgeDist     = radius - dist;            // kenara ne kadar yakın
                float shadowBlend  = Mathf.Clamp01(edgeDist / shadowThickness);

                if (edgeDist < shadowThickness)
                    pixels[index] = Color.Lerp(shadowColor, baseColor, shadowBlend);
                else
                    pixels[index] = baseColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture,
                                  new Rect(0, 0, width, height),
                                  new Vector2(0.5f, 0.5f),
                                  100f);          // pixelsPerUnit
        zeminSpriteRenderer.sprite = sprite;
        zeminSpriteRenderer.transform.localScale *= .25f;
    }
}
using UnityEngine;

public class gTas : MonoBehaviour{
    public int rakam;
    public Color renk;
    private SpriteRenderer zeminSpriteRenderer;

    private void Awake(){
        gameObject.SetActive(false);
        zeminSpriteRenderer = transform.Find("Zemin").GetComponent<SpriteRenderer>();
    }

    private void Start(){
        zeminSpriteRenderer.color = renk;
        CreateBg();
    }

    // kendi zemini hazirla
    public void CreateBg(){
        int width = 96 * 4;
        int height = 96 * 4;
        float radius = 200; // yuvarlak köşe yarıçapı
        float shadowThickness = 25f; // gölge kalınlığı
        float shadowAlpha = 0.1f; // gölgenin maksimum alfa değeri (0 - 1)

        // Renkleri ayarlayın
        Color baseColor = renk; // varsayılan zemin rengi
        Color shadowColor = new Color(1, 1, 1, shadowAlpha);

        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                int index = x + y * width;

                // Köşe yuvarlaklığı kontrolü
                bool isRoundedCorner =
                    (x < radius && y < radius &&
                     Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) > radius) ||
                    (x > width - radius && y < radius &&
                     Vector2.Distance(new Vector2(x, y), new Vector2(width - radius, radius)) > radius) ||
                    (x < radius && y > height - radius &&
                     Vector2.Distance(new Vector2(x, y), new Vector2(radius, height - radius)) > radius) ||
                    (x > width - radius && y > height - radius &&
                     Vector2.Distance(new Vector2(x, y), new Vector2(width - radius, height - radius)) > radius);

                if (isRoundedCorner){
                    pixels[index] = new Color(0, 0, 0, 0); // şeffaf köşe
                    continue;
                }

                // Alt ve sağ kenar için gölge hesaplama
                float shadowBlend = 0;

                if (y < shadowThickness)
                    shadowBlend = Mathf.Max(shadowBlend, 1 - (y / shadowThickness));

                if (x > width - shadowThickness)
                    shadowBlend = Mathf.Max(shadowBlend, (x - (width - shadowThickness)) / shadowThickness);

                if (shadowBlend > 0)
                    pixels[index] = Color.Lerp(baseColor, shadowColor, shadowBlend);
                else
                    pixels[index] = baseColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        zeminSpriteRenderer.sprite = sprite;
        zeminSpriteRenderer.transform.localScale *= .25f;
    }
}
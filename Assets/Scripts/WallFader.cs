using UnityEngine;
using System.Collections;

// tanya ai je lah

public class WallFader : MonoBehaviour
{
    [SerializeField] private Renderer wallRenderer;
    [SerializeField] private float transparentAlpha = 0.3f; // Tingkat transparan (0 = hilang, 1 = padat)
    [SerializeField] private float fadeSpeed = 5f;

    private Material[] originalMaterials;
    private Material[] transparentMaterials;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (wallRenderer == null) wallRenderer = GetComponent<Renderer>();

        // Simpan material asli dinding
        originalMaterials = wallRenderer.materials;
        transparentMaterials = new Material[originalMaterials.Length];

        // Buat versi transparan dari material asli secara otomatis via kode
        for (int i = 0; i < originalMaterials.Length; i++)
        {
            transparentMaterials[i] = new Material(originalMaterials[i]);
            
            // Mengubah mode material menjadi Transparent di URP
            transparentMaterials[i].SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
            transparentMaterials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            transparentMaterials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentMaterials[i].SetInt("_ZWrite", 0); // Matikan ZWrite agar tidak menghalangi objek di belakangnya
            transparentMaterials[i].DisableKeyword("_ALPHATEST_ON");
            transparentMaterials[i].EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            transparentMaterials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }

    // Fungsi ini dipanggil dari Player saat masuk ke belakang dinding
    public void FadeToTransparent()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        wallRenderer.materials = transparentMaterials;
        fadeCoroutine = StartCoroutine(FadeAlpha(transparentAlpha));
    }

    // Fungsi ini dipanggil saat Player keluar dari belakang dinding
    public void FadeToOpaque()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAlpha(1f, () => {
            wallRenderer.materials = originalMaterials; // Kembalikan ke material asli yang ringan
        }));
    }

    private IEnumerator FadeAlpha(float targetAlpha, System.Action onComplete = null)
    {
        Material[] currentMats = wallRenderer.materials;
        float currentAlpha = currentMats[0].color.a;

        while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            foreach (var mat in currentMats)
            {
                Color c = mat.color;
                c.a = currentAlpha;
                mat.color = c;
            }
            yield return null;
        }

        onComplete?.Invoke();
    }
}
using UnityEngine;
using System.Collections;

public class WallFader : MonoBehaviour
{
    [SerializeField] private Renderer wallRenderer;
    [SerializeField] private float transparentAlpha = 0.3f;
    [SerializeField] private float fadeSpeed = 5f;

    private Material[] originalMaterials;
    private Material[] transparentMaterials;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (wallRenderer == null) wallRenderer = GetComponent<Renderer>();

        originalMaterials = wallRenderer.materials;
        transparentMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            transparentMaterials[i] = new Material(originalMaterials[i]);
            transparentMaterials[i].SetFloat("_Surface", 1);
            transparentMaterials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            transparentMaterials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentMaterials[i].SetInt("_ZWrite", 0);
            transparentMaterials[i].DisableKeyword("_ALPHATEST_ON");
            transparentMaterials[i].EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            transparentMaterials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }

    public void FadeToTransparent()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        // Pastikan kita pakai transparentMaterials sebelum mulai fade
        wallRenderer.materials = transparentMaterials;
        fadeCoroutine = StartCoroutine(FadeAlpha(transparentAlpha));
    }

    public void FadeToOpaque()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        // FIX: snapshot material saat ini ke local var biar coroutine nggak kehilangan referensi saat swap
        fadeCoroutine = StartCoroutine(FadeAlpha(1f, () => wallRenderer.materials = originalMaterials));
    }

    private IEnumerator FadeAlpha(float targetAlpha, System.Action onComplete = null)
    {
        // FIX: snapshot dulu biar aman walau materials di-swap di tengah jalan
        Material[] mats = wallRenderer.materials;
        float currentAlpha = mats[0].color.a;

        while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

            foreach (var mat in mats)
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
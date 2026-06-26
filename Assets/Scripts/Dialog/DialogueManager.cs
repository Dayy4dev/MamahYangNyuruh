using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI spaceToContinueText;

    [Header("Settings")]
    public float typingSpeed = 0.05f;

    [Header("Optional: Scene Transition")]
    [Tooltip("Opsional: Gambar full screen untuk efek fade-to-black.")]
    public Image fadeImage;
    [Range(1f, 10f)]
    public float fadeDuration = 1.5f;

    private string[] lines;
    private int index;
    private bool isTyping;
    private string targetSceneName; // Menyimpan nama scene tujuan secara dinamis

    public bool DialogueActive { get; private set; }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        if (spaceToContinueText != null) spaceToContinueText.gameObject.SetActive(false);
        if (dialogueText != null) dialogueText.gameObject.SetActive(false);
        
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    // Fungsi StartDialogue sekarang menerima parameter scene tujuan (opsional)
    public void StartDialogue(string[] dialogueLines, string nextScene = "")
    {
        targetSceneName = nextScene; // Simpan nama scene yang dikirim oleh trigger
        DialogueActive = true;
        lines = dialogueLines;
        index = 0;

        dialoguePanel.SetActive(true);
        if (dialogueText != null) dialogueText.gameObject.SetActive(true);
        if (spaceToContinueText != null) spaceToContinueText.gameObject.SetActive(false);

        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in lines[index].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (spaceToContinueText != null) spaceToContinueText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!dialoguePanel.activeSelf || !Input.GetKeyDown(KeyCode.Space)) return;

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = lines[index];
            isTyping = false;
            if (spaceToContinueText != null) spaceToContinueText.gameObject.SetActive(true);
        }
        else
        {
            NextLine();
        }
    }

    private void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            if (spaceToContinueText != null) spaceToContinueText.gameObject.SetActive(false);
            StartCoroutine(TypeLine());
            return;
        }

        // KUNCI LOGIC PINDAH SCENE:
        // Jika targetSceneName diisi oleh trigger, lakukan perpindahan scene
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            dialoguePanel.SetActive(false);
            DialogueActive = false;

            if (fadeImage != null)
                StartCoroutine(FadeAndLoad());
            else
                StartCoroutine(LoadNextScene());
        }
        else
        {
            // Jika dikosongkan, matikan dialog secara normal di scene yang sama
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        DialogueActive = false;
        isTyping = false;
        lines = null;
        index = 0;
        targetSceneName = ""; 

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (spaceToContinueText != null) spaceToContinueText.gameObject.SetActive(false);
        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.gameObject.SetActive(false);
        }
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(targetSceneName);
    }

    private IEnumerator FadeAndLoad()
    {
        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, fadeDuration);
            c.a = Mathf.Clamp01(t);
            fadeImage.color = c;
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(targetSceneName);
    }
}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
	[Tooltip("Optional: name of the scene to load when dialogue finishes naturally.")]
	public string nextSceneName;

	public GameObject dialoguePanel;

	[Tooltip("Optional UI Image used to perform a fade-to-black transition. Assign a full-screen UI Image here.")]
	public Image fadeImage;

	[Tooltip("Duration of the fade in seconds (1.0 - 2.0)")]
	[Range(1f, 10f)]
	public float fadeDuration = 1.5f;

	public TextMeshProUGUI spaceToContinueText;

	public TextMeshProUGUI dialogueText;

	public float typingSpeed = 0.05f;

	private string[] lines;

	private int index;

	private bool isTyping;

	public bool DialogueActive { get; private set; }

	private void Start()
	{
		dialoguePanel.SetActive(value: false);
		if (spaceToContinueText != null)
		{
			spaceToContinueText.gameObject.SetActive(value: false);
		}
		if (dialogueText != null)
		{
			dialogueText.gameObject.SetActive(value: false);
		}
		if (fadeImage != null)
		{
			fadeImage.gameObject.SetActive(value: false);
			Color color = fadeImage.color;
			color.a = 0f;
			fadeImage.color = color;
		}
	}

	public void StartDialogue(string[] dialogueLines)
	{
		DialogueActive = true;
		lines = dialogueLines;
		index = 0;
		dialoguePanel.SetActive(value: true);
		if (dialogueText != null)
		{
			dialogueText.gameObject.SetActive(value: true);
		}
		if (spaceToContinueText != null)
		{
			spaceToContinueText.gameObject.SetActive(value: false);
		}
		StartCoroutine(TypeLine());
	}

	private IEnumerator TypeLine()
	{
		isTyping = true;
		dialogueText.text = "";
		char[] array = lines[index].ToCharArray();
		foreach (char c in array)
		{
			dialogueText.text += c;
			yield return new WaitForSeconds(typingSpeed);
		}
		isTyping = false;
		if (spaceToContinueText != null)
		{
			spaceToContinueText.gameObject.SetActive(value: true);
		}
	}

	private void Update()
	{
		if (!dialoguePanel.activeSelf || !Input.GetKeyDown(KeyCode.Space))
		{
			return;
		}
		if (isTyping)
		{
			StopAllCoroutines();
			dialogueText.text = lines[index];
			isTyping = false;
			if (spaceToContinueText != null)
			{
				spaceToContinueText.gameObject.SetActive(value: true);
			}
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
			if (spaceToContinueText != null)
			{
				spaceToContinueText.gameObject.SetActive(value: false);
			}
			StartCoroutine(TypeLine());
			return;
		}
		dialoguePanel.SetActive(value: false);
		DialogueActive = false;
		if (spaceToContinueText != null)
		{
			spaceToContinueText.gameObject.SetActive(value: false);
		}
		if (dialogueText != null)
		{
			dialogueText.gameObject.SetActive(value: false);
		}
		if (!string.IsNullOrEmpty(nextSceneName))
		{
			if (fadeImage != null)
			{
				StartCoroutine(FadeAndLoad());
			}
			else
			{
				StartCoroutine(LoadNextScene());
			}
		}
	}

	public void EndDialogue()
	{
		StopAllCoroutines();
		DialogueActive = false;
		isTyping = false;
		lines = null;
		index = 0;
		if (dialoguePanel != null)
		{
			dialoguePanel.SetActive(value: false);
		}
		if (spaceToContinueText != null)
		{
			spaceToContinueText.gameObject.SetActive(value: false);
		}
		if (dialogueText != null)
		{
			dialogueText.text = "";
			dialogueText.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator LoadNextScene()
	{
		yield return new WaitForSeconds(0.2f);
		SceneManager.LoadScene(nextSceneName);
	}

	private IEnumerator FadeAndLoad()
	{
		fadeImage.gameObject.SetActive(value: true);
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
		SceneManager.LoadScene(nextSceneName);
	}
}

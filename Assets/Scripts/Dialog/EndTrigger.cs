using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndTrigger : MonoBehaviour
{
	[Header("UI References")]
	public Image blackScreen;

	public TextMeshProUGUI endText;

	public TextMeshProUGUI creditText;

	[Header("Settings")]
	public float fadeDuration = 2f;

	private bool triggered;

	private void Start()
	{
		SetUIVisible(visible: false, 0f);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!triggered && other.CompareTag("Player"))
		{
			triggered = true;
			StartCoroutine(FadeToEnd());
		}
	}

	private void SetUIVisible(bool visible, float alpha)
	{
		if (blackScreen != null)
		{
			Color color = blackScreen.color;
			color.a = alpha;
			blackScreen.color = color;
		}
		if (endText != null)
		{
			endText.gameObject.SetActive(visible);
		}
		if (creditText != null)
		{
			creditText.gameObject.SetActive(visible);
		}
	}

	private IEnumerator FadeToEnd()
	{
		blackScreen.gameObject.SetActive(value: true);
		endText.gameObject.SetActive(value: true);
		creditText.gameObject.SetActive(value: true);
		Color color = blackScreen.color;
		float t = 0f;
		while (t < fadeDuration)
		{
			t += Time.deltaTime;
			color.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
			blackScreen.color = color;
			yield return null;
		}
		color.a = 1f;
		blackScreen.color = color;
	}
}

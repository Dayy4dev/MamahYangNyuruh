using UnityEngine;
using System.Collections;
using TMPro;

public class CandyBox : MonoBehaviour
{
    private string myEffect;
    private PartnerSystem partnerSystem;
    private bool isSelected = false;
    private bool isPlayerNearby = false; // Deteksi jarak player

    [Header("UI / Visual Components")]
    [SerializeField] private TextMeshPro textMesh; 

    [Header("Candy Models Setup")]
    [SerializeField] private GameObject healCandyPrefab;
    [SerializeField] private GameObject maxHpCandyPrefab;
    [SerializeField] private GameObject buffCandyPrefab;
    [SerializeField] private GameObject damageCandyPrefab;

    private GameObject activeCandyModel;

    public void InitializeBox(string effect, PartnerSystem system)
    {
        myEffect = effect;
        partnerSystem = system;
        isSelected = false;
        isPlayerNearby = false;

        if (textMesh != null)
        {
            textMesh.text = "???"; 
        }

        SpawnCorrespondingCandy();
    }

    private void SpawnCorrespondingCandy()
    {
        GameObject prefabToSpawn = null;
        switch (myEffect)
        {
            case "Heal": prefabToSpawn = healCandyPrefab; break;
            case "MaxHP": prefabToSpawn = maxHpCandyPrefab; break;
            case "BuffDamage": prefabToSpawn = buffCandyPrefab; break;
            case "InstantDamage": prefabToSpawn = damageCandyPrefab; break;
        }

        if (prefabToSpawn != null)
        {
            activeCandyModel = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
            activeCandyModel.transform.parent = transform; 
            activeCandyModel.SetActive(false); 
        }
    }

    // CEK INPUT TOMBOL E SETIAP FRAME
    void Update()
    {
        if (isPlayerNearby && !isSelected && partnerSystem != null)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                isSelected = true;
                // Panggil fungsi pemilihan ke PartnerSystem
                partnerSystem.ProcessBoxSelection(myEffect, this);
            }
        }
    }

    public void RevealAndCleanUp(bool chosenByPlayer)
    {
        if (textMesh != null)
        {
            switch (myEffect)
            {
                case "Heal": textMesh.text = "Full Heal permen"; textMesh.color = Color.green; break;
                case "MaxHP": textMesh.text = "Max HP +30"; textMesh.color = Color.cyan; break;
                case "BuffDamage": textMesh.text = "Buff ATK Permanen"; textMesh.color = Color.red; break;
                case "InstantDamage": textMesh.text = "Kutukan -50% HP"; textMesh.color = Color.black; break;
            }
        }

        if (activeCandyModel != null)
        {
            activeCandyModel.SetActive(true);
        }

        if (chosenByPlayer)
        {
            StartCoroutine(FlyUpRoutine());
        }
        else
        {
            StartCoroutine(FadeAndDestroyRoutine());
        }
    }

    private IEnumerator FlyUpRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(0, 1f, 0); 
        float duration = 1f; 
        float elapsed = 0f;

        if (activeCandyModel != null) activeCandyModel.transform.parent = null;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            if (activeCandyModel != null)
            {
                activeCandyModel.transform.position = Vector3.Lerp(startPos, targetPos, percent);
                activeCandyModel.transform.Rotate(Vector3.up, 360f * Time.deltaTime);
            }
            yield return null;
        }

        if (activeCandyModel != null) Destroy(activeCandyModel);
        Destroy(gameObject);
    }

    private IEnumerator FadeAndDestroyRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        if (activeCandyModel != null) Destroy(activeCandyModel);
        Destroy(gameObject);
    }

    // DETEKSI PLAYER MENDEKAT
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log($"Dekat kotak permen ({myEffect}). Tekan 'Q' untuk memilih!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
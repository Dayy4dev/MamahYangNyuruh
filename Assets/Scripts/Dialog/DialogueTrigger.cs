using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public string[] dialogueLines;

    [Header("Minimap Settings")]
    public GameObject minimapUI; 

    [Header("Player Movement Settings")]
    // 1. Ganti 'MonoBehaviour' di bawah ini dengan nama script jalan Player kamu (misal: PlayerMovement)
    // jika kamu menggunakan script buatan sendiri.
    public MonoBehaviour playerMovementScript; 

    private bool triggered = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true; 
            
            // Matikan minimap
            if (minimapUI != null)
            {
                minimapUI.SetActive(false);
            }

            // 2. Matikan script gerakan Player agar tidak bisa jalan
            if (playerMovementScript != null)
            {
                playerMovementScript.enabled = false;
            }

            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue(dialogueLines);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggered = false;

            // Nyalakan kembali minimap
            if (minimapUI != null)
            {
                minimapUI.SetActive(true);
            }

            // 3. Nyalakan kembali script gerakan Player agar bisa jalan lagi
            if (playerMovementScript != null)
            {
                playerMovementScript.enabled = true;
            }

            if (dialogueManager != null && dialogueManager.DialogueActive)
            {
                dialogueManager.EndDialogue();
            }
        }
    }
}
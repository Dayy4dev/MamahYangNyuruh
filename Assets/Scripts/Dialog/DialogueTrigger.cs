using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public string[] dialogueLines;

    [Header("Player Movement Settings")]
    public MonoBehaviour playerMovementScript; 

    [Header("Scene Transition Settings")]
    [Tooltip("Centang jika ingin pindah scene setelah dialog ini selesai.")]
    public bool changeSceneAfterDialogue = false;
    [Tooltip("Tulis nama scene tujuan (Hanya diisi jika opsi di atas dicentang).")]
    public string nextSceneName;

    private bool triggered = false; 
    private bool dialogueStartedInThisTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true; 
            
            if (playerMovementScript != null)
            {
                playerMovementScript.enabled = false;
            }

            if (dialogueManager != null)
            {
                if (changeSceneAfterDialogue && !string.IsNullOrEmpty(nextSceneName))
                {
                    // Kirim dialog SEKALIGUS perintah pindah scene ke scene tujuan
                    dialogueManager.StartDialogue(dialogueLines, nextSceneName);
                }
                else
                {
                    // Hanya putar dialog biasa tanpa pindah scene
                    dialogueManager.StartDialogue(dialogueLines);
                }
                
                dialogueStartedInThisTrigger = true;
            }
        }
    }

    private void Update()
    {
        if (dialogueStartedInThisTrigger && dialogueManager != null && !dialogueManager.DialogueActive)
        {
            ResetPlayerMovement();
        }
    }

    private void ResetPlayerMovement()
    {
        dialogueStartedInThisTrigger = false;

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggered = false;

            if (dialogueManager != null && dialogueManager.DialogueActive && dialogueStartedInThisTrigger)
            {
                dialogueManager.EndDialogue();
                ResetPlayerMovement();
            }
        }
    }
}
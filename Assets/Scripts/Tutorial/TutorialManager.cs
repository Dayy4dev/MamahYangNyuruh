using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI Dialog (Tinggal Drag)")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI textNama;
    public TextMeshProUGUI textIsi;

    [Header("Grup Objek (Tinggal Drag)")]
    public GameObject itemsTutorial;      // Objek Items_Tutorial
    public GameObject meleeGroup;          // Objek MeleeGroup
    public GameObject rangedGroup;         // Objek RangedGroup

    [Header("Status (Otomatis dari Script Lain)")]
    public bool sudahAmbilPedang = false;
    public bool sudahAmbilMeriam = false;
    public bool sudahBuangSenjata = false;
    public int enemiesDefeated = 0;        // Diisi dari EnemyDummy.cs saat fungsi Die() dipanggil

    private void Awake() => Instance = this;

    private void Start()
    {
        // Pastikan grup item dan musuh mati dulu di awal game
        if(itemsTutorial) itemsTutorial.SetActive(false);
        if(meleeGroup) meleeGroup.SetActive(false);
        if(rangedGroup) rangedGroup.SetActive(false);

        StartCoroutine(AlurTutorialMudah());
    }

    private IEnumerator AlurTutorialMudah()
    {
        // --- FASE 1: DIALOG AWAL ---
        yield return StartCoroutine(TampilkanDialog("MC", "Huh? Where am I? Why are all these toys so colossally huge?!"));
        yield return StartCoroutine(TampilkanDialog("MC", "Wait... aren't you one of my toys?! Why are you alive?!"));
        yield return StartCoroutine(TampilkanDialog("Guide", "Welcome to the Astral Playroom, partner! Let’s get you moving!"));

        // --- FASE 2: DETEKSI GERAK (WASD) ---
        SetInstruksiLayar("Guide", "Use WASD to walk around this wacky place. Follow me!");
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D));
        yield return new WaitForSeconds(2f); 

       // --- FASE 3: DETEKSI PICKUP PEDANG & SERANG ---
        if (itemsTutorial != null) itemsTutorial.SetActive(true); 
        if (meleeGroup != null) meleeGroup.SetActive(true);       
        
        SetInstruksiLayar("Guide", "Uh-oh, look alive! Pick up that Balloon Sword with F!");
        yield return new WaitUntil(() => sudahAmbilPedang == true);

        SetInstruksiLayar("Guide", "Great! Now Left Click to slice them down!");
        
        // 🔴 PERBAIKAN AMAN: Tunggu sampai objek MeleeGroup kosong ATAU semua anaknya hancur secara fisik
        yield return new WaitUntil(() => meleeGroup == null || meleeGroup.GetComponentsInChildren<Transform>().Length <= 1);
        
        // --- FASE 4: DETEKSI BUANG & GANTI MERIAM ---
        if (rangedGroup != null) rangedGroup.SetActive(true); 
        
        SetInstruksiLayar("Guide", "They're too far! Press G to drop your sword!");
        yield return new WaitUntil(() => sudahBuangSenjata == true);

        SetInstruksiLayar("Guide", "Now, press F to pick up that Air Cannon, and blast them!");
        yield return new WaitUntil(() => sudahAmbilMeriam == true); 

        // 🔵 PERBAIKAN AMAN: Tunggu sampai objek RangedGroup kosong ATAU semua anaknya hancur secara fisik
        yield return new WaitUntil(() => rangedGroup == null || rangedGroup.GetComponentsInChildren<Transform>().Length <= 1);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

       // --- FASE 5: DIALOG AKHIR & PINDAH GAME UTAMA ---
        // Kita pakai SetInstruksiLayar agar teks berubah, lalu diberi jeda waktu otomatis (3-4 detik)
        SetInstruksiLayar("Guide", "Remember, you can press 1, 2, 3 to switch weapons. Unarmed makes you run faster!");
        yield return new WaitForSeconds(4f); // Tunggu 4 detik agar player sempat membaca

        SetInstruksiLayar("Guide", "Your imagination built this place. Time to wake up. Let's move!");
        yield return new WaitForSeconds(3f); // Tunggu 3 detik

        // Beri pengaman tambahan: Munculkan kursor sebelum pindah scene utama jika dibutuhkan
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pindah ke Gameplay Utama menggunakan LevelManager bawaan Anda
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene("PlayScene", "CircleWipe");
        }
        else
        {
            SceneManager.LoadScene("PlayScene");
        }
    }

    private IEnumerator TampilkanDialog(string nama, string isi)
    {
        dialoguePanel.SetActive(true);
        textNama.text = nama;
        textIsi.text = isi;
        yield return new WaitForSeconds(0.4f); 
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));
    }

    private void SetInstruksiLayar(string nama, string isi)
    {
        dialoguePanel.SetActive(true);
        textNama.text = nama;
        textIsi.text = isi;
    }
}
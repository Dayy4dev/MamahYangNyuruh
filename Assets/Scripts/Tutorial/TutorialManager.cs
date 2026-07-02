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
        // --- FASE 1: DIALOG AWAL (Mencerminkan kebingungan & nostalgia) ---
        yield return StartCoroutine(TampilkanDialog("MC", "Huh? Where am I? Why are my old toys so colossally huge?!"));
        yield return StartCoroutine(TampilkanDialog("MC", "Wait... you're my favorite toy from when I was a kid! You're alive?!"));
        yield return StartCoroutine(TampilkanDialog("Guide", "Welcome back to the Astral Playroom, partner! It's been a long time. Let's get you moving."));

        // --- FASE 2: DETEKSI GERAK (WASD) ---
        SetInstruksiLayar("Guide", "Use WASD to walk around this old dreamscape. Follow me, the other toys have changed...");
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D));
        yield return new WaitForSeconds(2f); 

       // --- FASE 3: DETEKSI PICKUP PEDANG & SERANG (Melawan mainan yang terabaikan) ---
        if (itemsTutorial != null) itemsTutorial.SetActive(true); 
        if (meleeGroup != null) meleeGroup.SetActive(true);       
        
        SetInstruksiLayar("Guide", "Watch out! The abandoned toys are angry. Pick up your old Balloon Sword with F!");
        yield return new WaitUntil(() => sudahAmbilPedang == true);

        SetInstruksiLayar("Guide", "Great! Now Left Click to fight through these forgotten memories!");
        
        // 🔴 PERBAIKAN AMAN: Tunggu sampai objek MeleeGroup kosong ATAU semua anaknya hancur secara fisik
        yield return new WaitUntil(() => meleeGroup == null || meleeGroup.GetComponentsInChildren<Transform>().Length <= 1);
        
        // --- FASE 4: DETEKSI BUANG & GANTI MERIAM ---
        if (rangedGroup != null) rangedGroup.SetActive(true); 
        
        SetInstruksiLayar("Guide", "More of them ahead, and they're too far! Press G to drop your sword!");
        yield return new WaitUntil(() => sudahBuangSenjata == true);

        SetInstruksiLayar("Guide", "Now, press F to grab the Air Cannon, and blast them away!");
        yield return new WaitUntil(() => sudahAmbilMeriam == true); 

        // 🔵 PERBAIKAN AMAN: Tunggu sampai objek RangedGroup kosong ATAU semua anaknya hancur secara fisik
        yield return new WaitUntil(() => rangedGroup == null || rangedGroup.GetComponentsInChildren<Transform>().Length <= 1);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

       // --- FASE 5: DIALOG AKHIR & PINDAH GAME UTAMA (Tema escape & wake up) ---
        // Kita pakai SetInstruksiLayar agar teks berubah, lalu diberi jeda waktu otomatis (3-4 detik)
        SetInstruksiLayar("Guide", "Remember, press 1, 2, 3 to switch weapons. Unarmed makes you run faster to escape!");
        yield return new WaitForSeconds(4f); // Tunggu 4 detik agar player sempat membaca

        SetInstruksiLayar("Guide", "Your imagination built this place, but neglect twisted it. It's time to wake up. Let's move!");
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
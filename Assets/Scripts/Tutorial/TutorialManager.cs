using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Tambahan untuk UI Image
using UnityEngine.SceneManagement;
using UnityEngine.AI; // Tambahan untuk pergerakan AI

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI Dialog (Tinggal Drag)")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI textNama;
    public TextMeshProUGUI textIsi;
    public Image potretUI;        // Drag komponen UI Image untuk wajah di sini
    public Sprite wajahMC;        // Drag gambar 2D wajah MC
    public Sprite wajahGuide;     // Drag gambar 2D wajah karakter kardus

    [Header("Grup Objek (Tinggal Drag)")]
    public GameObject itemsTutorial;      
    public GameObject meleeGroup;          
    public GameObject rangedGroup;         

    [Header("Pergerakan Guide")]
    public NavMeshAgent guideAgent; // Drag karakter Guide kamu (pastikan punya komponen NavMeshAgent)
    public Transform titikSenjata;  // Buat Empty GameObject di dekat lokasi pedang, lalu drag ke sini

    [Header("Status (Otomatis dari Script Lain)")]
    public bool sudahAmbilPedang = false;
    public bool sudahAmbilMeriam = false;
    public bool sudahBuangSenjata = false;
    public int enemiesDefeated = 0;        

    private void Awake() => Instance = this;

    private void Start()
    {
        if(itemsTutorial) itemsTutorial.SetActive(false);
        if(meleeGroup) meleeGroup.SetActive(false);
        if(rangedGroup) rangedGroup.SetActive(false);

        StartCoroutine(AlurTutorialMudah());
    }

    private IEnumerator AlurTutorialMudah()
    {
        // --- FASE 1: DIALOG AWAL ---
        yield return StartCoroutine(TampilkanDialog("MC", "Huh? Where am I? Why are my old toys so colossally huge?!"));
        yield return StartCoroutine(TampilkanDialog("MC", "Wait... you're my favorite toy from when I was a kid! You're alive?!"));
        yield return StartCoroutine(TampilkanDialog("Guide", "Welcome back to the Astral Playroom, partner! It's been a long time. Let's get you moving."));

        // --- FASE 2: DETEKSI GERAK (WASD) ---
        SetInstruksiLayar("Guide", "Use WASD to walk around this old dreamscape. Follow me, the other toys have changed...");
        
        // --- GUIDE MULAI BERJALAN ---
        if (guideAgent != null && titikSenjata != null)
        {
            // Perintahkan guide berjalan ke lokasi titik senjata
            guideAgent.SetDestination(titikSenjata.position);
        }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D));
        yield return new WaitForSeconds(2f); 

       // --- FASE 3: DETEKSI PICKUP PEDANG & SERANG ---
        if (itemsTutorial != null) itemsTutorial.SetActive(true); 
        if (meleeGroup != null) meleeGroup.SetActive(true);       
        
        SetInstruksiLayar("Guide", "Watch out! The abandoned toys are angry. Pick up your old Balloon Sword with F!");
        yield return new WaitUntil(() => sudahAmbilPedang == true);

        SetInstruksiLayar("Guide", "Great! Now Hold Right Click To Aim, Then Left Click to fight through these forgotten memories!");
        
        yield return new WaitUntil(() => meleeGroup == null || meleeGroup.GetComponentsInChildren<Transform>().Length <= 1);
        
        // --- FASE 4: DETEKSI BUANG & GANTI MERIAM ---
        if (rangedGroup != null) rangedGroup.SetActive(true); 
        
        SetInstruksiLayar("Guide", "More of them ahead, and they're too far! Press G to drop your sword!");
        yield return new WaitUntil(() => sudahBuangSenjata == true);

        SetInstruksiLayar("Guide", "Now, press F to grab the Air Cannon, and blast them away!");
        yield return new WaitUntil(() => sudahAmbilMeriam == true); 

        yield return new WaitUntil(() => rangedGroup == null || rangedGroup.GetComponentsInChildren<Transform>().Length <= 1);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

       // --- FASE 5: DIALOG AKHIR & PINDAH GAME UTAMA ---
        SetInstruksiLayar("Guide", "Remember, press 1, 2, 3 to switch weapons. Unarmed makes you run faster to escape!");
        yield return new WaitForSeconds(4f); 

        SetInstruksiLayar("Guide", "Your imagination built this place, but neglect twisted it. It's time to wake up. Let's move!");
        yield return new WaitForSeconds(3f); 

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
        UpdateUI(nama, isi);
        yield return new WaitForSeconds(0.4f); 
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space));
    }

    private void SetInstruksiLayar(string nama, string isi)
    {
        UpdateUI(nama, isi);
    }

    // Fungsi baru untuk menangani pergantian teks dan wajah sekaligus
    private void UpdateUI(string nama, string isi)
    {
        dialoguePanel.SetActive(true);
        textNama.text = nama;
        textIsi.text = isi;

        // Cek siapa yang bicara, lalu ganti potretnya
        if (potretUI != null)
        {
            if (nama == "MC" && wajahMC != null)
                potretUI.sprite = wajahMC;
            else if (nama == "Guide" && wajahGuide != null)
                potretUI.sprite = wajahGuide;
        }
    }
}
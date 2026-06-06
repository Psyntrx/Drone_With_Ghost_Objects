using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstructionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionPanel;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI pageIndicatorText; // e.g. "1 / 4"
    public Button continueButton;
    public TextMeshProUGUI continueButtonText;

    [Header("Slides")]
    [TextArea(3, 6)]
    public string[] slides;

    private int currentSlide = 0;

    private void Start()
    {
        continueButton.onClick.AddListener(OnContinue);
        ShowSlide(0);
        instructionPanel.SetActive(true);
        Time.timeScale = 0f; // Pause game while reading
    }

    private void ShowSlide(int index)
    {
        instructionText.text = slides[index];
        pageIndicatorText.text = $"{index + 1} / {slides.Length}";

        bool isLast = index >= slides.Length - 1;
        continueButtonText.text = isLast ? "Start" : "Continue";
    }

    private void OnContinue()
    {
        currentSlide++;

        if (currentSlide >= slides.Length)
        {
            instructionPanel.SetActive(false);
            Time.timeScale = 1f; // Resume game
        }
        else
        {
            ShowSlide(currentSlide);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Maneja toda la interfaz del juego: HUD (monedas, vidas)
/// y paneles de victoria/derrota con temática de "salvar la isla".
/// Usa TextMeshPro para los textos.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD - Elementos en pantalla")]
    [Tooltip("Texto TMP que muestra el contador de monedas (ej: '5 / 10').")]
    public TMP_Text coinsText;
    [Tooltip("Texto TMP que muestra las vidas restantes.")]
    public TMP_Text livesText;

    [Header("Panel de Victoria")]
    public GameObject winPanel;
    public TMP_Text winMessageText;
    public Button winRestartButton;

    [Header("Panel de Derrota")]
    public GameObject losePanel;
    public TMP_Text loseMessageText;
    public Button loseRestartButton;

    void Start()
    {
        // Ocultar paneles al inicio
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Suscribirse a eventos del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged += UpdateCoinsUI;
            GameManager.Instance.OnLivesChanged += UpdateLivesUI;
            GameManager.Instance.OnGameWon += ShowWinScreen;
            GameManager.Instance.OnGameLost += ShowLoseScreen;

            // Inicializar UI con valores actuales
            UpdateCoinsUI(GameManager.Instance.CoinsCollected);
            UpdateLivesUI(GameManager.Instance.CurrentLives);
        }
        else
        {
            Debug.LogWarning("UIManager: No se encontró GameManager en la escena.");
        }

        // Configurar botones de reinicio
        if (winRestartButton != null)
            winRestartButton.onClick.AddListener(OnRestartClicked);
        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(OnRestartClicked);
    }

    void OnDestroy()
    {
        // Desuscribirse de eventos para evitar errores
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= UpdateCoinsUI;
            GameManager.Instance.OnLivesChanged -= UpdateLivesUI;
            GameManager.Instance.OnGameWon -= ShowWinScreen;
            GameManager.Instance.OnGameLost -= ShowLoseScreen;
        }
    }

    private void UpdateCoinsUI(int coins)
    {
        if (coinsText != null)
        {
            int target = GameManager.Instance != null ? GameManager.Instance.coinsToWin : 0;
            coinsText.text = $"Monedas: {coins} / {target}";
        }
    }

    private void UpdateLivesUI(int lives)
    {
        if (livesText != null)
        {
            // Formato compatible con LiberationSans SDF (sin emojis)
            livesText.text = $"Vidas: x{lives}";
        }
    }

    private void ShowWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winMessageText != null)
                winMessageText.text = "SALVASTE LA ISLA!\nLas monedas ancestrales han restaurado\nel equilibrio de la isla.";
        }
    }

    private void ShowLoseScreen()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            if (loseMessageText != null)
                loseMessageText.text = "LA ISLA FUE DESTRUIDA...\nNo lograste reunir las monedas a tiempo.\nIntentar de nuevo?";
        }
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartLevel();
    }
}

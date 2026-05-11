using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton central que controla el flujo del juego:
/// monedas recogidas, vidas, estados de victoria/derrota.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Won, Lost }

    [Header("Configuración de Victoria")]
    [Tooltip("Cantidad de monedas necesarias para ganar y salvar la isla.")]
    public int coinsToWin = 10;

    [Header("Configuración de Vidas")]
    [Tooltip("Cantidad de vidas con las que empieza el jugador.")]
    public int startingLives = 3;

    // Estado actual
    public GameState CurrentState { get; private set; } = GameState.Playing;
    public int CoinsCollected { get; private set; }
    public int CurrentLives { get; private set; }

    // Eventos para que la UI y otros sistemas reaccionen
    public System.Action<int> OnCoinsChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action OnGameWon;
    public System.Action OnGameLost;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Inicializar estado aquí (Awake) para que esté listo
        // antes de que UIManager.Start() lea los valores
        CurrentLives = startingLives;
        CoinsCollected = 0;
        CurrentState = GameState.Playing;
    }

    void Start()
    {
        // Asegurar que el cursor esté visible y el tiempo corra
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Llamado por Coin.cs cuando el jugador recoge una moneda.
    /// </summary>
    public void CollectCoin()
    {
        if (CurrentState != GameState.Playing) return;

        CoinsCollected++;
        OnCoinsChanged?.Invoke(CoinsCollected);

        Debug.Log($"Moneda recogida: {CoinsCollected}/{coinsToWin}");

        // Verificar condición de victoria
        if (CoinsCollected >= coinsToWin)
        {
            WinGame();
        }
    }

    /// <summary>
    /// Llamado por PlayerHealth cuando el jugador pierde una vida.
    /// Retorna true si el jugador aún tiene vidas, false si se acabaron.
    /// </summary>
    public bool LoseLife()
    {
        if (CurrentState != GameState.Playing) return false;

        CurrentLives--;
        OnLivesChanged?.Invoke(CurrentLives);

        Debug.Log($"Vida perdida. Vidas restantes: {CurrentLives}");

        if (CurrentLives <= 0)
        {
            LoseGame();
            return false;
        }

        return true; // Aún tiene vidas, puede hacer respawn
    }

    private void WinGame()
    {
        CurrentState = GameState.Won;
        Debug.Log("¡VICTORIA! ¡La isla ha sido salvada!");

        // Desbloquear cursor para interactuar con la UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        OnGameWon?.Invoke();
    }

    private void LoseGame()
    {
        CurrentState = GameState.Lost;
        Debug.Log("DERROTA. La isla fue destruida...");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        OnGameLost?.Invoke();
    }

    /// <summary>
    /// Reinicia el nivel actual desde cero (todas las vidas, monedas, etc.)
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

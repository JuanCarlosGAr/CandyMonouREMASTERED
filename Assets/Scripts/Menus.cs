using UnityEngine;
using System.Collections;

public class Menus : MonoBehaviour
{
    // Declara dos variables públicas para arrastrar tus Canvas desde el editor
    public GameObject menuCanvas;
    public GameObject juegoCanvas;
    public GameObject finishCanvas; 
    //public GameObject potions;
    [SerializeField] private PotionBoard potionBoard;
    [SerializeField] private GameManager gameManager;

    // Esta función se llamará cuando se presione el botón
    public void IniciarJuego()
    {
        // Apaga el canvas del menú
        menuCanvas.SetActive(false);
        potionBoard.isStarted = true; // Indica que el juego está activo
        StartCoroutine(LoadingBoard()); // Inicia la corrutina para cargar el tablero después de un retraso
        // Prende el canvas del tablero de juego
        juegoCanvas.SetActive(true);
        // potions.SetActive(true);

       Debug.Log("IniciarJuego() called - Menu canvas deactivated, Juego canvas activated");
       Debug.Log(menuCanvas.activeSelf);
 
    }

    private IEnumerator LoadingBoard()
    {
        yield return new WaitForSeconds(1f);
        potionBoard.InitializeBoard();
    }
    public void RestartGame()
    {
        // Recarga la escena actual para reiniciar el juego
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    public void Finishgame()
    {
        gameManager.ShowGameOverAfterDelay();
    }
}

using UnityEngine;
using System.Collections;

public class Menus : MonoBehaviour
{
    // Declara dos variables p�blicas para arrastrar tus Canvas desde el editor
    public GameObject menuCanvas;
    public GameObject juegoCanvas;
    public GameObject finishCanvas; 
    //public GameObject potions;
    [SerializeField] private PotionBoard potionBoard;
    [SerializeField] private GameManager gameManager;

    // Esta funci�n se llamar� cuando se presione el bot�n
    public void IniciarJuego()
    {
        // Apaga el canvas del men�
        menuCanvas.SetActive(false);
        potionBoard.isStarted = true; // Indica que el juego est� activo
        StartCoroutine(LoadingBoard()); // Inicia la corrutina para cargar el tablero despu�s de un retraso
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

}

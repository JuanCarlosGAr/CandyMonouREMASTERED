using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    public GameObject bombExplosionEffect;  // Efecto para bombas
    public GameObject lightningExplosionEffect; // Efecto para rayos
    [SerializeField] private int maxPowerUps = 4;
    private int currentPowerUps = 0;
    private bool hasPlayedNoMatchSound = false;
    //define the size of the board
    public int width = 6;
    public int height = 8;
    //define some spacing for the board
    public float spacingX;
    public float spacingY;
    //get a reference to our potion prefabs
    public GameObject[] potionPrefabs;
    //get a reference to the collection nodes potionBoard + GO
    public Node[,] potionBoard;
    public GameObject potionBoardGO;

    public List<GameObject> potionsToDestroy = new();
    public GameObject potionParent;

    [SerializeField]
    private Potion selectedPotion;
    private SoundManager soundManager; // Referencia al SoundManager

    [SerializeField]
    private bool isProcessingMove;

    [SerializeField]
    List<Potion> potionsToRemove = new();

    //layoutArray
    public ArrayLayout arrayLayout;
    //public static of potionboard
    public static PotionBoard Instance;

    public GameObject explosionEffect; // Arrastra el prefab desde Unity

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeBoard();
                // Encuentra el objeto SoundManager en la escena
        soundManager = FindObjectOfType<SoundManager>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
            {
                if (isProcessingMove)
                    return;

                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                Debug.Log("I have a clicked a potion it is: " + potion.gameObject);

                SelectPotion(potion);
            }
        }
    }

void InitializeBoard()
{
    currentPowerUps = 0;
    DestroyPotions();
    potionBoard = new Node[width, height];
    spacingX = (float)(width - 1) / 2;
    spacingY = (float)((height - 1) / 2) + 1;

    List<Vector2Int> powerUpPositions = new List<Vector2Int>();

    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            Vector2 position = new Vector2(x - spacingX, y - spacingY);
            if (arrayLayout.rows[y].row[x])
            {
                potionBoard[x, y] = new Node(false, null);
            }
            else
            {
                int randomIndex = 0; // *** Declarada al inicio del bloque
                bool isNearPowerUp = false; // *** Declarada antes de usarse

                // Verificar proximidad
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        int checkX = x + dx;
                        int checkY = y + dy;
                        if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                        {
                            if (powerUpPositions.Contains(new Vector2Int(checkX, checkY)))
                            {
                                isNearPowerUp = true;
                                break;
                            }
                        }
                    }
                    if (isNearPowerUp) break;
                }

                float powerUpChance = 0.05f;
                bool canSpawnPowerUp = currentPowerUps < maxPowerUps 
                                      && !isNearPowerUp 
                                      && Random.value < powerUpChance 
                                      && potionPrefabs.Length > 5;

                if (canSpawnPowerUp)
                {
                    randomIndex = Random.Range(5, 7);
                    currentPowerUps++;
                    powerUpPositions.Add(new Vector2Int(x, y));
                }
                else
                {
                    randomIndex = Random.Range(0, 5);
                }

                GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                potion.transform.SetParent(potionParent.transform);
                potion.GetComponent<Potion>().SetIndicies(x, y);
                potionBoard[x, y] = new Node(true, potion);
                potionsToDestroy.Add(potion);
            }
        }
    }

    if (CheckBoard())
    {
        Debug.Log("Re-generating board due to matches");
        InitializeBoard();
    }
    else
    {
        Debug.Log("Board is valid. Starting game!");
    }
}
    private void DestroyPotions()
    {
        if (potionsToDestroy != null)
        {
            foreach (GameObject potion in potionsToDestroy)
            {
                Destroy(potion);
            }
            potionsToDestroy.Clear();
        }
    }

    public bool CheckBoard()
    {
        if (GameManager.Instance.isGameEnded)
            return false;
        Debug.Log("Checking Board");
        bool hasMatched = false;

        potionsToRemove.Clear();

        foreach(Node nodePotion in potionBoard)
        {
            if (nodePotion.potion != null)
            {
                nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //checking if potion node is usable
                if (potionBoard[x,y].isUsable)
                {
                    //then proceed to get potion class in node.
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    //ensure its not matched
                    if(!potion.isMatched)
                    {
                        //run some matching logic

                        MatchResult matchedPotions = IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            MatchResult superMatchedPotions = Combo(matchedPotions);
                            Debug.Log("NormalMatch");
                            potionsToRemove.AddRange(superMatchedPotions.connectedPotions);

                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                                pot.isMatched = true;

                            hasMatched = true;
                        }
                         if (matchedPotions.connectedPotions.Count >= 4)
                        {
                            MatchResult superMatchedPotions = Combo(matchedPotions);
                            Debug.Log("MoveMatch");
                            potionsToRemove.AddRange(superMatchedPotions.connectedPotions);

                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                                pot.isMatched = true;

                            hasMatched = true;
                        }
                         if (matchedPotions.connectedPotions.Count >= 5)
                        {
                            MatchResult superMatchedPotions = Combo(matchedPotions);
                            Debug.Log("SuperMoveMatch");
                            potionsToRemove.AddRange(superMatchedPotions.connectedPotions);

                            foreach (Potion pot in superMatchedPotions.connectedPotions)
                                pot.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves, bool isPowerUpActivation = false)
    {
        List<Potion> extraPotionsToRemove = new List<Potion>();

        foreach (Potion potionToRemove in potionsToRemove)
        {
            potionToRemove.isMatched = false;

           if (potionToRemove.potionType == PotionType.Bomb)
              {
                Explode(potionToRemove.xIndex, potionToRemove.yIndex, extraPotionsToRemove); 
               }
            else if (potionToRemove.potionType == PotionType.Lightning)
              {
                DestroyRowOrColumn(potionToRemove.xIndex, potionToRemove.yIndex, extraPotionsToRemove);
              }
        }
         potionsToRemove.AddRange(extraPotionsToRemove);

        RemoveAndRefill(potionsToRemove);
        GameManager.Instance.ProcessTurn(potionsToRemove.Count, _subtractMoves, false);
        yield return new WaitForSeconds(0.4f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false, isPowerUpActivation));
        }
    }
    
    private void Explode(int x, int y, List<Potion> potionsList)
    {
        SoundManager.Instance.PlayExplosionSound();
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (i >= 0 && i < width && j >= 0 && j < height)
                {
                    AddPotionToList(i, j, potionsList);
                     if (potionBoard[i, j].potion != null)
                    {
                    Potion potion = potionBoard[i, j].potion.GetComponent<Potion>();
                    potion.customExplosionEffect = bombExplosionEffect; // Efecto de bomba
                    }
                }
            }
        }
    }
        private void DestroyRowOrColumn(int x, int y, List<Potion> potionsList)
    {
         SoundManager.Instance.PlayLightningSound();
        // Destruir fila completa
        for (int i = 0; i < width; i++)
        {
            AddPotionToList(i, y, potionsList);
                    if (potionBoard[i, y].potion != null)
                    {
                      Potion potion = potionBoard[i, y].potion.GetComponent<Potion>();
                      potion.customExplosionEffect = lightningExplosionEffect; // Efecto de rayo
                    }
        }

        // Destruir columna completa
        for (int j = 0; j < height; j++)
        {
            AddPotionToList(x, j, potionsList);
              if (potionBoard[x, j].potion != null)
              {
                  Potion potion = potionBoard[x, j].potion.GetComponent<Potion>();
                  potion.customExplosionEffect = lightningExplosionEffect; // Efecto de rayo
              }
        }
    }
        private void AddPotionToList(int x, int y, List<Potion> potionsList)
    {
        if (potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
        {
            Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
            if (!potionsList.Contains(potion) && !potion.isMatched)
            {
                potion.isMatched = true;
                potionsList.Add(potion);
            }
        }
    }
    private void RemoveAndRefill(List<Potion> _potionsToRemove)
    {
        //Removing the potion and clearing the board at that location
        foreach (Potion potion in _potionsToRemove)
        {
                    // Usar efecto personalizado si existe
              GameObject effectPrefab = potion.customExplosionEffect != null ? 
                                 potion.customExplosionEffect : 
                                 explosionEffect;

                 GameObject effect = Instantiate(effectPrefab, potion.transform.position, Quaternion.identity);
                  Destroy(effect, 2f);

                // Limpiar efecto personalizado para futuras instancias
                 potion.customExplosionEffect = null;
            if (potion.potionType == PotionType.Bomb || potion.potionType == PotionType.Lightning)
            {
            currentPowerUps--;
            }
            Destroy(effect, 1f);
            Destroy(potion.gameObject);
            //getting it's x and y indicies and storing them
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            //Destroy the potion
            Destroy(potion.gameObject);

            //Create a blank node on the potion board.
            potionBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        for (int x=0; x < width; x++)
        {
            for (int y=0; y <height; y++)
            {
                if (potionBoard[x, y].potion == null)
                {
                    Debug.Log("The location X: " + x + " Y: " + y + " is empty, attempting to refill it.");
                    RefillPotion(x, y);
                }
            }
        }
    }

    private void RefillPotion(int x, int y)
    {
        //y offset
        int yOffset = 1;

        //while the cell above our current cell is null and we're below the height of the board
        while (y + yOffset < height && potionBoard[x,y + yOffset].potion == null)
        {
            //increment y offset
            Debug.Log("The potion above me is null, but i'm not at the top of the board yet, so add to my yOffset and try again. Current Offset is: " + yOffset + " I'm about to add 1.");
            yOffset++;
        }

        //we've either hit the top of the board or we found a potion

        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            //we've found a potion

            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            //Move it to the correct location
            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);
            Debug.Log("I've found a potion when refilling the board and it was in the location: [" + x + "," + (y + yOffset) + "] we have moved it to the location: [" + x + "," + y + "]");
            //Move to location
            potionAbove.MoveToTarget(targetPos);
            //update incidices
            potionAbove.SetIndicies(x, y);
            //update our potionBoard
            potionBoard[x, y] = potionBoard[x, y + yOffset];
            //set the location the potion came from to null
            potionBoard[x, y + yOffset] = new Node(true, null);
        }

        //if we've hit the top of the board without finding a potion
        if (y + yOffset == height)
        {
            Debug.Log("I've reached the top of the board without finding a potion");
            SpawnPotionAtTop(x);
        }

    }

    private void SpawnPotionAtTop(int x)
    {
    int randomIndex; // Declaración única
    int index = FindIndexOfLowestNull(x);

    // Lógica para generar power-up
    if (currentPowerUps < maxPowerUps && Random.value < 0.05f && potionPrefabs.Length > 5)
    {
        randomIndex = Random.Range(5, 7);
        currentPowerUps++;
    }
    else
    {
        randomIndex = Random.Range(0, 5);
    }
        int locationToMoveTo = 8 - index;
        Debug.Log("About to spawn a potion, ideally i'd like to put it in the index of: " + index);
        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        //set indicies
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        //set it on the potion board
        potionBoard[x, index] = new Node(true, newPotion);
        //move it to that location
        Vector3 targetPosition = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMoveTo, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);
    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = 7; y >= 0; y--)
        {
            if (potionBoard[x,y].potion == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }




    #region Cascading Potions


    //


    #endregion

    #region MatchingLogic
    private MatchResult Combo(MatchResult _matchedResults)
    {
        //if we have a horizontal or long horizontal match
        if (_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
        {
            //for each potion...
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                //check up
                CheckDirection(pot, new Vector2Int(0, 1), extraConnectedPotions);
                //check down
                CheckDirection(pot, new Vector2Int(0, -1), extraConnectedPotions);

                //do we have 2 or more potions that have been matched against this current potion.
                if (extraConnectedPotions.Count >= 1)
                {
                    Debug.Log("Combo Horizontal x 1");
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);

                    //return our super match
                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            //we didn't have a super match, so return our normal match
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        else if (_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
        {
            //for each potion...
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                //check right
                CheckDirection(pot, new Vector2Int(1, 0), extraConnectedPotions);
                //check left
                CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedPotions);

                //do we have 2 or more potions that have been matched against this current potion.
                if (extraConnectedPotions.Count >= 1)
                {
                    Debug.Log("Combo Vertical x 1");
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    //return our super match
                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            //we didn't have a super match, so return our normal match
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        //this shouldn't be possible, but a null return is required so the method is valid.
        return null;
    }

    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        //check right
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
        //check left
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);
        //have we made a 3 match? (Horizontal Match)
        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal horizontal match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }
        //checking for more than 3 (Long horizontal Match)
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long horizontal match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }
        //clear out the connectedpotions
        connectedPotions.Clear();
        //readd our initial potion
        connectedPotions.Add(potion);

        //check up
        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
        //check down
        CheckDirection(potion, new Vector2Int(0,-1), connectedPotions);

        //have we made a 3 match? (Vertical Match)
        if (connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal vertical match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }
        //checking for more than 3 (Long Vertical Match)
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long vertical match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };
        }
         else
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.None
            };
        }
    }

    void CheckDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
          if (pot.potionType == PotionType.Bomb || pot.potionType == PotionType.Lightning) {
        return;
    }
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        //check that we're within the boundaries of the board
        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x,y].isUsable)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                //does our potionType Match? it must also not be matched
                if(!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
                {
                    connectedPotions.Add(neighbourPotion);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
                
            }
            else
            {
                break;
            }
        }
    }
    #endregion

    #region Swapping Potions

    //select potion
    public void SelectPotion(Potion _potion)
{
    // Si no tenemos una poción seleccionada actualmente, seleccionamos la poción clicada
    if (selectedPotion == null)
    {
        Debug.Log(_potion);
        selectedPotion = _potion;
        selectedPotion.Select(); // Escalar la poción seleccionada
        soundManager.PlaySound(); // Reproducir sonido al seleccionar
    }
    // Si seleccionamos la misma poción dos veces, deseleccionamos
    else if (selectedPotion == _potion)
    {
        selectedPotion.Deselect(); // Restablecer la escala a su tamaño original
        selectedPotion = null;
        soundManager.PlayNoMatchSound();
    }
    // Si hay una poción seleccionada y no es la actual, intentamos hacer un intercambio
    else if (selectedPotion != _potion)
    {
        selectedPotion.Deselect(); // Restablecer la escala de la poción previamente seleccionada
        SwapPotion(selectedPotion, _potion);
        selectedPotion = null;
    }
}
    //swap potion - logic
    private void SwapPotion(Potion _currentPotion, Potion _targetPotion)
    {
        if (!IsAdjacent(_currentPotion, _targetPotion))
        {
            soundManager.PlayNoMatchSound();
            return;
        }

        DoSwap(_currentPotion, _targetPotion);

        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentPotion, _targetPotion));
    }
    //do swap
    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion;

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

        //update indicies.
        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;
        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        _currentPotion.MoveToTarget(potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion.transform.position);

        _targetPotion.MoveToTarget(potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position);
    }

private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion) {
    yield return new WaitForSeconds(0.2f);

    bool isPowerUpMoved = _currentPotion.potionType == PotionType.Bomb || 
                          _currentPotion.potionType == PotionType.Lightning ||
                          _targetPotion.potionType == PotionType.Bomb || 
                          _targetPotion.potionType == PotionType.Lightning;

    // Si se movió un power-up, activarlo directamente
    if (isPowerUpMoved) {
        List<Potion> powerUpEffects = new List<Potion>();
        Potion movedPowerUp = _currentPotion.potionType == PotionType.Bomb || 
                              _currentPotion.potionType == PotionType.Lightning ? 
                              _currentPotion : _targetPotion;

        // Aplicar efecto según el tipo
        if (movedPowerUp.potionType == PotionType.Bomb) {
            Explode(movedPowerUp.xIndex, movedPowerUp.yIndex, powerUpEffects);
        } 
        else if (movedPowerUp.potionType == PotionType.Lightning) {
            DestroyRowOrColumn(movedPowerUp.xIndex, movedPowerUp.yIndex, powerUpEffects);
        }

        // Eliminar el power-up y las fichas afectadas
        powerUpEffects.Add(movedPowerUp);
        RemoveAndRefill(powerUpEffects);
        GameManager.Instance.ProcessTurn(powerUpEffects.Count, true, true); // Restar un movimiento
        yield return new WaitForSeconds(0.4f);

        // Verificar matches después de la explosión (opcional)
        if (CheckBoard()) {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    } 
    else {
        // Lógica original para matches normales
        if (CheckBoard()) {
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        } 
        else {
            DoSwap(_currentPotion, _targetPotion);
            soundManager.PlayNoMatchSound();
        }
    }

    isProcessingMove = false;
}


    //IsAdjacent
    private bool IsAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

    //ProcessMatches

    #endregion

}

public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}



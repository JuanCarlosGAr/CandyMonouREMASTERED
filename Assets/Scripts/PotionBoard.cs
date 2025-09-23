using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    // Variables for UI elements and effects
    private GameObject comboTxt;
    private GameObject SMatchtxt;
    public GameObject bombExplosionEffect;  // Effect for bombs
    public GameObject lightningExplosionEffect; // Effect for lightning
    public GameObject explosionEffect; // Explosion effect prefab
    [SerializeField] private int maxPowerUps = 4;
    private int currentPowerUps = 0;
    private bool hasPlayedNoMatchSound = false;

    // Board dimensions
    public int width = 6;
    public int height = 8;

    // Spacing for the board
    public float spacingX;
    public float spacingY;

    // Potion prefabs and board references
    public GameObject[] potionPrefabs;
    public Node[,] potionBoard;
    public GameObject potionBoardGO;

    // List to keep track of potions to destroy
    public List<GameObject> potionsToDestroy = new();
    public GameObject potionParent;

    // Selected potion and sound manager references
    [SerializeField]
    private Potion selectedPotion;
    private SoundManager soundManager;

    // Flag to check if a move is being processed
    [SerializeField]
    private bool isProcessingMove;

    // List of potions to remove
    [SerializeField]
    List<Potion> potionsToRemove = new();

    // Layout array for the board
    public ArrayLayout arrayLayout;

    // Singleton instance of PotionBoard
    public static PotionBoard Instance;
    
    //swipe
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isDragging = false;

    public bool isStarted = false;

    private void Awake()
    {
        // Set the singleton instance
        Instance = this;
    }

    void Start()
    {
        // Initialize the board and find references
        InitializeBoard();
        soundManager = FindObjectOfType<SoundManager>();
        comboTxt = GameObject.Find("combo_txt");
        SMatchtxt = GameObject.Find("SMatch_txt");

        if (comboTxt != null)
        {
            comboTxt.SetActive(false);
        }
        else
        {
            Debug.LogError("No se encontró el objeto combo_txt en la jerarquía.");
        }
        if (SMatchtxt != null)
        {
            SMatchtxt.SetActive(false);
        }
        else
        {
            Debug.LogError("No se encontró el objeto SMatchtxt en la jerarquía.");
        }

        if (soundManager == null)
        {
            Debug.LogError("No se encontró el objeto SoundManager en la jerarquía.");
        }
        
    }

    private IEnumerator ActivateAndDeactivateCoroutine(float delay)
    {
        // Coroutine to activate and deactivate combo text
        if (comboTxt != null)
        {
            comboTxt.SetActive(true);
            yield return new WaitForSeconds(delay);
            comboTxt.SetActive(false);
        }
        else
        {
            Debug.LogError("comboTxt is null. Cannot activate or deactivate.");
        }
    }
    private IEnumerator SMActivateAndDeactivateCoroutine(float delay)
    {
        // Coroutine to activate and deactivate combo text
        if (SMatchtxt != null)
        {
            SMatchtxt.SetActive(true);
            yield return new WaitForSeconds(delay);
            SMatchtxt.SetActive(false);
        }
        else
        {
            Debug.LogError("SMatchtxt is null. Cannot activate or deactivate.");
        }
    }
    private void Update()
    {
    // Handle mouse input for selecting potions
    if (Input.GetMouseButtonDown(0))
    {
        startTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    if (Input.GetMouseButtonUp(0) && isDragging)
    {
        endTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = endTouchPosition - startTouchPosition;

        if (direction.magnitude > 0.05f) // Minimum swipe distance
        {
            RaycastHit2D startHit = Physics2D.Raycast(startTouchPosition, Vector2.zero);
            RaycastHit2D endHit = Physics2D.Raycast(endTouchPosition, Vector2.zero);

            if (startHit.collider != null && startHit.collider.gameObject.GetComponent<Potion>() &&
                endHit.collider != null && endHit.collider.gameObject.GetComponent<Potion>())
            {
                Potion startPotion = startHit.collider.gameObject.GetComponent<Potion>();
                Potion endPotion = endHit.collider.gameObject.GetComponent<Potion>();

                if (IsAdjacent(startPotion, endPotion))
                {
                    SelectPotion(startPotion);
                    SelectPotion(endPotion);
                }
                else
                {
                    soundManager.PlayNoMatchSound();
                }
            }
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
            {
                if (isProcessingMove)
                    return;

                Potion potion = hit.collider.gameObject.GetComponent<Potion>();
                Debug.Log("I have clicked a potion it is: " + potion.gameObject);

                SelectPotion(potion);
            }
        }

        isDragging = false;
    }
    }

    public void InitializeBoard()
    {
        if (isStarted == false) return;
        // Initialize the board with potions
        currentPowerUps = 0;
        DestroyPotions();
        potionBoard = new Node[width, height];
        spacingX = (float)((width) / 2.7);
        spacingY = (float)((height) / 1.6);

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
                    int randomIndex = GetRandomPotionIndex(x, y, powerUpPositions);
                    GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                    potion.transform.SetParent(potionParent.transform);
                    Potion potionComponent = potion.GetComponent<Potion>();
                    potionComponent.SetIndicies(x, y);
                    potionBoard[x, y] = new Node(true, potion);
                    potionsToDestroy.Add(potion);

                    if (randomIndex >= 6) // Check if it's a power-up
                    {
                        StartCoroutine(ShakePowerUp(potionComponent)); // Start shaking the power-up
                    }
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
            GameManager.Instance.moves = 10;
            Debug.Log("Board is valid. Starting game!");
        }
    }

    private int GetRandomPotionIndex(int x, int y, List<Vector2Int> powerUpPositions)
    {
        // Get a random potion index, considering power-up chances
        bool isNearPowerUp = IsNearPowerUp(x, y, powerUpPositions);
        float powerUpChance = 0.05f;
        bool canSpawnPowerUp = currentPowerUps < maxPowerUps 
                              && !isNearPowerUp 
                              && Random.value < powerUpChance 
                              && potionPrefabs.Length > 7;

        if (canSpawnPowerUp)
        {
            currentPowerUps++;
            powerUpPositions.Add(new Vector2Int(x, y));
            return Random.Range(6, 8);
        }
        else
        {
            return Random.Range(0, 5);
        }
    }

    private bool IsNearPowerUp(int x, int y, List<Vector2Int> powerUpPositions)
    {
        // Check if a position is near a power-up
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
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void DestroyPotions()
    {
        // Destroy all potions in the list
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
        // Check the board for matches
        if (GameManager.Instance.isGameEnded)
            return false;
        Debug.Log("Checking Board");
        bool hasMatched = false;

        potionsToRemove.Clear();

        foreach (Node nodePotion in potionBoard)
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
                if (potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
                {
                    Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();

                    if (!potion.isMatched)
                    {
                        MatchResult matchedPotions = IsConnected(potion);

                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            MatchResult superMatchedPotions = SuperMatch(matchedPotions);

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
        // Process the turn when there are matches on the board
        List<Potion> extraPotionsToRemove = new List<Potion>();

        foreach (Potion potionToRemove in potionsToRemove)
        {
            if (potionToRemove != null)
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
        }
        potionsToRemove.AddRange(extraPotionsToRemove);
        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(ShakeAndDestroyPotions(potionsToRemove));
        GameManager.Instance.ProcessTurn(potionsToRemove.Count, _subtractMoves, false);
        yield return new WaitForSeconds(0.2f); // Increase wait time to ensure destruction animations finish

        if (CheckBoard())
        {
            yield return StartCoroutine(ProcessTurnOnMatchedBoard(false, isPowerUpActivation));
        }
        else
        {
            yield return StartCoroutine(RemoveAndRefillCoroutine(potionsToRemove));
        }
    }

    private void AddPotionToList(int x, int y, List<Potion> potionsList)
    {
        // Add a potion to the list if it is usable and not already matched
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

    private void Explode(int x, int y, List<Potion> potionsList)
    {
        // Handle explosion effect for bomb power-up
        SoundManager.Instance.PlayExplosionSound();
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (i >= 0 && i < width && j >= 0 && j < height)
                {
                    AddPotionToList(i, j, potionsList);
                    if (potionBoard[i, j].potion != null && !(i == x && j == y))
                    {
                        Potion potion = potionBoard[i, j].potion.GetComponent<Potion>();
                        potion.customExplosionEffect = explosionEffect; // Normal match effect
                    }
                }
            }
        }
        // Apply power-up effect at the power-up location
        if (potionBoard[x, y].potion != null)
        {
            Potion powerUpPotion = potionBoard[x, y].potion.GetComponent<Potion>();
            powerUpPotion.customExplosionEffect = bombExplosionEffect; // Bomb effect
        }
    }

    private void DestroyRowOrColumn(int x, int y, List<Potion> potionsList)
    {
        // Handle destruction effect for lightning power-up
        SoundManager.Instance.PlayLightningSound();
        
        // Destroy entire row
        StartCoroutine(InstantiateLightningEffectInSequence(x, y, potionsList, true));

        // Destroy entire column
        StartCoroutine(InstantiateLightningEffectInSequence(x, y, potionsList, false));
    }

    private IEnumerator InstantiateLightningEffectInSequence(int x, int y, List<Potion> potionsList, bool isRow)
    {
     // Apply power-up effect at the power-up location at the beginning
    if (potionBoard[x, y].potion != null)
    {
        Potion powerUpPotion = potionBoard[x, y].potion.GetComponent<Potion>();
        powerUpPotion.customExplosionEffect = lightningExplosionEffect; // Lightning effect
        InstantiateLightningEffect(x, y, !isRow, 0, 0);
    }

    int length = isRow ? width : height;
    int start = isRow ? x : y;

    for (int offset = 0; offset < length; offset++)
    {
        // Positive direction
        int posX = isRow ? start + offset : x;
        int posY = isRow ? y : start + offset;

        if ((isRow && posX < width) || (!isRow && posY < height))
        {
            if ((isRow && posX != x) || (!isRow && posY != y))
            {
                AddPotionToList(posX, posY, potionsList);
                if (potionBoard[posX, posY].potion != null)
                {
                    Potion potion = potionBoard[posX, posY].potion.GetComponent<Potion>();
                    // potion.customExplosionEffect = explosionEffect; // Normal match effect
                }
                InstantiateLightningEffect(posX, posY, !isRow, posX - x, posY - y);
            }
        }

        // Negative direction
        posX = isRow ? start - offset : x;
        posY = isRow ? y : start - offset;

        if ((isRow && posX >= 0) || (!isRow && posY >= 0))
        {
            if ((isRow && posX != x) || (!isRow && posY != y))
            {
                AddPotionToList(posX, posY, potionsList);
                if (potionBoard[posX, posY].potion != null)
                {
                    Potion potion = potionBoard[posX, posY].potion.GetComponent<Potion>();
                    // potion.customExplosionEffect = explosionEffect; // Normal match effect
                }
                InstantiateLightningEffect(posX, posY, !isRow, posX - x, posY - y);
            }
        }

        yield return new WaitForSeconds(0.1f); // Wait a bit before instantiating the next effect
    }

    // Destroy potions after the effects
    yield return StartCoroutine(ShakeAndDestroyPotions(potionsList));
    }

    private void InstantiateLightningEffect(int x, int y, bool isVertical, int offsetX, int offsetY)
    {
        // Instantiate lightning effect at the given position
        Vector3 position = new Vector3(x - spacingX, y - spacingY, 0);
        GameObject effect = Instantiate(lightningExplosionEffect, position, Quaternion.identity);

        // Determine rotation based on offset direction
        if (isVertical)
        {
            if (offsetY > 0)
            {
                effect.transform.Rotate(0, 0, 90); // Rotate 90 degrees for positive vertical
            }
            else if (offsetY < 0)
            {
                effect.transform.Rotate(0, 0, 270); // Rotate 270 degrees for negative vertical
            }
        }
        else
        {
            if (offsetX < 0)
            {
                effect.transform.Rotate(0, 0, 180); // Rotate 180 degrees for negative horizontal
            }
        }

        Destroy(effect, 0.5f); // Destroy the effect after 0.5 seconds
    }

    private IEnumerator ShakeAndDestroyPotions(List<Potion> potionsToRemove)
    {
        // Shake and destroy potions in the list
        float shakeDuration = 0.2f;
        float shakeMagnitude = 15f;

        foreach (Potion potion in potionsToRemove)
        {
            if (potion != null)
            {
                StartCoroutine(ShakePotion(potion, shakeDuration, shakeMagnitude));
            }
        }

        yield return new WaitForSeconds(shakeDuration);

        foreach (Potion potion in potionsToRemove)
        {
            if (potion != null)
            {
                GameObject effectPrefab = null;

                // Check if the potion has a custom explosion effect
                if (potion.customExplosionEffect != null)
                {
                    effectPrefab = potion.customExplosionEffect;
                }
                else if (potion.potionType != PotionType.Bomb && potion.potionType != PotionType.Lightning)
                {
                    // Use normal match effect only if it's not a power-up
                    effectPrefab = explosionEffect;
                }

                if (effectPrefab != null)
                {
                    GameObject effect = Instantiate(effectPrefab, potion.transform.position, Quaternion.identity);
                    Destroy(effect, 0.5f);
                }

                potion.customExplosionEffect = null;

                if (potion.potionType == PotionType.Bomb || potion.potionType == PotionType.Lightning)
                {
                    currentPowerUps--;
                }

                int _xIndex = potion.xIndex;
                int _yIndex = potion.yIndex;

                Destroy(potion.gameObject);

                potionBoard[_xIndex, _yIndex] = new Node(true, null);
            }
        }
    }

    private IEnumerator ShakePotion(Potion potion, float duration, float magnitude)
    {
        // Shake a potion for a given duration and magnitude
        if (potion == null || potion.transform == null)
        {
            yield break;
        }

        Vector3 originalRotation = potion.transform.eulerAngles;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            if (potion == null || potion.transform == null)
            {
                yield break;
            }

            float z = Random.Range(-1f, 1f) * magnitude;
            potion.transform.eulerAngles = new Vector3(originalRotation.x, originalRotation.y, originalRotation.z + z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        if (potion != null && potion.transform != null)
        {
            potion.transform.eulerAngles = originalRotation;
        }
    }

    private IEnumerator ShakePowerUp(Potion powerUp)
    {
        while (powerUp != null && powerUp.gameObject != null)
        {
            yield return StartCoroutine(ShakePotion(powerUp, 1.0f, 15f)); // Shake for 1 second
            yield return new WaitForSeconds(3.0f); // Wait for 3 seconds
        }
    }

    // Modify RemoveAndRefill to use ShakeAndDestroyPotions
    private void RemoveAndRefill(List<Potion> _potionsToRemove)
    {
        StartCoroutine(RemoveAndRefillCoroutine(_potionsToRemove));
    }

    private IEnumerator RemoveAndRefillCoroutine(List<Potion> _potionsToRemove)
    {
        // Coroutine to remove and refill potions
        yield return StartCoroutine(ShakeAndDestroyPotions(_potionsToRemove));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (potionBoard[x, y].potion == null)
                {
                    Debug.Log("The location X: " + x + " Y: " + y + " is empty, attempting to refill it.");
                    RefillPotion(x, y);
                }
            }
        }

        yield return new WaitForSeconds(0.2f); // Increase wait time to ensure destruction animations finish

        if (CheckBoard())
        {
            yield return StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    private void RefillPotion(int x, int y)
    {
        // Refill a potion at the given position
        int yOffset = 1;

        while (y + yOffset < height && potionBoard[x,y + yOffset].potion == null)
        {
            yOffset++;
        }

        if (y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
        {
            Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);
            potionAbove.MoveToTarget(targetPos);
            potionAbove.SetIndicies(x, y);
            potionBoard[x, y] = potionBoard[x, y + yOffset];
            potionBoard[x, y + yOffset] = new Node(true, null);
        }

        if (y + yOffset == height)
        {
            SpawnPotionAtTop(x);
        }
    }

    private void SpawnPotionAtTop(int x)
    {
        // Spawn a new potion at the top of the board
        int randomIndex;
        int index = FindIndexOfLowestNull(x);

        // Logic to generate power-up
        if (currentPowerUps < maxPowerUps && Random.value < 0.05f && potionPrefabs.Length > 6)
        {
            randomIndex = Random.Range(6, 8);
            currentPowerUps++;
        }
        else
        {
            randomIndex = Random.Range(0, 6);
        }
        int locationToMoveTo = 8 - index;
        GameObject newPotion = Instantiate(potionPrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newPotion.transform.SetParent(potionParent.transform);
        newPotion.GetComponent<Potion>().SetIndicies(x, index);
        potionBoard[x, index] = new Node(true, newPotion);
        Vector3 targetPosition = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMoveTo, newPotion.transform.position.z);
        newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);

        if (randomIndex >= 6) // Check if it's a power-up
        {
            StartCoroutine(ShakePowerUp(newPotion.GetComponent<Potion>())); // Start shaking the power-up
        }
    }

    private int FindIndexOfLowestNull(int x)
    {
        // Find the lowest null index in the column
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

    // Placeholder for cascading potions logic

    #endregion

    #region MatchingLogic
    private MatchResult SuperMatch(MatchResult _matchedResults)
    {
        // Check for super matches
        if (_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(pot, new Vector2Int(0, 1), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(0, -1), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    StartCoroutine(ActivateAndDeactivateCoroutine(1.0f));
                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        else if (_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
        {
            foreach (Potion pot in _matchedResults.connectedPotions)
            {
                List<Potion> extraConnectedPotions = new();
                CheckDirection(pot, new Vector2Int(1, 0), extraConnectedPotions);
                CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedPotions);

                if (extraConnectedPotions.Count >= 2)
                {
                    extraConnectedPotions.AddRange(_matchedResults.connectedPotions);
                    StartCoroutine(ActivateAndDeactivateCoroutine(1.0f));
                    return new MatchResult
                    {
                        connectedPotions = extraConnectedPotions,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedPotions = _matchedResults.connectedPotions,
                direction = _matchedResults.direction
            };
        }
        return null;
    }

    MatchResult IsConnected(Potion potion)
    {
        // Check if a potion is connected to others
        List<Potion> connectedPotions = new();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);
        
        if (connectedPotions.Count == 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }
        else if (connectedPotions.Count > 3)
        {
            bool _addMoves = true;
            if (_addMoves)
                GameManager.Instance.moves += 1;

            StartCoroutine(SMActivateAndDeactivateCoroutine(1.0f));
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }
        connectedPotions.Clear();
        connectedPotions.Add(potion);

        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
        CheckDirection(potion, new Vector2Int(0,-1), connectedPotions);

        if (connectedPotions.Count == 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }
        else if (connectedPotions.Count > 3)
        {
            bool _addMoves = true;
            if (_addMoves)
                GameManager.Instance.moves += 1;

            StartCoroutine(SMActivateAndDeactivateCoroutine(1.0f));
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
        // Check for matches in a specific direction
        if (pot.potionType == PotionType.Bomb || pot.potionType == PotionType.Lightning) {
            return;
        }
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x, y].isUsable && potionBoard[x, y].potion != null)
            {
                Potion neighbourPotion = potionBoard[x, y].potion?.GetComponent<Potion>();
                if (neighbourPotion == null) break;

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

    // Select a potion
    public void SelectPotion(Potion _potion)
    {
        if (selectedPotion == null)
        {
            selectedPotion = _potion;
            selectedPotion.Select();
            soundManager.PlaySound();
        }
        else if (selectedPotion == _potion)
        {
            selectedPotion.Deselect();
            selectedPotion = null;
            soundManager.PlayNoMatchSound();
        }
        else if (selectedPotion != _potion)
        {
            if (IsAdjacent(selectedPotion, _potion))
            {
                selectedPotion.Deselect();
                SwapPotion(selectedPotion, _potion);
                selectedPotion = null;
            }
            else
            {
                selectedPotion.Deselect();
                selectedPotion = _potion;
                selectedPotion.Select();
                soundManager.PlaySound();
            }
        }
    }

    // Swap two potions
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

    // Perform the swap
    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion;

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

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

        if (isPowerUpMoved) {
            List<Potion> powerUpEffects = new List<Potion>();
            Potion movedPowerUp = _currentPotion.potionType == PotionType.Bomb || 
                                  _currentPotion.potionType == PotionType.Lightning ? 
                                  _currentPotion : _targetPotion;

            ApplyPowerUpEffect(movedPowerUp, powerUpEffects);

            powerUpEffects.Add(movedPowerUp);
            yield return StartCoroutine(ShakeAndDestroyPotions(powerUpEffects)); // Asegurarse de que se complete ShakeAndDestroyPotions
            yield return new WaitForSeconds(0.2f); // Aumentar el tiempo de espera para asegurar que las animaciones de destrucción terminen
            yield return StartCoroutine(RemoveAndRefillCoroutine(powerUpEffects));
            GameManager.Instance.ProcessTurn(powerUpEffects.Count, true, true);

            if (CheckBoard()) {
                yield return StartCoroutine(ProcessTurnOnMatchedBoard(false));
            }
        } 
        else {
            if (CheckBoard()) {
                yield return StartCoroutine(ProcessTurnOnMatchedBoard(true));
            } 
            else {
                DoSwap(_currentPotion, _targetPotion);
                soundManager.PlayNoMatchSound();
            }
        }

        isProcessingMove = false;
    }

    private void ApplyPowerUpEffect(Potion movedPowerUp, List<Potion> powerUpEffects)
    {
        if (movedPowerUp.potionType == PotionType.Bomb) {
            Explode(movedPowerUp.xIndex, movedPowerUp.yIndex, powerUpEffects);
        } 
        else if (movedPowerUp.potionType == PotionType.Lightning) {
            DestroyRowOrColumn(movedPowerUp.xIndex, movedPowerUp.yIndex, powerUpEffects);
        }
    }

    // Check if two potions are adjacent
    private bool IsAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

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



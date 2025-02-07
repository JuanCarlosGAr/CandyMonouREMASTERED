using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // Detección de matches horizontal, vertical y diagonal
    public List<Vector2Int> FindMatches(PotionBoard board)
    {
        var matches = new List<Vector2Int>();
        var cells = board.GetActiveCells();
        
        foreach (var cell in cells)
        {
            if (!cell.potion)
                continue;
            
            // Verificar horizontal
            CheckHorizontal(cell, cells, matches);
            // Verificar vertical
            CheckVertical(cell, cells, matches);
            // Verificar diagonal derecha
            CheckDiagonalRight(cell, cells, matches);
            // Verificar diagonal izquierda
            CheckDiagonalLeft(cell, cells, matches);
        }
        
        return matches;
    }

    void CheckHorizontal(Vector2Int cell, Dictionary<Vector2Int, Potion> cells, List<Vector2Int> matches)
    {
        var sameColor = new List<Vector2Int>();
        sameColor.Add(cell);

        // Ver a la derecha
        Vector2Int current = cell + Vector2Int.Right;
        while (current.x < 9 && cells.ContainsKey(current) && cells[cell].color == cells[current].color)
        {
            sameColor.Add(current);
            current = current + Vector2Int.Right;
        }

        // Ver a la izquierda
        current = cell - Vector2Int.Right;
        while (current.x >= 0 && cells.ContainsKey(current) && cells[cell].color == cells[current].color)
        {
            sameColor.Add(current);
            current = current - Vector2Int.Right;
        }

        matches.AddRange(sameColor);
    }

    void CheckVertical(Vector2Int cell, Dictionary<Vector2Int, Potion> cells, List<Vector2Int> matches)
    {
        var sameColor = new List<Vector2Int>();
        sameColor.Add(cell);

        // Ver arriba
        Vector2Int current = cell + Vector2Int.Up;
        while (current.y < 9 && cells.ContainsKey(current) && cells[cell].color == cells[current].color)
        {
            sameColor.Add(current);
            current = current + Vector2Int.Up;
        }

        // Ver abajo
        current = cell - Vector2Int.Up;
        while (current.y >= 0 && cells.ContainsKey(current) && cells[cell].color == cells[current].color)
        {
            sameColor.Add(current);
            current = current - Vector2Int.Up;
        }

        matches.AddRange(sameColor);
    }

    void CheckDiagonalRight(Vector2Int cell, Dictionary<Vector2Int, Potion> cells, List<Vector2Int> matches)
    {
        var sameColor = new List<Vector2Int>();
        sameColor.Add(cell);

        // Ver diagonal derecha-arriba
        Vector2Int current = cell + Vector2Int.Right + Vector2Int.Up;
        while (current.x < 9 && current.y < 9 && cells.ContainsKey(current) && cells[cell].color == cells[current].color)
        {
            sameColor.Add(current);
            current += Vector2Int.Right + Vector2Int.Up;
        }

        // Ver diagonal derecha-abajo
        current = cell + Vector2Int.Right - Vector2Int.Up;
        while (current.x < 9 && current.y >= 0 && cells.ContainsKey(current) && cells[cell].color == cells[current].color)
        {
            sameColor.Add(current);
            current += Vector2Int.Right - Vector2Int.Up;
        }

        matches.AddRange(sameColor);
    }

    void CheckDiagonalLeft(Vector2Int cell, Dictionary<Vector2Int, Potion> cells, List<Vector2Int> matches)
    {
        var sameColor = new List<Vector2Int>();
        sameColor.Add(cell);

        // Ver diagonal izquierda-arriba
        Vector2Int current = cell - Vector2Int.Right + Vector2Int.Up;
        while (current.x >= 0 && current.y < 9 && cells.ContainsKey(current) && cells[cell].color == cells[current].color)
        {
            sameColor.Add(current);
            current += Vector2Int.Left + Vector2Int.Up;
        }

        // Ver diagonal izquierda-abajo
        current = cell - Vector2Int.Right - Vector2Int.Up;
        while (current.x >= 0 && current.y >= 0 && cells.ContainsKey(current) && cells[cell].color == cells[current].color)
        {
            sameColor.Add(current);
            current += Vector2Int.Left - Vector2Int.Up;
        }

        matches.AddRange(sameColor);
    }
}

public class ProcessMatches
{
    public static void Process(PotionBoard board)
    {
        var matches = MatchManager.Instance.FindMatches(board);
        if (matches.Count >= 4)
        {
            Dictionary<Vector2Int, int> matchCounts = new Dictionary<Vector2Int, int>();
            
            foreach (var pos in matches)
            {
                if (!matchCounts.ContainsKey(pos))
                    matchCounts[pos] = 0;
                
                matchCounts[pos]++;
            }

            // Calcular los movimientos extras
            int extraMoves = 0;
            foreach (var pair in matchCounts)
            {
                switch (pair.Value)
                {
                    case 4:
                        extraMoves += 1; // +1 move por cada conexión de 4
                        break;
                    case 5:
                        extraMoves += 2; // +2 moves por cada conexión de 5
                        break;
                    // Puedes ajustar este sistema según tus necesidades
                }
            }

            if (extraMoves > 0)
            {
                GameManager.Instance.AddMoves(extraMoves);
                Debug.Log($"Se han agregado {extraMoves} movimientos extras.");
            }
        }
    }
}

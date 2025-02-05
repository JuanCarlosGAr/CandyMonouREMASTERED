using System.Collections.Generic;
using UnityEngine;

public class MatchCalculator : MonoBehaviour {
    public BoardGrid PotionBoard;
    
    // Direcciones posibles (arriba, abajo, izquierda, derecha)
    private List<Direction> directions = new List<Direction>() {
        new Direction(0, 1),
        new Direction(1, 0),
        new Direction(-1, 0),
        new Direction(0, -1)
    };
      public static MatchCalculator Instance; // Singleton reference

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Para que persista entre escenas
        }
        else
        {
            // Si ya hay una instancia, mata esta.gameObject para evitar duplicaciones
            Destroy(gameObject);
        }
    }

    // Método público que otros scripts pueden llamar para calcular las coincidencias
    public void CalculateMatches(List<Potion> potions)
    {
        // Lógica para encontrar y calcular las coincidencias entre losembros de la lista 'potions'
        // Implementa tu lógica de cálculo de coincidencias aquí
    }

    // Devuelve el bonificador basado en la longitud de las conexiones
    private int CalculateBonusMoves() {
        int bonus = 0;
        bool[,] visited = new bool[boardGrid.rows, boardGrid.cols];
        
        for (int i = 0; i < boardGrid.rows; i++) {
            for (int j = 0; j < boardGrid.cols; j++) {
                if (!visited[i, j] && boardGrid.GetCell(i, j).IsOccupied) {
                    int count = ExploreConnection(i, j, visited);
                    if (count >= 3) {
                        bonus += (count - 2); // Ajustar según las reglas deseadas
                    }
                }
            }
        }
        
        return bonus;
    }

    // Función para explorar y contar una conexión
    private int ExploreConnection(int row, int col, bool[,] visited) {
        Queue<Cell> queue = new Queue<Cell>();
        Cell startCell = boardGrid.GetCell(row, col);
        queue.Enqueue(startCell);
        visited[row, col] = true;
        int count = 0;
        
        while (queue.Count > 0) {
            Cell current = queue.Dequeue();
            count++;
            
            foreach (Direction dir in directions) {
                int newRow = current.row + dir.dRow;
                int newCol = current.col + dir.dCol;
                
                if (newRow >= 0 && newRow < boardGrid.rows && 
                    newCol >= 0 && newCol < boardGrid.cols &&
                    !visited[newRow, newCol] &&
                    boardGrid.GetCell(newRow, newCol).IsOccupied &&
                    boardGrid.GetCell(newRow, newCol).type == startCell.type) {
                    
                    visited[newRow, newCol] = true;
                    queue.Enqueue(boardGrid.GetCell(newRow, newCol));
                }
            }
        }
        
        return count;
    }

    // Lógica principal para calcular y aplicar bonificaciones
    public void CalculateAndApplyBonus() {
        int bonusMoves = CalculateBonusMoves();
        GameManager.Instance.ProcessTurn(bonusMoves);
    }
}

// Clase auxiliar para representar direcciones
public class Direction {
    public int dRow;
    public int dCol;

    public Direction(int row, int col) {
        dRow = row;
        dCol = col;
    }
}

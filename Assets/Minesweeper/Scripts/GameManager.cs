using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Board Properties")]
    public Board board;
    [Space(10)]
    [SerializeField]private int width;
    [SerializeField] private int height;
    [Space(10)]
    [SerializeField] private int mineCount;

    [Header("Cell Properties")]
    private Cell[,] cells;
    private Cell[] mineCells;

    [Header("Solver")]
    [SerializeField] private Solver solver;
    private int currentNonMineCellCount;
    private int currentFlaggedCellCount;
    [Space(10)]
    public bool gameOver;

    [Header("UI Manager")]
    [SerializeField] private UIManager uIManager;

    public void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        cells = new Cell[width, height];
        mineCells = new Cell[mineCount];

        currentNonMineCellCount = (width * height) - mineCount;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();
        board.Draw(cells);

        //Changes camera orthographic size to board width respectively
        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
        Camera.main.orthographicSize += width / 2f;
    }

    private void GenerateCells()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Cell cell = new Cell();
                cell.position = new Vector2Int(i, j);
                cell.type = Cell.Type.Empty;
                cells[i, j] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        int x, y;

        for (int i = 0; i < mineCount; i++)
        {
            do
            {
                x = Random.Range(0, width);
                y = Random.Range(0, height);

            } while (cells[x, y].type == Cell.Type.Mine);

            cells[x, y].type = Cell.Type.Mine;
            mineCells[i] = cells[x, y];
        }
    }

    private void GenerateNumbers()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Cell cell = cells[i, j];

                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }
                cell.number = CountMines(i, j);

                if (cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                cells[i, j] = cell;
            }
        }

    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0, x, y;

        for (int adjX = -1; adjX <= 1; adjX++)
        {
            for (int adjY = -1; adjY <= 1; adjY++)
            {
                if (adjX == 0 && adjY == 0)
                {
                    continue;
                }

                x = cellX + adjX;
                y = cellY + adjY;

                if (GetCell(x, y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void Update()
    {
        if (!gameOver)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Flag(GetCellAtMousePosition());
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Reveal(GetCellAtMousePosition());
            }
        }
    }

    public void Flag(Cell cell)
    {
        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }


        currentFlaggedCellCount++;
        cell.flagged = !cell.flagged;
        cells[cell.position.x, cell.position.y] = cell;
        board.DrawSingleCell(cells[cell.position.x, cell.position.y]);

        if (CheckWinCondition()) gameOver = true;
    }

    public void Reveal(Cell cell)
    {
        if (gameOver) return;

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.Empty:
                Flood(cell);
                break;
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Number:
                RevealNumber(cell);
                break;
        }

        if (CheckWinCondition()) gameOver = true;

    }

    public void RevealNumber(Cell cell)
    {
        currentNonMineCellCount--;

        solver.boundaryCells.Add(cell);

        cell.revealed = true;
        cells[cell.position.x, cell.position.y] = cell;
        board.DrawSingleCell(cells[cell.position.x, cell.position.y]);
    }

    public void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        currentNonMineCellCount--;
        
        cell.revealed = true;
        cells[cell.position.x, cell.position.y] = cell;
        board.DrawSingleCell(cells[cell.position.x, cell.position.y]);

        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
        }
        else
        {
            solver.boundaryCells.Add(cell);
        }

        if (CheckWinCondition()) gameOver = true;
    }

    private void Explode(Cell cell)
    {
        uIManager.UpdateStatus("Oops! you stepped on mine");

        gameOver = true;

        cell.revealed = true;
        cell.exploded = true;
        cells[cell.position.x, cell.position.y] = cell;
        board.DrawSingleCell(cells[cell.position.x, cell.position.y]);


        for (int i = 0; i < mineCount; i++)
        {
            if (cell.position != mineCells[i].position)
            {
                mineCells[i].revealed = true;
                cells[cell.position.x, cell.position.y] = mineCells[i];
                board.DrawSingleCell(cells[cell.position.x, cell.position.y]);
            }
        }

        StartCoroutine("ResetLevel");
    }

    //Restarts game on explode after 3 seconds
    public IEnumerator ResetLevel()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(0);
    }

    public bool CheckWinCondition()
    {
        if (currentNonMineCellCount == 0)
        {
            uIManager.UpdateStatus("You revealed all possible cells");
            FlagRemainingCells();
            return true;
        }
        else if (currentFlaggedCellCount == mineCount)
        {
            uIManager.UpdateStatus("You flagged all mines");
            RevealRemainingNumberCells();
            return true;
        }
        else
        {
            return false;
        }
    }

    // Returns a valid cell if available, invalid cell if not
    public Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return cells[x, y];
        }

        return new Cell();
    }

    public Cell GetCellAtMousePosition()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        Vector2Int cellPosition = Vector2Int.zero;

        if (hit && hit.collider != null && hit.collider.CompareTag("Cell"))
        {
            cellPosition.x = (int)hit.collider.transform.position.x;
            cellPosition.y = (int)hit.collider.transform.position.y;
        }
        else
        {
            cellPosition = Vector2Int.one * -1;
        }

        Cell cell = GetCell(cellPosition.x, cellPosition.y);
        return cell;
    }

    // Checks whether the position is out of bounds
    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public Cell RevealRandomCell()
    {
        int x, y;
        do
        {
            x = Random.Range(0, width);
            y = Random.Range(0, height);

        } while (cells[x, y].revealed || cells[x,y].flagged);

        Reveal(cells[x, y]);
        return cells[x,y];
    }

    public void FlagRemainingCells()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!cells[i,j].flagged)
                {
                    cells[i,j].flagged = true;
                    board.DrawSingleCell(cells[i,j]);
                }
            }
        }
    }
   
    public void RevealRemainingNumberCells()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!cells[i, j].revealed && !cells[i,j].flagged)
                {
                    cells[i, j].revealed = true;
                    board.DrawSingleCell(cells[i, j]);
                }
            }
        }
    }

    public void RunSolver()
    {
        solver.StartSolving(cells);
    }
}

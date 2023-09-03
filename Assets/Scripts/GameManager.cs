using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Board Properties")]
    public Board board;
    [Space(10)]
    public int width;
    public int height;
    public int mineCount;

    private Cell[,] cells;
    private Cell[] mineCells;

    private bool gameOver;

    private int nonMineCellCount;

    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        cells = new Cell[width, height];
        mineCells = new Cell[mineCount];

        nonMineCellCount = (width * height) - mineCount;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();
        board.Draw(cells);

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
                cell.position = new Vector3Int(i, j, 0);
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
                Flag();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }
        }
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }

        cell.flagged = !cell.flagged;
        cells[cellPosition.x, cellPosition.y] = cell;
        board.DrawSingleCell(cells[cellPosition.x, cellPosition.y]);
    }


    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        Cell cell = GetCell(cellPosition.x, cellPosition.y);

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
                nonMineCellCount--;
                cell.revealed = true;
                cells[cellPosition.x, cellPosition.y] = cell;
                board.DrawSingleCell(cells[cellPosition.x, cellPosition.y]);
                break;
        }

        CheckWinCondition();
    }

    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        nonMineCellCount--;

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
    }

    private void Explode(Cell cell)
    {
        Debug.Log("Game Over");

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
    }

    private void CheckWinCondition()
    {
        if (nonMineCellCount == 0)
        {
            gameOver = true;
        }
    }

    private Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return cells[x, y];
        }

        return new Cell();
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}

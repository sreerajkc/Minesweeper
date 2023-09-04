
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using UnityEngine;

public class Solver : MonoBehaviour
{
    private IEnumerator solveCor;
    /*public void Solve(Cell[,] cells)
    {
        int width = cells.GetLength(0);
        int height = cells.GetLength(1);

        List<Cell> neighbours, flaggedNeighbours;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                neighbours = GetNeighbours(cells[i, j]);

                if (cells[i, j].number == neighbours.Count)
                {
                    FlagAll(neighbours);
                }

                flaggedNeighbours = GetFlaggedNeighbours(cells[i, j]);

                if (cells[i, j].number == flaggedNeighbours.Count)
                {
                    RevealAll(GetNonFlaggedNeighbours(cells[i, j]));
                }
            }
        }


    }*/

    public Vector2Int cellPos;

    public void Solve(Cell[,] cells)
    {
        /*int width = cells.GetLength(0);
        int height = cells.GetLength(1);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                *//*yield return new WaitForSeconds(1);
                Debug.Log(i + ":" + j + "-" + cells[i, j].revealed);*//*

                if (!cells[i, j].revealed)
                {
                    continue;
                }
                SolveCell(cells[i, j]);
                yield break;

                *//*neighbours = GetNeighbours(cells[i, j]);

                if (cells[i, j].number == neighbours.Count)
                {
                    FlagAll(neighbours);
                }

                flaggedNeighbours = GetFlaggedNeighbours(cells[i, j]);

                if (cells[i, j].number == flaggedNeighbours.Count)
                {
                    RevealAll(GetNonFlaggedNeighbours(cells[i, j]));
                }*//*
            }
        }*/

        SolveCell(cells[cellPos.x, cellPos.y]);
    }

    public void SolveCell(Cell cell, Cell pairedCell = null)
    {
        if (GameManager.Instance.CheckWinCondition())
        {

            return;
        }

        List<Cell> neighbours, flaggedNeighbours;

        neighbours = GetNeighbours(cell);
        if (cell.number == neighbours.Count)
        {
            FlagAll(neighbours);
        }

        flaggedNeighbours = GetFlaggedNeighbours(cell);

        if (cell.number == flaggedNeighbours.Count)
        {
            RevealAll(GetNonFlaggedNeighbours(cell));
        }

        Cell validNeighbour = GetValidNeighbour(cell, pairedCell);

        if (validNeighbour.type == Cell.Type.Invalid)
        {
            return;
        }

        List<Cell> nonFlaggedNeighbours_Cell = GetNonFlaggedNeighbours(cell);
        List<Cell> nonFlaggedNeighbours_b = GetNonFlaggedNeighbours(validNeighbour);

        Debug.LogWarning("A :" + cell.position.x + " : " + cell.position.y);
        Debug.Log("NON FLAGGED NEIGHBOURS A");
        PrintList(nonFlaggedNeighbours_Cell);
        Debug.LogWarning("B :" + validNeighbour.position.x + " : " + validNeighbour.position.y);
        Debug.Log("NON FLAGGED NEIGHBOURS B");
        PrintList(nonFlaggedNeighbours_b);

        List<Cell> tempHashA = nonFlaggedNeighbours_Cell.Except(nonFlaggedNeighbours_b).ToList();
        Debug.Log("A|B");
        PrintList(tempHashA);

        List<Cell> tempHashB = nonFlaggedNeighbours_b.Except(nonFlaggedNeighbours_Cell).ToList();
        Debug.Log("B|A");
        PrintList(tempHashB);

        int aNum = cell.number - flaggedNeighbours.Count;
        int bNum = validNeighbour.number - GetFlaggedNeighbours(validNeighbour).Count;

        if (Mathf.Abs(aNum - bNum) == tempHashA.Count)
        {
            FlagAll(tempHashA.ToList());
            RevealAll(tempHashB.ToList());
        }

        solveCor = SolveAgain(validNeighbour, cell);
        StartCoroutine(solveCor);
    }

    IEnumerator SolveAgain(Cell cell, Cell pairedCell)
    {
        yield return new WaitForSeconds(1);
        SolveCell(cell, pairedCell);
        yield break;
    }

    public Cell GetValidNeighbour(Cell cell, Cell pairedCell)
    {
        int x, y;
        Cell cellToLookAt;

        if (cell.validNeighbours.Count == 0)
        {

        }

        for (int adjX = -1; adjX <= 1; adjX++)
        {
            for (int adjY = -1; adjY <= 1; adjY++)
            {
                if (adjX == 0 && adjY == 0)
                {
                    continue;
                }

                x = cell.position.x + adjX;
                y = cell.position.y + adjY;

                cellToLookAt = GameManager.Instance.GetCell(x, y);

                if (pairedCell != null && cellToLookAt.position == pairedCell.position)
                {
                    continue;
                }

                if (cellToLookAt.type == Cell.Type.Number && cellToLookAt.revealed && GetNeighbours(cellToLookAt).Count != 0)
                {
                    return cellToLookAt;
                }
            }
        }
        return new Cell();
    }

    public List<Cell> GetNeighbours(Cell cell)
    {
        int x, y;
        Cell cellToLookAt;
        List<Cell> neighbours = new List<Cell>();

        for (int adjX = -1; adjX <= 1; adjX++)
        {
            for (int adjY = -1; adjY <= 1; adjY++)
            {
                if (adjX == 0 && adjY == 0)
                {
                    continue;
                }

                x = cell.position.x + adjX;
                y = cell.position.y + adjY;

                cellToLookAt = GameManager.Instance.GetCell(x, y);

                if (cellToLookAt.type != Cell.Type.Invalid && !cellToLookAt.revealed)
                {
                    neighbours.Add(cellToLookAt);
                }
            }
        }
        return neighbours;
    }

    public List<Cell> GetFlaggedNeighbours(Cell cell)
    {
        int x, y;
        Cell adjCell;
        List<Cell> flaggedNeighbours = new List<Cell>();

        for (int adjX = -1; adjX <= 1; adjX++)
        {
            for (int adjY = -1; adjY <= 1; adjY++)
            {
                if (adjX == 0 && adjY == 0)
                {
                    continue;
                }

                x = cell.position.x + adjX;
                y = cell.position.y + adjY;

                adjCell = GameManager.Instance.GetCell(x, y);

                if (adjCell.type != Cell.Type.Invalid && adjCell.flagged)
                {
                    flaggedNeighbours.Add(adjCell);
                }
            }
        }
        return flaggedNeighbours;
    }

    public List<Cell> GetNonFlaggedNeighbours(Cell cell)
    {
        int x, y;
        Cell adjCell;
        List<Cell> nonFlaggedNeighbours = new List<Cell>();

        for (int adjX = -1; adjX <= 1; adjX++)
        {
            for (int adjY = -1; adjY <= 1; adjY++)
            {
                if (adjX == 0 && adjY == 0)
                {
                    continue;
                }

                x = cell.position.x + adjX;
                y = cell.position.y + adjY;

                adjCell = GameManager.Instance.GetCell(x, y);

                if (adjCell.type != Cell.Type.Invalid && !adjCell.revealed && !adjCell.flagged)
                {
                    nonFlaggedNeighbours.Add(adjCell);
                }
            }
        }
        return nonFlaggedNeighbours;
    }

    public void FlagAll(List<Cell> neighbours)
    {
        for (int i = 0; i < neighbours.Count; i++)
        {
            if (!neighbours[i].flagged)
            {
                GameManager.Instance.Flag(neighbours[i]);
            }
        }
    }

    public void RevealAll(List<Cell> neighbours)
    {
        for (int i = 0; i < neighbours.Count; i++)
        {
            if (neighbours[i].type == Cell.Type.Empty)
            {
                GameManager.Instance.Flood(neighbours[i]);
            }
            else
            {
                GameManager.Instance.RevealNumber(neighbours[i]);
            }
        }
    }


    public void PrintList(List<Cell> cells)
    {
        foreach (Cell cell in cells)
        {
            Debug.Log(cell.position.x + ":" + cell.position.y);
        }
    }

}

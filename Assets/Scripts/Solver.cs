﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Solver : MonoBehaviour
{
    private List<Tuple<Cell, Cell>> cellPairs = new List<Tuple<Cell, Cell>>();

    private int width;
    private int height;

    private bool updatedOnce;

    private IEnumerator solveCoroutine;

    public void Solve(Cell[,] cells)
    {
        //First time check
        if (width == 0 || height == 0)
        {
            width = cells.GetLength(0);
            height = cells.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cell cell = cells[x, y];
                    List<Cell> allNeighbours = GetNeighbours(cell);

                    for (int i = 0; i < allNeighbours.Count; i++)
                    {
                        Tuple<Cell, Cell> pair = new Tuple<Cell, Cell>(cell, allNeighbours[i]);
                        cellPairs.Add(pair);
                    }
                }
            }

            GameManager.Instance.RevealRandomCell();
        }
        solveCoroutine = SolveRoutine(cells);
        StartCoroutine(solveCoroutine);
    }

    public IEnumerator SolveRoutine(Cell[,] cells)
    {
        List<Cell> neighbours, flaggedNeighbours;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = cells[x, y];

                if (GameManager.Instance.gameOver)
                {
                    yield break;
                }
                if (!cell.revealed)
                {
                    continue;
                }

                neighbours = GetNeighbours(cell);
                if (cell.number == neighbours.Count)
                {
                    yield return new WaitForSeconds(.1f);
                    updatedOnce = true;
                    FlagAll(neighbours);
                }

                flaggedNeighbours = GetFlaggedNeighbours(cell);
                if (cell.number == flaggedNeighbours.Count)
                {
                    yield return new WaitForSeconds(.1f);
                    updatedOnce = true;
                    RevealAll(GetNonFlaggedNeighbours(cell));
                }
            }
        }

        for (int i = 0; i < cellPairs.Count; i++)
        {
            if (GameManager.Instance.gameOver)
            {
                yield break;
            }

            Cell A = cellPairs[i].Item1;
            Cell B = cellPairs[i].Item2;

            if (!A.revealed || !B.revealed || A.number != 0 || B.number != 0)
            {
                continue;
            }

            List<Cell> nfc_A = GetNonFlaggedNeighbours(A);
            List<Cell> nfc_B = GetNonFlaggedNeighbours(B);

            List<Cell> AdiffB = nfc_A.Except(nfc_B).ToList();
            List<Cell> BdiffA = nfc_B.Except(nfc_A).ToList();

            int validCellCountA = A.number - GetFlaggedNeighbours(A).Count;
            int validCellCountB = B.number - GetFlaggedNeighbours(B).Count;

            if (validCellCountA - validCellCountB == AdiffB.Count)
            {
                yield return new WaitForSeconds(.1f);
                updatedOnce = true;
                FlagAll(AdiffB);
                RevealAll(BdiffA);
            }
        }

        Debug.Log(updatedOnce);

        if (!updatedOnce)
        {
            GameManager.Instance.RevealRandomCell();
        }
        else
        {
            updatedOnce = false;
        }

        StopCoroutine(solveCoroutine);
        solveCoroutine = SolveRoutine(cells);
        StartCoroutine(solveCoroutine);
    }

    public List<Cell> GetNeighbours(Cell cell)
    {
        int x, y;
        Cell neighbour;
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

                neighbour = GameManager.Instance.GetCell(x, y);

                if (neighbour.type != Cell.Type.Invalid && !neighbour.revealed)
                {
                    neighbours.Add(neighbour);
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

                if (adjCell.type != Cell.Type.Invalid && !adjCell.revealed && adjCell.flagged)
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
            GameManager.Instance.Reveal(neighbours[i]);
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

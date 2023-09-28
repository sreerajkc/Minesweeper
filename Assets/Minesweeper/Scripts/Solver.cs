
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Solver uses Sets Algorithm to solve the board.
/// </summary>
public class Solver : MonoBehaviour
{
    //All the possible cellPairs;
    public List<Tuple<Cell, Cell>> cellPairs = new List<Tuple<Cell, Cell>>();

    public List<Cell> boundaryCells;

    [SerializeField] private float timeInterval;

    //Checks whether the board is updated in on iteration
    private bool boardUpdated = false;

    private IEnumerator solveCoroutine;

    public void StartSolving(Cell[,] cells)
    {
        //Randomly reveal a cell at first time
        GameManager.Instance.RevealRandomCell();

        solveCoroutine = SolveRoutine(boundaryCells);
        StartCoroutine(solveCoroutine);
    }


    /// <summary>
    /// Couroutine for checking mines to flag and number cells to reveal
    /// </summary>
    /// <param name="boundaryCells">Revealed number cells with unrevealed cells as neighbours</param>
    /// <returns></returns>
    public IEnumerator SolveRoutine(List<Cell> boundaryCells)
    {
        List<Cell> neighbours, flaggedNeighbours, nonFlaggedNeighbours;

        for (int i = 0; i < boundaryCells.Count; i++)
        {
            Cell cell = boundaryCells[i];

            if (GameManager.Instance.gameOver)
            {
                yield break;
            }

            if (cell.type == Cell.Type.Empty)
            {
                RemoveBoundaryCell(cell);
                continue;
            }

            nonFlaggedNeighbours = GetNonFlaggedNeighbours(cell);
            if (nonFlaggedNeighbours.Count == 0)
            {
                CheckNeighbourCounts(cell);
            }

            neighbours = GetNeighbours(cell);

            if (cell.number == neighbours.Count)
            {
                yield return new WaitForSeconds(timeInterval);
                boardUpdated = true;
                FlagAll(neighbours);
            }

            flaggedNeighbours = GetFlaggedNeighbours(cell);

            if (cell.number == flaggedNeighbours.Count)
            {
                yield return new WaitForSeconds(timeInterval);
                boardUpdated = true;
                RevealAll(GetNonFlaggedNeighbours(cell));
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

            if (!boundaryCells.Contains(A) || !boundaryCells.Contains(B))
            {
                continue;
            }

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
                yield return new WaitForSeconds(timeInterval);
                boardUpdated = true;
                FlagAll(AdiffB);
                RevealAll(BdiffA);
            }
        }

        //Reveals a random cell when board is not updated
        if (!boardUpdated)
        {
            yield return new WaitForSeconds(timeInterval);
            GameManager.Instance.RevealRandomCell();
        }
        else
        {
            boardUpdated = false;
        }


        StopCoroutine(solveCoroutine);
        if (!GameManager.Instance.gameOver)
        {
            solveCoroutine = SolveRoutine(boundaryCells);
            StartCoroutine(solveCoroutine);
        }
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
                boundaryCells.Remove(neighbours[i]);
                GameManager.Instance.Flag(neighbours[i]);
            }
        }
    }

    public void RevealAll(List<Cell> neighbours)
    {
        for (int i = 0; i < neighbours.Count; i++)
        {
            boundaryCells.Add(neighbours[i]);
            GameManager.Instance.Reveal(neighbours[i]);
        }
    }

    public void RemoveBoundaryCell(Cell cell)
    {
        for (int i = 0; i < cellPairs.Count; i++)
        {
            if (cellPairs[i].Item1 == cell || cellPairs[i].Item2 == cell)
            {
                cellPairs.RemoveAt(i);
            }
        }

        boundaryCells.Remove(cell);
    }

    public void RemovePair(Cell a, Cell b)
    {
        cellPairs.Remove(Tuple.Create(a, b));
    }

    public void AddBoundaryCell(Cell cell)
    {
        boundaryCells.Add(cell);

        List<Cell> nfn = GetNonFlaggedNeighbours(cell);

        for (int i = 0; i < nfn.Count; i++)
        {
            List<Cell> fn_a = GetFlaggedNeighbours(cell);
            List<Cell> nfn_a = GetNonFlaggedNeighbours(cell);

            List<Cell> fn_b = GetFlaggedNeighbours(nfn[i]);
            List<Cell> nfn_b = GetNonFlaggedNeighbours(nfn[i]);

            List<Cell> a_u_B = fn_a.Union(fn_b).ToList();

            if (nfn_a.Count == 0 && nfn_b.Count == 0 && fn_a.Count == fn_b.Count && fn_b.Count == a_u_B.Count)
            {
                RemovePair(cell, nfn[i]);
                continue;
            }

            cellPairs.Add(Tuple.Create(cell, nfn[i]));
        }
    }

    private void CheckNeighbourCounts(Cell cell)
    {
        List<Cell> nfn = GetNonFlaggedNeighbours(cell);
        List<int> indexes = new List<int>();

        for (int i = 0; i < nfn.Count; i++)
        {
            if (GetNonFlaggedNeighbours(nfn[i]).Count != 0)
            {
                indexes.Add(i);
            }
        }

        if (indexes.Count == 0)
        {
            RemoveBoundaryCell(cell);
        }
        else
        {
            for (int i = 0; i < indexes.Count; i++)
            {
                RemovePair(cell, nfn[indexes[i]]);
            }
        }
    }

}

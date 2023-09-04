using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Solver : MonoBehaviour
{
    public void Solve(/*List<Cell> cellsToCheck*/ Cell[,] cells)
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


    }

    public IEnumerator SolveRoutine(Cell[,] cells)
    {
        int width = cells.GetLength(0);
        int height = cells.GetLength(1);

        List<Cell> neighbours, flaggedNeighbours;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                yield return new WaitForSeconds(1);
                Debug.Log(i + ":" + j + "-" + cells[i, j].revealed);

                if (!cells[i, j].revealed)
                {
                    continue;
                }

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

    [ContextMenu("R")]
    public void showBorder()
    {
        /*solver.Solve(cells);*/
        StartCoroutine("SolveRoutine", GameManager.Instance.cells);
    }
}

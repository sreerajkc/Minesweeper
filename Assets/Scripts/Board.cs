using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public SpriteRenderer[,] spriteMap;

    public GameObject spritePref;

    public Sprite tileUnknown;
    public Sprite tileEmpty;
    public Sprite tileMine;
    public Sprite tileExploded;
    public Sprite tileFlag;
    public Sprite tileNumber1;
    public Sprite tileNumber2;
    public Sprite tileNumber3;
    public Sprite tileNumber4;
    public Sprite tileNumber5;
    public Sprite tileNumber6;
    public Sprite tileNumber7;
    public Sprite tileNumber8;

    public void Draw(Cell[,] cells)
    {
        int width = cells.GetLength(0);
        int height = cells.GetLength(1);

        spriteMap = new SpriteRenderer[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Cell cell = cells[i, j];
                GameObject go = Instantiate(spritePref, transform);
                go.transform.position = new Vector3(cell.position.x, cell.position.y, 0);
                go.transform.name = "[" + i + "," + j + "]";
                spriteMap[i, j] = go.GetComponent<SpriteRenderer>();
                SetSprite(cell.position, GetSprite(cell));
            }
        }
    }

    private void SetSprite(Vector2Int position, Sprite sprite)
    {
        spriteMap[position.x, position.y].sprite = sprite;
    }

    public void DrawSingleCell(Cell cell)
    {
        SetSprite(cell.position, GetSprite(cell));
    }

    private Sprite GetSprite(Cell cell)
    {
        if (cell.revealed)
        {
            return GetRevealedTile(cell);
        }
        else if (cell.flagged)
        {

            return tileFlag;
        }
        else
        {
            return tileUnknown;
        }
    }

    private Sprite GetRevealedTile(Cell cell)
    {
        switch (cell.type)
        {
            case Cell.Type.Empty: return tileEmpty;
            case Cell.Type.Mine: return (cell.exploded ? tileExploded : tileMine);
            case Cell.Type.Number: return GetNumberTile(cell);
            default: return null;
        }

    }

    private Sprite GetNumberTile(Cell cell)
    {
        switch (cell.number)
        {
            case 1: return tileNumber1;
            case 2: return tileNumber2;
            case 3: return tileNumber3;
            case 4: return tileNumber4;
            case 5: return tileNumber5;
            case 6: return tileNumber6;
            case 7: return tileNumber7;
            case 8: return tileNumber8;
            default: return null;
        }
    }
}

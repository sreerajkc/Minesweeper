
using UnityEngine;

public class Board : MonoBehaviour
{
    private SpriteRenderer[,] spriteMap;

    [Header("Sprite prefab")]
    public GameObject spritePref;

    [Header("Board sprites")]
    public Sprite spritesUnknown;
    public Sprite spriteEmpty;
    public Sprite spriteMine;
    public Sprite spriteExploded;
    public Sprite spriteFlag;
    public Sprite spriteNumber1;
    public Sprite spriteNumber2;
    public Sprite spriteNumber3;
    public Sprite spriteNumber4;
    public Sprite spriteNumber5;
    public Sprite spriteNumber6;
    public Sprite spriteNumber7;
    public Sprite spriteNumber8;

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
            return GetRevealedSprite(cell);
        }
        else if (cell.flagged)
        {

            return spriteFlag;
        }
        else
        {
            return spritesUnknown;
        }
    }

    private Sprite GetRevealedSprite(Cell cell)
    {
        switch (cell.type)
        {
            case Cell.Type.Empty: return spriteEmpty;
            case Cell.Type.Mine: return (cell.exploded ? spriteExploded : spriteMine);
            case Cell.Type.Number: return GetNumberSprite(cell);
            default: return null;
        }

    }

    private Sprite GetNumberSprite(Cell cell)
    {
        switch (cell.number)
        {
            case 1: return spriteNumber1;
            case 2: return spriteNumber2;
            case 3: return spriteNumber3;
            case 4: return spriteNumber4;
            case 5: return spriteNumber5;
            case 6: return spriteNumber6;
            case 7: return spriteNumber7;
            case 8: return spriteNumber8;
            default: return null;
        }
    }
}

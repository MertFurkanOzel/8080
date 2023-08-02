using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MainSC : MonoBehaviour
{
    [SerializeField] ScriptableBlock[] Blocks = new ScriptableBlock[6];
    [SerializeField] GameObject[] Cells;
    [SerializeField] Image BlockHolder;
    [SerializeField] Sprite EmptyCellImage;
    [SerializeField] GridLayoutGroup gridLayoutGroup;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TextMeshProUGUI gameOverScoreText;
    [SerializeField] TextMeshProUGUI highScoreText;
    private CellSC lastCell;
    ScriptableBlock NextBlock;
    private Vector2 startMousePos = Vector2.zero;
    private readonly Stack<Direction> directions = new();
    private readonly Stack<CellSC> swipeCells = new();
    private int swipeCount = 0;
    private int _Score;
    public int Score
    {
        get {return _Score; }
        set {
            _Score = value;
            scoreText.text = value.ToString();
           }
    }
    enum Direction
    {
        none,
        right,
        left,
        up,
        down,
    }
    private void Awake()
    {
        NextBlock = GetRandomBlock();
    }
   
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            List<RaycastResult> raycastResult = new List<RaycastResult>();
            Touch touch = Input.GetTouch(0);
            pointer.position = touch.position;
            EventSystem.current.RaycastAll(pointer, raycastResult);
            foreach (RaycastResult result in raycastResult)
            {
                if (touch.phase == TouchPhase.Began)
                    startMousePos = touch.position;

                GameObject cell = result.gameObject;
                CellSC cellsc = cell.GetComponent<CellSC>();
                if (touch.phase == TouchPhase.Began && result.gameObject.CompareTag("Cell") && cellsc.scriptableBlock == null)
                {
                    //first touch
                    Debug.Log(cellsc.CellNumber());
                    cellsc.scriptableBlock = NextBlock;
                    lastCell = cellsc;
                    directions.Push(Direction.none);
                    swipeCells.Push(cellsc);
                }
                else if(touch.phase!=TouchPhase.Began)
                {
                    if (Vector2.Distance(touch.position, startMousePos) > gridLayoutGroup.cellSize.x)
                    {                            
                        Direction direction = GetDirection(touch.position - startMousePos);
                        CellSC targetCell = DirectionCell(direction, lastCell);
                        if (targetCell == null)
                            return;
                        Debug.Log(targetCell.CellNumber());
                        if (targetCell.scriptableBlock == null && lastCell.scriptableBlock.blockValue > 5 && swipeCells.Contains(lastCell))
                        {
                            swipeCount++;
                            targetCell.scriptableBlock = Blocks[System.Array.IndexOf(Blocks, NextBlock)-swipeCount];
                            lastCell.scriptableBlock= Blocks[System.Array.IndexOf(Blocks, NextBlock) - swipeCount];
                            targetCell.prev = lastCell;
                            lastCell = targetCell;
                            directions.Push(direction);
                            swipeCells.Push(targetCell);
                        }
                        else if (IsReturned(direction))
                        {
                            swipeCount--;
                            lastCell.scriptableBlock = null;
                            lastCell = lastCell.prev;
                            lastCell.scriptableBlock = Blocks[System.Array.IndexOf(Blocks, NextBlock) - swipeCount];
                            directions.Pop();
                            swipeCells.Pop();
                        }
                        startMousePos = touch.position;
                    }
                }
            }
            if (touch.phase == TouchPhase.Ended)
            {
                directions.Clear();
                if(swipeCells.Count>0)
                NextBlock = GetRandomBlock();               
                swipeCount = 0;
                StartCoroutine(Boom(swipeCells));
                swipeCells.Clear();
            }
            raycastResult.Clear();
        }
    }

    private bool IsReturned(Direction direction)
    {
        if (directions.Count > 0)
        {
            return Vector2.Dot(DirectionToVec2(direction), DirectionToVec2(directions.Peek())) switch
            {
                -1 => true,
                _ => false
            };
        }
        else
            return false;
        
    }
    private Vector2 DirectionToVec2(Direction direct)
    {
        switch (direct)
        {
            case Direction.right:
                return Vector2.right;
            case Direction.left:
                return Vector2.left;
            case Direction.up:
                return Vector2.up;
            case Direction.down:
                return Vector2.down;
            default:
                return Vector2.zero;
        }
    }
    private Direction GetDirection(Vector2 vekt)
    {
        //float deg = Vector2.SignedAngle(Vector2.right, vekt);
        float deg = Mathf.Atan2(vekt.y, vekt.x) * Mathf.Rad2Deg;
        if (deg < 0)
        {
            deg = 360 - Mathf.Abs(deg);
        }
        Direction direction = (deg) switch
        {
            (> 315) or (< 45) => Direction.right,
            (> 45) and (< 135) => Direction.up,
            (> 135) and (< 225) => Direction.left,
            (> 225) and (< 315) => Direction.down,
            _ => Direction.none,
        };
        return direction;
    }
    private CellSC DirectionCell(Direction direct, CellSC cell)
    {
        int cellIndex = cell.CellNumber() - 1;
        if (direct == Direction.right && cell.hasNeighborRight)
        {
            return Cells[cellIndex + 1].GetComponent<CellSC>();
        }
        else if (direct == Direction.left && cell.hasNeighborLeft)
        {
            return Cells[cellIndex - 1].GetComponent<CellSC>();
        }
        else if (direct == Direction.up && cell.hasNeighborTop)
        {
            return Cells[cellIndex - 5].GetComponent<CellSC>();
        }
        else if (direct == Direction.down && cell.hasNeighborDown)
        {
            return Cells[cellIndex + 5].GetComponent<CellSC>();
        }
        else
            return null;
    }
    private ScriptableBlock GetRandomBlock()
    {
        ScriptableBlock block = Blocks[Random.Range(1, 101) switch
        {
            (> 0) and (<= 32)   => 0, //%32
            (> 32) and (<= 62)   => 1, //%30
            (> 62) and (<= 87)  => 2, //%25
            (> 87) and (<= 97) => 3, //%10
            (> 97) and (<= 100) => 4, //%3
            _ => 0
        }];
        BlockHolder.sprite = block.sprite;
        return block;
    }
    private List<CellSC> GetSameBlocks(CellSC cell, List<CellSC> sameCells = null)
    {
        if (sameCells == null)
        {
            sameCells = new List<CellSC>();
            sameCells.Add(cell);
        }
        List<CellSC> Neighbor = new List<CellSC>();

        Neighbor.Add(DirectionCell(Direction.right, cell));
        Neighbor.Add(DirectionCell(Direction.left, cell));
        Neighbor.Add(DirectionCell(Direction.up, cell));
        Neighbor.Add(DirectionCell(Direction.down, cell));           
        foreach (var item in Neighbor)
        {
            if (item == null)
                continue;
            if(cell.scriptableBlock==item.scriptableBlock&&!sameCells.Contains(item))
            {
                sameCells.Add(item);
                GetSameBlocks(item,sameCells);
            }
        }
        return sameCells;
    }

    private IEnumerator Boom(Stack<CellSC> cells)
    {
        foreach (var item in cells)
        {
            List<CellSC> Cells = GetSameBlocks(item);
            if (Cells.Count >= 3)
            {
                foreach (var asd in Cells)
                {
                    if (asd != item)
                    asd.scriptableBlock = null;
                }
                if (item.scriptableBlock != null)
                {
                    int blockNumber = System.Array.IndexOf(Blocks, item.scriptableBlock);
                    if (blockNumber >= 5)
                    {
                        Boom8080(item);
                        Score += (Cells.Count - 1) * 1000;
                    }
                    else
                    {
                        Score += Blocks[blockNumber].blockValue*(Cells.Count-1);
                        //Debug.LogError($"block value= {Blocks[blockNumber].blockValue} cells count={Cells.Count - 1}");
                        item.scriptableBlock = Blocks[blockNumber + 1];
                        Stack<CellSC> cell = new Stack<CellSC>();
                        cell.Push(item);
                        yield return new WaitForSeconds(0.25f);
                        StartCoroutine(Boom(cell));
                    }                   
                }
            }
            else if(CellsIsFull())
            {
                gameOver();
            }
        } 
    }

    private void gameOver()
    {
        gameOverPanel.SetActive(true);
        //gameOverScoreText.text = Score.ToString();
        //if(PlayerPrefs.HasKey("HighScore"))
        //{
        //    highScoreText.text = PlayerPrefs.GetString("HighScore");
        //}
    }

    private bool CellsIsFull()
    {
        bool b = true;
        foreach (var item in Cells)
        {
            if(item.GetComponent<CellSC>().scriptableBlock==null)
            {
                b = false;
                break;
            }           
        }
        return b;
    }
    private void Boom8080(CellSC cell)
    {
        List<CellSC> neighbors = new List<CellSC>();
        neighbors.Add(DirectionCell(Direction.up, cell));
        neighbors.Add(cell);
        neighbors.Add(DirectionCell(Direction.down, cell));
        List<CellSC> allNeighbors = new List<CellSC>();
        allNeighbors.AddRange(neighbors);
        foreach (var item in neighbors)
        { 
            if(item!= null)
            {
                if(item.hasNeighborRight)
                {
                    allNeighbors.Add(Cells[item.CellNumber()].GetComponent<CellSC>());
                }
                if(item.hasNeighborLeft)
                {
                    allNeighbors.Add(Cells[item.CellNumber() - 2].GetComponent<CellSC>());
                }
            }
        }
        foreach (var item in allNeighbors)
        {
            if (item != null)
            {
                item.scriptableBlock = null;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellSC : MonoBehaviour
{
    public int row;
    public int column;
    public Sprite defaultSprite;
    public bool hasNeighborLeft, hasNeighborRight, hasNeighborTop, hasNeighborDown;
    private ScriptableBlock _scriptableBlock;

    public CellSC prev;
    public int CellNumber()
    {
        return (row - 1) * 5 + column;
    }
    public ScriptableBlock scriptableBlock
    {
        get { return _scriptableBlock; }
        set
        {
            _scriptableBlock = value;
            if (value != null)
                GetComponent<Image>().sprite = value.sprite;
            else
                GetComponent<Image>().sprite = defaultSprite;
        }
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

public class Move
{
    public int m_UnitID = -1;
    public int m_FromTileID = -1;
    public int m_ToTileID = -1;
    public int m_KilledUnitID = -1; //If we killed an unit on the last turn, the id is here so we can revert it.
};

public class Board : MonoBehaviour, IHasChanged
{
    [SerializeField]
    private GameObject m_TilePrefab = null;

    [SerializeField]
    private GameObject m_ReserveTilePrefab = null;

    [SerializeField]
    private GameObject[] m_UnitPrefab = null;

    [SerializeField]
    private int[] m_UnitAmount = null;

    [SerializeField]
    private Color[] m_Colors;

    private List<GameObject> m_Tiles;
    private List<GameObject> m_Units;
    private Move m_LastMove;

    private bool m_IsInitialized = false;

	// Use this for initialization
	void OnEnable()
    {
        if (!m_IsInitialized)
        {
            GameManager.instance.Subscribe(Callback.OnGetBoard, OnGetBoard);

            m_Tiles = new List<GameObject>();
            m_Units = new List<GameObject>();

            m_LastMove = new Move();

            GenerateBoard();
            GenerateUnits();

            m_IsInitialized = true;
        }
	}
	
    private void GenerateBoard()
    {
        float width = 44;
        float height = 38;

        //Reserve tile
        GameObject obj = GameObject.Instantiate(m_ReserveTilePrefab) as GameObject;
        obj.GetComponent<RectTransform>().SetParent(gameObject.GetComponent<RectTransform>());
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, -225.0f);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(1.0f, 170.0f);
        obj.GetComponent<RectTransform>().localScale = new Vector2(1.2f, 1.2f);
        m_Tiles.Add(obj);

        //11 lines
        int rowWidth = 6;
        float startX = -(width * 3) + (width / 2);
        float startY = height * 5 + 110.0f;
        int startColor = 0;

        for (int i = 0; i < 11; ++i)
        {
            float x = startX;
            int color = startColor;

            for (int j = 0; j < rowWidth; ++j)
            {
                obj = GameObject.Instantiate(m_TilePrefab) as GameObject;
                obj.GetComponent<RectTransform>().SetParent(gameObject.GetComponent<RectTransform>()); //Parent this to us
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, startY);
                obj.GetComponent<Image>().color = m_Colors[color];

                //Manage neighbours backwards (so we are sure they already exist)


                m_Tiles.Add(obj);

                x += width;

                color += 1;
                if (color >= m_Colors.Length) color = 0;
            }

            //Alter the width & start X coord of the new row
            if (i < 5)
            {
                startX -= (width * 0.5f);

                rowWidth += 1;
                startColor += 1;
                if (startColor >= m_Colors.Length) startColor = 0;
            }
            else
            {
                startX += (width * 0.5f);
                rowWidth -= 1;

                startColor -= 1;
                if (startColor < 0) startColor = m_Colors.Length - 1;
            }

            //Alter the y coord of the next row
            startY -= height;
        }

        transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
    }

    private void GenerateUnits()
    {
        //Black & white
        for (int side = 0; side < 2; ++side)
        {
            //Unit type
            for (int unit = 0; unit < m_UnitAmount.Length; ++unit)
            {
                //Spawn number of units
                for (int i = 0; i < m_UnitAmount[unit]; ++i)
                {
                    GameObject obj = GameObject.Instantiate(m_UnitPrefab[unit]) as GameObject;

                    obj.transform.SetParent(m_Tiles[0].transform); //Parent this to the reserve slot
                    obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                    Color color = Color.white;
                    if (side == 1) color = Color.black;

                    obj.GetComponent<Image>().color = color;
                    m_Units.Add(obj);
                }
            }
        }
    }

    private void OnGetBoard()
    {
        string boardInfo = GameManager.instance.CurrentGame.m_Board;

        int unitID = 0;
        while (boardInfo.Length > 0)
        {
            string strTileID = boardInfo.Substring(0, boardInfo.IndexOf("/"));
            boardInfo = boardInfo.Remove(0, boardInfo.IndexOf("/") + 1);

            int tileID = -1;
            Int32.TryParse(strTileID, out tileID);

            if (tileID != -1)
            {
                m_Units[unitID].transform.SetParent(m_Tiles[tileID].transform);
                m_Units[unitID].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            ++unitID;
        }
    }

    public void SubmitMove()
    {
        GameManager.instance.SubmitMove(m_LastMove.m_UnitID, m_LastMove.m_ToTileID, m_LastMove.m_KilledUnitID);
    }

    public void HasChanged(GameObject tileObject)
    {
        //Revert the last move if there is any
        if (m_LastMove.m_UnitID != -1)
        {
            m_Units[m_LastMove.m_UnitID].transform.SetParent(m_Tiles[m_LastMove.m_FromTileID].transform);
            m_Units[m_LastMove.m_UnitID].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            if (m_LastMove.m_KilledUnitID != -1)
            {
                m_Units[m_LastMove.m_KilledUnitID].transform.SetParent(m_Tiles[m_LastMove.m_ToTileID].transform);
                m_Units[m_LastMove.m_KilledUnitID].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
        }

        m_LastMove.m_FromTileID = m_Tiles.IndexOf(Unit.m_DraggedUnit.OldParent.gameObject); //Get the old TileID
        m_LastMove.m_ToTileID =   m_Tiles.IndexOf(tileObject); //Get the new TileID
        m_LastMove.m_UnitID =     m_Units.IndexOf(Unit.m_DraggedUnit.gameObject); //Get the unit ID

        //Get the killed unit ID
        if (tileObject.transform.childCount > 0)
        {
            m_LastMove.m_KilledUnitID = m_Units.IndexOf(tileObject.transform.GetChild(0).gameObject);

            //We killed a unit, move it to the first row
            Transform killedUnit = tileObject.transform.GetChild(0);
            killedUnit.SetParent(m_Tiles[0].transform);
            killedUnit.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            //tileObject.transform.GetChild(0).gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true; //Unable to move
        }
        else
        {
            m_LastMove.m_KilledUnitID = -1;
        }

        //Set the currently dragged item to this tile
        Unit.m_DraggedUnit.gameObject.transform.SetParent(tileObject.transform);
    }
}

namespace UnityEngine.EventSystems
{
    public interface IHasChanged : IEventSystemHandler
    {
        void HasChanged(GameObject obj);
    }
}
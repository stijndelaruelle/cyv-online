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

    public void Reset()
    {
        m_UnitID = -1;
        m_FromTileID = -1;
        m_ToTileID = -1;
        m_KilledUnitID = -1;
    }
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

    [SerializeField]
    private Button m_SubmitButton = null;

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
        obj.GetComponent<RectTransform>().SetParent(transform.parent.GetComponent<RectTransform>());
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, -175.0f);
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(1.0f, 170.0f);
        obj.GetComponent<RectTransform>().localScale = new Vector2(1.0f, 1.0f);
        m_Tiles.Add(obj);

        //11 lines
        int rowWidth = 6;
        float startX = -(width * 3) + (width / 2);
        float startY = height * 5;
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

        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0.0f, 110.0f);
        transform.localScale = new Vector3(0.75f, 0.75f, 1.0f);
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
        //Load the board
        string boardInfo = GameManager.instance.CurrentGame.m_Board;
        GameInfo currentGame = GameManager.instance.CurrentGame;

        //Reset rotation & all the locked states
        transform.rotation = Quaternion.identity;

        foreach (GameObject unit in m_Units)
        {
            unit.transform.rotation = Quaternion.identity;
            unit.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }

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

                //Lock the pieces if you're not allowed to move them
                bool lockUnit = false;

                if (tileID == 0 && currentGame.m_GameState != GameState.Setup) lockUnit = true;

                if (currentGame.m_PlayerColor == GameColor.White)
                {
                    if (currentGame.m_GameState != GameState.WhiteTurn) lockUnit = true;
                    if (unitID > 25)                                    lockUnit = true;
                }

                if (currentGame.m_PlayerColor == GameColor.Black)
                {
                    if (currentGame.m_GameState != GameState.BlackTurn) lockUnit = true;
                    if (unitID <= 25)                                   lockUnit = true;
                }

                if (lockUnit)
                {
                    m_Units[unitID].GetComponent<CanvasGroup>().blocksRaycasts = false;
                }
            }
            ++unitID;
        }

        //Rotate the board if we're black
        if (currentGame.m_PlayerColor == GameColor.Black)
        {
            //Rotate the board
            transform.Rotate(new Vector3(0.0f, 0.0f, 180.0f));

            //Rotate all the pieces, so they still face down
            foreach(GameObject unit in m_Units)
            {
                if (unit.transform.parent != m_Tiles[0].transform)
                {
                    unit.transform.Rotate(new Vector3(0.0f, 0.0f, 180.0f));
                }
            }
        }

        //Change the text of the button to reflect the gamestate
        switch (currentGame.m_GameState)
        {
            case GameState.Setup:
            {
                UpdateSubmitButton("Submit setup", true);
                break;
            }

            case GameState.WhiteTurn:
            {
                if (currentGame.m_PlayerColor == GameColor.White)
                {
                    UpdateSubmitButton("Submit turn", true);
                }
                else
                {
                    UpdateSubmitButton("It's " + currentGame.m_OpponentName + " his turn", false);
                }
                break;
            }

            case GameState.BlackTurn:
            {
                if (currentGame.m_PlayerColor == GameColor.Black)
                {
                    UpdateSubmitButton("Submit turn", true);
                }
                else
                {
                    UpdateSubmitButton("It's " + currentGame.m_OpponentName + " his turn", false);
                }
                break;
            }

            case GameState.GameComplete:
            {
                UpdateSubmitButton("Someone won and someone lost, hooray!", false);
                break;
            }

            default:
                break;
        }
            
        m_LastMove.Reset();
    }

    public void SubmitMove()
    {
        GameInfo currentGame = GameManager.instance.CurrentGame;

        //Is this setup time? 
        if (currentGame.m_GameState == 0)
        {
            //Compile a list of all the tiles
            int fromID = 0;
            int toID = 25;

            if (currentGame.m_PlayerColor == GameColor.Black)
            {
                fromID += 26;
                toID += 26;
            }

            List<int> tileIds = new List<int>();
            for (int unitID = fromID; unitID <= toID; ++unitID)
            {
                int tileID = m_Tiles.IndexOf(m_Units[unitID].transform.parent.gameObject);

                if (tileID < 1)
                {
                    Debug.Log("Not all units were placed!");
                    return;
                }

                tileIds.Add(tileID);
            }

            GameManager.instance.SubmitSetup(tileIds);
        }
        else
        {
            if (m_LastMove.m_UnitID == -1)
            {
                Debug.Log("You didn't move a unit!");
                return;
            }

            //Only move when it's your turn
            if (currentGame.m_PlayerColor == GameColor.White)
            {
                if (currentGame.m_GameState == GameState.WhiteTurn)
                {
                    if (m_LastMove.m_UnitID > 25)
                    {
                        Debug.Log("It's not your unit! You are WHITE, and you moved a BLACK unit!");
                    }
                    else
                    {
                        GameManager.instance.SubmitMove(m_LastMove.m_UnitID, m_LastMove.m_ToTileID, m_LastMove.m_KilledUnitID);
                        m_LastMove.Reset();

                        //Change the text of the button
                        UpdateSubmitButton("It's " + currentGame.m_OpponentName + " his turn", false);
                    }
                }
                else
                {
                    Debug.Log("It's not your turn! You are WHITE, and the turn is for BLACK!");
                }
            }
            else
            {
                if (currentGame.m_GameState == GameState.BlackTurn)
                {
                    if (m_LastMove.m_UnitID <= 25)
                    {
                        Debug.Log("It's not your unit! You are BLACK, and you moved a WHITE unit!");
                    }
                    else
                    {
                        GameManager.instance.SubmitMove(m_LastMove.m_UnitID, m_LastMove.m_ToTileID, m_LastMove.m_KilledUnitID);
                        m_LastMove.Reset();

                        //Change the text of the button
                        UpdateSubmitButton("It's " + currentGame.m_OpponentName + " his turn", false);
                    }
                }
                else
                {
                    Debug.Log("It's not your turn! You are BLACK, and the turn is for WHITE!");
                }
            }
        }
    }

    public void HasChanged(GameObject tileObject)
    {
        //Don't use the last move system when we are in setup mode
        if (GameManager.instance.CurrentGame.m_GameState != 0)
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

            int unitID = m_Units.IndexOf(Unit.m_DraggedUnit.gameObject); //Get the unit ID

            //Only update when we didn't move the same unit
            if (m_LastMove.m_UnitID != unitID)
            {
                m_LastMove.m_FromTileID = m_Tiles.IndexOf(Unit.m_DraggedUnit.OldParent.gameObject); //Get the old TileID
            }
            m_LastMove.m_UnitID = unitID;
            m_LastMove.m_ToTileID = m_Tiles.IndexOf(tileObject); //Get the new TileID


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
        }

        //Set the currently dragged item to this tile
        Unit.m_DraggedUnit.gameObject.transform.SetParent(tileObject.transform);
    }

    private void UpdateSubmitButton(string text, bool enabled)
    {
        m_SubmitButton.gameObject.transform.GetChild(0).GetComponent<Text>().text = text;
        m_SubmitButton.enabled = enabled;

        //Disable all the units as well
        if (!enabled)
        {
            foreach(GameObject unit in m_Units)
            {
                unit.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }
    }
}

namespace UnityEngine.EventSystems
{
    public interface IHasChanged : IEventSystemHandler
    {
        void HasChanged(GameObject obj);
    }
}
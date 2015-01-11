using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class MenuFiller : MonoBehaviour
{
    [SerializeField]
    private GameObject m_GameButton = null;

    [SerializeField]
    private GameObject m_NewGameButton = null;

    [SerializeField]
    private GameObject m_Label = null;

    [SerializeField]
    private RectTransform m_GameListPanel = null;

    private List<GameObject> m_SpawnedButtons = null; //We don't reuse buttons, even tough that's better for memory. This is just a quick prototype.

    private void Start()
    {
        //subscribe
        GameManager.instance.Subscribe(Callback.OnRefresh, OnRefresh);
        m_SpawnedButtons = new List<GameObject>();

        OnRefresh();
    }

    private void OnDestroy()
    {
        //GameManager.instance.Unsubscribe(Callback.OnRefresh, OnRefresh);
    }

    private void OnRefresh()
    {
        ClearObjects();
        List<GameInfo> lstGameInfo = GameManager.instance.GameInfo;

        //Cache data
        List<GameInfo> lstYourTurn = new List<GameInfo>();
        List<GameInfo> lstOpponentsTurn = new List<GameInfo>();
        List<GameInfo> lstCompleted = new List<GameInfo>();

        foreach (GameInfo gameInfo in lstGameInfo)
        {
            if ((gameInfo.m_GameState == GameState.WhiteTurn && gameInfo.m_PlayerColor == GameColor.White) ||
                (gameInfo.m_GameState == GameState.BlackTurn && gameInfo.m_PlayerColor == GameColor.Black) ||
                (gameInfo.m_GameState == GameState.Setup))
            {
                lstYourTurn.Add(gameInfo);
            }

            else if ((gameInfo.m_GameState == GameState.WhiteTurn && gameInfo.m_PlayerColor != GameColor.White) ||
                    (gameInfo.m_GameState == GameState.BlackTurn && gameInfo.m_PlayerColor != GameColor.Black))
            {
                lstOpponentsTurn.Add(gameInfo);
            }

            else if (gameInfo.m_GameState == GameState.GameComplete)
            {
                lstCompleted.Add(gameInfo);
            }
        }

        //-------------------------

        AddLabel("Your turn");
        
        foreach (GameInfo gameInfo in lstYourTurn)
        {
            AddButton(gameInfo.m_GameID, gameInfo.m_OpponentName);
        }

        AddNewGameButton();

        //-------------------------

        if (lstOpponentsTurn.Count > 0)
        {
            AddLabel("Opponents turn");

            foreach (GameInfo gameInfo in lstOpponentsTurn)
            {
                AddButton(gameInfo.m_GameID, gameInfo.m_OpponentName);
            }
        }

        //-------------------------
        if (lstCompleted.Count > 0)
        {
            AddLabel("Completed");

            foreach (GameInfo gameInfo in lstCompleted)
            {
                AddButton(gameInfo.m_GameID, gameInfo.m_OpponentName);
            }
        }
    }

    private void AddLabel(string title)
    {
        if (m_GameListPanel == null) return;

        GameObject label = GameObject.Instantiate(m_Label) as GameObject;
        m_SpawnedButtons.Add(label);

        RectTransform labelTransform = label.GetComponent<RectTransform>();
        labelTransform.SetParent(m_GameListPanel);

        label.GetComponent<Text>().text = title;
    }

    private void AddButton(int gameID, string name)
    {
        if (m_GameListPanel == null) return;

        GameObject btnGame = GameObject.Instantiate(m_GameButton) as GameObject;
        m_SpawnedButtons.Add(btnGame);

        RectTransform btnTransform = btnGame.GetComponent<RectTransform>();
        Button btnButton = btnGame.GetComponent<Button>();

        btnTransform.SetParent(m_GameListPanel);
        btnTransform.GetChild(0).GetComponent<Text>().text = "v.s. " + name;
        btnTransform.sizeDelta = new Vector2(1.0f, 40.0f);

        btnButton.onClick.AddListener(() => GameManager.instance.GetBoard(gameID));
        btnButton.onClick.AddListener(() => transform.parent.gameObject.GetComponent<MenuSlider>().Close());
        btnButton.onClick.AddListener(() => transform.parent.gameObject.GetComponent<MenuSwitcher>().ShowBoardPanel());
    }

    private void AddNewGameButton()
    {
        if (m_GameListPanel == null) return;

        GameObject btnNewGame = GameObject.Instantiate(m_NewGameButton) as GameObject;
        m_SpawnedButtons.Add(btnNewGame);

        RectTransform btnNewTransform = btnNewGame.GetComponent<RectTransform>();
        Button btnNewButton = btnNewGame.GetComponent<Button>();

        btnNewTransform.SetParent(m_GameListPanel);
        btnNewTransform.sizeDelta = new Vector2(1.0f, 40.0f);

        //Add click events
        btnNewButton.onClick.AddListener(() => transform.parent.gameObject.GetComponent<MenuSwitcher>().ShowNewPanel());
        btnNewButton.onClick.AddListener(() => transform.parent.gameObject.GetComponent<MenuSlider>().Close());
    }

    private void ClearObjects()
    {
        foreach(GameObject obj in m_SpawnedButtons)
        {
            GameObject.Destroy(obj);
        }

        m_SpawnedButtons.Clear();
    }
}

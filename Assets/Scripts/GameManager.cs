using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum GameState
{
    Setup,
    WhiteTurn,
    BlackTurn,
    GameComplete
}

public enum GameColor
{
    White,
    Black
}

public enum UnitID
{
    WhiteMountain1,
    WhiteMountain2,
    WhiteMountain3,
    WhiteMountain4,
    WhiteMountain5,
    WhiteMountain6,
    WhiteKing1,
    WhiteRabble1,
    WhiteRabble2,
    WhiteRabble3,
    WhiteRabble4,
    WhiteRabble5,
    WhiteRabble6,
    WhiteCrossbows1,
    WhiteCrossbows2,
    WhiteSpears1,
    WhiteSpears2,
    WhiteLightHorse1,
    WhiteLightHorse2,
    WhiteCatapult1,
    WhiteCatapult2,
    WhiteElephant1,
    WhiteElephant2,
    WhiteHeavyHorse1,
    WhiteHeavyHorse2,
    WhiteDragon,

    BlackMountain1,
    BlackMountain2,
    BlackMountain3,
    BlackMountain4,
    BlackMountain5,
    BlackMountain6,
    BlackKing1,
    BlackRabble1,
    BlackRabble2,
    BlackRabble3,
    BlackRabble4,
    BlackRabble5,
    BlackRabble6,
    BlackCrossbows1,
    BlackCrossbows2,
    BlackSpears1,
    BlackSpears2,
    BlackLightHorse1,
    BlackLightHorse2,
    BlackCatapult1,
    BlackCatapult2,
    BlackElephant1,
    BlackElephant2,
    BlackHeavyHorse1,
    BlackHeavyHorse2,
    BlackDragon,
}

//Should be a struct
public class GameInfo
{
    public int m_GameID;
    public GameColor m_PlayerColor; //What color are we playing?
    public GameState m_GameState;
    public string m_OpponentName;
    public string m_Board;
};

public class GameManager : MonoBehaviour
{
    //Make this a singleton
    private static GameManager m_Instance;
    public static GameManager instance
    {
        get
        {
            if (m_Instance == null) m_Instance = GameObject.FindObjectOfType<GameManager>();
            return m_Instance;
        }
    }

    //delegates
    private event Globals.CallbackHandler OnRefresh = null;
    private event Globals.CallbackHandler OnGetBoard = null;
    
    private string m_CreateGameURL = "http://blargal.com/cyvasse/create_game.php?";
    private string m_GetGamesURL = "http://blargal.com/cyvasse/get_games.php?";
    private string m_GetBoardURL = "http://blargal.com/cyvasse/get_board.php?";
    private string m_MoveURL = "http://blargal.com/cyvasse/move.php?";

    private List<GameInfo> m_GameInfo = new List<GameInfo>();
    public List<GameInfo> GameInfo
    {
        get { return m_GameInfo; }
    }

    private int m_CurrentGameID = 0;
    public GameInfo CurrentGame
    {
        get { return m_GameInfo.Find(x => (x.m_GameID == m_CurrentGameID)); }
    }

    //---------------------
    // Functions
    //---------------------
    public void CreateGame()
    {
        AccountManager accountManager = AccountManager.instance;

        if (!accountManager.LoginCheck()) return;

        if (accountManager.SelectedUserID == -1)
        {
            Debug.Log("WE DON'T HAVE AN OPPONENT!");
            return;
        }

        if (accountManager.ID == accountManager.SelectedUserID)
        {
            Debug.Log("ARE YOU REALLY GOING TO CHALLENGE YOURSELF?");
            return;
        }

        StartCoroutine(CreateGameRoutine());
    }

    public void RefreshGames()
    {
        if (!AccountManager.instance.LoginCheck()) return;
        StartCoroutine(RefreshGamesRoutine());
    }

    public void GetBoard(int gameID)
    {
        if (!AccountManager.instance.LoginCheck()) return;
        StartCoroutine(GetBoardRoutine(gameID));
    }

    public void SubmitMove(int unitID, int tileID, int killedID)
    {
        if (!AccountManager.instance.LoginCheck()) return;
        StartCoroutine(SubmitMoveRoutine(unitID, tileID, killedID));
    }

    public void SubmitSetup(List<int> tileIds)
    {
        if (!AccountManager.instance.LoginCheck()) return;
        StartCoroutine(SubmitSetupRoutine(tileIds));
    }

    //------------
    // Routines
    //------------
    private IEnumerator CreateGameRoutine()
    {
        AccountManager accountManager = AccountManager.instance;

        //Create a hash for security reasons
        string hash = Globals.Md5(accountManager.ID.ToString() + accountManager.SelectedUserID.ToString() + Globals.SECRET_KEY);

        string url = m_CreateGameURL + "p1=" + accountManager.ID +
                                       "&p2=" + accountManager.SelectedUserID +
                                       "&hash=" + hash;

        WWW www = new WWW(url);
        yield return www;

        //Handle technical error
        if (www.error != null)
        {
            Debug.Log("There was an error creating a game in: " + www.error);
            yield return null;
        }

        //Handle user error
        char firstChar = www.text[0]; //get a 1 or 0 (true of false in php)
        string text = www.text.Remove(0, 1);

        if (firstChar == '1')
        {
            Debug.Log("We created a game!");
            RefreshGames();
        }
        else
        {
            Debug.Log(text);
        }
    }

    private IEnumerator RefreshGamesRoutine()
    {
        AccountManager accountManager = AccountManager.instance;

        m_GameInfo.Clear();

        //Create a hash for security reasons
        string hash = Globals.Md5(accountManager.ID.ToString() + Globals.SECRET_KEY);

        string url = m_GetGamesURL + "p=" + accountManager.ID +
                                     "&hash=" + hash;

        WWW www = new WWW(url);
        yield return www;

        //Handle technical error
        if (www.error != null)
        {
            Debug.Log("There was an error getting games in: " + www.error);
            yield return null;
        }

        //Handle user error
        char firstChar = www.text[0]; //get a 1 or 0 (true of false in php)
        string text = www.text.Remove(0, 1);

        if (firstChar == '1')
        {
            while (text.Length > 1)
            {
                GameInfo gameInfo = new GameInfo();
                string strGameID = text.Substring(0, text.IndexOf("/"));
                text = text.Remove(0, text.IndexOf("/") + 1);

                string strGameState = text.Substring(0, text.IndexOf("/"));
                text = text.Remove(0, text.IndexOf("/") + 1);

                string strColor = text.Substring(0, text.IndexOf("/"));
                text = text.Remove(0, text.IndexOf("/") + 1);

                string opponentName = text.Substring(0, text.IndexOf("/"));
                text = text.Remove(0, text.IndexOf("/") + 1);

                int gameID = -1;
                Int32.TryParse(strGameID, out gameID);

                int gameState = -1;
                Int32.TryParse(strGameState, out gameState);

                int color = -1;
                Int32.TryParse(strColor, out color);

                if (gameID >= 0 && gameState >= 0 && color >= 0)
                {
                    gameInfo.m_GameID = gameID;
                    gameInfo.m_GameState = (GameState)gameState;
                    gameInfo.m_PlayerColor = (GameColor)color;
                    gameInfo.m_OpponentName = opponentName;
                    gameInfo.m_Board = "";
                    m_GameInfo.Add(gameInfo);
                }
            }

            //Callback
            if (OnRefresh != null) OnRefresh();
        }
        else
        {
            Debug.Log(text);
        }
    }

    private IEnumerator GetBoardRoutine(int gameID)
    {
        m_CurrentGameID = gameID;

        AccountManager accountManager = AccountManager.instance;

        //Create a hash for security reasons
        string hash = Globals.Md5(accountManager.ID.ToString() + gameID.ToString() + Globals.SECRET_KEY);

        string url = m_GetBoardURL + "p=" + accountManager.ID +
                                     "&b=" + gameID +
                                     "&hash=" + hash;

        WWW www = new WWW(url);
        yield return www;

        //Handle technical error
        if (www.error != null)
        {
            Debug.Log("There was an error getting board in: " + www.error);
            yield return null;
        }

        //Handle user error
        char firstChar = www.text[0]; //get a 1 or 0 (true of false in php)
        string text = www.text.Remove(0, 1);

        if (firstChar == '1')
        {
            GameInfo gameInfo = m_GameInfo.Find(x => (x.m_GameID == gameID));
            gameInfo.m_Board = text;
            if (OnGetBoard != null) OnGetBoard();
        }
        else
        {
            Debug.Log(text);
        }
        yield return null;
    }

    private IEnumerator SubmitMoveRoutine(int unitID, int tileID, int killedID)
    {
        AccountManager accountManager = AccountManager.instance;

        //Create a hash for security reasons
        string hash = Globals.Md5(accountManager.ID.ToString() +CurrentGame.m_GameID.ToString() + unitID.ToString() + tileID.ToString() + killedID.ToString() + Globals.SECRET_KEY);

        string url = m_MoveURL + "p=" + accountManager.ID +
                                 "&g=" + CurrentGame.m_GameID +
                                 "&u=" + unitID +
                                 "&t=" + tileID +
                                 "&k=" + killedID +
                                 "&hash=" + hash;

        WWW www = new WWW(url);
        yield return www;

        //Handle technical error
        if (www.error != null)
        {
            Debug.Log("There was an error moving a unit in: " + www.error);
            yield return null;
        }

        //Handle user error
        char firstChar = www.text[0]; //get a 1 or 0 (true of false in php)
        string text = www.text.Remove(0, 1);

        if (firstChar == '1')
        {
            Debug.Log("We moved a unit!");
        }
        else
        {
            Debug.Log(text);
        }
    }

    private IEnumerator SubmitSetupRoutine(List<int> tileIds)
    {
        AccountManager accountManager = AccountManager.instance;

        //Create a hash for security reasons
        string hashString = accountManager.ID.ToString() + CurrentGame.m_GameID.ToString();
        for (int i = 0; i < tileIds.Count; ++i) { hashString += tileIds[i].ToString(); }
        hashString += Globals.SECRET_KEY;

        string hash = Globals.Md5(hashString);

        //Create the url
        string url = m_MoveURL + "p=" + accountManager.ID +
                                 "&g=" + CurrentGame.m_GameID;

        for (int i = 0; i < tileIds.Count; ++i) { m_MoveURL += "&u" + i + "=" + tileIds[i]; }
        m_MoveURL += "&hash=" + hash;

        //WWW call
        WWW www = new WWW(url);
        yield return www;

        //Handle technical error
        if (www.error != null)
        {
            Debug.Log("There was an error submitting the setup in: " + www.error);
            yield return null;
        }

        //Handle user error
        char firstChar = www.text[0]; //get a 1 or 0 (true of false in php)
        string text = www.text.Remove(0, 1);

        if (firstChar == '1')
        {
            Debug.Log("We did a setup!");
        }
        else
        {
            Debug.Log(text);
        }
    }

    //---------------------
    // Event subscriptions
    //---------------------
    public void Subscribe(Callback callback, Globals.CallbackHandler func)
    {
        switch (callback)
        {
            case Callback.OnRefresh:
                OnRefresh += func;
                break;

            case Callback.OnGetBoard:
                OnGetBoard += func;
                break;

            default:
                break;
        }
    }

    public void Unsubscribe(Callback callback, Globals.CallbackHandler func)
    {
        switch (callback)
        {
            case Callback.OnRefresh:
                OnRefresh -= func;
                break;

            case Callback.OnGetBoard:
                OnGetBoard -= func;
                break;

            default:
                break;
        }
    }
}

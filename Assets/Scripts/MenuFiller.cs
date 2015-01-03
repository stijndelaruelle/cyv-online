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

        float y = 290;

        AddLabel("Your turn", ref y);
        
        foreach (GameInfo gameInfo in lstGameInfo)
        {
            AddButton(gameInfo.m_GameID, gameInfo.m_OpponentName, ref y);
        }

        AddNewGameButton(y);
    }

    private void AddLabel(string title, ref float y)
    {
        GameObject label = GameObject.Instantiate(m_Label) as GameObject;
        m_SpawnedButtons.Add(label);

        RectTransform labelTransform = label.GetComponent<RectTransform>();

        labelTransform.SetParent(gameObject.GetComponent<RectTransform>());
        labelTransform.anchoredPosition = new Vector3(0.0f, y, 0.0f);
        labelTransform.sizeDelta = new Vector2(1.0f, 60.0f);

        label.GetComponent<Text>().text = title;

        y -= label.GetComponent<RectTransform>().sizeDelta.y;
    }

    private void AddButton(int gameID, string name, ref float y)
    {
        GameObject btnGame = GameObject.Instantiate(m_GameButton) as GameObject;
        m_SpawnedButtons.Add(btnGame);

        RectTransform btnTransform = btnGame.GetComponent<RectTransform>();
        Button btnButton = btnGame.GetComponent<Button>();

        btnTransform.SetParent(gameObject.GetComponent<RectTransform>());
        btnTransform.anchoredPosition = new Vector3(0.0f, y, 0.0f);
        btnTransform.sizeDelta = new Vector2(1.0f, 40.0f);

        btnTransform.GetChild(0).GetComponent<Text>().text = "v.s. " + name;

        btnButton.onClick.AddListener(() => GameManager.instance.GetBoard(gameID));
        btnButton.onClick.AddListener(() => gameObject.GetComponent<MenuSlider>().Close());

        y -= btnGame.GetComponent<RectTransform>().sizeDelta.y;
    }

    private void AddNewGameButton(float y)
    {
        GameObject btnNewGame = GameObject.Instantiate(m_NewGameButton) as GameObject;
        m_SpawnedButtons.Add(btnNewGame);

        RectTransform btnNewTransform = btnNewGame.GetComponent<RectTransform>();
        Button btnNewButton = btnNewGame.GetComponent<Button>();

        btnNewTransform.SetParent(gameObject.GetComponent<RectTransform>());
        btnNewTransform.anchoredPosition = new Vector3(0.0f, y, 0.0f);
        btnNewTransform.sizeDelta = new Vector2(1.0f, 40.0f);

        //Add click events
        btnNewButton.onClick.AddListener(() => gameObject.GetComponent<MenuSwitcher>().ShowNewPanel());
        btnNewButton.onClick.AddListener(() => gameObject.GetComponent<MenuSlider>().Close());
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

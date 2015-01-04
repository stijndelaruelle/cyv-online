using UnityEngine;
using System.Collections;

public class MenuSwitcher : MonoBehaviour
{
	public GameObject m_MainPanel;
	public GameObject m_CreateNewPanel;
    public GameObject m_BoardPanel;

    public void ShowMainPanel()
    {
        m_MainPanel.SetActive(true);
        m_CreateNewPanel.SetActive(false);
        m_BoardPanel.SetActive(false);
    }

	public void ShowNewPanel()
	{
		m_MainPanel.SetActive(false);
		m_CreateNewPanel.SetActive(true);
        m_BoardPanel.SetActive(false);
	}

    public void ShowBoardPanel()
    {
        m_MainPanel.SetActive(false);
        m_CreateNewPanel.SetActive(false);
        m_BoardPanel.SetActive(true);
    }
}

using UnityEngine;
using System.Collections;

public class MenuSwitcher : MonoBehaviour
{
	public GameObject m_MainPanel;
	public GameObject m_CreateNewPanel;

	public void ShowNewPanel()
	{
		m_MainPanel.SetActive(false);
		m_CreateNewPanel.SetActive(true);
	}

	public void ShowMainPanel()
	{
		m_MainPanel.SetActive(true);
		m_CreateNewPanel.SetActive(false);
	}
}

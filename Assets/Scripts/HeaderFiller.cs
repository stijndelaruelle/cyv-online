using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HeaderFiller : MonoBehaviour
{
    [SerializeField]
    private Text m_ProfileName;

    private void Start()
    {
        AccountManager.instance.Subscribe(Callback.OnLogin, OnLogin);
	}

    private void OnDestroy()
    {
        AccountManager.instance.Subscribe(Callback.OnLogout, OnLogout);
    }

	private void OnLogin()
    {
        m_ProfileName.text = AccountManager.instance.Username;
    }

    private void OnLogout()
    {
        m_ProfileName.text = "Not logged in!";
    }
}

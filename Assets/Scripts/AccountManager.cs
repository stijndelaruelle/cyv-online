using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class AccountManager : MonoBehaviour
{
    //Make this a singleton
    private static AccountManager m_Instance;
    public static AccountManager instance
    {
        get
        {
            if (m_Instance == null) m_Instance = GameObject.FindObjectOfType<AccountManager>();
            return m_Instance;
        }
    }

    //delegates
    private event Globals.CallbackHandler OnLogin = null;
    private event Globals.CallbackHandler OnLogout = null;

    //Variables
    [SerializeField]
    private InputField m_UsernameField;

    [SerializeField]
    private InputField m_PasswordField;

    [SerializeField]
    private InputField m_SearchField;

    [SerializeField]
    private Text m_TextField; //temp

    private string m_LoginURL = "http://blargal.com/cyvasse/login.php?";
    private string m_RegisterURL = "http://blargal.com/cyvasse/create_account.php?"; //be sure to add a ? to your url
    private string m_SearchAccountsURL = "http://blargal.com/cyvasse/search_account.php?";

    private string m_Username = "";
    public string Username
    {
        get { return m_Username; }
    }

    private int m_ID = -1;
    public int ID
    {
        get { return m_ID; }
    }

    private int m_SelectedUserID = -1;
    public int SelectedUserID
    {
        get { return m_SelectedUserID; }
    }

    //---------------------
    // Functions
    //---------------------
    public void Login()
    {
		StartCoroutine(LoginRoutine(m_UsernameField.text, m_PasswordField.text));
    }

    public void Logout()
    {
        if (!LoginCheck()) return;
        m_Username = "";
        m_ID = -1;
        m_SelectedUserID = -1;

        if (OnLogout != null) OnLogout();
    }

    public void Register()
    {
		StartCoroutine(RegisterRoutine(m_UsernameField.text, m_PasswordField.text));
    }

	public void SearchAccount()
	{
        if (!LoginCheck()) return;
		StartCoroutine(SearchAccountRoutine (m_SearchField.text));
	}

    //------------
    // Routines
    //------------
    private IEnumerator LoginRoutine(string username, string password)
    {
        //Create a hash for security reasons
        string hash = Globals.Md5(username + password + Globals.SECRET_KEY);

        string url = m_LoginURL + "username="  + WWW.EscapeURL(username) +
                                  "&password=" + WWW.EscapeURL(password) +
                                  "&hash="     + hash;

        WWW www = new WWW(url);
        yield return www;

        //Handle technical error
        if (www.error != null)
        {
            Debug.Log("There was an error logging in: " + www.error);
            yield return null;
        }

        //Handle user error
        char firstChar = www.text[0];
        string text = www.text.Remove(0, 1);

        if (firstChar == '1')
        {
            m_Username = username;
            Int32.TryParse(text, out m_ID); //Get our ID

            Debug.Log("LOGGED IN AS " + username + "! (id: " + m_ID + ")");
            if (OnLogin != null) OnLogin(); //Callback
        }
        else
        {
            Debug.Log(text);
        }
    }

    private IEnumerator RegisterRoutine(string username, string password)
    {
        //Create a hash for security reasons
        string hash = Globals.Md5(username + password + Globals.SECRET_KEY);

        string url = m_RegisterURL + "username="  + WWW.EscapeURL(username) +
                                     "&password=" + WWW.EscapeURL(password) +
                                     "&hash="     + hash;

        // Post the URL to the site and create a download object to get the result.
        WWW www = new WWW(url);
        yield return www; // Wait until the download is done

        //Handle technical error
        if (www.error != null)
        {
            Debug.Log("There was an error registering an account: " + www.error);
            yield return null;
        }

        //Handle user error
        char firstChar = www.text[0];
        string text = www.text.Remove(0, 1);

        if (firstChar == '1')
        {
            Debug.Log("REGISTERED AS " + username + "!");
            Login();
        }
        else
        {
            Debug.Log(text);
        }
    }

    private IEnumerator SearchAccountRoutine(string username)
	{
		//Create a hash for security reasons
        string hash = Globals.Md5(username + Globals.SECRET_KEY);

        string url = m_SearchAccountsURL + "username=" + WWW.EscapeURL(username) +
										   "&hash="    + WWW.EscapeURL(hash);

		// Post the URL to the site and create a download object to get the result.
        WWW www = new WWW(url);
        yield return www; // Wait until the download is done

        //Handle technical error
        if (www.error != null)
		{
			Debug.Log("There was an error searching for accounts: " + www.error);
            yield return null;
		}

        //Handle user error
        char firstChar = www.text[0]; //get a 1 or 0 (true of false in php)
        string text = www.text.Remove(0, 1);

        if (firstChar == '1')
        {
            string strOpponentID = text.Substring(0, text.IndexOf("/"));
            text = text.Remove(0, text.IndexOf("/") + 1);

            Int32.TryParse(strOpponentID, out m_SelectedUserID);
            m_TextField.text = text;

            Debug.Log("FOUND USER " + text + "! (id: " + m_SelectedUserID + ")"); 
        }
        else
        {
            m_TextField.text = text;
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
            case Callback.OnLogin:
                OnLogin += func;
                break;

            case Callback.OnLogout:
                OnLogout += func;
                break;

            default:
                break;
        }
    }

    public void Unsubscribe(Callback callback, Globals.CallbackHandler func)
    {
        switch (callback)
        {
            case Callback.OnLogin:
                OnLogin -= func;
                break;

            case Callback.OnLogout:
                OnLogout -= func;
                break;

            default:
                break;
        }
    }

    //------------
    // Utility
    //------------
    public bool LoginCheck()
    {
        if (m_ID == -1)
        {
            Debug.Log("WE ARE NOT LOGGED IN!");
        }

        return (m_ID != -1);
    }
}
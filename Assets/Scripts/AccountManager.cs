using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class AccountManager : MonoBehaviour
{
    public InputField m_UsernameField;
    public InputField m_PasswordField;
	public InputField m_SearchField;
	public Text m_TextField; //temp

    private string m_SecretKey = "CyvasseKey"; // Edit this value and make sure it's the same as the one stored on the server

    private string m_LoginURL = "http://blargal.com/cyvasse/login.php?";
    private string m_RegisterURL = "http://blargal.com/cyvasse/create_account.php?"; //be sure to add a ? to your url
    private string m_SearchAccountsURL = "http://blargal.com/cyvasse/search_account.php?";
    private string m_CreateGameURL = "http://blargal.com/cyvasse/create_game.php?";

    private string m_Username = ""; //Reason we keep strings instead of user id's is for security. (let the server decide on ID's)
    private string m_OpponentName = "";

    public void Login()
    {
		StartCoroutine(LoginRoutine(m_UsernameField.text, m_PasswordField.text));
    }

    public void Logout()
    {
        StartCoroutine(LogoutRoutine());
    }

    public void Register()
    {
		StartCoroutine(RegisterRoutine(m_UsernameField.text, m_PasswordField.text));
    }

	public void SearchAccount()
	{
		StartCoroutine(SearchAccountRoutine (m_SearchField.text));
	}

    public void CreateGame()
    {
        if (m_Username == "")
        {
            m_TextField.text = "You are not logged in?";
            Debug.Log("WE ARE NOT LOGGED IN!");
            return;
        }

        if (m_OpponentName == "")
        {
            Debug.Log("WE DON'T HAVE AN OPPONENT!");
            return;
        }

        if (m_Username == m_OpponentName)
        {
            Debug.Log("ARE YOU REALLY GOING TO CHALLENGE YOURSELF?");
            return;
        }

        StartCoroutine(CreateGameRoutine());
    }

    //------------
    // Routines
    //------------
    private IEnumerator LoginRoutine(string username, string password)
    {
        //Create a hash for security reasons
        string hash = Md5Sum(username + password + m_SecretKey);

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
            Debug.Log("LOGGED IN AS " + username + "!");
            m_Username = username;
        }
        else
        {
            Debug.Log(text);
        }
    }

    private IEnumerator LogoutRoutine()
    {
        yield return null;
    }

    private IEnumerator RegisterRoutine(string username, string password)
    {
        //Create a hash for security reasons
        string hash = Md5Sum(username + password + m_SecretKey);

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
		string hash = Md5Sum(username + m_SecretKey);

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
            m_TextField.text = text;
            m_OpponentName = username;
        }
        else
        {
            m_TextField.text = text;
            Debug.Log(text);
        }
	}

    private IEnumerator CreateGameRoutine()
    {
        //Create a hash for security reasons
        string hash = Md5Sum(m_Username + m_OpponentName + m_SecretKey);

        string url = m_CreateGameURL + "p1=" + WWW.EscapeURL(m_Username) +
                                       "&p2=" + WWW.EscapeURL(m_OpponentName) +
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
        }
        else
        {
            Debug.Log(text);
        }
    }

    private string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }
}
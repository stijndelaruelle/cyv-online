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

    public void Login()
    {
		StartCoroutine(LoginRoutine(m_UsernameField.text, m_PasswordField.text));
    }

    public void Logout()
    {

    }

    public void Register()
    {
		StartCoroutine(RegisterRoutine(m_UsernameField.text, m_PasswordField.text));
    }

	public void SearchAccount()
	{
		StartCoroutine(SearchAccountRoutine (m_SearchField.text));
	}

    //------------
    // Routines
    //------------
    IEnumerator LoginRoutine(string username, string password)
    {
        //Create a hash for security reasons
        string hash = Md5Sum(username + password + m_SecretKey);

        string get_url = m_LoginURL + "username="  + WWW.EscapeURL(username) +
                                      "&password=" + password +
                                      "&hash="     + hash;

        WWW get = new WWW(get_url);
        yield return get;

        //Handle error
        if (get.error != null)
        {
            Debug.Log("There was an error logging in: " + get.error);
        }

        //Login succeeded
        if (get.text == "1")
        {
            Debug.Log("LOGGED IN AS " + username + "!");
        }
        else
        {
            Debug.Log(get.text);
        }
    }

    IEnumerator RegisterRoutine(string username, string password)
    {
        //Create a hash for security reasons
        string hash = Md5Sum(username + password + m_SecretKey);

        string post_url = m_RegisterURL + "username="  + WWW.EscapeURL(username) +
                                        "&password=" + password +
                                        "&hash="     + hash;

        // Post the URL to the site and create a download object to get the result.
        WWW post = new WWW(post_url);
        yield return post; // Wait until the download is done

        //Handle error
        if (post.error != null)
        {
            Debug.Log("There was an error registering an account: " + post.error);
            yield return null;
        }
    }

	//Crazy version of SearchAccountRoutine, not used because searching this way would take pretty long
	IEnumerator SearchAccountRoutine(string username)
	{
		//Create a hash for security reasons
		string hash = Md5Sum(username + m_SecretKey);
		
		string get_url = m_SearchAccountsURL + "username=" + WWW.EscapeURL(username) +
											   "&hash="    + hash;

		// Post the URL to the site and create a download object to get the result.
		WWW get = new WWW(get_url);
		yield return get; // Wait until the download is done

		//Handle error
		if (get.error != null)
		{
			Debug.Log("There was an error searching for accounts: " + get.error);
		}

		//Use info
		m_TextField.text = get.text;
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
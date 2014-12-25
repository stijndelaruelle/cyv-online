using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class AccountManager : MonoBehaviour
{
    public InputField username;
    public InputField password;
    public Text accounts;

    private string secretKey = "CyvasseKey"; // Edit this value and make sure it's the same as the one stored on the server

    private string loginURL = "http://blargal.com/cyvasse/login.php?";
    private string registerURL = "http://blargal.com/cyvasse/create_account.php?"; //be sure to add a ? to your url
    private string listAccountsURL = "http://blargal.com/cyvasse/list_accounts.php";

    public void Start()
    {
        ListAccounts();
    }

    public void Login()
    {
        StartCoroutine(LoginRoutine(username.text, password.text));
    }

    public void Logout()
    {

    }

    public void Register()
    {
        StartCoroutine(RegisterRoutine(username.text, password.text));
    }

    public void ListAccounts()
    {
        StartCoroutine(ListAccountsRoutine());
    }

    //------------
    // Routines
    //------------
    IEnumerator LoginRoutine(string username, string password)
    {
        //Create a hash for security reasons
        string hash = Md5Sum(username + password + secretKey);

        string get_url = loginURL + "username=" + WWW.EscapeURL(username) +
                                    "&password=" + password +
                                    "&hash=" + hash;

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
        string hash = Md5Sum(username + password + secretKey);

        string post_url = registerURL + "username=" + WWW.EscapeURL(username) +
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

        ListAccounts();
    }

    IEnumerator ListAccountsRoutine()
    {
        WWW get = new WWW(listAccountsURL);
        yield return get;

        //Handle error
        if (get.error != null)
        {
            Debug.Log("There was an error listing accounts: " + get.error);
        }

        accounts.text = get.text; // this is a GUIText that will display the scores in game.
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
using UnityEngine;
using UnityEngine.SceneManagement;
using WatermelonDataTool.Serializer;

public class auth : MonoBehaviour
{
    public string username { get; private set; }
    public string session_id { get; private set; }
    public bool authing { private set; get; } = false;
    public bool authed { private set; get; } = false;

    public void setCreds(string username, string session_id)
    {
        PlayerPrefs.SetString("username",username);
        PlayerPrefs.SetString("session_id", session_id);
        this.username = username;
        this.session_id = session_id;
    }

    public void SignIn(string username, string key)
    {
        if (authing) return;
        authing = true;
        this.username = username;
        Watermelon p = new Watermelon();
        p.setobj("username", username);
        p.setobj("key", key);
        print("sign in:\n" + p);

        try
        {
            communicator.instance.Communicate("sign_in", OnReceive, OnError, p, false);
        }
        catch (System.Exception ex) { print("idk why this error happens but it works well"); }//idk why
    }
    private void OnReceive(Watermelon reply)
    {
        authing = false;
        string result = reply.getObj<string>("result");
        authed = (result.Substring(0, 6) != "error:");

        if (authed)
        {
            setCreds(username, result);
            SceneManager.LoadScene("home");
        }
        else
        {
            setCreds(username, "");
        }
        
    }
    private void OnError(string error_msg)
    {
        authing = false;
        print("error at auth: " + error_msg);
    }


    public static auth instance;
    private void Awake()
    {
        username = PlayerPrefs.GetString("username");
        session_id = PlayerPrefs.GetString("session_id");

        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
}

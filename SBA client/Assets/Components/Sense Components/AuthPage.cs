using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuthPage : MonoBehaviour
{
    public TMP_InputField username, password;
    public bool autoSignIn = false;
    // Start is called before the first frame update
    void Start()
    {
        //auto auth
        if (autoSignIn && auth.instance.username != "" && auth.instance.session_id != "")
        {
            print("auto sign in:");
            auth.instance.SignIn(auth.instance.username, auth.instance.session_id);
        }
    }

    public void ManualSignIn()
    {
        //print("manual sign in:");
        auth.instance.SignIn(username.text, password.text);
    }

}

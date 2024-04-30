using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScene : MonoBehaviour
{
    [SerializeField] Image fade;
    [SerializeField] Button startBt;
    bool isstart = false;
    float timmer;
    [SerializeField] float nextscenetime;
    [SerializeField] GameObject dondest;
    void Start()
    {
        startBt.onClick.AddListener(nextScecnbool);
    }

    void Update()
    {
        if(isstart == true)
        {
            timmer += Time.deltaTime;
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b,Mathf.Lerp(0,1, timmer/ nextscenetime));
            if (timmer > nextscenetime)
            {
                isstart = false;
                SceneManager.LoadSceneAsync(1);
            }

        }
    }
    void nextScecnbool()
    {
        isstart = true;
    }
}

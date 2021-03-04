using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingNextScene : MonoBehaviour
{
    public int sceneNumber = 2;

    public Slider loadingBar;

    public Text loadingText;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TransitionNextScene(sceneNumber));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator TransitionNextScene(int num)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(num);

        ao.allowSceneActivation = false;

        while(!ao.isDone)
        {
            loadingBar.value = ao.progress;
            float prog = ao.progress * 100f;
            loadingText.text = (ao.progress * 100f).ToString() + "%";

            if(ao.progress >= 0.9f)
            {
                ao.allowSceneActivation = true;
            }

            yield return null;

        }
    }
}

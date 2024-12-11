using DBModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameUIManager : MonoBehaviour
{
    [SerializeField] GameObject SettingPanel;

    //ProgressBar
    [SerializeField] Slider progressBarSlider;
    [SerializeField] TMP_Text progressText;

    [SerializeField] GameObject ClearPanel;

    [SerializeField] IngameManager ingameManager;
    IngameObjectList ingameObjectList;

    private void Start()
    {
        ingameManager.onDataLoad += OnDataLoad;

        SettingPanel.SetActive(false);
        ClearPanel.SetActive(false);
    }

    private void OnDataLoad()
    {
        ingameObjectList = ingameManager.GetObjectList();
    }


    public void SetProgressBar()
    {
        StartCoroutine(LerpProgressBar());
    }

    private IEnumerator LerpProgressBar()
    {
        int checkedObjectNum = ingameObjectList.getChecked();
        float totalObject = ingameObjectList.ingameObjectList.Count;

        progressText.text = checkedObjectNum.ToString() + " / " + totalObject.ToString();

        float delta = 0f;
        float duration = 0.5f;
        float startValue = progressBarSlider.value;
        float endValue = (float)checkedObjectNum / totalObject;

        while (delta <= duration)
        {
            float t = delta / duration;
            progressBarSlider.value = Mathf.Lerp(startValue, endValue, t);

            //Debug.Log(progressBar.value);

            delta += Time.deltaTime;
            yield return null;
        }

        progressText.text = checkedObjectNum.ToString() + " / " + totalObject.ToString();

        if(checkedObjectNum == totalObject)
        {
            progressText.text = "Clear!!";

            ingameManager.ClearGame();

            ClearPanel.SetActive(true);
        }
    }
    
    public void OpenMenuButton()
    {
        SettingPanel.SetActive(true);
    }

    public void ExitMenuButton()
    {
        SettingPanel.SetActive(false);
    }

    public void RestartButton()
    {
        // 새로 빈 mapInfo 생성.
        //MapInfo resetMapInfo = new MapInfo(IngameManager.Instance.GetMapInfo().id.Length);

        //BuhitDB.Instance.UpdateMapInfo(resetMapInfo);
        MySceneManager.Instance.ReloadScene();
    }

    public void HomeButton()
    {
        //BuhitDB.Instance.UpdateMapInfo(IngameManager.Instance.GetMapInfo());
        MySceneManager.Instance.ChangeScene("MapSelect");
    }
    
    public void ProgressBarSetActive(bool active)
    {
        progressBarSlider.gameObject.SetActive(active);
    }

    public void ExitClearPanel()
    {
        ClearPanel.SetActive(false);
    }
}

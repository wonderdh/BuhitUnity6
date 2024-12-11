using UnityEngine;

public class StationHomeManager : MonoBehaviour
{

    public void OnClickHomeBtn()
    {
        MySceneManager.Instance.ChangeScene("Home");
    }

    public void OnClickMapBtn(string mapName)
    {
        Debug.Log("ChangeScene");
        MySceneManager.Instance.ChangeScene(mapName);
    }
}

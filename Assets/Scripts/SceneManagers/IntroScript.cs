using UnityEngine;

public class IntroScript : MonoBehaviour
{

    public void ToHome()
    {
        MySceneManager.Instance.ChangeScene("Home");
    }
}

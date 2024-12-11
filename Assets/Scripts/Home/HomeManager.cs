using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using DBModels;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using static Unity.Burst.Intrinsics.X86.Avx;

public class HomeManager : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("Panel_StationSelect")]
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    public RectTransform content;
    HorizontalLayoutGroup contentHorizontalLayoutGroup;
    public float snapSpeed = 10f;
    public float scaleSpeed = 10f;

    float scroll_pos = 0;
    float[] pos;

    private float[] positions;
    private bool isDragging;
    private int targetIndex;
    private bool btnClicked = false;

    [Header("Camellia")]
    public TMP_Text Text_Camellia;

    [Header("Panel_StageLock")]
    public GameObject Panel_StageLock;
    public GameObject Panel_Unlock_Fail;
    public GameObject Panel_Unlock_Success;
    public Image Image_StageLock_StationSprite;
    public TMP_Text Image_StageLock_Msg;

    
    bool initSwipe = false;

    private HomeSceneData data;

    int currentCamellia = 0;

    private List<Transform> BtnStationList;
    private List<Sprite> BtnStationImageList;
    
    private void printData()
    {
        Debug.Log(data.userData.camellia);
        Debug.Log(data.stations.stationList[0].stationName);
        Debug.Log(data.stations.stationList[0].isUnlocked);
    }

    async void Start()
    {
        //Panel SetActive init
        Panel_StageLock.SetActive(false);

        await InitData();

        //printData();
        

        InitSwipe();
    }

    async Task InitData()
    {
        data = await FirebaseRDBManager.Instance.GetUserDataHomeScene();
    }

    void Update()
    {
        if(!initSwipe) { return; } 

        SwipeUpdate();
    }

    void InitSwipe()
    {
        currentCamellia = data.userData.camellia;

        UpdateCamellia();

        BtnStationList = new List<Transform>();
        BtnStationImageList = new List<Sprite>();

        contentHorizontalLayoutGroup = content.GetComponent<HorizontalLayoutGroup>();

        int childSize = (int)Math.Ceiling(Screen.height * 0.5);

        int childN = content.childCount; // 맵 갯수
        int padding = (Screen.width / 2 - childSize / 2);

        float size = (childN * childSize) + (padding * 2);

        contentHorizontalLayoutGroup.padding.left = padding;
        contentHorizontalLayoutGroup.padding.right = padding;
        content.sizeDelta = new Vector2(size, 0);

        positions = new float[childN];
        float step = 1f / (positions.Length - 1);

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = i * step;
        }

        for (int i = 0; i < childN; i++)
        {
            int index = i; // 로컬 변수를 사용하여 클로저 문제를 해결합니다.
            Transform childT = content.GetChild(i);
            childT.GetComponent<RectTransform>().sizeDelta = new Vector2(childSize, childSize);
            childT.GetComponent<Button>().onClick.AddListener(() => OnElementClick(index));

            BtnStationList.Add(childT);
            BtnStationImageList.Add(childT.GetComponent<Image>().sprite);

            Station tmpStation = data.stations.getStation(childT.name);
            
            StationLockUpdate(i, tmpStation.isUnlocked);
        }

        initSwipe = true;
    }

    void StationLockUpdate(int i, int unlock)
    {
        Transform tmp = BtnStationList[i];

        Image childImg = tmp.GetComponent<Image>();
        Color alphaHalf = childImg.color;

        if (unlock == 0)
        {
            alphaHalf.a = 0.5f;

            tmp.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            alphaHalf.a = 1f;

            tmp.GetChild(0).gameObject.SetActive(false);
        }

        childImg.color = alphaHalf;
    }

    // 스와이프 기능
    void SwipeUpdate()
    {
        if (!isDragging)
        {
            if (!btnClicked)
            {
                targetIndex = FindClosestIndex(scroll_pos);
            } 

            scroll_pos = scrollRect.horizontalNormalizedPosition;
            float targetPosition = positions[targetIndex];

            // scrollVelocity 대신 Time.deltaTime을 사용
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scroll_pos, targetPosition, snapSpeed * Time.deltaTime);

            // 목표 위치에 충분히 가까워지면 정확히 맞춤
            if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetPosition) < 0.001f)
            {
                scrollRect.horizontalNormalizedPosition = targetPosition;
                btnClicked = false;
            }
        } 

        UpdateChildScales();
    }

    void UpdateChildScales()
    {
        float currentPosition = scrollRect.horizontalNormalizedPosition;

        for (int i = 0; i < positions.Length; i++)
        {
            float distance = Mathf.Abs(currentPosition - positions[i]);
            float scale = Mathf.Lerp(1f, 0.8f, distance * positions.Length);
            content.GetChild(i).localScale = Vector3.Lerp(content.GetChild(i).localScale, Vector3.one * scale, Time.deltaTime * scaleSpeed);
        }
    }

    int FindClosestIndex(float currentPosition)
    {
        float minDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < positions.Length; i++)
        {
            float distance = Mathf.Abs(currentPosition - positions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    public void OnBeginDrag(PointerEventData data)
    {
        isDragging = true;
        btnClicked = false;
    }

    public void OnEndDrag(PointerEventData data)
    {
        isDragging = false;
        btnClicked = false;
    }

    // 역 클릭 시
    void OnElementClick(int index)
    {
        btnClicked = true;
        targetIndex = index;

        string clickedBtnName = EventSystem.current.currentSelectedGameObject.name;

        if (Mathf.Abs(positions[index] - scroll_pos) < 0.01)
        {
            if (data.stations.getStation(clickedBtnName).isUnlocked == 0)
            {
                // unlock
                Debug.Log("Need Unlock");
                Unlock();
            }
            else
            {
                Debug.Log("Opened");

                MySceneManager.Instance.ChangeScene(clickedBtnName + "Home");
            }
        }

        scroll_pos = positions[index];

        //UIPanel.sprite = BackgroundImgaes[index];
    }

    public void Unlock()
    {
        Image_StageLock_StationSprite.sprite = BtnStationImageList[targetIndex];
        Image_StageLock_Msg.text = "Unlock\n" + BtnStationList[targetIndex].name + "?";

        Panel_StageLock.SetActive(true);
    }

    public void CloseUnlockPanel()
    {
        Panel_StageLock.SetActive(false);
        Panel_Unlock_Success.SetActive(false);
        Panel_Unlock_Fail.SetActive(false);
    }
    

    public void onClickUnlockButton()
    {
        String targetStationName = BtnStationList[targetIndex].name;
        Station targetStation = data.stations.getStation(targetStationName);

        int unlockCamellia = targetStation.unlockCamellia;

        if (currentCamellia < unlockCamellia)
        {
            Panel_Unlock_Fail.SetActive(true);
            Panel_Unlock_Success.SetActive(false);
        }
        else
        {
            Debug.Log("unlockCamellia :" + unlockCamellia);
            Debug.Log(currentCamellia);

            currentCamellia -= unlockCamellia;
            UpdateCamellia();

            data.userData.camellia -= unlockCamellia;
            targetStation.isUnlocked = 1;

            FirebaseRDBManager.Instance.UpdateUnlockData(data.userData.camellia, targetStationName);

            StationLockUpdate(targetIndex, 1);

            Panel_Unlock_Fail.SetActive(false);
            Panel_Unlock_Success.SetActive(true);
        }
    }

    private void UpdateCamellia()
    {
        Text_Camellia.text = "X " + currentCamellia;
    }

    /*
    public void CloseMsgPanel()
    {
        FailMessage.SetActive(false);
        SuccessMessage.SetActive(false);
        messagePanel.SetActive(false);
    }

    public void PanelSetActive(bool active)
    {
        //messagePanel.SetActive(active);
        unlockPanel.SetActive(active);
    }
    */

    /*
// 스크롤 할때마다 글 업데이트
void UpdateDescription(int index)
{
    if (index >= 0 && index < content.transform.childCount)
    {
        mapNameText.text = ssif.Name[index];

        progressBar.value = ssif.ClearedMap[index] / ssif.TotalMap[index];
        progress.text = progressBar.value * 100 + "%";

        focusedMapIndex = index;

        UIPanel.sprite = BackgroundImgaes[index];
    }
}

private IEnumerator LerpProgressBar(float progressValue)
{
    float delta = 0f;
    float duration = 0.2f;
    float startValue = 0;

    while (delta <= duration)
    {
        float t = delta / duration;
        progressBar.value = Mathf.Lerp(startValue, progressValue, t);

        delta += Time.deltaTime;
        yield return null;
    }
}
*/

}

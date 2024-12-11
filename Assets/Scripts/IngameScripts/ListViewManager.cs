using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

using DBModels;
using UnityEngine.EventSystems;

public class ListViewManager : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{

    /*
    private static ListViewManager instance;

    public static ListViewManager Instace
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    */


    [SerializeField] GameObject listView;           // GamePanel/ListScroll/Viewport/ListView
    [SerializeField] Scrollbar scrollbar;           // GamePanel/ListScroll/Scrollbar
    [SerializeField] GameObject descriptionImg;     // GamePanel/ListScroll/DescriptionImg
    [SerializeField] TMP_Text descriptionText;      // GamePanel/ListScroll/DescriptionImg/DescriptionText
    [SerializeField] RectTransform ListScroll;
    [SerializeField] HorizontalLayoutGroup contentHorizontalLayoutGroup;

    float scroll_pos = 0;
    float[] pos;

    private int listViewCount;                      // 오브젝트 이미지 갯수

    private GameObject[] hiddenObjectsListImg;      // listView에 있는 오브젝트 이미지들 가져오기.

    [SerializeField ]IngameManager ingameManager;
    IngameObjectList ingameObjectList;

    void Start()
    {
        ingameManager.onDataLoad += OnDataLoad;

        //StartCoroutine(SwipeUpdate());
    }

    private void OnDataLoad()
    {
        ingameObjectList = ingameManager.GetObjectList();

        StartCoroutine(InitMapInfo());
    }

    bool UISetActive = true;


    void Update()
    {
        if (ingameObjectList == null)
        {
            return;
        }

        //swipeUpdate();
        HandleScrolling();
    }

    IEnumerator InitMapInfo()
    {
        while (ingameObjectList == null)
        {
            yield return null;
        }

        hiddenObjectsListImg = new GameObject[listViewCount];

        for (int i = 0; i < listViewCount; i++)
        {
            hiddenObjectsListImg[i] = listView.transform.GetChild(i).gameObject;

            if (ingameObjectList.ingameObjectList[i].objectInfo.isChecked == 1)
            {
                CheckMark(i);
            }
        }

        initSwipe();
    }

    private void initSwipe()
    {
        float listScrollHeight = (Screen.height * ListScroll.anchorMax.y) - (Screen.height * ListScroll.anchorMin.y) - 200;
        float listScrollWidth = (Screen.width * ListScroll.anchorMax.x) - (Screen.width * ListScroll.anchorMin.x);

        float padding = listScrollWidth / 2 - listScrollHeight / 2;

        listViewCount = listView.transform.childCount;

        float size = (listViewCount * listScrollHeight) + (padding * 2);

        listView.GetComponent<RectTransform>().sizeDelta = new Vector2((int)size, (int)listScrollHeight);
        contentHorizontalLayoutGroup.padding.left = (int)padding;
        contentHorizontalLayoutGroup.padding.right = (int)padding;


        pos = new float[listViewCount];
        float distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }

        for (int i = 0; i < listViewCount; i++)
        {
            int index = i; // 로컬 변수를 사용하여 클로저 문제를 해결합니다.
            listView.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(() => OnElementClick(index));
            listView.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(listScrollHeight, listScrollHeight);
        }
    }
    /*
    // listViewList 스크롤 관련
    private void swipeUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            if (MySceneManager.Instance.IsPointerOverUIObject())
            {
                scroll_pos = scrollbar.value;
                UISetActive = true;
            } else
            {
                UISetActive = false;
            }

            ingameManager.SetUIActive(UISetActive);
        } else
        {
            ScrollBarValueLerp();
        }
        

        ListViewScaleLerp();
    }

    private void ScrollBarValueLerp()
    {
        //Debug.Log("ScrollBarValueLerp");

        float flag = (1f / (pos.Length * 2f));

        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + flag && scroll_pos > pos[i] - flag)
            {
                scrollbar.value = Mathf.Lerp(scrollbar.value, pos[i], 0.1f);
            }
        }
    }

    IEnumerator SCrollBarValueLerpCoroutine()
    {
        Debug.Log("ScrollBarValueLerpCoroutine");

        float flag = (1f / (pos.Length * 2f));

        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + flag && scroll_pos > pos[i] - flag)
            {
                while(Math.Abs(scrollbar.value - scroll_pos) >= 0.01)
                {
                    scrollbar.value = Mathf.Lerp(scrollbar.value, pos[i], 0.1f);
                    Debug.Log(scrollbar.value);
                    yield return null;
                }
            }
        }

        //Debug.Log("Coroutine End");
    }

    private void ListViewScaleLerp()
    {
        //Debug.Log("ListViewScaleLerp");

        float flag = (1f / (pos.Length * 2f));

        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + flag && scroll_pos > pos[i] - flag)
            {
                listView.transform.GetChild(i).localScale = Vector2.Lerp(listView.transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);
                UpdateDescription(i);
                for (int a = 0; a < pos.Length; a++)
                {
                    if (a != i)
                    {
                        listView.transform.GetChild(a).localScale = Vector2.Lerp(listView.transform.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
    }

    void OnElementClick(int index)
    {
        scroll_pos = pos[index];
        Debug.Log("targetIndex = " + index);
        StartCoroutine(SCrollBarValueLerpCoroutine());
    }
    */
    void UpdateDescription(int i)
    {
        if (i >= 0 && i < listViewCount)
        {
            descriptionText.text = ingameObjectList.ingameObjectList[i].objectInfo.Description;
        }
    }

    public void CheckMark(int i)
    {
        Image img = hiddenObjectsListImg[i].GetComponent<Image>();
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0.5f);

        GameObject checkMark = hiddenObjectsListImg[i].transform.GetChild(0).gameObject;
        checkMark.SetActive(true);
    }

    public void SetActiveDescription(bool activate)
    {
        descriptionImg.SetActive(activate);
    }

    //
    private float targetScrollPos;
    public bool isDragging = false;
    private const float SNAP_SPEED = 10f;
    private const float DRAG_THRESHOLD = 0.01f;

    private void HandleScrolling()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (MySceneManager.Instance.IsPointerOverUIObject())
            {
                isDragging = true;
                UISetActive = true;
            }
            else
            {
                UISetActive = false;
            }
            ingameManager.SetUIActive(UISetActive);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            scroll_pos = scrollbar.value;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;
                FindNearestSnapPoint();
            }
        }

        if (!isDragging)
        {
            UpdateScrollPosition();
        }

        UpdateListViewScale();
    }

    private void FindNearestSnapPoint()
    {
        float nearestDistance = float.MaxValue;
        int nearestIndex = 0;

        for (int i = 0; i < pos.Length; i++)
        {
            float distance = Mathf.Abs(scroll_pos - pos[i]);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        targetScrollPos = pos[nearestIndex];
    }

    private void UpdateScrollPosition()
    {
        if (Mathf.Abs(scrollbar.value - targetScrollPos) > 0.001f)
        {
            scrollbar.value = Mathf.Lerp(scrollbar.value, targetScrollPos, Time.deltaTime * SNAP_SPEED);
        }
        else
        {
            scrollbar.value = targetScrollPos;
        }
    }

    private void UpdateListViewScale()
    {
        int currentIndex = GetCurrentIndex();

        for (int i = 0; i < pos.Length; i++)
        {
            float targetScale = (i == currentIndex) ? 1f : 0.8f;
            Vector3 currentScale = listView.transform.GetChild(i).localScale;
            listView.transform.GetChild(i).localScale = Vector3.Lerp(
                currentScale,
                new Vector3(targetScale, targetScale, 1f),
                Time.deltaTime * SNAP_SPEED
            );
        }

        if (currentIndex >= 0)
        {
            UpdateDescription(currentIndex);
        }
    }

    private int GetCurrentIndex()
    {
        float currentPos = scrollbar.value;
        float minDistance = float.MaxValue;
        int nearestIndex = -1;

        for (int i = 0; i < pos.Length; i++)
        {
            float distance = Mathf.Abs(currentPos - pos[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    public void OnElementClick(int index)
    {
        targetScrollPos = pos[index];
        isDragging = false;
    }

    public void OnBeginDrag(PointerEventData data)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData data)
    {
        isDragging = false;
    }
}

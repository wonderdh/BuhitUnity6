using DBModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

public class IngameManager : MonoBehaviour
{
    [Header("StationName")]
    public String StationName;

    // Map

    [SerializeField] GameObject background;


    [SerializeField] GameObject[] hiddenObject;


    [SerializeField] GameObject content;

    [SerializeField] int contentCount;


    [SerializeField] GameObject[] hiddenObjectList;

    GameObject touchedObject = null;

    //MapInfo mapInfo = null;

    //Find Animation

    [SerializeField] GameObject findAnimPrefab;

    // Audio
    [SerializeField] AudioManager audioManager;
    [SerializeField] ListViewManager listViewManager;
    [SerializeField] InGameCamMove inGameCamMove;
    [SerializeField] IngameUIManager ingameUIManager;

    IngameObjectList ingameObjectList;

    public Action onDataLoad;

    async void Awake()
    {
        ingameObjectList = await FirebaseRDBManager.Instance.GetMapData(StationName, SceneManager.GetActiveScene().name);
        onDataLoad.Invoke();

        foreach (IngameObject ingameObject in ingameObjectList.ingameObjectList)
        {
            Debug.Log(ingameObject.id + ingameObject.objectInfo.Description + ingameObject.objectInfo.isChecked);
        }

        initBackground();

        //initMapInfo();
        initContent();

        ingameUIManager.SetProgressBar();
    }

    public void Update()
    {
        CheckTouch();

        if (inGameCamMove.isMoving)
        {
            touchedObject = null;
        }
    }

    public void CheckTouch()
    {
        OnTouchStart();
        //OnTouchEnd();
    }

    // 터치 감지
    private void OnTouchStart()
    {
        if (Input.GetMouseButtonDown(0))
        { // if left button pressed...
            touchedObject = null;

            if (MySceneManager.Instance.IsPointerOverUIObject())
            {
                return;
            }

            SetUIActive(false);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (!(hit.transform.tag == "HiddenObjects") || inGameCamMove.isMoving)
                {
                    touchedObject = null;
                    return;
                }

                touchedObject = hit.transform.gameObject;
            }
        }
    }

    
    private void OnTouchEnd()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (MySceneManager.Instance.IsPointerOverUIObject())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == touchedObject)
                {
                    for (int i = 0; i < hiddenObject.Length; i++)
                    {
                        if (hiddenObject[i] == hit.collider.gameObject)
                        {
                            // 이미 찾은건지 아닌지 확인
                            if (ingameObjectList.ingameObjectList[i].objectInfo.isChecked == 1)
                            {
                                break;
                            }

                            // 이미 찾은게 아닐 경우
                            listViewManager.CheckMark(i);

                            GameObject findAnim = Instantiate(findAnimPrefab, hiddenObject[i].transform);

                            //mapInfo.isChecked[i] = 1; // 업데이트

                            SetUIActive(true);
                            ingameUIManager.SetProgressBar();

                            audioManager.PlaySuccess();

                            break;
                        }
                    }
                }
            }
        }
    }
    

    // 배경 및 리스트 초기화
    private void initBackground()
    {
        if (background == null)
        {
            background = GameObject.FindGameObjectWithTag("Background");

            contentCount = background.transform.childCount;

            hiddenObject = new GameObject[contentCount];

            for (int i = 0; i < contentCount; i++)
            {
                hiddenObject[i] = background.transform.GetChild(i).gameObject;
            }
        }
        else
        {
            contentCount = background.transform.childCount;
        }

        //mapInfo = new MapInfo(contentCount);
    }

    private void initContent()
    {
        if (content == null)
        {
            content = GameObject.FindGameObjectWithTag("Content");

            int childCount = content.transform.childCount;

            hiddenObjectList = new GameObject[childCount];

            for (int i = 0; i < childCount; i++)
            {
                hiddenObjectList[i] = content.transform.GetChild(i).gameObject;
            }
        }
    }

    public IngameObjectList GetObjectList()
    {
        return ingameObjectList;
    }

    public void SetUIActive(bool active)
    {
        listViewManager.SetActiveDescription(active);
        ingameUIManager.ProgressBarSetActive(active);
    }
    /*
     완료 시 소리 재생. 여기 있을 필요가 없음.
             if (progressBar.value >= 0.95f)
        {
            progressText.text = "Complete!!";

            audioManager.PlayClear();
        }
    */
    // 저장 및 맵 관련 기능들

    /*
    private void initMapInfo()
    {
        mapInfo = BuhitDB.Instance.GetMapInfo(contentCount);
        mapInfo.PrintMapInfo();
    }

    public void resetMapInfo()
    {
        BuhitDB.Instance.ResetMap(contentCount);
    }

    public MapInfo GetMapInfo()
    {
        return mapInfo;
    }

    */
    public void ClearGame()
    {
        Debug.Log("ClearGame");
        //BuhitDB.Instance.OnClearGame();
    }
    

}
using DBModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

public class IngameManager : MonoBehaviour
{
    [Header("StationName")]
    public String stationName;
    public String mapName;
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
        mapName = SceneManager.GetActiveScene().name;

        Debug.Log(stationName+ mapName);

        ingameObjectList = await FirebaseRDBManager.Instance.GetMapData(stationName, mapName);
        onDataLoad.Invoke();

        initBackground();

        //initMapInfo();
        initContent();
    }

    public void Update()
    {
        if (inGameCamMove.isMoving)
        {
            touchedObject = null;
            return;
        }
        CheckTouch();
    }

    public void CheckTouch()
    {
        //Debug.Log("CheckTouch");
        OnTouchStart();
        OnTouchEnd();
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
                Debug.Log(hit.transform.name);

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
                    Debug.Log("Same");
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
                            ingameObjectList.ingameObjectList[i].objectInfo.isChecked = 1;
                            FirebaseRDBManager.Instance.UpdateObject(stationName, mapName, i + 1);

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


        }
        contentCount = background.transform.childCount;

        hiddenObject = new GameObject[contentCount];

        for (int i = 0; i < contentCount; i++)
        {
            hiddenObject[i] = background.transform.GetChild(i).gameObject;
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

    public void ClearGame()
    {
        FirebaseRDBManager.Instance.OnClearMap(stationName, mapName);
    }
    

}
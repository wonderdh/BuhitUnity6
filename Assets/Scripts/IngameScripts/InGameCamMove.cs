using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class InGameCamMove : MonoBehaviour
{
    [SerializeField]
    float camMoveSpeed = 0.2f;
    float camZoomSpeed = 0.5f;

    [SerializeField]
    float zoomIn = 2f;
    float zoomOut = 3.8f;

    [SerializeField]
    float maxCamMoveX = 17f;
    [SerializeField]
    float maxCamMoveY = 13f;

    public bool isMoving;
    bool isMoveable;

    Vector3 lastMousePosition;
    Vector3 lastCamPosition;
    Vector3 newCamPosition;

    [SerializeField] ListViewManager listViewManager;

    enum GESTURE
    {
        MOVE = 1,
        ZOOM,
    }

    private void Start()
    {
        isMoving = false;
        isMoveable = true;

        lastCamPosition = Camera.main.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (listViewManager.isDragging)
        {
            return;
        }

        MoveCam();
        ZoomCamTouch();
        ZoomCamMouse();

        moveCheck();

        isMoved();

        //Debug.Log(isMoveable + "," + isMoving);
    }

    private void moveCheck()
    {
        if(isMoving)
        {
            return;
        }

        if (!Input.GetMouseButton(0) && MySceneManager.Instance.IsPointerOverUIObject())
        {
            isMoveable = false;
            return;
        } else if (!MySceneManager.Instance.IsPointerOverUIObject())
        {
            isMoveable = true;
        }
    }

    private void MoveCam()
    {
        if (isMoveable == false)
        {
            return;
        }

        if (Input.touchCount == (int)GESTURE.MOVE || Input.GetMouseButton(0))
        {
            float camX;
            float camY;
            float deltaX;
            float deltaY;

            Touch touch;

            if (Input.GetMouseButton(0))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    lastMousePosition = Input.mousePosition;
                }

                Vector3 delta = Input.mousePosition - lastMousePosition;
                lastMousePosition = Input.mousePosition;

                camX = Camera.main.transform.position.x;
                camY = Camera.main.transform.position.y;
                deltaX = delta.x;
                deltaY = delta.y;
            }
            
            else{
                touch = Input.touches[0];

                camX = Camera.main.transform.position.x;
                camY = Camera.main.transform.position.y;
                deltaX = touch.deltaPosition.x;
                deltaY = touch.deltaPosition.y;
            }

            moveCode(camX, camY, deltaX, deltaY);
        }
    }

    private void moveCode(float camX, float camY, float deltaX, float deltaY)
    {
        // 카메라 확대/축소에 따른 이동 속도 비율 계산
        float zoomFactor = Camera.main.orthographicSize / zoomOut;
        float adjustedCamMoveSpeed = camMoveSpeed * zoomFactor;

        float camMoveX = camX - (deltaX * Time.deltaTime * adjustedCamMoveSpeed);
        float camMoveY = camY - (deltaY * Time.deltaTime * adjustedCamMoveSpeed);

        if (camMoveX >= maxCamMoveX || camMoveX <= -maxCamMoveX)
        {
            camMoveX = camX;
        }

        if (camMoveY >= maxCamMoveY || camMoveY <= -maxCamMoveY)
        {
            camMoveY = camY;
        }

        newCamPosition = new Vector3(
            camMoveX,
            camMoveY,
            Camera.main.transform.position.z);

        Camera.main.transform.position = newCamPosition;
    }

    private void ZoomCamTouch()
    {
        if (Input.touchCount == (int)GESTURE.ZOOM)
        {
            if (MySceneManager.Instance.IsPointerOverUIObject())
            {
                return;
            }

            Touch touch_1 = Input.touches[0];
            Touch touch_2 = Input.touches[1];

            //이전 프레임의 터치 좌표를 구한다.
            Vector2 t1PrevPos = touch_1.position - touch_1.deltaPosition;
            Vector2 t2PrevPos = touch_2.position - touch_2.deltaPosition;

            //이전 프레임과 현재 프레임 움직임 크기를 구함.
            float prevDeltaMag = (t1PrevPos - t2PrevPos).magnitude;
            float deltaMag = (touch_1.position - touch_2.position).magnitude;

            //두 크기값의 차를 구해 줌 인/아웃의 크기값을 구한다.
            float deltaMagDiff = prevDeltaMag - deltaMag;

            Camera.main.orthographicSize += (deltaMagDiff * Time.deltaTime * camZoomSpeed);
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, zoomIn, zoomOut);
        }
    }

    private void ZoomCamMouse()
    {
        float scrollData;
        scrollData = Input.GetAxis("Mouse ScrollWheel");

        if (scrollData != 0)
        {
            Debug.Log(scrollData);
            Camera.main.orthographicSize -= scrollData;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, zoomIn, zoomOut);
        }
    }

    private void isMoved()
    {
        if (lastCamPosition == newCamPosition)
        {
            isMoving = false;
        } else
        {
            isMoving = true;
        }

        lastCamPosition = newCamPosition;
    }
}

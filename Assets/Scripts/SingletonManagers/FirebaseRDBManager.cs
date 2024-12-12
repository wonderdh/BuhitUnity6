using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.CompilerServices;

using DBModels;
using System.Collections.Generic;
using Unity.VisualScripting;

public class FirebaseRDBManager : MonoBehaviour
{
    private static FirebaseRDBManager instance;
    public static FirebaseRDBManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private DatabaseReference databaseReference;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        FirebaseAuthManager.Instance.authInitialized += myStart;
    }

    private async void myStart()
    {
        await InitializeFirebaseRealtimeDBAsync();
    }

    private async Task InitializeFirebaseRealtimeDBAsync()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase database initialized successfully.");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase 초기화 실패: {ex.Message}");
        }
    }

    public async void OnSignUp(string userId)
    {
        try
        {
            string initJson;
            string filePath = Path.Combine(Application.streamingAssetsPath, "init.json");

            // 플랫폼별 파일 읽기 처리
            if (Application.platform == RuntimePlatform.Android)
            {
                using (UnityWebRequest www = UnityWebRequest.Get(filePath))
                {
                    await www.SendWebRequest();
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"파일 읽기 실패: {www.error}");
                        return;
                    }
                    initJson = www.downloadHandler.text;
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor ||
                     Application.platform == RuntimePlatform.WindowsPlayer ||
                     Application.platform == RuntimePlatform.OSXEditor ||
                     Application.platform == RuntimePlatform.OSXPlayer ||
                     Application.platform == RuntimePlatform.LinuxEditor ||
                     Application.platform == RuntimePlatform.LinuxPlayer)
            {
                // PC 플랫폼 (Windows, Mac, Linux)
                Debug.Log("PC 플랫폼");
                initJson = File.ReadAllText(filePath);
            }
            else
            {
                // iOS 및 기타 플랫폼
                using (UnityWebRequest www = UnityWebRequest.Get(filePath))
                {
                    await www.SendWebRequest();
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"파일 읽기 실패: {www.error}");
                        return;
                    }
                    initJson = www.downloadHandler.text;
                }
            }

            // Firebase에 저장
            DatabaseReference userRef = databaseReference.Child("users").Child(userId);
            await userRef.SetRawJsonValueAsync(initJson).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("데이터 저장 실패: " + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("데이터 저장 성공");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveUserData 에러: {e.Message}");
        }
    }

    public async Task<HomeSceneData> GetUserDataHomeScene()
    {
        DatabaseReference userRef = databaseReference.Child("users").Child(FirebaseAuthManager.Instance.GetCurrentUserId());

        try
        {
            var snapshot = await userRef.GetValueAsync();
            if (snapshot.Exists)
            {
                HomeSceneData initData = new HomeSceneData();

                // 스테이션 데이터 가져오기
                var stations = snapshot.Child("Stations").Children;
                foreach (var station in stations)
                {
                    Station tmp = new Station();
                    tmp.stationName = station.Key;
                    tmp.isUnlocked = int.Parse(station.Child("isUnlocked").Value.ToString());
                    tmp.unlockCamellia = int.Parse(station.Child("unlockCamellia").Value.ToString());
                    // 스테이션 데이터 처리

                    initData.stations.stationList.Add(tmp);
                }

                // 유저 데이터 가져오기
                var userData = snapshot.Child("userData");
                initData.userData.nickname = userData.Child("nickName").Value.ToString();
                initData.userData.camellia = int.Parse(userData.Child("camellia").Value.ToString());

                // 가져온 데이터 활용
                return initData;
            }
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 로드 실패: {e.Message}");
            return null;
        }
    }
    public async Task<IngameObjectList> GetMapData(String stationName, String mapName)
    {
        DatabaseReference mapRef = databaseReference.Child("users").Child(FirebaseAuthManager.Instance.GetCurrentUserId()).Child("Stations").Child(stationName).Child("Maps").Child(mapName).Child("objectCheckList");

        try
        {
            var snapshot = await mapRef.GetValueAsync();
            if (snapshot.Exists)
            {
                Debug.Log("존재는 함");
                IngameObjectList ingameObjectList = new IngameObjectList();

                // 스테이션 데이터 가져오기
                foreach (var ingameObject in snapshot.Children)
                {
                    IngameObject tmpIO = new IngameObject();

                    tmpIO.id = ingameObject.Key;

                    string discription = ingameObject.Child("Description").Value?.ToString();
                    int isCleared = int.Parse(ingameObject.Child("isCleared").Value?.ToString());

                    tmpIO.objectInfo.Description = discription;
                    tmpIO.objectInfo.isChecked = isCleared;

                    ingameObjectList.ingameObjectList.Add(tmpIO);

                    Debug.Log(discription + isCleared);
                }

                // 가져온 데이터 활용
                return ingameObjectList;
            }
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 로드 실패: {e.Message}");
            return null;
        }
    }


    // 데이터 업데이트-----------------------------------------
    public void UpdateUnlockData(int camellia, String targetStationName)
    {
        Debug.Log(targetStationName + camellia);

        String targetUserId = FirebaseAuthManager.Instance.GetCurrentUserId();
        // 업데이트할 데이터 준비
        Dictionary<string, object> updates = new Dictionary<string, object>();

        updates["/users/" + targetUserId + "/userData/camellia"] = camellia;
        updates["/users/" + targetUserId + "/Stations/" + targetStationName + "/isUnlocked"] = 1;

        // 멀티패스 업데이트 실행
        databaseReference.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("데이터가 성공적으로 업데이트되었습니다.");
            }
            else
            {
                Debug.LogError("데이터 업데이트 중 오류 발생: " + task.Exception);
            }
        });
    }

    // 데이터 업데이트-----------------------------------------
    public void InitData(string stationName, string mapName, int n)
    {
        String targetUserId = FirebaseAuthManager.Instance.GetCurrentUserId();
        // 업데이트할 데이터 준비
        Dictionary<string, object> updates = new Dictionary<string, object>();

        for(int i = 1; i <= n; i++)
        {
            updates["/users/" + targetUserId + "/Stations/" + stationName + "/Maps/" + mapName + "/objectCheckList/" + i + "/isCleared"] = 0;
        }

        // 멀티패스 업데이트 실행
        databaseReference.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("데이터가 성공적으로 업데이트되었습니다.");
            }
            else
            {
                Debug.LogError("데이터 업데이트 중 오류 발생: " + task.Exception);
            }
        });
    }


    public void UpdateObject(String stationName, String mapName, int objectId)
    {
        String targetUserId = FirebaseAuthManager.Instance.GetCurrentUserId();

        string path = "/users/" + targetUserId + "/Stations/" + stationName + "/Maps/" + mapName + "/objectCheckList/" + objectId + "/isCleared";

        // 업데이트할 데이터 준비
        Dictionary<string, object> updates = new Dictionary<string, object>();

        updates[path] = 1;

        // 멀티패스 업데이트 실행
        databaseReference.UpdateChildrenAsync(updates).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("오브젝트데이터가 성공적으로 업데이트되었습니다.");
            }
            else
            {
                Debug.LogError("데이터 업데이트 중 오류 발생: " + task.Exception);
            }
        });
    }

    //구조 다시 해야됨 임시로 해놓은거임
    public void OnClearMap(String stationName, String mapName)
    {
        Debug.Log("게임 종료 시");
        String targetUserId = FirebaseAuthManager.Instance.GetCurrentUserId();

        DatabaseReference mapRef = FirebaseDatabase.DefaultInstance.GetReference("users/" + targetUserId + "/Stations/" + stationName + "/clearedMapCount");

        bool isCleared = false;

        mapRef.RunTransaction(mutableData =>
        {
            int value = 0;
            if (mutableData.Value != null)
            {
                value = int.Parse(mutableData.Value.ToString());
            }
            
            if(value != 0)
            {
                isCleared = true;
            }
            else
            {
                mutableData.Value = value + 1;
            }

            Debug.Log(isCleared);

            if (!isCleared)
            {
                Debug.Log(isCleared + "2");

                UpdateCamellia(3);
            }

            return TransactionResult.Success(mutableData);
        });
    }

    public void UpdateCamellia(int camellia)
    {
        String targetUserId = FirebaseAuthManager.Instance.GetCurrentUserId();

        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users/" + targetUserId + "/userData/camellia");

        Debug.Log("동백 업뎃");
        // camellia 값 증가
        userRef.RunTransaction(mutableData =>
        {
            int value = 0;
            if (mutableData.Value != null)
            {
                value = int.Parse(mutableData.Value.ToString());
            }
            mutableData.Value = value + 3;
            return TransactionResult.Success(mutableData);
        });
    }
}




public static class ExtensionMethods
{
    public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOp.completed += obj => { tcs.SetResult(null); };
        return ((Task)tcs.Task).GetAwaiter();
    }
}

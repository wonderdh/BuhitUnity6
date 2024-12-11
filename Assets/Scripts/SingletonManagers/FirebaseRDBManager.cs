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

    private async void Start()
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


    public void UpdateUnlockData(int camellia, String targetStationName)
    {
        Debug.Log(targetStationName + camellia);

        String targetUserId = FirebaseAuthManager.Instance.GetCurrentUserId();
        // 업데이트할 데이터 준비
        Dictionary<string, object> updates = new Dictionary<string, object>();

        updates["/users/" + targetUserId + "/userData/camellia"] = camellia;
        updates["/users/" + targetUserId + "/Stations/" + targetStationName + "/isUnlocked"] = 1;

        // 멀티패스 업데이트 실행
        databaseReference.UpdateChildrenAsync(updates).ContinueWith(task => {
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

using Firebase.Auth;
using Firebase;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class FirebaseAuthManagerSingleton : MonoBehaviour
{
    private static FirebaseAuthManagerSingleton instance = null;

    public static FirebaseAuthManagerSingleton Instance
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

    public Firebase.FirebaseApp app;
    public event Action FirebaseInitialized;

    public bool signedIn = false;
    private FirebaseAuth auth;
    private FirebaseUser user;

    private bool tryLogin = true;

    public event Action StateAction;
    public Action<bool> SignInAction;

    enum ENUM_STATE
    {
        DEFAULT = 0, //0
        SIGN_UP,
        ERROR_EMAIL_BLANK,
        ERROR_EMAIL_INVALID,
        ERROR_EMAIL_NOTFOUND,
        ERROR_EMAIL_ALREADY_IN_USE,
        ERROR_PASSWORD_BLANK,
        ERROR_PASSWORD_INVALID,
        ERROR_PASSWORD_WEAK,
    }

    public int CurrentState = (int)ENUM_STATE.DEFAULT;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeFirebaseAsync();
    }

    private async Task InitializeFirebaseAsync()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                auth.StateChanged += AuthStateChanged;
                Debug.Log("Firebase Auth initialized successfully");
            }
            else
            {
                throw new Exception($"Firebase 종속성 오류: {dependencyStatus}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase 초기화 실패: {ex.Message}");
        }
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        Debug.Log("StateChange");

        if (!tryLogin) { return; }
        signedIn = user != auth.CurrentUser && auth.CurrentUser != null && auth.CurrentUser.IsValid();

        Debug.Log("signedIn = " + signedIn);

        if (!signedIn && user == null)
        {
            FirebaseInitialized?.Invoke();
        }

        if (auth.CurrentUser != user)
        {
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                SignInAction?.Invoke(signedIn);
            }

            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                SignInAction?.Invoke(signedIn);
            }
        }

    }

    public async void SignInWithEmail(string email, string password)
    {
        Debug.Log(email + password);

        tryLogin = true;

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);

            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        }
        catch (FirebaseException e)
        {
            switch (e.ErrorCode)
            {
                case (int)AuthError.MissingEmail:
                    Debug.LogError("이메일을 입력해주세요.");
                    CurrentState = (int)ENUM_STATE.ERROR_EMAIL_BLANK;
                    break;
                case (int)AuthError.InvalidEmail:
                    Debug.LogError("유효하지 않은 이메일 형식입니다.");
                    CurrentState = (int)ENUM_STATE.ERROR_EMAIL_INVALID;
                    break;
                case (int)AuthError.UserNotFound:
                    Debug.LogError("존재하지 않는 계정입니다.");
                    CurrentState = (int)ENUM_STATE.ERROR_EMAIL_NOTFOUND;
                    break;
                case (int)AuthError.MissingPassword:
                    Debug.LogError("비밀번호를 입력해주세요.");
                    CurrentState = (int)ENUM_STATE.ERROR_PASSWORD_BLANK;
                    break;
                case (int)AuthError.WrongPassword:
                    Debug.LogError("비밀번호가 올바르지 않습니다.");
                    CurrentState = (int)ENUM_STATE.ERROR_PASSWORD_INVALID;
                    break;
                default:
                    Debug.LogError($"로그인 실패: {e.Message}");
                    break;
            }
            tryLogin = false;
        }
        catch (Exception e)
        {
            Debug.LogError($"알 수 없는 오류 발생: {e.Message}");
            tryLogin = false;
        }

        StateAction?.Invoke();
    }

    public void SignOut()
    {
        Debug.Log("SignOUt");
        if (auth != null) auth.SignOut();
        if (user != null) user.DeleteAsync();
    }

    public async void CreateUserWithEmail(string email, string password)
    {
        Debug.Log("Create User With Email");
        tryLogin = false;

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            Debug.LogFormat("회원가입 성공: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            // 회원가입 성공 후 추가 처리
            FirebaseRDBManager.Instance.OnSignUp(result.User.UserId);
            CurrentState = (int)ENUM_STATE.SIGN_UP;

        }
        catch (FirebaseException e)
        {
            switch (e.ErrorCode)
            {
                case (int)AuthError.MissingEmail:
                    Debug.LogError("이메일을 입력해주세요.");
                    CurrentState = (int)ENUM_STATE.ERROR_EMAIL_BLANK;
                    break;
                case (int)AuthError.EmailAlreadyInUse:
                    Debug.LogError("이미 사용 중인 이메일입니다.");
                    CurrentState = (int)ENUM_STATE.ERROR_EMAIL_ALREADY_IN_USE;
                    break;
                case (int)AuthError.InvalidEmail:
                    Debug.LogError("유효하지 않은 이메일 형식입니다.");
                    CurrentState = (int)ENUM_STATE.ERROR_EMAIL_INVALID;
                    break;
                case (int)AuthError.MissingPassword:
                    Debug.LogError("비밀번호를 입력해주세요.");
                    CurrentState = (int)ENUM_STATE.ERROR_PASSWORD_BLANK;
                    break;
                case (int)AuthError.WeakPassword:
                    Debug.LogError("비밀번호가 너무 약합니다. 6자리 이상 입력해주세요.");
                    CurrentState = (int)ENUM_STATE.ERROR_PASSWORD_WEAK;
                    break;
                default:
                    Debug.LogError($"회원가입 실패: {e.Message}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"알 수 없는 오류 발생: {e.Message}");
        }

        StateAction?.Invoke();
        SignOut();
        /*
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created. 
            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            FirebaseRDBManager.Instance.OnSignUp(result.User.UserId);

            SignOut();
        });*/

    }

    void OnDestroy()
    {
        //auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    public bool isSignedIn()
    {
        return signedIn;
    }

    public int GetCurrentState()
    {
        return CurrentState;
    }

    public void ResetState()
    {
        CurrentState = (int)ENUM_STATE.DEFAULT;
    }
}
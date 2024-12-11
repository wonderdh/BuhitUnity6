using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Sign In")]
    public GameObject Panel_SignIn;
    public TMP_InputField IF_SignIn_Email;
    public TMP_InputField IF_SignIn_Pw;

    [Header("Sign Up")]
    public GameObject Panel_SignUp;
    public TMP_InputField IF_SignUp_Email;
    public TMP_InputField IF_SignUp_Pw;
    public TMP_InputField IF_SignUp_PwCheck;
    public Button Btn_SignUp;
    public GameObject Panel_Success;

    public Sprite Sprite_Icon_error;
    public Sprite Sprite_Icon_check;
    public Image Image_PW;

    [Header("Panel_Error")]
    public GameObject Panel_Error;
    public TMP_Text Text_Error_Msg;

    bool justSignedIn = false;

    bool isChecked = false;

    public void Awake()
    {
        //Debug.Log("LoginAwake");

        Panel_SignIn.SetActive(true);
        Panel_SignUp.SetActive(false);
        Panel_SignUp.SetActive(false);

        Panel_Error.SetActive(false);

        Btn_SignUp.interactable = false;

        //OnChangedState(FirebaseAuthManager.Instance.signedIn);
        IF_SignUp_PwCheck.onValueChanged.AddListener(OnTypingConfirmPassword);
        IF_SignUp_Pw.onValueChanged.AddListener(OnTypingConfirmPassword);

        //Firebase 관련
        FirebaseAuthManager.Instance.LoginState += OnChangedState;
        FirebaseAuthManager.Instance.StateAction += TrySignInState;
    }

    private void Start()
    {
        //Debug.Log("Start");

        if(isChecked == false)
        {
            if(FirebaseAuthManager.Instance.signedIn == false)
            {
                MySceneManager.Instance.DoorOpen();
            }

            isChecked = true;
        }
    }

    private void OnChangedState(bool signedIn)
    {
        //Debug.Log("OnChangedState : " + signedIn);

        if (signedIn)
        {
            if (justSignedIn)
            {
                MySceneManager.Instance.ChangeScene("Intro");
            } else
            {
                MySceneManager.Instance.ChangeSceneWithoutDoorOpen("Home");
            }
        } else
        {
            MySceneManager.Instance.DoorOpen();
        }

        justSignedIn = false;
        isChecked = true;
    }


    enum ENUM_STATE
    {
        DEFAULT,
        SIGN_UP,
        ERROR_EMAIL_BLANK,
        ERROR_EMAIL_INVALID,
        ERROR_EMAIL_NOTFOUND,
        ERROR_EMAIL_ALREADY_IN_USE,
        ERROR_PASSWORD_BLANK,
        ERROR_PASSWORD_INVALD,
        ERROR_PASSWORD_WEAK,
    }


    private void TrySignInState(int CurrentState)
    {
        Debug.Log("StateChanged" + CurrentState);

        switch (CurrentState)
        {
            case (int)ENUM_STATE.SIGN_UP:
                // 성공 메세지 뜨고 Signin panel로.
                Panel_Success.SetActive(true);
                break;
            case (int)ENUM_STATE.ERROR_EMAIL_BLANK:
                Text_Error_Msg.text = "Please Write Email!";
                Panel_Error.SetActive(true);
                break;
            case (int)ENUM_STATE.ERROR_EMAIL_INVALID:
                Text_Error_Msg.text = "Email Invalid Format!";
                Panel_Error.SetActive(true);
                break;
            case (int)ENUM_STATE.ERROR_EMAIL_NOTFOUND:
                Text_Error_Msg.text = "User Not Found!";
                Panel_Error.SetActive(true);
                break;
            case (int)ENUM_STATE.ERROR_EMAIL_ALREADY_IN_USE:
                Text_Error_Msg.text = "Email Already In Use!";
                Panel_Error.SetActive(true);
                break;
            case (int)ENUM_STATE.ERROR_PASSWORD_BLANK:
                Text_Error_Msg.text = "Please Enter Password!";
                Panel_Error.SetActive(true);
                break;
            case (int)ENUM_STATE.ERROR_PASSWORD_INVALD:
                Text_Error_Msg.text = "Wrong Password!";
                Panel_Error.SetActive(true);
                break;
            case (int)ENUM_STATE.ERROR_PASSWORD_WEAK:
                Text_Error_Msg.text = "Weak Password!";
                Panel_Error.SetActive(true);
                break;
            default:
                break;
        }
    }


    public void BtnSignIn()
    {
        justSignedIn = true;
        FirebaseAuthManager.Instance.SignInWithEmail(IF_SignIn_Email.text, IF_SignIn_Pw.text);
    }

    public void SignOut()
    {
        justSignedIn = false;
        FirebaseAuthManager.Instance.SignOut();
    }

    public void Create()
    {
        //FirebaseAuthManager.Instance.CreateUserWithEmail(email.text, password.text);
    }

    public void OnClickSignUpBtn()
    {
        Panel_SignUp.SetActive(true);
        Panel_SignIn.SetActive(false);
    }

    public void OnClickBackBtn()
    {
        Panel_SignIn.SetActive(true);
        Panel_SignUp.SetActive(false);
        Panel_Success.SetActive(false);

        IF_SignUp_Email.text = string.Empty;
        IF_SignUp_Pw.text = string.Empty;
        IF_SignUp_PwCheck.text = string.Empty;
    }

    public void OnTypingConfirmPassword(string text)
    {
        Debug.Log(IF_SignUp_PwCheck.text);

        if (IF_SignUp_Pw.text != "" && IF_SignUp_PwCheck.text.Equals(IF_SignUp_Pw.text))
        {
            Btn_SignUp.interactable = true;
            Image_PW.sprite = Sprite_Icon_check;
        }
        else
        {
            Btn_SignUp.interactable = false;
            Image_PW.sprite = Sprite_Icon_error;
        }
    }

    public void BtnSignUp()
    {
        FirebaseAuthManager.Instance.CreateUserWithEmail(IF_SignUp_Email.text, IF_SignUp_Pw.text);
    }

    public void CloseErrorPanelBtn()
    {
        Panel_Error.SetActive(false);
    }

    public void OnDestroy()
    {
        FirebaseAuthManager.Instance.LoginState -= OnChangedState;
        FirebaseAuthManager.Instance.StateAction -= TrySignInState;
        //Debug.Log("Bye");
    }
}
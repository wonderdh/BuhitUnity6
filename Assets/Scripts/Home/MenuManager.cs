using UnityEngine;

public class MenuManager : MonoBehaviour
{

    [Header("Panel_Popups")]
    public GameObject Panel_Popups;

    public GameObject Popup_MyInfo;
    public GameObject Popup_ItemShop;
    public GameObject Popup_Mission;
    public GameObject Popup_Settings;

    [Header("Popup_Shops")]
    public GameObject Popup_Shops;
    public GameObject Popup_Shop_Coin;
    public GameObject Popup_Shop_Gem;
    public GameObject Popup_Shop_Life;

    private void Start()
    {
        OnClickCloseBtn();
    }

    public void OnClickPopup_ItemShop()
    {
        Popup_ItemShop.SetActive(true);
        Panel_Popups.SetActive(true);
    }

    public void OnClickPopup_Mission()
    {
        Popup_Mission.SetActive(true);
        Panel_Popups.SetActive(true);
    }

    public void OnClickPopup_Settings()
    {
        Popup_Settings.SetActive(true);
        Panel_Popups.SetActive(true);
    }

    public void OnClickPopup_Shops()
    {
        Popup_Shops.SetActive(true);
        Popup_Shop_Coin.SetActive(true);
        Popup_Shop_Gem.SetActive(false);
        Popup_Shop_Life.SetActive(false);
        Panel_Popups.SetActive(true);
    }

    public void OnClickPopup_Shop_Gem()
    {
        Popup_Shops.SetActive(true);
        Popup_Shop_Coin.SetActive(false);
        Popup_Shop_Gem.SetActive(true);
        Popup_Shop_Life.SetActive(false);
        Panel_Popups.SetActive(true);
    }

    public void OnClickPopup_Shop_Life()
    {
        Popup_Shops.SetActive(true);
        Popup_Shop_Coin.SetActive(false);
        Popup_Shop_Gem.SetActive(false);
        Popup_Shop_Life.SetActive(true);
        Panel_Popups.SetActive(true);
    }

    public void OnClickCloseBtn()
    {
        Panel_Popups.SetActive(false);

        Popup_MyInfo.SetActive(false);
        Popup_ItemShop.SetActive(false);
        Popup_Mission.SetActive(false);
        Popup_Settings.SetActive(false);
        Popup_Shops.SetActive(false);

        Popup_Shop_Coin.SetActive(false);
        Popup_Shop_Gem.SetActive(false);
        Popup_Shop_Life.SetActive(false);
    }

    public void OnClickSignOutBtn()
    {
        FirebaseAuthManager.Instance.SignOut();
        MySceneManager.Instance.ChangeScene("Login");
    }
}

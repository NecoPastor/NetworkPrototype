using Steamworks;
using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public uint appId = 480;
    //public static NetworkManager Instance { get; private set; }
    private static bool isInitialized = false;
    public bool IsInitialized { get; private set; }

    [SerializeField] private TMP_Text textDebug;
    private void Awake()
    {
        //if (Instance != null && Instance != this)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        //Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSteam();
    }

    private void InitializeSteam()
    {
        DontDestroyOnLoad(this);

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test failed. Wrong DLL or platform version.");
            textDebug.text = "[Steamworks.NET] Packsize Test failed. Wrong DLL or platform version.";
            return;
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test failed. Missing steam_api64.dll steam_api.dll.");
            textDebug.text = "[Steamworks.NET] DllCheck Test failed. Missing steam_api64.dll steam_api.dll.";
            return;
        }

        try
        {
            System.IO.File.WriteAllText("steam_appid.txt", appId.ToString());

            isInitialized = SteamAPI.Init();

            if (!isInitialized)
            {
                Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Is Steam running?");
                textDebug.text = "[Steamworks.NET] SteamAPI_Init() failed. Is Steam running?";
                return;
            }

            string personName = SteamFriends.GetPersonaName();
            CSteamID steamId = SteamUser.GetSteamID();
            Debug.Log($"[Steamworks.NET] Connected! Player: {personName} ({steamId})");
            textDebug.text = $"[Steamworks.NET] Connected! Player: {personName} ({steamId})";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Steamworks.NET] Exception during init: {e.Message}");
            textDebug.text = $"[Steamworks.NET] Exception during init: {e.Message}";
        }
    }

    private void Update()
    {
        if (isInitialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    private void OnApplicationQuit()
    {
        ShutdownSteam();
    }

    private void ShutdownSteam()
    {
        if (isInitialized)
        {
            try
            {
                SteamAPI.RunCallbacks();
                SteamAPI.Shutdown();

                Debug.Log("[Steamworks.NET] SteamAPI Shutdown completed.");
                textDebug.text = "[Steamworks.NET] SteamAPI Shutdown completed.";
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Steamworks.NET] Shutdown exception: {e.Message}");
                textDebug.text = $"[Steamworks.NET] Shutdown exception: {e.Message}";
            }
            finally
            {
                isInitialized = false;
            }
        }
    }
}
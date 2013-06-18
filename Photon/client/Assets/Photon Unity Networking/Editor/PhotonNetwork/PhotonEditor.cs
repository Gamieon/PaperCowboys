// ----------------------------------------------------------------------------
// <copyright file="PhotonEditor.cs" company="Exit Games GmbH">
//   PhotonNetwork Framework for Unity - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   MenuItems and in-Editor scripts for PhotonNetwork.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class PhotonEditor : EditorWindow
{
    protected static AccountService.Origin RegisterOrigin = AccountService.Origin.Pun;

    protected Vector2 scrollPos = Vector2.zero;

    protected static string DocumentationLocation = "Assets/Photon Unity Networking/PhotonNetwork-Documentation.pdf";

    protected static string UrlFreeLicense = "http://www.exitgames.com/Download/Photon";

    protected static string UrlDevNet = "http://doc.exitgames.com/photon-cloud";

    protected static string UrlForum = "http://forum.exitgames.com";

    protected static string UrlCompare = "http://doc.exitgames.com/photon-cloud/PhotonCloudvsServer";

    protected static string UrlHowToSetup = "http://doc.exitgames.com/photon-server/PhotonIn5Min";

    protected static string UrlAppIDExplained = "http://doc.exitgames.com/photon-cloud/PhotonDashboard";

    protected static string UrlAccountPage = "https://www.exitgames.com/Account/SignIn?email="; // opened in browser

    protected static string UrlCloudDashboard = "https://cloud.exitgames.com/Dashboard?email=";


    private enum GUIState
    {
        Uninitialized, 

        Main, 

        Setup
    }

    private enum PhotonSetupStates
    {
        RegisterForPhotonCloud, 

        EmailAlreadyRegistered, 

        SetupPhotonCloud, 

        SetupSelfHosted
    }

    private GUIState guiState = GUIState.Uninitialized;

    private bool isSetupWizard = false;

    private PhotonSetupStates photonSetupState = PhotonSetupStates.RegisterForPhotonCloud;
    
    private static double lastWarning = 0;

    private string photonAddress = "127.0.0.1";

    private int photonPort = ServerSettings.DefaultMasterPort;

    private string emailAddress = string.Empty;

    private string cloudAppId = string.Empty;
    
    private static bool dontCheckPunSetupField;

    private static Texture2D HelpIcon;
    private static Texture2D WizardIcon;

    /// <summary>
    /// Can be used to (temporarily) disable the checks for PUN Setup and scene PhotonViews.
    /// This will prevent scene PhotonViews from being updated, so be careful. 
    /// When you re-set this value, checks are used again and scene PhotonViews get IDs as needed.
    /// </summary>
    protected static bool dontCheckPunSetup
    {
        get
        {
            return dontCheckPunSetupField;
        }
        set
        {
            if (dontCheckPunSetupField != value)
            {
                dontCheckPunSetupField = value;
            }
        }
    }

    protected static Type WindowType = typeof(PhotonEditor);

    protected static string WindowTitle = "PUN Wizard";

    static PhotonEditor()
    {
        EditorApplication.projectWindowChanged += EditorUpdate;
        EditorApplication.hierarchyWindowChanged += EditorUpdate;
        EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
        EditorApplication.update += OnUpdate;
        
        HelpIcon = AssetDatabase.LoadAssetAtPath("Assets/Photon Unity Networking/Editor/PhotonNetwork/help.png", typeof(Texture2D)) as Texture2D;
        WizardIcon = AssetDatabase.LoadAssetAtPath("Assets/Photon Unity Networking/photoncloud-icon.png", typeof(Texture2D)) as Texture2D;
    }

    [MenuItem("Window/Photon Unity Networking &p")]
    protected static void Init()
    {
        PhotonEditor win = GetWindow(WindowType, false, WindowTitle, true) as PhotonEditor;
        win.InitPhotonSetupWindow();
     
        win.isSetupWizard = false;
        win.SwitchMenuState(GUIState.Main);
    }


    /// <summary>Creates an Editor window, showing the cloud-registration wizard for Photon (entry point to setup PUN).</summary>
    protected static void ShowRegistrationWizard()
    {
        PhotonEditor win = GetWindow(WindowType, false, WindowTitle, true) as PhotonEditor;
        win.isSetupWizard = true;
        win.InitPhotonSetupWindow();
    }

    /// <summary>Re-initializes the Photon Setup window and shows one of three states: register cloud, setup cloud, setup self-hosted.</summary>
    protected void InitPhotonSetupWindow()
    {
        this.minSize = MinSize;

        this.SwitchMenuState(GUIState.Setup);
        this.ReApplySettingsToWindow();

        switch (PhotonEditor.Current.HostType)
        {
            case ServerSettings.HostingOption.PhotonCloud:
                this.photonSetupState = PhotonSetupStates.SetupPhotonCloud;
                break;
            case ServerSettings.HostingOption.SelfHosted:
                this.photonSetupState = PhotonSetupStates.SetupSelfHosted;
                break;
            case ServerSettings.HostingOption.NotSet:
            default:
                this.photonSetupState = PhotonSetupStates.RegisterForPhotonCloud;
                break;
        }
    }

    static bool DidRefresh;

    // called 100 times / sec but we only check if isCompiling
    private static void OnUpdate()
    {
        if (!DidRefresh && !EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            // Debug.Log("Post-compile refresh of RPC index. isPlayingOrWillChangePlaymode: " + EditorApplication.isPlayingOrWillChangePlaymode);
            DidRefresh = true;
            UpdateRpcList();
        }
    }

    // called in editor, opens wizard for initial setup, keeps scene PhotonViews up to date and closes connections when compiling (to avoid issues)
    private static void EditorUpdate()
    {
        if (dontCheckPunSetup || PhotonEditor.Current == null)
        {
            return;
        }

        // serverSetting is null when the file gets deleted. otherwise, the wizard should only run once and only if hosting option is not (yet) set
        if (!PhotonEditor.Current.DisableAutoOpenWizard && PhotonEditor.Current.HostType == ServerSettings.HostingOption.NotSet)
        {
            ShowRegistrationWizard();
        }

        // Workaround for TCP crash. Plus this surpresses any other recompile errors.
        if (EditorApplication.isCompiling)
        {
            if (PhotonNetwork.connected)
            {
                if (lastWarning > EditorApplication.timeSinceStartup - 3)
                {
                    // Prevent error spam
                    Debug.LogWarning("Unity recompile forced a Photon Disconnect");
                    lastWarning = EditorApplication.timeSinceStartup;
                }

                PhotonNetwork.Disconnect();
            }
        }
    }

    // called in editor on change of play-mode (used to show a message popup that connection settings are incomplete)
    private static void PlaymodeStateChanged()
    {
        if (dontCheckPunSetup || EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (PhotonEditor.Current.HostType == ServerSettings.HostingOption.NotSet)
        {
            EditorUtility.DisplayDialog("Warning", "You have not yet run the Photon setup wizard! Your game won't be able to connect. See Windows -> Photon Unity Networking.", "Ok");
        }
    }

    private void SwitchMenuState(GUIState newState)
    {
        this.guiState = newState;
        if (this.isSetupWizard && newState != GUIState.Setup)
        {
            this.Close();
        }
    }

    protected virtual void OnGUI()
    {
        this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);

        if (this.guiState == GUIState.Uninitialized)
        {
            this.ReApplySettingsToWindow();
            this.guiState = (PhotonEditor.Current.HostType == ServerSettings.HostingOption.NotSet) ? GUIState.Setup : GUIState.Main;
        }

        if (this.guiState == GUIState.Main)
        {
            this.OnGuiMainWizard();
        }
        else
        {
            this.OnGuiRegisterCloudApp();
        }

        GUILayout.EndScrollView();
    }

    protected virtual void OnGuiRegisterCloudApp()
    {
        GUI.skin.label.wordWrap = true;
        if (!this.isSetupWizard)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Main Menu", GUILayout.ExpandWidth(false)))
            {
                this.SwitchMenuState(GUIState.Main);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(15);
        }

        if (this.photonSetupState == PhotonSetupStates.RegisterForPhotonCloud)
        {
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Connect to Photon Cloud");
            EditorGUILayout.Separator();
            GUI.skin.label.fontStyle = FontStyle.Normal;

            GUILayout.Label("Using the Photon Cloud is free for development. If you don't have an account yet, enter your email and register.");
            EditorGUILayout.Separator();
            this.emailAddress = EditorGUILayout.TextField("Email:", this.emailAddress);

            if (GUILayout.Button("Send"))
            {
                GUIUtility.keyboardControl = 0;
                this.RegisterWithEmail(this.emailAddress);
            }

            GUILayout.Space(20);


            GUILayout.Label("I am already signed up. Let me enter my AppId.");
            if (GUILayout.Button("Setup"))
            {
                this.photonSetupState = PhotonSetupStates.SetupPhotonCloud;
            }
            EditorGUILayout.Separator();


            GUILayout.Label("I want to register by a website.");
            if (GUILayout.Button("Open account website"))
            {
                EditorUtility.OpenWithDefaultApp(UrlAccountPage + Uri.EscapeUriString(this.emailAddress));
            }

            EditorGUILayout.Separator();

            GUILayout.Label("I want to host my own server. Let me set it up.");

            if (GUILayout.Button("Open self-hosting settings"))
            {
                this.photonAddress = ServerSettings.DefaultServerAddress;
                this.photonPort = ServerSettings.DefaultMasterPort;
                this.photonSetupState = PhotonSetupStates.SetupSelfHosted;
            }

            GUILayout.FlexibleSpace();


            if (!InternalEditorUtility.HasAdvancedLicenseOnBuildTarget(BuildTarget.Android) || !InternalEditorUtility.HasAdvancedLicenseOnBuildTarget(BuildTarget.iPhone))
            {
                GUILayout.Label("Note: Export to mobile will require iOS Pro / Android Pro.");
            }
            EditorGUILayout.Separator();
        }
        else if (this.photonSetupState == PhotonSetupStates.EmailAlreadyRegistered)
        {
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Oops!");
            GUI.skin.label.fontStyle = FontStyle.Normal;

            GUILayout.Label("The provided e-mail-address has already been registered.");

            if (GUILayout.Button("Mh, see my account page"))
            {
                EditorUtility.OpenWithDefaultApp(UrlCloudDashboard + Uri.EscapeUriString(this.emailAddress));
            }

            EditorGUILayout.Separator();

            GUILayout.Label("Ah, I know my Application ID. Get me to setup.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                this.photonSetupState = PhotonSetupStates.RegisterForPhotonCloud;
            }

            if (GUILayout.Button("Setup"))
            {
                this.photonSetupState = PhotonSetupStates.SetupPhotonCloud;
            }

            GUILayout.EndHorizontal();
        }
        else if (this.photonSetupState == PhotonSetupStates.SetupPhotonCloud)
        {
            // cloud setup
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Connect to Photon Cloud");
            GUI.skin.label.fontStyle = FontStyle.Normal;

            EditorGUILayout.Separator();
            this.OnGuiSetupCloudAppId();
            this.OnGuiCompareAndHelpOptions();
        }
        else if (this.photonSetupState == PhotonSetupStates.SetupSelfHosted)
        {
            // self-hosting setup
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUILayout.Label("Setup own Photon Host");
            GUI.skin.label.fontStyle = FontStyle.Normal;

            EditorGUILayout.Separator();

            this.OnGuiSetupSelfhosting();
            this.OnGuiCompareAndHelpOptions();
        }
    }

    protected virtual void OnGuiMainWizard()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(WizardIcon);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        GUILayout.Label("Photon Unity Networking (PUN) Wizard", EditorStyles.boldLabel);
        if (!InternalEditorUtility.HasAdvancedLicenseOnBuildTarget(BuildTarget.Android) || !InternalEditorUtility.HasAdvancedLicenseOnBuildTarget(BuildTarget.iPhone))
        {
            GUILayout.Label("Note: Export to mobile will require iOS Pro / Android Pro.");
        }
        EditorGUILayout.Separator();

        // settings button
        GUILayout.BeginHorizontal();
        GUILayout.Label("Settings", EditorStyles.boldLabel, GUILayout.Width(100));
        if (GUILayout.Button(new GUIContent("Setup", "Setup wizard for setting up your own server or the cloud.")))
        {
            this.InitPhotonSetupWindow();
        }

        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        // converter
        GUILayout.BeginHorizontal();
        GUILayout.Label("Converter", EditorStyles.boldLabel, GUILayout.Width(100));
        if (GUILayout.Button(new GUIContent("Start", "Converts pure Unity Networking to Photon Unity Networking.")))
        {
            PhotonConverter.RunConversion();
        }

        GUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        // find / select settings asset
        GUILayout.BeginHorizontal();
        GUILayout.Label("Settings File", EditorStyles.boldLabel, GUILayout.Width(100));
        if (GUILayout.Button(new GUIContent("Locate settings asset", "Highlights the used photon settings file in the project.")))
        {
            EditorGUIUtility.PingObject(PhotonEditor.Current);
        }

        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        // documentation
        GUILayout.BeginHorizontal();
        GUILayout.Label("Documentation", EditorStyles.boldLabel, GUILayout.Width(100));
        GUILayout.BeginVertical();
        if (GUILayout.Button(new GUIContent("Open PDF", "Opens the local documentation pdf.")))
        {
            EditorUtility.OpenWithDefaultApp(DocumentationLocation);
        }

        if (GUILayout.Button(new GUIContent("Open DevNet", "Online documentation for Photon.")))
        {
            EditorUtility.OpenWithDefaultApp(UrlDevNet);
        }

        if (GUILayout.Button(new GUIContent("Open Cloud Dashboard", "Review Cloud App information and statistics.")))
        {
            EditorUtility.OpenWithDefaultApp(UrlCloudDashboard + Uri.EscapeUriString(this.emailAddress));
        }

        if (GUILayout.Button(new GUIContent("Open Forum", "Online support for Photon.")))
        {
            EditorUtility.OpenWithDefaultApp(UrlForum);
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();   
    }

    protected virtual void OnGuiCompareAndHelpOptions()
    {
        GUILayout.FlexibleSpace();

        GUILayout.Label("Questions? Need help or want to give us feedback? You are most welcome!");
        if (GUILayout.Button("See the Photon Forum"))
        {
            Application.OpenURL(UrlForum);
        }

        if (photonSetupState != PhotonSetupStates.SetupSelfHosted)
        {
            if (GUILayout.Button("Open Dashboard (web)"))
            {
                EditorUtility.OpenWithDefaultApp(UrlCloudDashboard + Uri.EscapeUriString(this.emailAddress));
            }
        }
    }

    bool open = false;
    bool helpRegion = false;

    protected virtual void OnGuiSetupCloudAppId()
    {
        GUILayout.Label("Your AppId");

        GUILayout.BeginHorizontal();
        this.cloudAppId = EditorGUILayout.TextField(this.cloudAppId);
        
        open = GUILayout.Toggle(open, HelpIcon, GUIStyle.none, GUILayout.ExpandWidth(false));
        
        GUILayout.EndHorizontal();
        
        if (open) GUILayout.Label("The AppId a Guid that identifies your game in the Photon Cloud. Find it on your dashboard page.");



        EditorGUILayout.Separator();

        GUILayout.Label("Cloud Region");

        int selectedRegion = ServerSettings.FindRegionForServerAddress(this.photonAddress);


        GUILayout.BeginHorizontal();
        int toolbarValue = GUILayout.Toolbar(selectedRegion, ServerSettings.CloudServerRegionNames);
        helpRegion = GUILayout.Toggle(helpRegion, HelpIcon, GUIStyle.none, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        if (helpRegion) GUILayout.Label("Photon Cloud has regional servers. Picking one near your customers improves ping times. You could use more than one but this setup does not support it.");

        if (selectedRegion != toolbarValue)
        {
            //Debug.Log("Replacing region: " + selectedRegion + " with: " + toolbarValue);
            this.photonAddress = ServerSettings.FindServerAddressForRegion(toolbarValue);
        }

        EditorGUILayout.Separator();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            GUIUtility.keyboardControl = 0;
            this.ReApplySettingsToWindow();
        }

        

        if (GUILayout.Button("Save"))
        {
            GUIUtility.keyboardControl = 0;
			this.cloudAppId = this.cloudAppId.Trim();
            PhotonEditor.Current.UseCloud(this.cloudAppId, selectedRegion);
            PhotonEditor.Save();

            EditorUtility.DisplayDialog("Success", "Saved your settings.", "ok");
        }

        GUILayout.EndHorizontal();


        
        GUILayout.Space(20);

        GUILayout.Label("Running my app in the cloud was fun but...\nLet me setup my own Photon server.");

        if (GUILayout.Button("Open self-hosting settings"))
        {
            this.photonAddress = ServerSettings.DefaultServerAddress;
            this.photonPort = ServerSettings.DefaultMasterPort;
            this.photonSetupState = PhotonSetupStates.SetupSelfHosted;
        }

        EditorGUILayout.Separator();
        GUILayout.Label("I am not quite sure how 'my own host' compares to 'cloud'.");
        if (GUILayout.Button("See comparison page"))
        {
            Application.OpenURL(UrlCompare);
        }
    }

    protected virtual void OnGuiSetupSelfhosting()
    {
        GUILayout.Label("Your Photon Server");

        this.photonAddress = EditorGUILayout.TextField("Address/ip:", this.photonAddress);
        this.photonPort = EditorGUILayout.IntField("Port:", this.photonPort);

        EditorGUILayout.Separator();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            GUIUtility.keyboardControl = 0;
            this.ReApplySettingsToWindow();
        }

        if (GUILayout.Button("Save"))
        {
            GUIUtility.keyboardControl = 0;

            PhotonEditor.Current.UseMyServer(this.photonAddress, this.photonPort, null);
            PhotonEditor.Save();

            EditorUtility.DisplayDialog("Success", "Saved your settings.", "ok");
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        // license
        GUILayout.BeginHorizontal();
        GUILayout.Label("Licenses", EditorStyles.boldLabel, GUILayout.Width(100));

        if (GUILayout.Button(new GUIContent("Free License Download", "Get your free license for up to 100 concurrent players.")))
        {
            EditorUtility.OpenWithDefaultApp(UrlFreeLicense);
        }

        GUILayout.EndHorizontal();


        GUILayout.Space(20);


        GUILayout.Label("Running my own server is too much hassle..\nI want to give Photon's free app a try.");

        if (GUILayout.Button("Get the free cloud app"))
        {
            this.cloudAppId = string.Empty;
            this.photonSetupState = PhotonSetupStates.RegisterForPhotonCloud;
        }

        EditorGUILayout.Separator();
        GUILayout.Label("I am not quite sure how 'my own host' compares to 'cloud'.");
        if (GUILayout.Button("See comparison page"))
        {
            Application.OpenURL(UrlCompare);
        }
    }

    protected virtual void RegisterWithEmail(string email)
    {
        EditorUtility.DisplayProgressBar("Connecting", "Connecting to the account service..", 0.5f);
        var client = new AccountService();
        client.RegisterByEmail(email, RegisterOrigin); // this is the synchronous variant using the static RegisterOrigin. "result" is in the client

        EditorUtility.ClearProgressBar();
        if (client.ReturnCode == 0)
        {
            PhotonEditor.Current.UseCloud(client.AppId, 0);
            PhotonEditor.Save();
            this.ReApplySettingsToWindow();
            this.photonSetupState = PhotonSetupStates.SetupPhotonCloud;
        }
        else
        {
            if (client.Message.Contains("Email already registered"))
            {
                this.photonSetupState = PhotonSetupStates.EmailAlreadyRegistered;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", client.Message, "OK");
                // Debug.Log(client.Exception);
                this.photonSetupState = PhotonSetupStates.RegisterForPhotonCloud;
            }
        }
    }

    #region SettingsFileHandling

    private static ServerSettings currentSettings;
    private Vector2 MinSize = new Vector2(350, 400);

    public static ServerSettings Current
    {
        get
        {
            if (currentSettings == null)
            {
                // find out if ServerSettings can be instantiated (existing script check)
                ScriptableObject serverSettingTest = CreateInstance("ServerSettings");
                if (serverSettingTest == null)
                {
                    Debug.LogError("Photon Unity Networking (PUN) is missing the 'ServerSettings' script. Re-import PUN to fix this.");
                    return null;
                }
                DestroyImmediate(serverSettingTest);

                // try to load settings from file
                ReLoadCurrentSettings();

                // if still not loaded, create one
                if (currentSettings == null)
                {
                    string settingsPath = Path.GetDirectoryName(PhotonNetwork.serverSettingsAssetPath);
                    if (!Directory.Exists(settingsPath))
                    {
                        Directory.CreateDirectory(settingsPath);
                        AssetDatabase.ImportAsset(settingsPath);
                    }

                    currentSettings = (ServerSettings) ScriptableObject.CreateInstance("ServerSettings");
                    if (currentSettings != null)
                    {
                        AssetDatabase.CreateAsset(currentSettings, PhotonNetwork.serverSettingsAssetPath);
                    }
                    else
                    {
                        Debug.LogError("Photon Unity Networking (PUN) is missing the 'ServerSettings' script. Re-import PUN to fix this.");
                    }
                }
            }

            return currentSettings;
        }

        protected set
        {
            currentSettings = value;
        }
    }

    public static void Save()
    {
        EditorUtility.SetDirty(PhotonEditor.Current);
    }

    public static void ReLoadCurrentSettings()
    {
        // this now warns developers if there are more than one settings files in resources folders. first will be used.
        UnityEngine.Object[] settingFiles = Resources.LoadAll(PhotonNetwork.serverSettingsAssetFile, typeof(ServerSettings));
        if (settingFiles != null && settingFiles.Length > 0)
        {
            PhotonEditor.Current = (ServerSettings)settingFiles[0];

            if (settingFiles.Length > 1)
            {
                Debug.LogWarning("There are more than one " + PhotonNetwork.serverSettingsAssetFile + " files in 'Resources' folder. Check your project to keep only one. Using: " + AssetDatabase.GetAssetPath(PhotonEditor.Current));
            }
        }
    }

    protected void ReApplySettingsToWindow()
    {
        this.cloudAppId = string.IsNullOrEmpty(PhotonEditor.Current.AppID) ? string.Empty : PhotonEditor.Current.AppID;
        this.photonAddress = string.IsNullOrEmpty(PhotonEditor.Current.ServerAddress) ? string.Empty : PhotonEditor.Current.ServerAddress;
        this.photonPort = PhotonEditor.Current.ServerPort;
    }
    
    public static void UpdateRpcList()
    {
        HashSet<string> additionalRpcs = new HashSet<string>();
        HashSet<string> currentRpcs = new HashSet<string>();

        var types = GetAllSubTypesInScripts(typeof(MonoBehaviour));

        foreach (var mono in types)
        {
            MethodInfo[] methods = mono.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MethodInfo method in methods)
            {
                if (method.IsDefined(typeof(UnityEngine.RPC), false))
                {
                    currentRpcs.Add(method.Name);

                    if (!PhotonEditor.Current.RpcList.Contains(method.Name))
                    {
                        additionalRpcs.Add(method.Name);
                    }
                }
            }
        }

        if (additionalRpcs.Count > 0)
        {
            // LIMITS RPC COUNT
            if (additionalRpcs.Count + PhotonEditor.Current.RpcList.Count >= byte.MaxValue)
            {
                if (currentRpcs.Count <= byte.MaxValue)
                {
                    bool clearList = EditorUtility.DisplayDialog("Warning: RPC-list becoming incompatible!", "Your project's RPC-list is full, so we can't add some RPCs just compiled.\n\nBy removing outdated RPCs, the list will be long enough but incompatible with older client builds!\n\nMake sure you change the game version where you use PhotonNetwork.ConnectUsingSettings().", "Remove outdated RPCs", "Cancel");
                    if (clearList)
                    {
                        PhotonEditor.Current.RpcList.Clear();
                        PhotonEditor.Current.RpcList.AddRange(currentRpcs);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning: RPC-list is full!", "Your project's RPC-list is too long for PUN.\n\nYou can change PUN's source to use short-typed RPC index. Look for comments 'LIMITS RPC COUNT'\n\nAlternatively, remove some RPC methods (use more parameters per RPC maybe).\n\nAfter a RPC-list refresh, make sure you change the game version where you use PhotonNetwork.ConnectUsingSettings().", "Skip RPC-list update");
                    return;
                }
            }

            PhotonEditor.Current.RpcList.AddRange(additionalRpcs);
            EditorUtility.SetDirty(PhotonEditor.Current);
        }
    }

    public static void ClearRpcList()
    {
        bool clearList = EditorUtility.DisplayDialog("Warning: RPC-list Compatibility", "PUN replaces RPC names with numbers by using the RPC-list. All clients must use the same list for that.\n\nClearing it most likely makes your client incompatible with previous versions! Change your game version or make sure the RPC-list matches other clients.", "Clear RPC-list", "Cancel");
        if (clearList)
        {
            PhotonEditor.Current.RpcList.Clear();
            Debug.LogWarning("Cleared the PhotonServerSettings.RpcList! This makes new builds incompatible with older ones. Better change game version in PhotonNetwork.ConnectUsingSettings().");
        }
    }

    public static System.Type[] GetAllSubTypesInScripts(System.Type aBaseClass)
    {
        var result = new System.Collections.Generic.List<System.Type>();
        System.Reflection.Assembly[] AS = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var A in AS)
        {
            // this skips all but the Unity-scripted assemblies for RPC-list creation. You could remove this to search all assemblies in project
            if (!A.FullName.StartsWith("Assembly-"))
            {
                // Debug.Log("Skipping Assembly: " + A);
                continue;
            }

            //Debug.Log("Assembly: " + A.FullName);
            System.Type[] types = A.GetTypes();
            foreach (var T in types)
            {
                if (T.IsSubclassOf(aBaseClass))
                    result.Add(T);
            }
        }
        return result.ToArray();
    }

    #endregion
}

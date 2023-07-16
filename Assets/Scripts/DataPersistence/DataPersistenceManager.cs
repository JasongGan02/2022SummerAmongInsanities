using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool disableDataPersistence = false;
    [SerializeField] private bool initializeDataIfNull = false;
    [SerializeField] private bool overrideSelectedProfileId = false;
    [SerializeField] private string testSelectedProfileId = "test";

    [Header("File Storage Config")]

    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    [Header("Auto Saving Config")]
    [SerializeField] private float autoSaveTimeSeconds = 60f;

    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    private string selectedProfileId = "";

    private Coroutine autoSaveCoroutine;
    public static DataPersistenceManager instance { get; private set; } 

    private void Awake()
    {
        if(instance != null)
        {
            Debug.Log("Found more than one Data Persistence Manager in the scene. Destroy the newest one.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (disableDataPersistence)
        {
            Debug.LogWarning("Data Persistence is currently disabled!");
        }
        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);

       InitializeSelectedProfileId();
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();

        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
        }
        autoSaveCoroutine = StartCoroutine(AutoSave());
    }


    public void ChangeSelectedProfileId(string newProfileId)
    {
        this.selectedProfileId = newProfileId;
        LoadGame(); 
    }
    
    public void DeleteProfileData(string profileId)
    {
        dataHandler.Delete(profileId);
        InitializeSelectedProfileId();
        LoadGame();
    }

    private void InitializeSelectedProfileId()
    {
        this.selectedProfileId = dataHandler.GetMostRecentlyUpdatedProfileId();

        if (overrideSelectedProfileId)
        {
            this.selectedProfileId = testSelectedProfileId;
            Debug.LogWarning("Overrode Selected Profile Id with test id: " + testSelectedProfileId);
        }
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {   
        if (disableDataPersistence)
        {
            return;
        }
        //load any saved data from a file using the data handler.
        this.gameData = dataHandler.Load(selectedProfileId);
        Debug.Log(Application.persistentDataPath);

        if (this.gameData == null && initializeDataIfNull)
        {
            Debug.Log("No saved data found. Initializing new data.");
            NewGame();
        }

        //if no data can be loaded, don't continue
        if (this.gameData == null)
        {
            Debug.Log("No saved data found. A new game needs to be started before data can be loaded.");
            return;
        }

        //TODO - push the loaded data to all other scripts that need it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }


        
    }

    public void SaveGame()
    {
        if (disableDataPersistence)
        {
            return;
        }

        if(this.gameData == null)
        {
            Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
            return;
        }
        // pass the data to other scripts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(gameData);
        }
        
        gameData.lastUpdated = System.DateTime.Now.ToBinary();
        // save that data to a file using the data handler
        dataHandler.Save(gameData, selectedProfileId);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public bool HasGameData()
    {
        return this.gameData != null;
    }

    public Dictionary<string, GameData> GetAllProfilesGameData()
    {
        return dataHandler.LoadAllProfiles();
    }

    private IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveTimeSeconds);
            SaveGame();
            Debug.Log("Auto Saved Game.");
        }
    }
}

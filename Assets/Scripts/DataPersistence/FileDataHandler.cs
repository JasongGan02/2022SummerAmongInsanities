using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
   private string dataDirPath = "";
   private string dataFileName = "";
   private bool useEncryption = false;
   private readonly string encryptionCodeWord = "word";
   private readonly string backupExtension = ".bak";

   public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
   {
      this.dataDirPath = dataDirPath;
      this.dataFileName = dataFileName;
      this.useEncryption = useEncryption;
   }


   public void Save(GameData data, string profileId)
   {
        if (profileId == null)
        {
            return;
        }
        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName); //for diff OS's
        string backupFilePath = fullPath + backupExtension;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            //Serialize C# object to JSON 
            string dataToStore = JsonUtility.ToJson(data, true);

            if(useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            GameData verifiedGameData = Load(profileId);
            if (verifiedGameData != null)
            {
                File.Copy(fullPath, backupFilePath, true);
            }
            else
            {
                throw new Exception("Save file could not be verified and backup could not be created.");
                
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
        
   }

   public GameData Load(string profileId, bool allowRestoreFromBackup = true)
   {
        if (profileId == null)
        {
            return null;
        }

        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName); //for diff OS's
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                //load the serialized data from the file
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if(useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                // deserialize the data from Json back into the C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                if(allowRestoreFromBackup)
                {
                    Debug.LogWarning("Failed to load data file. Attempting to roll back \n" + e);
                    bool rollbackSuccess = AttemptRollback(fullPath);
                    if (rollbackSuccess)
                    {
                        //try to load again recursively
                        loadedData = Load(profileId, false);
                    }
                }
                else
                {
                    Debug.LogError("Error occured when trying to load file at path " + fullPath + " and backup did not work. \n" + e);
                }
            }
        }
        return loadedData;
   }

   public void Delete(string profileId)
   {
        if (profileId == null)
        {
            return;
        }

        string  fullPath = Path.Combine(dataDirPath, profileId, dataFileName); //for diff OS's
        try 
        {
            if (File.Exists(fullPath))
            {
                Directory.Delete(Path.GetDirectoryName(fullPath), true);
            }
            else
            {
                Debug.LogWarning("Tried to delete profile but it does not exist. ProfileId: " + profileId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to delete profile data for profileId: " + profileId + " at pathL: " + fullPath + "\n" + e);
        }
   }
   
   public Dictionary<string, GameData> LoadAllProfiles()
   {
        Dictionary<string, GameData> profileDictionary = new Dictionary<string, GameData>();
        IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();
        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            string profileId = dirInfo.Name;
            string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
            if (!File.Exists(fullPath))
            {
               Debug.LogWarning("Skipping directory when loading all profiles because it does not contain data: " + profileId);
               continue;
            }

            GameData profileData = Load(profileId);

            if(profileData != null)
            {
                profileDictionary.Add(profileId, profileData);
            }
            else
            {
                Debug.LogError("Tried to load profile but something went wrong. ProfileId: " + profileId);
            }
        }
        return profileDictionary;
   }

   public string GetMostRecentlyUpdatedProfileId()
   {
        string mostRecentProfileId = null;

        Dictionary<string, GameData> profilesGameData = LoadAllProfiles();
        foreach (KeyValuePair<string, GameData> pair in profilesGameData)
        {
            string profileId = pair.Key;
            GameData gameData = pair.Value;

            if (gameData == null)
            {
                continue;
            }
            
            if (mostRecentProfileId == null)
            {
                mostRecentProfileId = profileId;
            }
            else
            {
                DateTime mostRecentDateTime = DateTime.FromBinary(profilesGameData[mostRecentProfileId].lastUpdated);
                DateTime newDateTime = DateTime.FromBinary(gameData.lastUpdated);
                if (newDateTime > mostRecentDateTime)
                {
                    mostRecentProfileId = profileId;
                }
            }
        }

        return mostRecentProfileId;
   }

   private string EncryptDecrypt(string data)
   {
        string modifiedData = "";
        for (int i =0; i < data.Length; i++)
        {
            modifiedData += (char) (data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }  
        return modifiedData;
   }

   private bool AttemptRollback(string fullPath)
   {
        bool success = false;
        string backupFilePath = fullPath + backupExtension;
        try
        {
            if (File.Exists(backupFilePath))
            {
                File.Copy(backupFilePath, fullPath, true);
                success = true;
                Debug.LogWarning("Rollback successful for file: " + backupFilePath);
            }
            else
            {
                throw new Exception("Tried to roll back, but backup file does not exist");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to rollback data file: " + backupFilePath + "\n" + e);
        }
        return success;
   }
}

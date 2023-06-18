using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
   private string dataDirPath = "";
   private string dataFileName = "";

   public FileDataHandler(string dataDirPath, string dataFileName)
   {
      this.dataDirPath = dataDirPath;
      this.dataFileName = dataFileName;
   }

   public GameData Load()
   {
        string fullPath = Path.Combine(dataDirPath, dataFileName); //for diff OS's
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

                // deserialize the data from Json back into the C# object
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load data from file: " + fullPath + "\n" + e);
            }
        }
        return loadedData;
   }

   public void Save(GameData data)
   {
        string fullPath = Path.Combine(dataDirPath, dataFileName); //for diff OS's
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            //Serialize C# object to JSON 
            string dataToStore = JsonUtility.ToJson(data, true);

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to save data to file: " + fullPath + "\n" + e);
        }
        
   }
}

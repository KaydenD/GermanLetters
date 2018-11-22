using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
namespace SavedControls
{
    public class jsonStructTextboxes
    {
        // Field
        public string name { get; set; }
        public string value { get; set; }

        public jsonStructTextboxes(string name1, string value1)
        {
            name = name1;
            value = value1;
        }
    }

    public class jsonStructComboBoxes
    {
        // Field
        public string name { get; set; }
        public int value { get; set; }

        public jsonStructComboBoxes(string name1, int value1)
        {
            name = name1;
            value = value1;
        }
    }

    public class syncTextBoxNameNotSet : Exception
    {
        public syncTextBoxNameNotSet(string message)
           : base(message)
        {
        }
    }

    public class syncTextBox : TextBox
    {
        public static string saveDir { get; set; } = AppDomain.CurrentDomain.BaseDirectory.ToString() + "savedTextboxes.txt";
        private static Dictionary<syncTextBox, jsonStructTextboxes> listOfInstances = new Dictionary<syncTextBox, jsonStructTextboxes>();
        public static StatusEnum status { get; private set; } = StatusEnum.unloaded;
        public enum StatusEnum { unloaded, loading, loaded };
        public string preInitName { get; private set; }


        public syncTextBox()
        {
            TextChanged += TextChangedEventHandler;
            listOfInstances.Add(this, new jsonStructTextboxes(this.Name, this.Text));
        }

        private void TextChangedEventHandler(object sender, EventArgs e)
        {
            if (status == StatusEnum.loaded)
            {
                saveSyncBox(this);
                updateFileWithCurValues(saveDir);
            }
        }

        private static string readTxtFromFile(string dir)
        {
            StreamReader fileToSave;
            if (File.Exists(saveDir))
            {
                fileToSave = new StreamReader(saveDir);
            }
            else
            {
                StreamWriter FileWriter = new StreamWriter(saveDir, false);
                FileWriter.Close();
                FileWriter = null;
                fileToSave = new StreamReader(saveDir);
            }
            string json = fileToSave.ReadToEnd();
            fileToSave.Close();
            fileToSave = null;
            return json;
        }

        public static void updateFileWithCurValues()
        {
            StreamWriter FileWriter = new StreamWriter(saveDir, false);
            List<jsonStructTextboxes> tempList = new List<jsonStructTextboxes>(listOfInstances.Values);
            FileWriter.Write(JsonConvert.SerializeObject(tempList, Formatting.Indented));
            FileWriter.Close();
            FileWriter = null;
            tempList = null;
        }

        public static void updateFileWithCurValues(string dir)
        {
            StreamWriter FileWriter = new StreamWriter(dir, false);
            List<jsonStructTextboxes> tempList = new List<jsonStructTextboxes>(listOfInstances.Values);
            FileWriter.Write(JsonConvert.SerializeObject(tempList, Formatting.Indented));
            FileWriter.Close();
            FileWriter = null;
            tempList = null;
        }

        private static void changeNameOfBox(syncTextBox box, string newName, bool toSave = true)
        {
            listOfInstances[box].name = newName;
            if (toSave)
            {
                updateFileWithCurValues();
            }
        }

        public void saveSyncBox(syncTextBox oneTosave)
        {
            if (listOfInstances.ContainsKey(oneTosave))
            {
                listOfInstances[oneTosave].value = oneTosave.Text;
                listOfInstances[oneTosave].name = oneTosave.Name;
            }
            else
            {
                return;
            }
        }

        private static bool loadValuesFromKeys()
        {
            bool returnBool = false;
            foreach (KeyValuePair<syncTextBox, jsonStructTextboxes> loopVar in listOfInstances)
            {
                string temp1 = loopVar.Key.Name;
                if (temp1 == string.Empty)
                {
                    returnBool = true;
                    continue;
                }
                loopVar.Value.name = loopVar.Key.Name;
            }
            return returnBool;
        }

        private static void loadKeysFromValues()
        {
            foreach (KeyValuePair<syncTextBox, jsonStructTextboxes> loopVar in listOfInstances)
            {
                loopVar.Key.Text = loopVar.Value.value;
            }
        }

        public static bool load()
        {

            status = StatusEnum.loading;
            string json = readTxtFromFile(saveDir);
            if (loadValuesFromKeys())
            {
                throw new syncTextBoxNameNotSet("One or more syncTextBox.Name hasn't been set before calling the load method");
            }
            if (json == string.Empty)
            {
                foreach (KeyValuePair<syncTextBox, jsonStructTextboxes> kvp in listOfInstances)
                {
                    kvp.Key.saveSyncBox(kvp.Key);
                }
                updateFileWithCurValues(saveDir);
                status = StatusEnum.loaded;
                return false;
            }
            List<jsonStructTextboxes> listForJson = JsonConvert.DeserializeObject<List<jsonStructTextboxes>>(json);
            Dictionary<syncTextBox, jsonStructTextboxes> temp = new Dictionary<syncTextBox, jsonStructTextboxes>(listOfInstances);
            foreach (KeyValuePair<syncTextBox, jsonStructTextboxes> kvp in listOfInstances)
            {
                jsonStructTextboxes Struct = listForJson.Find(x => x.name.Equals(kvp.Key.Name));
                if (Struct == null)
                {
                    continue;
                }
                temp[kvp.Key] = Struct;
                Struct = null;
            }
            listOfInstances = temp;
            loadKeysFromValues();
            status = StatusEnum.loaded;
            temp = null;
            json = null;
            listForJson = null;
            return true;
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }

            set
            {
                if (status != StatusEnum.unloaded)
                {
                    base.Text = value;
                }
            }
        }

        public new string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                if (status == StatusEnum.loaded)
                {
                    base.Name = value;
                    changeNameOfBox(this, value);
                }
                else if (status == StatusEnum.loading)
                {
                    base.Name = value;
                }
                else if (preInitName == null)
                {
                    base.Name = value;
                    preInitName = value;
                }
            }
        }
    }

    public class syncComboBox : ComboBox
    {
        public static string saveDir { get; set; } = AppDomain.CurrentDomain.BaseDirectory.ToString() + "savedComboBoxes.txt";
        private static Dictionary<syncComboBox, jsonStructComboBoxes> listOfInstances = new Dictionary<syncComboBox, jsonStructComboBoxes>();
        public static StatusEnum status { get; private set; } = StatusEnum.unloaded;
        public enum StatusEnum { unloaded, loading, loaded };
        public static Action onAnyIndexChanged;

        public syncComboBox()
        {
            listOfInstances.Add(this, new jsonStructComboBoxes(this.Name, this.SelectedIndex));
            SelectedIndexChanged += SyncComboBox_SelectedIndexChanged;
        }

        private void SyncComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (status == StatusEnum.loaded)
            {
                UpdateSavedIndexes(this);
                if(onAnyIndexChanged != null)
                    onAnyIndexChanged();
            }

        }

        private static void UpdateSavedIndexes(syncComboBox x)
        {
            listOfInstances[x].value = x.SelectedIndex;
            SaveIndexsToFile();
        }



        public static void UpdateAllSavedIndexes()
        {
            foreach(KeyValuePair<syncComboBox, jsonStructComboBoxes> kvp in listOfInstances)
            {
                kvp.Value.value = kvp.Key.SelectedIndex;
            }
            SaveIndexsToFile();
        }

        private static void SaveIndexsToFile()
        {
            StreamWriter FileWriter = new StreamWriter(saveDir, false);
            List<jsonStructComboBoxes> jsonStructList = new List<jsonStructComboBoxes>(listOfInstances.Values);
            FileWriter.Write(JsonConvert.SerializeObject(jsonStructList, Formatting.Indented));
            FileWriter.Close();
            FileWriter = null;
            jsonStructList = null;
        }

        public static bool loadFromFile()
        {
            status = StatusEnum.loading;
            if (!File.Exists(saveDir))
            {
                status = StatusEnum.loaded;
                return false;
            }

            string json = readTxtFromFile(saveDir);

            if (json == string.Empty)
            {
                status = StatusEnum.loaded;
                return false;
            }

            List<jsonStructComboBoxes> listForJson;
            try
            {
                listForJson = JsonConvert.DeserializeObject<List<jsonStructComboBoxes>>(json);
            }
            catch { status = StatusEnum.loaded; return false; }

            Dictionary<syncComboBox, jsonStructComboBoxes> tempDict = new Dictionary<syncComboBox, jsonStructComboBoxes>(listOfInstances);
            foreach (KeyValuePair<syncComboBox, jsonStructComboBoxes> kvp in listOfInstances)
            {
                jsonStructComboBoxes structFromFile = listForJson.Find(x => x.name.Equals(kvp.Key.Name));
                if (structFromFile == null)
                {
                    continue;
                }
                tempDict[kvp.Key] = structFromFile;
                structFromFile = null;
            }
            listOfInstances = tempDict;
            setIndexsFromJsonStructs();
            status = StatusEnum.loaded;
            tempDict = null;
            json = null;
            listForJson = null;
            return true;
        }

        private static void setIndexsFromJsonStructs()
        {
            foreach (KeyValuePair<syncComboBox, jsonStructComboBoxes> kvp in listOfInstances)
            {
                kvp.Key.SelectedIndex = kvp.Value.value;
            }
        }

        private static string readTxtFromFile(string dir)
        {
            StreamReader fileToRead = new StreamReader(dir);
            string json = fileToRead.ReadToEnd();
            fileToRead.Close();
            fileToRead = null;
            return json;
        }

        public new string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                if (status == StatusEnum.unloaded)
                {
                    base.Name = value;
                    changeNameOfBox(this, value);
                }
            }
        }

        private void changeNameOfBox(syncComboBox syncComboBox, string value)
        {

            listOfInstances[syncComboBox].name = value;
        }
    }
}

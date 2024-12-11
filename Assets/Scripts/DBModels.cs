using System.Collections.Generic;
using UnityEngine;

namespace DBModels
{
    [System.Serializable]
    public class HomeSceneData
    {
        public UserData userData = new UserData();
        public Stations stations = new Stations();
    }

    public class UserData
    {
        public string nickname;
        public int camellia;
    }

    [System.Serializable]
    public class Stations
    {
        public List<Station> stationList = new List<Station>();

        public Station getStation(string stationName)
        {
            for(int i = 0; i < stationList.Count; i++)
            {
                if (stationList[i].stationName == stationName) { return stationList[i]; }
            }

            return null;
        }
    }

    [System.Serializable]
    public class Station
    {
        public string stationName;
        public int totalMapCount;
        public int clearedMapCount;
        public int unlockCamellia;
        public int isUnlocked;
        public List<Maps> maps = new List<Maps>();
    }


    [System.Serializable]
    public class Maps
    {
        public string Station;
        public string mapName;
        public int mapNumber;

        public IngameObjectList ingameObjectList = new IngameObjectList();
    }

    [System.Serializable]
    public class IngameObjectList
    {
        public List<IngameObject> ingameObjectList = new List<IngameObject>();

        public int getChecked()
        {
            int result = 0; 
            for (int i = 0; i < ingameObjectList.Count; i++)
            {
                if(ingameObjectList[i].objectInfo.isChecked == 1)
                {
                    result++;
                }
            }

            return result;
        }
    }

    [System.Serializable]
    public class IngameObject
    {
        public string id;

        public ObjectInfo objectInfo = new ObjectInfo();
    }

    [System.Serializable]
    public class ObjectInfo
    {
        public string Description;
        public int isChecked;
    }
}
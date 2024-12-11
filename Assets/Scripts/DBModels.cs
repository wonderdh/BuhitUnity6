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

        public IngameObjects ingameObjects;
    }

    [System.Serializable]
    public class IngameObjects
    {
        public List<Object> objects = new List<Object>();
    }
}
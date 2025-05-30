using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public List<GameObject> districts;
    public Dictionary<string, GameObject> districtDict;

    private void Start()
    {
        districtDict = districts.ToDictionary(e => e.name, e => e);
    }

    public string GetDistrictsName()
    {
        HashSet<string> uniqueNames = new HashSet<string>();

        foreach (var district in districts)
        {
            uniqueNames.Add(district.name);
        }

        string districtsName = string.Join(" | ", uniqueNames);
        return districtsName;
    }
}
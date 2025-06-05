using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitManager : Singleton<UnitManager>
{
    [SerializeField] private List<UnitController> UnitList = new List<UnitController>();

    //[SerializeField]
    public PlayerController PlayerUnit;

    public Dictionary<string, AllayController> allayUnitDict = new Dictionary<string, AllayController>();

    private void Start()
    {
        allayUnitDict = UnitList.OfType<AllayController>().ToDictionary(e => e.GetTeammateAI().teammateName, e => e);
    }

    public void SpawnUnit(UnitController unitController, Vector3 spawnPoint, Quaternion spawnRotation)
    {
        Instantiate(unitController, spawnPoint, spawnRotation);
        UnitList.Add(unitController);
    }
}
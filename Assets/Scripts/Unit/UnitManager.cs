using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitManager : Singleton<UnitManager>
{
    //TODO 리소스 관리 따로 필요?
    [SerializeField] private PlayerController PlayerUnitPrefab;
    [SerializeField] private TeammateController TeammateUnitPrefab;

    //모든 유닛 list
    private List<UnitController> UnitList = new List<UnitController>();

    //유닛 타입별 list
    private Dictionary<UnitController.EUnitTeamType, List<UnitController>> unitTeamTypeDict =
        new Dictionary<UnitController.EUnitTeamType, List<UnitController>>();

    public Dictionary<string, TeammateController> teammateUnitDict = new Dictionary<string, TeammateController>();

    private PlayerController playerUnit;


    private void Start()
    {
        foreach (UnitController.EUnitTeamType unitTeamType in Enum.GetValues(typeof(UnitController.EUnitTeamType)))
        {
            unitTeamTypeDict.Add(unitTeamType, new List<UnitController>());
        }

        /*teammateUnitDict = UnitList.OfType<TeammateController>()
            .ToDictionary(e => e.GetTeammateAI().teammateName, e => e);*/
    }

    public void InitSpawnUnit()
    {
        //플레이어유닛, 동료유닛
        playerUnit = SpawnUnit(PlayerUnitPrefab, new Vector3(-20, 0, -8), PlayerUnitPrefab.transform.rotation,
            UnitController.EUnitTeamType.Allay);
        SpawnUnit(TeammateUnitPrefab, new Vector3(14, -16, -12), TeammateUnitPrefab.transform.rotation,
            UnitController.EUnitTeamType.Allay);
        
        //TODO 동료유닛 생성 시 실행되도록 코드 리팩토링 필요
        teammateUnitDict = UnitList.OfType<TeammateController>()
            .ToDictionary(e => e.GetTeammateAI().teammateName, e => e);
    }

    public T SpawnUnit<T>(T unitController, Vector3 spawnPoint, Quaternion spawnRotation,
        UnitController.EUnitTeamType newUnitTeamType) where T : UnitController
    {
        T unit = Instantiate(unitController, spawnPoint, spawnRotation);
        unit.Init(newUnitTeamType);

        UnitList.Add(unit);
        unitTeamTypeDict[newUnitTeamType].Add(unit);

        return unit;
    }

    public List<UnitController> GetUnitTeamTypeList(UnitController.EUnitTeamType unitTeamType)
    {
        return unitTeamTypeDict[unitTeamType];
    }

    public PlayerController GetPlayerUnit()
    {
        return playerUnit;
    }
}
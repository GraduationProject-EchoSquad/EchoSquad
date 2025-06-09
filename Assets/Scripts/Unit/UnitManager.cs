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


    private void Awake()
    {
        foreach (UnitController.EUnitTeamType unitTeamType in Enum.GetValues(typeof(UnitController.EUnitTeamType)))
        {
            unitTeamTypeDict.Add(unitTeamType, new List<UnitController>());
        }
    }

    public void InitSpawnUnit()
    {
        //플레이어유닛, 동료유닛
        playerUnit = SpawnUnit(PlayerUnitPrefab, new Vector3(-20, 0, -8), PlayerUnitPrefab.transform.rotation,
            UnitController.EUnitTeamType.Allay);
        SpawnUnit(TeammateUnitPrefab, new Vector3(-20, 0, -8), TeammateUnitPrefab.transform.rotation,
            UnitController.EUnitTeamType.Allay);
    }

    public T SpawnUnit<T>(T unitController, Vector3 spawnPoint, Quaternion spawnRotation,
        UnitController.EUnitTeamType newUnitTeamType) where T : UnitController
    {
        T unit = Instantiate(unitController, spawnPoint, spawnRotation);
        unit.Init(newUnitTeamType);

        UnitList.Add(unit);
        unitTeamTypeDict[newUnitTeamType].Add(unit);

        //동료유닛은 teammateUnitDict에 처리
        if (unit is TeammateController teammateController)
        {
            teammateUnitDict.Add(teammateController.GetTeammateAI().teammateName, teammateController);
        }

        return unit;
    }

    public List<UnitController> GetUnitTeamTypeList(UnitController.EUnitTeamType unitTeamType)
    {
        return unitTeamTypeDict[unitTeamType];
    }
    //limitDistance까지만 탐색
    public UnitController GetNearestEnemyUnit(UnitController unitController, float limitDistance = float.MaxValue)
    {
        List<UnitController> enemyList = GetUnitTeamTypeList(unitController.GetOppositeTeamType()).Where(e => e.IsDead() == false).ToList();
        UnitController nearestEnemyUnit = null;
        float minSqrDistance = float.MaxValue;
        Vector3 myPosition = unitController.transform.position;
        
        foreach (var enemy in enemyList)
        {
            float sqrDistance = (enemy.transform.position - myPosition).sqrMagnitude;
            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                nearestEnemyUnit = enemy;
            }
        }

        if (nearestEnemyUnit != null && Vector3.Distance(myPosition, nearestEnemyUnit.transform.position) > limitDistance)
        {
            nearestEnemyUnit = null;
        }
        
        return nearestEnemyUnit;
    }

    public IEnumerable<UnitController> GetAliveEnemies(UnitController me) =>
    GetUnitTeamTypeList(me.GetOppositeTeamType()).Where(e => !e.IsDead());

    public IEnumerable<UnitController> GetVisibleEnemies(
        UnitController shooter,
        float maxDistance,
        float viewAngle,
        float eyeHeight,
        LayerMask excludeMask)
    {
        Vector3 eyePos = shooter.transform.position + Vector3.up * eyeHeight;

        return GetAliveEnemies(shooter)
            .Where(e =>
            {
                Vector3 dir = e.transform.position - eyePos;
                float distSqr = dir.sqrMagnitude;
                if (distSqr > maxDistance * maxDistance) return false;
                if (Vector3.Angle(shooter.transform.forward, dir) > viewAngle * 0.5f) return false;

                if (Physics.Raycast(eyePos, dir.normalized, out var hit, Mathf.Sqrt(distSqr), ~excludeMask))
                {
                    var hitCtrl = hit.collider.GetComponentInParent<UnitController>();
                    if (hitCtrl != e) return false;
                }

                return true;
            });
    }

    public PlayerController GetPlayerUnit()
    {
        return playerUnit;
    }
    
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitManager : MonoBehaviour
{
    [SerializeField]
    private List<UnitController> UnitList = new List<UnitController>();
    //[SerializeField]
    public PlayerController PlayerUnit;

    public Dictionary<string, UnitController> UnitDict = new Dictionary<string, UnitController>();
    private void Start()
    {
        UnitDict = UnitList.ToDictionary(e => e.GetTeammateAI().teammateName, e => e);
    }
}

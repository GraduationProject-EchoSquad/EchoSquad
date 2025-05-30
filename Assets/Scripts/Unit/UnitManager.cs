using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField]
    private List<TeammateAI> AIUnitList = new List<TeammateAI>();

    public Dictionary<string, TeammateAI> AIUnitDict = new Dictionary<string, TeammateAI>();
    private void Start()
    {
        AIUnitDict = AIUnitList.ToDictionary(e => e.teammateName, e => e);
    }
}

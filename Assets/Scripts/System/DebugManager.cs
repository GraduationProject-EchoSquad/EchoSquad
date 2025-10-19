using System.Linq;
using LLMUnitySamples;
using UnityEngine;

public class DebugManager : Singleton<DebugManager>
{
    [SerializeField] private ParsedCommand Command;

    public void DoCommand()
    {
        FunctionCalling.Instance.DoCommand(Command, UnitManager.Instance.teammateUnitDict.Values
            .Select(tc => tc.GetComponent<TeammateAI>())
            .Where(ai => ai != null)
            .ToList());
    }

    public void KillPlayer()
    {
        PlayerController player = UnitManager.Instance.GetPlayerUnit();
        if (player != null && !player.IsDead())
        {
            Debug.Log("[Debug] Forcing player death.");
            player.ForceDead();
        }
    }

    public void KillPlayerImmediately()
    {
        PlayerController player = UnitManager.Instance.GetPlayerUnit();
        if (player != null && !player.IsDead())
        {
            Debug.Log("[Debug] Forcing player death Immediately.");
            player.ForceDeadImmediately();
        }
    }

    public void SkipToNextWave()
    {
        WaveManager.Instance.enemiesRemaining = 0;
        
        foreach (var unitController in UnitManager.Instance.GetAliveEnemies(UnitManager.Instance.GetPlayerUnit()))
        {
            unitController.HandleDeath();
        }

        /*if (GameManager.Instance.CurrentGameState == GameManager.GameState.Wave)
        {
            Debug.Log("[Debug] Skipping to the next wave.");
            WaveManager.Instance.ForceEndWave(true);
        }*/
    }
}
using UnityEngine;

public class DebugManager : Singleton<DebugManager>
{
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
        if (GameManager.Instance.CurrentGameState == GameManager.GameState.Wave)
        {
            Debug.Log("[Debug] Skipping to the next wave.");
            WaveManager.Instance.ForceEndWave(true);
        }
    }
}

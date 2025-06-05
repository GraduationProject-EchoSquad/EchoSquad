using UnityEngine;

public class EffectManager : Singleton<EffectManager>
{
    public enum EffectType
    {
        Common,
        Flesh
    }
    
    public ParticleSystem commonHitEffectPrefab;
    public ParticleSystem fleshHitEffectPrefab;
    
    public void PlayHitEffect(Vector3 pos, Vector3 normal, Transform parent = null, EffectType effectType = EffectType.Common)
    {
        var targetPrefab = commonHitEffectPrefab;

        if (effectType == EffectType.Flesh)
        {
            targetPrefab = fleshHitEffectPrefab;
        }

        var effect = Instantiate(targetPrefab, pos, Quaternion.LookRotation(normal));

        if (parent != null) effect.transform.SetParent(parent);
        
        effect.Play();
    }
}
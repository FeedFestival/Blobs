using System.Collections.Generic;
using UnityEngine;

public class EffectsPool : MonoBehaviour
{
    private static EffectsPool _effectsPool;
    public static EffectsPool _ { get { return _effectsPool; } }
    public Dictionary<ParticleType, List<ParticleController>> ParticleControllers;

    void Awake()
    {
        _effectsPool = this;
    }

    public virtual void GenerateParticleControllers()
    {

    }

    public ParticleController GetParticle(ParticleType particleType)
    {
        int index = ParticleControllers[particleType].FindIndex(pc => pc.AvailableInPool == true);
        ParticleControllers[particleType][index].AvailableInPool = false;
        return ParticleControllers[particleType][index];
    }
}

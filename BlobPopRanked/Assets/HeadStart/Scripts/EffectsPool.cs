using System.Collections.Generic;
using UnityEngine;

public enum ParticleType
{
    SmallHit, BigExplosion
}

public class EffectsPool : MonoBehaviour
{
    private static EffectsPool _effectsPool;
    public static EffectsPool _ { get { return _effectsPool; } }
    public Dictionary<ParticleType, List<ParticleController>> ParticleControllers;
    public int SmallHitParticleCount;
    public int BigExplosionParticleCount;

    void Awake()
    {
        _effectsPool = this;
    }

    public void GenerateParticleControllers()
    {
        ParticleControllers = new Dictionary<ParticleType, List<ParticleController>>();

        ParticleControllers.Add(ParticleType.SmallHit, createSmallHitParticle());
        ParticleControllers.Add(ParticleType.BigExplosion, createExplosionParticle());
    }

    public ParticleController GetParticle(ParticleType particleType)
    {
        int index = ParticleControllers[particleType].FindIndex(pc => pc.AvailableInPool == true);
        ParticleControllers[particleType][index].AvailableInPool = false;
        return ParticleControllers[particleType][index];
    }

    private List<ParticleController> createSmallHitParticle()
    {
        var particleControllers = new List<ParticleController>();
        for (int i = 0; i < SmallHitParticleCount; i++)
        {
            var go = HiddenSettings._.GetAnInstantiated(PrefabBank._.SmallHitParticle);
            ParticleController pc = go.GetComponent<ParticleController>();
            pc.Init();
            pc.transform.SetParent(transform);
            pc.SetAutoplay(on: false);
            particleControllers.Add(pc);
        }
        return particleControllers;
    }

    private List<ParticleController> createExplosionParticle()
    {
        var particleControllers = new List<ParticleController>();
        for (int i = 0; i < BigExplosionParticleCount; i++)
        {
            var go = HiddenSettings._.GetAnInstantiated(PrefabBank._.BigExplosionParticle);
            ParticleController pc = go.GetComponent<ParticleController>();
            pc.Init();
            pc.transform.SetParent(transform);
            pc.SetAutoplay(on: false);
            particleControllers.Add(pc);
        }
        return particleControllers;
    }
}

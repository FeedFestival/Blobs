using System;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleType
{
    SmallHit, BigExplosion
}

namespace Assets.BlobPopClassic
{
    public class BlobPopEffects : EffectsPool
    {
        public int SmallHitParticleCount;
        public int BigExplosionParticleCount;
        [Serializable]
        public struct ParticleDictionary
        {
            public ParticleType Type;
            public GameObject Prefab;
        }
        [SerializeField]
        private ParticleDictionary[] _particleDefinitions;
        private Dictionary<ParticleType, GameObject> _particles;

        public void Init()
        {
            _particles = new Dictionary<ParticleType, GameObject>();
            foreach (var pd in _particleDefinitions)
            {
                _particles.Add(pd.Type, pd.Prefab);
            }
        }

        public override void GenerateParticleControllers()
        {
            ParticleControllers = new Dictionary<ParticleType, List<ParticleController>>();

            ParticleControllers.Add(ParticleType.SmallHit, createSmallHitParticle());
            ParticleControllers.Add(ParticleType.BigExplosion, createExplosionParticle());
        }

        private List<ParticleController> createSmallHitParticle()
        {
            var particleControllers = new List<ParticleController>();
            for (int i = 0; i < SmallHitParticleCount; i++)
            {
                var go = Instantiate(_particles[ParticleType.SmallHit], new Vector3(0, 0, 0), Quaternion.identity);
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
                var go = Instantiate(_particles[ParticleType.BigExplosion], new Vector3(0, 0, 0), Quaternion.identity);
                ParticleController pc = go.GetComponent<ParticleController>();
                pc.Init();
                pc.transform.SetParent(transform);
                pc.SetAutoplay(on: false);
                particleControllers.Add(pc);
            }
            return particleControllers;
        }
    }
}

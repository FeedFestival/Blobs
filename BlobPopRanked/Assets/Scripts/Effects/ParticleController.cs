using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.utils;
using static UnityEngine.ParticleSystem;

public class ParticleController : MonoBehaviour
{
    [Range(0.0f, 200.0f)]
    public float Size = 100.0f;
    public bool Set3DSize;
    public List<ParticleSystem> ParticleSystems;
    public List<float> OriginalSizes;
    public List<Vector3> Original3DSizes;

    void Start()
    {
        ChangeSize(init: true);
    }

    public void ChangeSize(bool init = false)
    {
        if (init)
        {
            OriginalSizes = new List<float>();
            Original3DSizes = new List<Vector3>();
        }
        foreach (ParticleSystem ps in ParticleSystems)
        {
            if (ps.main.startSize3D)
            {
                if (Set3DSize == false)
                {
                    continue;
                }

                if (ps.main.startSize.mode == ParticleSystemCurveMode.TwoConstants)
                {
                    if (init)
                    {
                        Original3DSizes.Add(new Vector3(ps.main.startSize.constantMax, ps.main.startSize.constantMax, ps.main.startSize.constantMax));
                        Original3DSizes.Add(new Vector3(ps.main.startSize.constantMin, ps.main.startSize.constantMin, ps.main.startSize.constantMin));
                    }

                    MainModule module = ps.main;
                    float newSizeMax = percent.Find(Size, module.startSize.constantMax);
                    float newSizeMin = percent.Find(Size, module.startSize.constantMin);
                    module.startSizeX = new MinMaxCurve(newSizeMax, newSizeMin);
                    module.startSizeY = new MinMaxCurve(newSizeMax, newSizeMin);
                    module.startSizeZ = new MinMaxCurve(newSizeMax, newSizeMin);
                }
            }
            else
            {
                if (init)
                {
                    OriginalSizes.Add(ps.main.startSize.constant);
                }

                ParticleSystem.MainModule module = ps.main;
                float newSize = percent.Find(Size, module.startSize.constant);
                module.startSize = newSize;
            }
        }
    }
}

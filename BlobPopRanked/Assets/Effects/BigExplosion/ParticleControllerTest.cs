using System.Collections.Generic;
using UnityEngine;

public class ParticleControllerTest : MonoBehaviour
{
    public List<Blob> TestBlobs;
    public int CurrentBlob = 0;
    public ParticleController particleController;

    void Start()
    {
        EffectsPool._.GenerateParticleControllers();

        int i = 0;

        foreach (var blob in TestBlobs)
        {
            (blob as IPoolObject).Show();
            blob.Init();
            blob.BlobReveries.SetColor((BlobColor)i);
            i++;
            if (i == 8) {
                i = 0;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            TestBlobs[CurrentBlob].Kill();
            CurrentBlob++;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            var pcBig = (particleController as PC_BigExplosion);
            pcBig.gameObject.SetActive(true);
            pcBig.SetColor(0);
            pcBig.Play();
        }
    }
}

using UnityEngine;
using System.Collections;
using static Timer;
using System.Collections.Generic;
using Assets.Scripts.utils;

public class Timer : MonoBehaviour
{
#pragma warning disable 0414 //
    public static readonly string _version = "1.0.1";
#pragma warning restore 0414 //
    private static Timer _Timer;
    public static Timer _ { get { return _Timer; } }

    private void Awake()
    {
        _Timer = this;
        DontDestroyOnLoad(gameObject);
        _internalWaits = new Queue<WaitOption>();
    }

    public delegate void InternalWaitCallback();
    private Queue<WaitOption> _internalWaits;

    public void InternalWait(InternalWaitCallback internalWaitCallback, float? seconds = null)
    {
        var waitOption = new WaitOption(internalWaitCallback, seconds);
        waitOption.WaitFunc = InternalWaitFunction(waitOption);
        _internalWaits.Enqueue(waitOption);
        StartCoroutine(waitOption.WaitFunc);
    }

    private IEnumerator InternalWaitFunction(WaitOption waitOption)
    {
        if (waitOption.WaitOneFrame)
        {
            yield return 0;
        }
        else
        {
            yield return new WaitForSeconds(waitOption.Seconds);
        }
        waitOption.WaitCallback();
        _internalWaits.Dequeue();
        // Debug.Log(__utils.DebugQueue<WaitOption>(_internalWaits, "_internalWaits"));
    }
}

public class WaitOption
{
    public InternalWaitCallback WaitCallback { get; set; }
    public IEnumerator WaitFunc { get; set; }
    public bool WaitOneFrame { get; set; }
    public float Seconds { get; set; }

    public WaitOption(InternalWaitCallback waitCallback, float? seconds)
    {
        WaitCallback = waitCallback;
        WaitOneFrame = !seconds.HasValue;
        if (WaitOneFrame == false)
        {
            Seconds = seconds.Value;
        }
    }

    public WaitOption()
    {
    }
}

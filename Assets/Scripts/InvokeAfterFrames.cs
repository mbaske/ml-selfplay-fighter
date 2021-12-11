using System.Collections;
using System;
using UnityEngine;

/// <summary>
/// Invokes callback after specified number of frames.
/// </summary>
public class InvokeAfterFrames : CustomYieldInstruction
{
    private readonly MonoBehaviour m_Context;
    private readonly Action m_Callback;
    private readonly int m_NumberOfFrames;
    private int m_TargetFrameCount;

    /// <summary>
    /// Invokes callback after specified number of frames.
    /// </summary>
    /// <param name="context">The MonoBehaviour</param>
    /// <param name="callback">The callback method</param>
    /// <param name="numberOfFrames">Number of frames to wait</param>
    public InvokeAfterFrames(MonoBehaviour context, Action callback, int numberOfFrames = 1)
    {
        m_Context = context;
        m_Callback = callback;
        m_NumberOfFrames = numberOfFrames;
    }

    public void Invoke()
    {
        m_TargetFrameCount = Time.frameCount + m_NumberOfFrames;
        m_Context.StartCoroutine(FrameCoroutine(m_Callback));
    }

    public override bool keepWaiting => Time.frameCount < m_TargetFrameCount;

    private IEnumerator FrameCoroutine(Action callback)
    {
        yield return this;
        callback.Invoke();
    }
}
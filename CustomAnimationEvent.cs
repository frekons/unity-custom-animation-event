using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class CustomAnimationEvent : StateMachineBehaviour
{
    public List<CustomAnimationEventInfo> CustomAnimationEvents = new List<CustomAnimationEventInfo>();

    //

    private List<IEnumerator> IEnumeratorsToStop = new List<IEnumerator>(); // these will be stopped in OnStateExit

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var monoBehaviour = animator.GetComponent<MonoBehaviour>();

        foreach (var customAnimationEvent in CustomAnimationEvents)
        {
            var enumerator = AnimationEventEnumerator(animator, customAnimationEvent, stateInfo);

            if (customAnimationEvent.DontCallIfAnimationEnds)
            {
                IEnumeratorsToStop.Add(enumerator);
            }

            monoBehaviour.StartCoroutine(enumerator);
        }
    }

    private IEnumerator AnimationEventEnumerator(Animator animator, CustomAnimationEventInfo customAnimationEvent, AnimatorStateInfo stateInfo)
    {
        yield return new WaitForSeconds(customAnimationEvent.GetWaitTime(stateInfo));

        customAnimationEvent.Callback.Invoke(animator);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var monoBehaviour = animator.GetComponent<MonoBehaviour>();

        foreach (var enumerator in IEnumeratorsToStop)
        {
            monoBehaviour.StopCoroutine(enumerator);
        }
    }
}


[Serializable]
public class CustomAnimationEventInfo
{
    public bool DontCallIfAnimationEnds = true;

    public bool IsFixedTime = true;

    public float FixedTime;

    [Range(0.0f, 1.0f)]
    public float ProgressPercent; // you may use CustomInspector to avoid 4 bytes more allocating, (you could use FixedTime member to calculate same thing according to IsFixedTime)

    public float GetWaitTime(AnimatorStateInfo stateInfo) // as second
    {
        if (IsFixedTime)
        {
            return FixedTime;
        }
        else // as progress percent
        {
            return ProgressPercent * stateInfo.length; // you may need to use AnimatorStateInfo.speedMultiplier, AnimatorStateInfo.speed, not sure
        }
    }

    public UnityEventAnimator Callback;
}

[Serializable]
public class UnityEventAnimator : UnityEvent<Animator>
{

}

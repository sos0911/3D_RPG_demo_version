using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class TwinnerExample : MonoBehaviour
    // 각 애니메이션 간 보간
{
    public string AnimString = string.Empty;
    public float AnimFloat = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        // hotween 초기화
        HOTween.Init(true, true, true);

        HOTween.To(this, 2.0f, new TweenParms()
            .Prop("AnimString", "hello world! first experience in hotween")
            .Loops(-1, LoopType.Yoyo));

        HOTween.To(this, 2.0f, new TweenParms()
            // 2초마다 10씩 증가함(continue)
            .Prop("AnimFloat", 10.0f)
            .Loops(-1, LoopType.Restart));

        //HOTween.To(transform, 4, "position", new Vector3(-3, 6, 0));
        /*
        HOTween.To(transform, 3, new TweenParms()
            .Prop("position", new Vector3(0, 4, 0), true)
            .Prop("rotation", new Vector3(0, 720, 0), true)
            .Loops(-1, LoopType.Yoyo)
            .OnStepComplete(OnTweenCompleted));
            */
        Sequence sequence = new Sequence(new SequenceParms().Loops(-1, LoopType.YoyoInverse));

        Tweener tweener1 = HOTween.To(transform, 1, new TweenParms().Prop("position", new Vector3(0, 4, 0), true));
        Tweener tweener2 = HOTween.To(transform, 1, new TweenParms().Prop("rotation", new Vector3(0, 720, 0), true));
        Tweener tweener3 = HOTween.To(transform, 1, new TweenParms().Prop("position", new Vector3(4, 4, 0), true));
        Tweener tweener4 = HOTween.To(transform, 1, new TweenParms().Prop("localscale", new Vector3(1, 2, 1), true));
        sequence.Append(tweener1);
        sequence.Append(tweener2);
        sequence.Append(tweener3);
        sequence.Append(tweener4);

        Color colorTo = GetComponent<MeshRenderer>().material.color;
        colorTo.a = 0.0f;

        Tweener tweener5 = HOTween.To(GetComponent<MeshRenderer>().material,
            sequence.duration * 0.5f, new TweenParms().Prop("color", colorTo));
        sequence.Insert(sequence.duration * 0.5f, tweener5);

        sequence.Play();
    }

    void OnTweenCompleted()
    {
        Debug.Log("Tweening completed");

    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 500, 100));
        GUILayout.Label("anim string : " + AnimString);
        GUILayout.Label("anim float : " + AnimFloat);
        GUILayout.EndArea();
    }

}

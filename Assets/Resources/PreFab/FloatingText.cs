using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    public Animator animator;
    private Text damagetxt;

    void OnEnable()
    {
        AnimatorClipInfo[] clipnfo = animator.GetCurrentAnimatorClipInfo(0);
        Destroy(gameObject, clipnfo[0].clip.length);
        damagetxt = animator.GetComponent<Text>();
    }

    public void SetText(string txt)
    {
        damagetxt.text = txt;
    }
}

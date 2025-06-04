using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetBool(string name, bool value)
    {
        if (animator != null)
        {
            animator.SetBool(name, value);
        }
    }

    public void SetTrigger(string name)
    {
        if (animator != null)
        {
            animator.SetTrigger(name);
        }
    }

    public void SetFloat(string name, float value)
    {
        if (animator != null)
        {
            animator.SetFloat(name, value);
        }
    }
}

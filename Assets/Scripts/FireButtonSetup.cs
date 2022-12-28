using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

using UnityEngine.EventSystems;

public class FireButtonSetup : MonoBehaviourPunCallbacks, IPointerDownHandler, IPointerUpHandler
{
    public Animator animator;
    public bool isPressed;
    public void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        animator.SetBool("clicked", true);

       

    }
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        animator.SetBool("clicked", false);
  

    }

}

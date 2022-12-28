using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerMovementController : MonoBehaviourPunCallbacks
{
    public Joystick floatingJoystick;
    public FixedTouchField ftf;

    private Animator animator;
    public AudioClip walkAudio;
    public AudioClip runAudio;

    public RigidbodyFirstPersonController FirstPersonController;
 

    private float audioRate;

    void Start()
    {
        FirstPersonController = GetComponent<RigidbodyFirstPersonController>();
        animator = GetComponent<Animator>();
    }
     void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
          
                player.GetComponent<PhotonView>().RPC("MovementSounds", RpcTarget.AllBuffered);
            
        }
    }   
    void FixedUpdate()
    {
        FirstPersonController.joystickControllerVector.x = floatingJoystick.Horizontal;
        FirstPersonController.joystickControllerVector.y = floatingJoystick.Vertical;
        FirstPersonController.mouseLook.fixedTouchFieldVector = ftf.TouchDist;

        animator.SetFloat("Horizontal", floatingJoystick.Horizontal);
        animator.SetFloat("Vertical", floatingJoystick.Vertical);

        if (Mathf.Abs(floatingJoystick.Horizontal) > 0.9f || Mathf.Abs(floatingJoystick.Vertical) > 0.9f)
        {
            FirstPersonController.movementSettings.ForwardSpeed = 9;
            animator.SetBool("IsRunning", true);
        }
        else if (Mathf.Abs(floatingJoystick.Horizontal) < 0.4 || Mathf.Abs(floatingJoystick.Vertical) < 0.4f)
        {
            FirstPersonController.movementSettings.ForwardSpeed = 8f;
            animator.SetBool("IsRunning", false);
        }
        else
        {
            FirstPersonController.movementSettings.ForwardSpeed = 8f;
            animator.SetBool("IsRunning", false);
        }
    }
    
    
    [PunRPC]
    public void MovementSounds()
    {
        if (Mathf.Abs(floatingJoystick.Horizontal) > 0.8f || Mathf.Abs(floatingJoystick.Vertical) > 0.8f)
        {  if (Time.time - audioRate > 0.3)
            {
                audioRate = Time.time;
                AudioSource.PlayClipAtPoint(runAudio, FirstPersonController.cam.transform.position);
            }
        }
        else if (Mathf.Abs(floatingJoystick.Horizontal) < 0.3 && Mathf.Abs(floatingJoystick.Horizontal) > 0.0 || Mathf.Abs(floatingJoystick.Vertical) < 0.3f && Mathf.Abs(floatingJoystick.Vertical) > 0.0f)
        {
            if (Time.time - audioRate > 0.3)
            {
                audioRate = Time.time;
                AudioSource.PlayClipAtPoint(walkAudio, FirstPersonController.cam.transform.position);
            }
        }
      
    }


    

 
}

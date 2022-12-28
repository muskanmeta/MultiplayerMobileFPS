using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;
using Photon.Realtime;
using UnityEngine.EventSystems;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab (Local or Remote)")]
    public GameObject[] LocalPlayerPrefab;
    public GameObject[] RemotePlayerPrefab;

    public GameObject PlayerUIPrefab;

    public Shooting shooting;
    public AudioClip shootSound;
    public AudioClip jumpSound;

    public TextMeshProUGUI playerNameText;

    public PlayerMovementController playerMovementController;
    public Camera FPScam;
    private Animator animator;

    public float fireRate;
    public bool shoot;
    private float audioRate;
    private float jumpRate;
    public bool jumped;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovementController = GetComponent<PlayerMovementController>();
        shooting = GetComponent<Shooting>();
        if (photonView.IsMine)
        {
            FPScam.enabled = true;

            foreach (GameObject gameObject in LocalPlayerPrefab)
            {
                gameObject.SetActive(true);
            }
            foreach (GameObject gameObject in RemotePlayerPrefab)
            {
                gameObject.SetActive(false);
            }

            animator.SetBool("isLocal", true);

            //instantiate playerUI
            GameObject playerUIGameObject = Instantiate(PlayerUIPrefab);
            playerMovementController.floatingJoystick = playerUIGameObject.transform.Find("MainPanel").transform.Find("FloatingJoystick").GetComponent<Joystick>();     
            playerMovementController.ftf = playerUIGameObject.transform.Find("MainPanel").transform.Find("RotationTouchFIeld").GetComponent<FixedTouchField>();


            //shooting button
            playerUIGameObject.transform.transform.Find("MainPanel").Find("FireButton").GetComponent<Button>().onClick.AddListener(() => shooting.Fire());
            //Adding eventTrigger to access pointerup/down in the PlayerPrefab Script
            EventTrigger eventTrigger = playerUIGameObject.transform.Find("MainPanel").transform.Find("FireButton").gameObject.AddComponent<EventTrigger>();
            var pointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            pointerDown.callback.AddListener((e) => shoot = true );
            eventTrigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            pointerUp.callback.AddListener((e) => shoot = false);
            eventTrigger.triggers.Add(pointerUp);

            //Jump button listener
            playerUIGameObject.transform.Find("MainPanel").Find("Jump").GetComponent<Button>().onClick.AddListener(() => Jump());

        }
        else
        {
            foreach (GameObject gameObject in LocalPlayerPrefab)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject gameObject in RemotePlayerPrefab)
            {
                gameObject.SetActive(true);
            }

            animator.SetBool("isLocal", false);

            FPScam.enabled = false;
            playerMovementController.enabled = false;
            GetComponent<RigidbodyFirstPersonController>().enabled = false;
           
        }
        playerNameText.text = GetComponent<PhotonView>().Owner.NickName;
    }

    public void Update()
    {
        if (shoot)
        {
            if (Time.time - audioRate > 1.5f)
            {
                AudioSource.PlayClipAtPoint(shootSound, transform.position);
                audioRate = Time.time;
            }

            if (Time.time - fireRate > 0.3f)
            {
                AudioSource.PlayClipAtPoint(shootSound, transform.position);
                fireRate = Time.time;
                shooting.Fire();
            }
        }
    }

  

    public void Jump()
    {

        if (photonView.IsMine)
        {
            GetComponent<RigidbodyFirstPersonController>().jumpController = true;
            if (GetComponent<RigidbodyFirstPersonController>().Grounded)
            {
                AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            }
        }
        
    }

}

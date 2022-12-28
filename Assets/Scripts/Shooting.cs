using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera FPS_Camera;
    public GameObject hitEffectPrefab;

    public AudioClip dieSound;
    public AudioClip hitSound;
    private Animator animator;

    [Header("Health Update")]
    public float maxHealth = 100f;
    private float health;
    public Image healthBar;

    public float recoverRate;
    private bool isDead;

    float rateCount;

    void Start()
    {
        isDead = false;
        health = maxHealth;
        healthBar.fillAmount = health / maxHealth;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        GameObject[] players =  GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players)
        {
            if (Time.time - recoverRate > 1f)
            {
                recoverRate = Time.time;
                player.GetComponent<PhotonView>().RPC("Recover", RpcTarget.AllBuffered, 2f);
            }
        }
    }


    public void Fire()
    {
        if (photonView.IsMine)
        {
            animator.SetBool("isAiming", GetComponent<PlayerSetup>().shoot);
        }

            Ray ray = FPS_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit _hit;
            if (Physics.Raycast(ray, out _hit, 100))
            {
         
                Debug.Log(_hit.collider.gameObject.name);
                CreateHitEffect(_hit.point);


                if (_hit.collider.gameObject.CompareTag("Player") && !_hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                {
                    _hit.collider.gameObject.GetComponent<PhotonView>().RPC("CreateHitEffect", RpcTarget.All, _hit.point);
                    _hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 10f);
                  
                }
            }
    }

    

    //other player's deducting damage should be updated to all the remote clients in the room.
    [PunRPC]
    public void TakeDamage(float damage,PhotonMessageInfo info)
    {
        health -=damage;

        if (Time.time - rateCount > 0.7f)
        {
            rateCount = Time.time;
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
        healthBar.fillAmount = health / maxHealth;
        if (health <= 0f)
        {
            isDead = true;
            Dead();
            health = 0;
           if(photonView.IsMine)
                AudioSource.PlayClipAtPoint(dieSound, transform.position);

            StartCoroutine(VanishText(info.Sender.NickName,info.photonView.Owner.NickName));
        }
     
    }
    [PunRPC]
    IEnumerator VanishText(string sender, string reciever)
    {
        GameObject status = GameObject.Find("LiveStatus");
        float vanishTime = 3.0f;
        while (vanishTime > 0.0f)
        {
            yield return new WaitForSeconds(1.0f);
            vanishTime -= 1.0f;
        status.GetComponent<Text>().text = sender + " killed " + reciever;

        }
        status.GetComponent<Text>().text = "";
    }


    [PunRPC]
    public void Recover(float addHealth)
    {
        if (health < maxHealth && !isDead)
        {
                health += addHealth;
                healthBar.fillAmount = health / maxHealth;
        }
    }

    //gun effects should be syncedand updated to all the remote clients in the room.(Through Pun RPC)
    [PunRPC]
    public void CreateHitEffect(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.5f);
    }

    void Dead()
    {
        if (photonView.IsMine)
        {
            animator.SetBool("isDead", true);
          
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        GameObject respawnText = GameObject.Find("RespawnText");
        float respawnTime = 5.0f;
        while(respawnTime > 0.0f)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime -= 1.0f;

            transform.GetComponent<PlayerMovementController>().enabled = false;
            transform.GetComponent<Shooting>().enabled = false;
            Debug.Log("You're Down. Respawning in: " + respawnTime.ToString(".00"));
            respawnText.GetComponent<Text>().text = "You're Down. Respawning in: " + respawnTime.ToString(".00");
        }

        animator.SetBool("isDead", false);
        isDead = false;
        respawnText.GetComponent<Text>().text ="";

        transform.GetComponent<PlayerMovementController>().enabled = true;
        transform.GetComponent<Shooting>().enabled = true;
        float spawnPoint = Random.Range(-25, -50);
        transform.position = new Vector3(spawnPoint, 0, spawnPoint);
        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);

    }

    [PunRPC]
    public void RegainHealth()
    {
        health = maxHealth;
        healthBar.fillAmount = health / maxHealth;
    }

  

  


}

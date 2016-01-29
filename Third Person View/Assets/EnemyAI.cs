using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{

    private Animator animator;
    private GameObject ak;
    private GameObject player;
    public float fieldOfViewAngle = 110f;
    public bool playerInSight;
    public Vector3 personalLastSighting;
    private CapsuleCollider col;
    private AnimatorStateInfo state;
    private ThirdPersonController person;
    private List<Transform> shootables;
    private Transform currentDestination;
    private GameObject activeBullet;

    public int currentHealth = 100;
    public int startingHealth = 100;

    private int shotDamage = 2;
    private LineRenderer render;
    private bool shooting;
    private bool bulletTravelling;
    private bool stillInSight;
    private bool isDead;
    public LayerMask mask;
    // Use this for initialization
    void Start()
    {
        shooting = false;
        player = GameObject.FindGameObjectWithTag("Player");
        person = player.GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        activeBullet = GameObject.Find("bullet");
        activeBullet.SetActive(false);
        ak = GameObject.Find("Ak-47Enemy");
        render = GetComponentInChildren<LineRenderer>();
        //CollectShootableBodyParts(player.transform);
        //go.transform.position = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        //state = animator.GetCurrentAnimatorStateInfo(0);
        //if (state.IsName("Shooting"))
        //{
        //    if (state.normalizedTime > 0.9f)
        //    {
        //        playerInSight = false;
        //        shooting = false;
        //        render.enabled = false;
        //        animator.SetBool("PlayerInSight", playerInSight);
        //    }
        //}
        if (shooting)
        {
            PointAtPlayer();
        }
        if (isDead && state.IsName("Deactivated"))
        {
            Debug.Log("Destroyed");
            Destroy(gameObject);
            Destroy(this);
            GetComponent<CapsuleCollider>().enabled = false;
            activeBullet = null;
            bulletTravelling = false;
            gameObject.SetActive(false);

        }
        if (!isDead && bulletTravelling && activeBullet != null)
        {
            activeBullet.transform.position = Vector3.Lerp(activeBullet.transform.position, currentDestination.position, Time.deltaTime * 10);
            float distance = Vector3.Distance(activeBullet.transform.position, currentDestination.position + currentDestination.forward);
            if (distance <= 1)
            {
                bulletTravelling = false;
                activeBullet.gameObject.SetActive(false);
                activeBullet.transform.position = transform.position;
                currentDestination.SendMessage("OnBulletEnter", SendMessageOptions.DontRequireReceiver);
                if (stillInSight)
                {
                    Debug.Log("Shootingg");
                    Shoot();
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            isDead = true;
            animator.SetBool("IsDead", isDead);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == player && !isDead)
        {
            Vector3 direction = other.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.forward);
            if (angle < fieldOfViewAngle * 0.5f)
            {
                stillInSight = false;
                RaycastHit hit;
                if (Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, col.radius))
                {
                    if ((hit.collider.gameObject == player || hit.collider.gameObject.transform.root == player.transform) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
                    {
                        bool animatorState = animator.GetBool("PlayerInSight");
                        playerInSight = false;
                        //Debug.Log(animatorState);
                        if (!animatorState)
                        {
                            playerInSight = animatorState;
                        }
                        if (!playerInSight)
                        {
                            Shoot();
                            person.CheckBodyPartHit(other, transform.position + transform.up, direction.normalized, col.radius);
                            playerInSight = true;
                            //Debug.Log("Shot");
                            animator.SetBool("PlayerInSight", playerInSight);

                        }
                        stillInSight = true;
                    }
                }
            }
        }
        animator.SetBool("StillInSight", stillInSight);
        if (!stillInSight)
        {
            shooting = false;
            render.enabled = false;

        }
        else
        {
            PointAtPlayer();
        }
    }

    void Shoot()
    {
        if (person.currentHealth > 0)
        {
            bool isBack = ChestOrBack();
            bulletTravelling = true;
            activeBullet.SetActive(true);
            person.TakeDamage(isBack, shotDamage, true);
            currentDestination = player.transform;
            //Debug.Log(currentDestination);
        }
        else
        {
            Debug.Log("YOU ARE DEAD");
        }
        shooting = true;
        render.enabled = true;
    }

    bool ChestOrBack()
    {
        float XRotationPlayer = person.transform.eulerAngles.y;
        float XRotationEnemy = transform.eulerAngles.y;
        Debug.Log(XRotationPlayer);
        float maxRotation = XRotationEnemy + 90;
        float minRotation = XRotationEnemy - 90;
        if (maxRotation > 359)
        {
            maxRotation -= 360;
            if (XRotationPlayer <= maxRotation || (XRotationPlayer <= 360 && XRotationPlayer >= minRotation))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (XRotationPlayer <= maxRotation && XRotationPlayer >= minRotation)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    void PointAtPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;

        //create the rotation we need to be in to look at the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        //rotate us over time according to speed until we are in the required rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2);
    }
}

using UnityEngine;
using System.Collections;

public class CollisionMessage : MonoBehaviour 
{	
	public Transform root;
    public bool sleeping;

    void Start()
    {
       root = transform.root;
    }

	void OnCollisionEnter(Collision other)
	{
        if (other != null )
		{           
            if(root)
            root.SendMessage("OnRagdollCollisionEnter", new RagdollCollision(gameObject, other));           
		}
	}

    void OnBulletEnter()
    {
        root.SendMessage("OnBulletCollisionEnter",this.gameObject);
    }

    void Sleep()
    {
        sleeping = false;
    }
}

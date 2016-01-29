using UnityEngine;
using System.Collections;

public class ObjectDamage : MonoBehaviour 
{
	public int damage;

	void OnCollisionEnter(Collision hit)
	{
		if(hit.collider.CompareTag("Player"))
		{
            bool FromLeft = false;
            if (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 359)
            {
                FromLeft = true;
            }
            object[] tempStorage = new object[3];
            tempStorage[0] = FromLeft;
            tempStorage[1] = damage;
            tempStorage[2] = false;
			// apply damage to PlayerHealth
			hit.transform.root.SendMessage ("UnWrap", tempStorage, SendMessageOptions.DontRequireReceiver);
			// activate the Ragdoll 
			hit.transform.root.SendMessage ("ActivateRagdoll", SendMessageOptions.DontRequireReceiver);
		}
	}
}
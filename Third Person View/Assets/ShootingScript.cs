using UnityEngine;
using System.Collections;

public class ShootingScript : MonoBehaviour {

    LineRenderer render;
	void Start () {
        render = GetComponent<LineRenderer>();
        transform.Translate(0, 0.11f, 0, Space.Self);
    }
	
	// Update is called once per frame
	void Update () {
        render.SetPosition(0, transform.position);
        render.SetPosition(1, transform.position + transform.forward*20);
	}
}

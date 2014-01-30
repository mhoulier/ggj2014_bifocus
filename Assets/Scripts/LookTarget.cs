using UnityEngine;
using System.Collections;

public class LookTarget : MonoBehaviour {

	public string targetTag;

	// Use this for initialization
	void Start()
	{
	
	}
	
	// Update is called once per frame
	void Update()
	{
		Transform o = GameObject.FindWithTag(targetTag).transform;
		transform.LookAt(o);
	}
}

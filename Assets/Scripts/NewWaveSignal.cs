using UnityEngine;
using System.Collections;

public class NewWaveSignal : MonoBehaviour {

	[SerializeField]
	private string m_newSignalTrigger = null;

	private Animator m_animator;
	// Use this for initialization
	void Start () {
	
		m_animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnNewWave()
	{
		// play the audio
		if (audio != null)
			audio.Play();

		// start the animation
		if (m_animator != null && m_newSignalTrigger != null)
			m_animator.SetTrigger(m_newSignalTrigger);
	}
}

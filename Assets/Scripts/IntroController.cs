using UnityEngine;
using System.Collections;




public class IntroController : MonoBehaviour {

	public AudioSource musicSound;
	public AudioSource helicoSound;
	public AudioSource pilotSound;
	public float audioTimerResolution = 5;

	private FlightIntro m_SceneManager = null;

	// Use this for initialization
	void Start ()
	{
		m_SceneManager = GetComponent<FlightIntro>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SwitchToMenu()
	{
		/*
		if (musicObject != null)
		{
			for (int i = 9; i > 0; i--)	
			{
				musicObject.volume = i * 0.1f;
				yield return new WaitForSeconds(0.5f);
			} 
		}//*/

		if (m_SceneManager != null)
		{
			m_SceneManager.RequestLevelTransition();
		}
		else
		{
			Application.LoadLevel("intro");
		}
	}

	public void PlayHelico()
	{
		if (helicoSound != null)
		{
			helicoSound.Play();
		}
	}

	public void PlayPilot()
	{
		if (pilotSound != null)
		{
			pilotSound.Play();
		}
	}

	public void FadeOutSounds()
	{
		if (pilotSound != null)
		{
			StartCoroutine(FadeOutAudio(pilotSound));
		}
		if (helicoSound != null)
		{
			StartCoroutine(FadeOutAudio(helicoSound));
		}
		if (musicSound != null)
		{
			StartCoroutine(FadeOutAudio(musicSound));
		}
	}

	
	IEnumerator FadeOutAudio (AudioSource audio) {
		
		float start = 1.0F;
		float end = 0.0F;
		float i = 0.0F;
		float step = 1.0F/audioTimerResolution;
		while (i <= 1.0F) 
		{
			//Debug.Log("fade out" + i);
			i += step * Time.deltaTime;
			audio.volume = Mathf.Lerp(start, end, i);
			yield return new WaitForSeconds(step * Time.deltaTime);
		}
	}

}

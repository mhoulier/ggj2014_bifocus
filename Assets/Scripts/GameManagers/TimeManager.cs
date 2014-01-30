using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
	private float m_FixedDeltaTimeRatio = 0.0f;
	private bool m_TimeScaleSetInFrame = false;
	
	void Start()
	{
		m_FixedDeltaTimeRatio = Time.fixedDeltaTime / Time.timeScale;
	}
	
	void Update()
	{
		//float deltaTime = Time.deltaTime;
		//Debug.Log( string.Format("DeltaTime: {0}", deltaTime) );
	}
	
	public void SetTimeScale(float _TimeScale)
	{
		bool timeScaleSetInFrame = m_TimeScaleSetInFrame;
		System.Diagnostics.Debug.Assert(timeScaleSetInFrame == false);
		m_TimeScaleSetInFrame = true;
		
		StartCoroutine( SetTimeScaleAtEndOfFrame(_TimeScale) );
	}
	
	private IEnumerator SetTimeScaleAtEndOfFrame(float _TimeScale)
	{
		yield return new WaitForEndOfFrame();
		
		SetTimeScaleInternal(_TimeScale);
	}
	
	private void SetTimeScaleInternal(float _TimeScale)
	{
		//Debug.Log( string.Format("Setting timeScale to {0}", _TimeScale) );
		
		Time.timeScale = _TimeScale;
		Time.fixedDeltaTime = m_FixedDeltaTimeRatio * _TimeScale;
		
		m_TimeScaleSetInFrame = false;
	}
}

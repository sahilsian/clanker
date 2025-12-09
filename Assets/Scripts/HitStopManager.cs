using UnityEngine;
using System;
using System.Collections;

public class HitStopManager : MonoBehaviour
{
    private static HitStopManager _instance;
    public static HitStopManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<HitStopManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("HitStopManager");
                    _instance = go.AddComponent<HitStopManager>();
                    Debug.Log("HitStopManager: Auto-created instance.");
                }
            }
            return _instance;
        }
    }

    private bool isStopped = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Public static method that enemies can call
    public static void TriggerHitStop(float duration = 0.1f)
    {
        if (Instance != null)
        {
            Instance.HitStop(duration);
        }
    }

    public void HitStop(float duration)
    {
        if (isStopped) return;
        Debug.Log($"HitStop Activated for {duration} seconds!");
        StartCoroutine(DoHitStop(duration));
    }

    private IEnumerator DoHitStop(float duration)
    {
        isStopped = true;
        float originalTimeScale = Time.timeScale;
        
        Time.timeScale = 0.0f;
        
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
        isStopped = false;
    }
}

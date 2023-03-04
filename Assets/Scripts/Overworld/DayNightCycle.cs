using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public float dayTime;
    public float fullDayTime;
    public float timeStep;
    public Transform dayLight;
    public float targetCameraRotation;
    public float lightRotationSpeed;


    #region Singleton
    public static DayNightCycle Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

    }
    #endregion

    private void Update()
    {
        if(dayLight.eulerAngles.z < targetCameraRotation)
        {
            dayLight.eulerAngles = new Vector3(0, 0, dayLight.eulerAngles.z + lightRotationSpeed * Time.deltaTime);
        }

    }

    public void AddStep()
    {
        dayTime += timeStep;
        targetCameraRotation = (360f / fullDayTime) * dayTime;
        if (dayTime >= fullDayTime)
        {
            dayTime = 0;
            dayLight.eulerAngles = Vector3.zero;
            OverworldController.Instance.RestockTowns();
            Debug.Log("Restocking towns");
        }
    }
}

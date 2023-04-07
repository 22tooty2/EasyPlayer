using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Crown
    public GameObject MyCamera;
    public GameObject cameraController;
    private float currentTargetDistance;

    //Servants
    public bool CanLook;

    [SerializeField] [Range(1f, 9f)] private float DefaultDistance;
    [SerializeField] [Range(1f, 9f)] private float MinDistance;
    [SerializeField] [Range(1f, 9f)] private float MaxDistance;

    [SerializeField] [Range(1f, 9f)] private float Smoothing = 4f;
    [SerializeField] [Range(1f, 9f)] private float zoomSensitivity = 1f;

    private CinemachineFramingTransposer farmingTransposer;
    private CinemachineInputProvider inputProvider;
    

    InputDirector director;

    // Start is called before the first frame update
    void Start()
    {
        director = GetComponent<InputDirector>();
        farmingTransposer = cameraController.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        inputProvider = cameraController.GetComponent<CinemachineInputProvider>();

        currentTargetDistance = DefaultDistance;
    }

    // Update is called once per frame
    void Update()
    {
        //Throne Room
        if (!CanLook)
            return;

        float zoomValue = inputProvider.GetAxisValue(2) * zoomSensitivity;
        currentTargetDistance = Mathf.Clamp(currentTargetDistance + zoomValue, MinDistance, MaxDistance);

        float currentDistance = farmingTransposer.m_CameraDistance;
        if (currentDistance == currentTargetDistance)
            return;
        float LerpedZoomValue = Mathf.Clamp(currentDistance, currentTargetDistance, Smoothing * Time.deltaTime);

        farmingTransposer.m_CameraDistance = LerpedZoomValue;
    }
}

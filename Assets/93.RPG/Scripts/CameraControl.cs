using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("카메라 기본속성")]
    private Transform myTransform = null;
    public GameObject Target = null;
    private Transform targetTransform = null;

    public enum CameraViewPointState { FIRST, SECOND, THIRD }
    public CameraViewPointState CameraState = CameraViewPointState.THIRD;

    [Header("3인칭 카메라")]
    public float Distance = 5.0f; // 타겟으로부터 떨어진 거리
    public float Height = 1.5f; // 타겟 height로부터의 추가높이
    public float HeightDamping = 2.0f;
    public float RotationDamping = 3.0f;

    [Header("2인칭 카메라")]
    public float RotateSpeed = 10.0f;

    [Header("1인칭 카메라")]
    public float SensitivityX = 5.0f;
    public float SensitivityY = 5.0f;
    private float RotationX = 0.0f;
    private float RotationY = 0.0f;
    public Transform FirstCameraSocket = null;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = GetComponent<Transform>();
        if (Target != null)
        {
            targetTransform = Target.transform;
        }
    }

    /// <summary>
    /// 3인칭 카메라
    /// </summary>
    void ThirdView()
    {
        float wantedRotationAngle = targetTransform.eulerAngles.y; // 현재 캐릭터의 y축 각도값
        float wantedHeight = targetTransform.position.y + Height; // = 목표 카메라의 높이

        float currentRotationAngle = myTransform.eulerAngles.y; // 현재 카메라의 y축 각도값
        float currentHeight = myTransform.position.y; // 현재 카메라의 높이

        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, RotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, HeightDamping * Time.deltaTime);

        // euler angle을 quaternion으로 변환하는 함수
        // unity는 내부적으로 quaternion으로 사용된다.
        Quaternion currentRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);
        myTransform.position = targetTransform.position;
        // quaternion 사용
        myTransform.position -= currentRotation * Vector3.forward * Distance;
        myTransform.position = new Vector3(myTransform.position.x, currentHeight, myTransform.position.z);
        // target 위치를 바라보게끔 카메라가 변함
        myTransform.LookAt(targetTransform);
    }
    /// <summary>
    /// 모델 주위로 보게 하는 뷰(2인칭)
    /// </summary>
    void SecondView()
    {
        myTransform.RotateAround(targetTransform.position, Vector3.up, RotateSpeed * Time.deltaTime);
        myTransform.LookAt(targetTransform);
    }
    /// <summary>
    /// 1인칭 view
    /// </summary>
    void FirstView()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 각도 조절 수식연산(-각도)
        RotationX = myTransform.localEulerAngles.y + mouseX * SensitivityX;
        RotationX = (RotationX > 180.0f) ? RotationX - 360.0f : RotationX;

        RotationY += mouseY * SensitivityY;
        RotationY = (RotationY > 180.0f) ? RotationY - 360.0f : RotationY;

        myTransform.localEulerAngles = new Vector3(-RotationY, RotationX, 0f);

        // 1인칭 카메라가 몸에 붙어 따라오기 위함
        myTransform.position = FirstCameraSocket.position;
    }
    /// <summary>
    /// update 뒤에 호출되는 업데이트
    /// </summary>
    private void LateUpdate()
    {
        if (Target == null)
        {
            return;
        }
        // start함수에서 발생된 것이 아님
        if (targetTransform == null)
        {
            targetTransform = Target.transform;
        }
        switch (CameraState)
        {
            case CameraViewPointState.THIRD:
                ThirdView();
                break;
            case CameraViewPointState.SECOND:
                SecondView();
                break;
            case CameraViewPointState.FIRST:
                FirstView();
                break;
        }
    }
}

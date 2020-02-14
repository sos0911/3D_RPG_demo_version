using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterControl : MonoBehaviour
{
    [Header("property for moving")]
    [Tooltip("기본이동속도")]
    public float MoveSpeed = 2.0f; // moving speed
    [Tooltip("달리기 이동속도")]
    public float RunSpeed = 3.5f; // running speed
    public float DirectionRotatespeed = 100.0f; // 이동방향 변경 위한 속도
    public float BodyRotationSpeed = 2.0f; // 캐릭터 방향 변경 위한 속도

    [Range(0.01f, 5.0f)] // 속도가 0이 되지 않게 하기 위함
    public float VelocityChangeSpeed = 0.1f; // 속도가 변경되기 위한 속도

    private Vector3 CurrentVelocity = Vector3.zero;
    private Vector3 MoveDirection = Vector3.zero;
    private CharacterController mycharactercontroller = null;
    private CollisionFlags collisionflags = CollisionFlags.None;
    private float gravity = 9.8f;
    private float verticalSpeed = 0.0f; // 수직 속도
    private bool CannotMove = false; // 이동불가 flag

    [Header("animation properties")]
    public AnimationClip IdleAnimClip = null;
    public AnimationClip WalkAnimClip = null;
    public AnimationClip RunAnimClip = null;
    public AnimationClip Attack1AnimClip = null;
    public AnimationClip Attack2AnimClip = null;
    public AnimationClip Attack3AnimClip = null;
    public AnimationClip Attack4AnimClip = null;
    private Animation myAnimation = null;
    // skill anim clip
    public AnimationClip SkillAnimClip = null;



    // 캐릭터 상태 enum
    public enum FighterState { None, Idle, Walk, Run, Attack, Skill }
    [Header("캐릭터 상태")]
    public FighterState myState = FighterState.None;
    public enum FighterAttackState { Attack1, Attack2, Attack3, Attack4}
    public FighterAttackState AttackState = FighterAttackState.Attack1;
    // 다음 공격 활성화?
    public bool NextAttack = false;

    [Header("전투관련")]
    public TrailRenderer AttackTrailRenderer = null;
    public CapsuleCollider AttackCapsuleCollider = null;
    public GameObject SkillEffect = null;


    // Start is called before the first frame update
    void Start()
    {
        mycharactercontroller = GetComponent<CharacterController>();
        myAnimation = GetComponent<Animation>();
        myAnimation.playAutomatically = false; // 자동재생 끄기
        myAnimation.Stop();
        myState = FighterState.Idle;
        myAnimation[IdleAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[WalkAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[RunAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[Attack1AnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[Attack2AnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[Attack3AnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[Attack4AnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[SkillAnimClip.name].wrapMode = WrapMode.Once;

        AddAnimationEvent(Attack1AnimClip, "OnAttackAnimFinish");
        AddAnimationEvent(Attack2AnimClip, "OnAttackAnimFinish");
        AddAnimationEvent(Attack3AnimClip, "OnAttackAnimFinish");
        AddAnimationEvent(Attack4AnimClip, "OnAttackAnimFinish");
        AddAnimationEvent(SkillAnimClip, "OnSkillAnimFinished");

    }

    // Update is called once per frame
    void Update()
    {
        Move();
        // move()로 velocity가 변경된 다음 그걸 바탕으로 몸통회전
        BodyDirectionChange();
        // 상태에 맞추어 애니메이션 재생
        AnimationControl();
        // 캐릭터 상태 변경
        CheckState();
        // 왼쪽 버튼 클릭으로 공격상태 변경
        InputControl();
        ApplyGravity();
        // 공격관련 컴포넌트 제어
        AttackComponentControl();
    }
    /// <summary>
    /// 이동 관련 함수
    /// </summary>
    void Move()
    {
        if (CannotMove == true)
        {
            return;
        }
        // 메인카메라의 게임오브젝트의 transform component
        Transform CameraTransform = Camera.main.transform;
        // 실제 카메라가 보는 방향이 월드에서는 무슨 방향?
        Vector3 forward = CameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        // 앞뒤 방향으로 좌우 방향을 얻어냄
        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 targetDirection = horizontal * right + vertical * forward;
        // 부드럽게 이동을 하기 위한 처리
        MoveDirection = Vector3.RotateTowards(MoveDirection, targetDirection, DirectionRotatespeed * Mathf.Deg2Rad * Time.deltaTime, 1000.0f);
        // 크기는 없애고 방향만 가져옴
        MoveDirection = MoveDirection.normalized;
        float speed = MoveSpeed;
        if (myState == FighterState.Run)
        {
            speed = RunSpeed;
        }
        Vector3 gravityVector = new Vector3(0.0f, verticalSpeed, 0.0f);
        Vector3 moveAmount = (MoveDirection * speed * Time.deltaTime) + gravityVector;
        // 충돌하는 것을 인지하기 위함
        collisionflags = mycharactercontroller.Move(moveAmount);

    }
    private void OnGUI()
    {
        // 충돌
        GUILayout.Label("충돌  :" + collisionflags.ToString());
        // 별개로 계속 호출됨
        // start()가 호출이 안됬는데 호출되던가 이동속도가 0이면 해당x
        if (mycharactercontroller != null && mycharactercontroller.velocity != Vector3.zero)
        {
            GUILayout.Label("current speed : " + GetVelocitySpeed().ToString());
            // 현재 방향(속도도 사실 포함됨)
            GUILayout.Label("current velocity Vector : " + mycharactercontroller.velocity.ToString());
            // 현재 속도
            GUILayout.Label("current velocity Magnitude : " + mycharactercontroller.velocity.magnitude.ToString());

        }
    }
    float GetVelocitySpeed()
    {
        // velocity return값 :vector3
        if (mycharactercontroller.velocity == Vector3.zero)
        {
            CurrentVelocity = Vector3.zero;
        }
        else
        {
            Vector3 goalVelocity = mycharactercontroller.velocity;
            goalVelocity.y = 0.0f;
            // fixeddeltatime 덕분에 시간이 갈수록 currentvelocity의 값은 증가됨
            CurrentVelocity = Vector3.Lerp(CurrentVelocity, goalVelocity, VelocityChangeSpeed * Time.fixedDeltaTime);

        }
        return CurrentVelocity.magnitude;
    }
    /// <summary>
    /// 몸통 방향을 이동시키는 함수
    /// </summary>
    void BodyDirectionChange()
    {
        // 움직이고 있다면?
        if (GetVelocitySpeed() > 0.0f)
        {
            Vector3 NewForward = mycharactercontroller.velocity;
            NewForward.y = 0.0f;
            transform.forward = Vector3.Lerp(transform.forward, NewForward, BodyRotationSpeed * Time.deltaTime);
        }
    }
    /// <summary>
    /// 애니메이션을 재생시키는 함수
    /// </summary>
    /// <param name="clip"></param>
    void AnimationPlay(AnimationClip clip)
    {
        myAnimation.clip = clip;
        myAnimation.CrossFade(clip.name);
    }
    /// <summary>
    /// 상태에 맞춰 애니메이션을 재생
    /// </summary>
    void AnimationControl()
    {
        switch (myState)
        {
            case FighterState.Idle:
                AnimationPlay(IdleAnimClip);
                break;
            case FighterState.Walk:
                AnimationPlay(WalkAnimClip);
                break;
            case FighterState.Run:
                AnimationPlay(RunAnimClip);
                break;
            case FighterState.Attack:
                // 공격상태일 시 공격 페이즈에 따라 알맞은 애니메이션 재생
                AttackAnimationControl();
                break;
            case FighterState.Skill:
                AnimationPlay(SkillAnimClip);
                break;
        }
    }
    /// <summary>
    /// 상태를 변경하는 함수
    /// </summary>
    void CheckState()
    {
        // 현재 속도를 얻어옴
        float currentSpeed = GetVelocitySpeed();
        switch (myState)
        {
            case FighterState.Idle:
                if (currentSpeed > 0.0f)
                {
                    myState = FighterState.Walk;
                }
                break;
            case FighterState.Walk:
                if (currentSpeed > 0.5f)
                {
                    myState = FighterState.Run;
                }
                else if (currentSpeed < 0.01f)
                {
                    myState = FighterState.Idle;
                }
                break;
            case FighterState.Run:
                if (currentSpeed < 0.5f)
                {
                    myState = FighterState.Walk;
                }
                if (currentSpeed < 0.01f)
                {
                    myState = FighterState.Idle;
                }
                break;
            case FighterState.Attack:
                CannotMove = true;
                break;
            case FighterState.Skill:
                CannotMove = true;
                break;

        }

    }
    void InputControl()
    {
        // 0,1 > 왼오, 2 > 가운데 휠
        // 마우스 왼쪽 버튼이 눌렸을때 control
        if (Input.GetMouseButtonDown(0) == true)
        {
            if (myState != FighterState.Attack)
            {
                // 공격중이 아닐 때는 공격으로 전환
                myState = FighterState.Attack;
                AttackState = FighterAttackState.Attack1;
            }
            else
            {
                // 공격 중이라면 애니메이션 연속 공격을 할지 안할지 결정
                switch (AttackState)
                {
                    case FighterAttackState.Attack1:
                        if (myAnimation[Attack1AnimClip.name].normalizedTime > 0.1f)
                        {
                            NextAttack = true;
                        }
                        break;
                    case FighterAttackState.Attack2:
                        if (myAnimation[Attack2AnimClip.name].normalizedTime > 0.1f)
                        {
                            NextAttack = true;
                        }
                        break;
                    case FighterAttackState.Attack3:
                        if (myAnimation[Attack3AnimClip.name].normalizedTime > 0.1f)
                        {
                            NextAttack = true;
                        }
                        break;
                    case FighterAttackState.Attack4:
                        if (myAnimation[Attack4AnimClip.name].normalizedTime > 0.1f)
                        {
                            NextAttack = true;
                        }
                        break;
                }
            }
        }
        // 만약 마우스 오른쪽 버튼을 눌렀다면
        if (Input.GetMouseButtonDown(1) == true)
        {
            // 공격 중이었다면, 공격을 그만두고 스킬 시전준비
            if (myState == FighterState.Attack)
            {
                AttackState = FighterAttackState.Attack1;
                NextAttack = false;
            }
            myState = FighterState.Skill;
        }

    }
    /// <summary>
    /// 공격 애니메이션 끝나면 호출되는 이벤트 함수
    /// </summary>
    void OnAttackAnimFinish()
    {
        if (NextAttack == true)
        {
            NextAttack = false;
            switch (AttackState)
            {
                case FighterAttackState.Attack1:
                    AttackState = FighterAttackState.Attack2;
                    break;
                case FighterAttackState.Attack2:
                    AttackState = FighterAttackState.Attack3;
                    break;
                case FighterAttackState.Attack3:
                    AttackState = FighterAttackState.Attack4;
                    break;
                case FighterAttackState.Attack4:
                    AttackState = FighterAttackState.Attack1;
                    break;
            }
        }
        else
        {
            // 다음 공격 모션이 없음
            CannotMove = false;
            // skill은 고려x?
            myState = FighterState.Idle;
            AttackState = FighterAttackState.Attack1;
        }
    }

    /// <summary>
    /// 스킬 애니메이션이 끝날때 호출되는 함수
    /// </summary>
    void OnSkillAnimFinished()
    {
        Vector3 position = transform.position;
        position += transform.forward * 2.0f;
        // skill effect는 js 미지원으로 대체됨. 아래줄이 스킬 이펙트가 맵에 구현되는 부분이다.
        Instantiate(SkillEffect, position, Quaternion.identity);
        CannotMove = false;
        myState = FighterState.Idle;
    }
    /// <summary>
    /// 애니메이션 클립 재생이 끝날때쯤 애니메이션 이벤트 함수 호출되게 추가함
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="Funcname"></param>
    // callback function
    void AddAnimationEvent(AnimationClip clip, string Funcname)
    {
        AnimationEvent newEvent = new AnimationEvent();
        newEvent.functionName = Funcname;
        newEvent.time = clip.length - 0.1f;
        clip.AddEvent(newEvent);
    }
    /// <summary>
    /// 공격 애니메이션 재생
    /// </summary>
    void AttackAnimationControl()
    {
        switch (AttackState)
        {
            case FighterAttackState.Attack1:
                AnimationPlay(Attack1AnimClip);
                break;
            case FighterAttackState.Attack2:
                AnimationPlay(Attack2AnimClip);
                break;
            case FighterAttackState.Attack3:
                AnimationPlay(Attack3AnimClip);
                break;
            case FighterAttackState.Attack4:
                AnimationPlay(Attack4AnimClip);
                break;
        }
    }

    void ApplyGravity()
    {
        if((collisionflags & CollisionFlags.CollidedBelow) != 0)
        {
            verticalSpeed = 0.0f;
        }
        else
        {
            verticalSpeed -= gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// 공격관련 component 제어
    /// </summary>
    void AttackComponentControl()
    {
        switch (myState)
        {
            // 공격중일때만 trail과 sword의 collide 활성화
            case FighterState.Attack:
            case FighterState.Skill:
                AttackTrailRenderer.enabled = true;
                AttackCapsuleCollider.enabled = true;
                break;
            default:
                AttackTrailRenderer.enabled = false;
                AttackCapsuleCollider.enabled = false;
                break;
        }
    }
}

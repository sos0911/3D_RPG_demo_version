using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class GobilnControl : MonoBehaviour
{
    public enum GoblinState { None, Idle, Patrol, Wait, MoveToTarget, Attack, Damage, Die}
    [Header("기본속성")]
    public GoblinState goblinstate = GoblinState.None;

    public float MoveSpeed = 2.0f;
    public GameObject TargetPlayer = null;
    // 몬스터의 target 객체
    public Transform TargetTransform = null;
    public Vector3 TargetPosition = Vector3.zero;

    private Animation myAnimation = null;
    private Transform myTransform = null;

    public AnimationClip IdleAnimClip = null;
    public AnimationClip MoveAnimClip = null;
    public AnimationClip AttackAnimClip = null;
    public AnimationClip DamageAnimClip= null;
    public AnimationClip DieAnimClip = null;

    [Header("전투관련 속성")]
    public int HP = 100;
    public float AttackRange = 1.5f;
    public GameObject DamageEffect = null;
    public GameObject DieEffect = null;

    private Tweener effectTweener = null;
    private SkinnedMeshRenderer skinMeshRenderer = null;


    // Start is called before the first frame update
    void Start()
    {
        goblinstate = GoblinState.Idle;
        // caching
        myAnimation = GetComponent<Animation>();
        myTransform = GetComponent<Transform>();
        // anim clips 기본세팅
        myAnimation[IdleAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[MoveAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[AttackAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DamageAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DamageAnimClip.name].layer= 10;
        myAnimation[DieAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DieAnimClip.name].layer = 10;
        // anim event append
        AddAnimationEvent(AttackAnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(DamageAnimClip, "OnDamageAnimFInished");
        AddAnimationEvent(DieAnimClip, "OnDieAnimFinished");
        // skin mesh caching
        skinMeshRenderer = myTransform.Find("body").GetComponent<SkinnedMeshRenderer>();
    }

    /// <summary>
    /// 고블린의 상태에 따라 동작제어
    /// </summary>
    void CheckState()
    {
        switch (goblinstate)
        {
            case GoblinState.Idle:
                IdleUpdate();
                break;
            case GoblinState.MoveToTarget:
            case GoblinState.Patrol:
                MoveUpdate();
                break;
            case GoblinState.Attack:
                AttackUpdate();
                break;
        }
    }

    void IdleUpdate()
    {
        // 타겟이 안 보이면 임의의 지점으로 레이캐스트를 쏴서 높이 구하고
        // 그곳으로 위치 이동
        // 이 함수에서는 높이를 구함
        if (TargetPlayer == null)
        {
            TargetPosition = new Vector3(myTransform.position.x + Random.Range(-10.0f, 10.0f), myTransform.position.y + 1000.0f, myTransform.position.z + Random.Range(-10.0f, 10.0f));
            // 이제 수직 아래로 레이를 쏴서 높이를 구한다.
            Ray ray = new Ray(TargetPosition, Vector3.down);
            RaycastHit raycasthit = new RaycastHit();
            // 충돌의 정보가 raycasthit에 저장됨
            if(Physics.Raycast(ray, out raycasthit, Mathf.Infinity) == true)
            {
                TargetPosition.y = raycasthit.point.y;
            }
            goblinstate = GoblinState.Patrol;
        }
        else
        {
            // 타겟이 있는 경우 그쪽으로 이동
            goblinstate = GoblinState.MoveToTarget;
        }
    }  
    /// <summary>
    /// 이동 상태일때의 동작함수
    /// </summary>
    void MoveUpdate()
    {
        Vector3 diff = Vector3.zero;
        Vector3 lookatPosition = Vector3.zero;

        switch (goblinstate)
        {
            case GoblinState.Patrol:
                if (TargetPosition != Vector3.zero)
                {
                    diff = TargetPosition - myTransform.position;
                    // 목표지점까지 거의 도달
                    if (diff.magnitude < AttackRange)
                    {
                        StartCoroutine(WaitUpdate());
                        return;
                    }
                    lookatPosition = new Vector3(TargetPosition.x, myTransform.position.y, TargetPosition.z);
                }
                break;
            case GoblinState.MoveToTarget:
                if (TargetPlayer != null)
                {
                    diff = TargetPlayer.transform.position - myTransform.position;

                    if (diff.magnitude < AttackRange)
                    {
                        goblinstate = GoblinState.Attack;
                        return;
                    }

                    lookatPosition = new Vector3(TargetPlayer.transform.position.x, myTransform.position.y, TargetPlayer.transform.position.z);
                }
                break;
        }

        Vector3 direction = diff.normalized;
        direction = new Vector3(direction.x, 0.0f, direction.z);
        Vector3 moveAmount = direction * MoveSpeed * Time.deltaTime;
        // world의 축 기준으로 moveamount만큼 이동
        myTransform.Translate(moveAmount, Space.World);

        myTransform.LookAt(lookatPosition);
    }
    /// <summary>
    /// 대기할때 취하는 동작 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitUpdate()
    {
        goblinstate = GoblinState.Wait;
        float waittime = Random.Range(1.0f, 3.0f);
        yield return new WaitForSeconds(waittime);
        goblinstate = GoblinState.Idle;
    }
    /// <summary>
    /// 애니메이션 재생 함수
    /// </summary>
    void AnimationControl()
    {
        switch (goblinstate)
        {
            case GoblinState.Wait:
                // 대기상태
            case GoblinState.Idle:
                myAnimation.CrossFade(IdleAnimClip.name);
                break;
                // 순찰 | 이동
            case GoblinState.Patrol:
            case GoblinState.MoveToTarget:
                myAnimation.CrossFade(MoveAnimClip.name);
                break;
                // 공격
            case GoblinState.Attack:
                myAnimation.CrossFade(AttackAnimClip.name);
                break;
                // 사망
            case GoblinState.Die:
                myAnimation.CrossFade(DieAnimClip.name);
                break;

        }
    }

    /// <summary>
    ///  인지범위 안에 다른 트리거 | 플레이어가 들어왔다면 호출됨
    /// </summary>
    /// <param name="target"></param>
    void OnSetTarget(GameObject target)
    {
        TargetPlayer = target;
        TargetTransform = TargetPlayer.transform;
        goblinstate = GoblinState.MoveToTarget;
    }
    /// <summary>
    /// 공격 상태일 시 동작함수
    /// </summary>
    void AttackUpdate()
    {
        // 자주 쓰이는 거리연산이면 magnitude를 추천.
        float distance = Vector3.Distance(TargetTransform.position, myTransform.position);
        if (distance > AttackRange + 0.5f)
        {
            // 타겟과의 거리가 멀어진다면 타겟으로 이동
            goblinstate = GoblinState.MoveToTarget;
        }
    }
    /// <summary>
    /// 피격되었나를 확인하는 함수
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerAttack") == true)
        {
            HP -= 10;
            if (HP > 0)
            {
                // 데미지이펙트 js 미지원으로 다른 것을 씀
                Instantiate(DamageEffect, other.transform.position, Quaternion.identity);
                myAnimation.CrossFade(DamageAnimClip.name);
                // 피격 tween effect
                DamageTweenEffect();
            }
            else
            {
                goblinstate = GoblinState.Die;
            }
        }
    }

    void DamageTweenEffect()
    {
        // 트윈 재생중이면 중복호출하지 않음
        if(effectTweener!=null && effectTweener.isComplete == false)
        {
            return;
        }
        Color colorTo = Color.red;
        effectTweener = HOTween.To(skinMeshRenderer.material, 0.2f, new TweenParms()
            .Prop("color", colorTo)
            .Loops(1, LoopType.Yoyo)
            .OnStepComplete(DamageTweenFinished));
    }
    
    void DamageTweenFinished()
    {
        skinMeshRenderer.material.color = Color.white;
    }
    // Update is called once per frame
    void Update()
    {
        CheckState();
        AnimationControl();
    }
    // 애니메이션 재생이 끝날 시 실행될 함수들
    void OnAttackAnimFinished()
    {
        Debug.Log("attack anim finished");

    }
    void OnDamageAnimFInished()
    {
        Debug.Log("damage anim finished");

    }
    void OnDieAnimFinished()
    {
        Debug.Log("Die anim finished");
        // js 미지원으로 effect 대체됨
        Instantiate(DieEffect, myTransform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    /// <summary>
    /// 애니메이션 이벤트 추가 함수. 애니메이션에 함수 형식을 추가
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="funcname"></param>
    void AddAnimationEvent(AnimationClip clip, string funcname)
    {
        AnimationEvent newEvent = new AnimationEvent();
        newEvent.functionName = funcname;
        newEvent.time = clip.length - 0.1f;
        clip.AddEvent(newEvent);
    }
}

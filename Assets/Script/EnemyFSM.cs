using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EnemyFSM : MonoBehaviour
{
    //에너미 상태 상수
    enum EnemyState
    {
        Idle,
        Move,
        Attack,
        Return,
        Damaged,
        Die
    }

    //에너미 상태 변수
    EnemyState m_State;

    //플레이어 발견 범위
    public float findDistance = 8f;

    //플레이어 트랜스폼
    Transform player;

    //공격가능범위
    public float attackDistance = 2f;

    //이동속도
    public float moveSpeed = 5f;

    CharacterController cc;

    //누적 시간
    float currentTime = 0;

    //공격 딜레이 시간
    float attackDelay = 2f;

    //공격력
    public int attackPower = 3;

    //초기위치
    Vector3 originPos;
    Quaternion originRot;

    //이동가능범위
    public float moveDistance = 20f;

    //체력
    public int hp = 15;

    int maxHp = 15;

    public Slider hpSlider;

    Animator anim;

    NavMeshAgent smith;


    // Start is called before the first frame update
    void Start()
    {
        //최초상태
        m_State = EnemyState.Idle;

        player = GameObject.Find("Player").transform;

        cc = GetComponent<CharacterController>();

        //초기위치
        originPos = transform.position;
        originRot = transform.rotation;

        anim = transform.GetComponentInChildren<Animator>();

        smith = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        switch(m_State)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Move:
                Move();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Return:
                Return();
                break;
            case EnemyState.Damaged:
                //Damaged();
                break;
            case EnemyState.Die:
                //Die();
                break;

        }

        hpSlider.value = (float)hp / (float)maxHp;
    }


    void Idle()
    {
        if(Vector3.Distance(transform.position, player.position) < findDistance)
        {
            m_State = EnemyState.Move;
            print("상태전환: Idle -> Move");

            //애니메이션 전환
            anim.SetTrigger("IdleToMove");
        }
    }

    void Move()
    {
        // 현재위치가 이동가능범위를 넘어 가면 복귀
        if (Vector3.Distance(transform.position, originPos) > moveDistance) {
            m_State = EnemyState.Return;
            print("상태전환: Move -> Return");

        // 플레이어와의 거리가 공격범위 밖이라면 플레이어를 향해 이동
        } else if (Vector3.Distance(transform.position, player.position) > attackDistance)
        {
            //Vector3 dir = (player.position - transform.position).normalized;

            //cc.Move(dir * moveSpeed * Time.deltaTime);

            //transform.forward = dir;

            smith.isStopped = true;
            smith.ResetPath();
            smith.stoppingDistance = attackDistance;
            smith.destination = player.position;

        // 아니라면 상태를 공격으로 전환
        } else
        {
            m_State = EnemyState.Attack;
            print("상태전환: Move -> Attack");

            currentTime = attackDelay;

            anim.SetTrigger("MoveToAttackDelay");
        }
    }

    void Attack()
    {
        // 플레이어와의 거리가 공격범위 이내라면 플레이어를 공격
        if (Vector3.Distance(transform.position, player.position) < attackDistance)
        {
            //일정시간마다 공격
            currentTime += Time.deltaTime;
            if(currentTime > attackDelay)
            {
                //player.GetComponent<PlayerMove>().DamageAction(attackPower);
                print("공격");
                currentTime = 0;

                anim.SetTrigger("StartAttack");
            }

        // 아니라면 상태를 이동으로 전환
        }
        else
        {
            m_State = EnemyState.Move;
            print("상태전환: Attack -> Move");
            currentTime = 0;

            anim.SetTrigger("AttackToMove");
        }

    }

    public void AttackAction()
    {
        player.GetComponent<PlayerMove>().DamageAction(attackPower);
    }

    void Return()
    {
        // 초기위치에서 거리가 0.1f 이상이라면 초기위치로 이동
        if (Vector3.Distance(transform.position, originPos) > 0.1f)
        {
            //Vector3 dir = (originPos - transform.position).normalized;
            //cc.Move(dir * moveSpeed * Time.deltaTime);

            //transform.forward = dir;

            smith.destination = originPos;
            smith.stoppingDistance = 0;

        }
        // 아니라면 현재위치를 초기위치로 지정 후 대기로 전환
        else
        {
            smith.isStopped = true;
            smith.ResetPath();

            transform.position = originPos;
            transform.rotation = originRot;
            hp = maxHp; 
            m_State = EnemyState.Idle;
            print("상태전환: Return -> Idle");

            anim.SetTrigger("MoveToIdle");
        }
    }

    //데미지 실행
    public void HitEnemy(int hitPower)
    {
        // 피격, 사망, 복귀 상태면 함수 종료
        if (m_State == EnemyState.Damaged || m_State == EnemyState.Die || m_State == EnemyState.Return)
        {
            return;
        }

        hp -= hitPower;

        smith.isStopped = true;
        smith.ResetPath();

        if (hp > 0)
        {
            m_State = EnemyState.Damaged;
            print("상태전환: Any -> Damaged");

            anim.SetTrigger("Damaged");
            Damaged();

        }
        else
        {
            m_State = EnemyState.Die;
            print("상태전환: Any -> Die");

            anim.SetTrigger("Die");
            Die();
        }
    }

    void Damaged()
    {
        StartCoroutine(DamageProcess());
    }

    //데미지 처리용 코루틴 함수
    IEnumerator DamageProcess()
    {
        //피격 모션 시간만큼 기다린다
        yield return new WaitForSeconds(1.0f);
        m_State = EnemyState.Move;
        print("상태전환: Damaged -> Move");
    }

    void Die()
    {
        StopAllCoroutines();

        StartCoroutine(DieProcess());
    }

    IEnumerator DieProcess()
    {
        //캐릭터 컨트롤러 비활성화
        cc.enabled = false;

        //2초 후 자기자신 제거
        yield return new WaitForSeconds(2f);
        print("소멸");
        Destroy(gameObject);

    }

}

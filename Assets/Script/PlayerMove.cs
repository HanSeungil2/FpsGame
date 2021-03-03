using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 7f;

    CharacterController cc;

    //중력변수
    float gravity = -20f;

    //수직속력변수
    float yVelocity = 0;

    //점프력 변수
    public float jumpPower = 10f;

    public bool isJumping = false;

    public int hp = 20;
    int maxHP = 20;

    public Slider hpSlider;

    public GameObject hitEffect;



    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gm.gState != GameManager.GameState.Run)
        {
            return;
        }

        //키 입력
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //방향
        Vector3 dir = new Vector3(h, 0, v);
        dir = dir.normalized;

        //카메라 기준으로 방향 변환
        dir = Camera.main.transform.TransformDirection(dir);

        //바닥착지
        if(isJumping && cc.collisionFlags == CollisionFlags.Below)
        {
            isJumping = false;
            yVelocity = 0;
        }

        //점프
        if (Input.GetButtonDown("Jump") && !isJumping)
        {
            yVelocity = jumpPower;
            isJumping = true;
        }

        //캐릭터 수직속도에 중력값 적용
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        //이동
        cc.Move(dir * moveSpeed * Time.deltaTime);

        //피격 슬라이더
        hpSlider.value = (float)hp / (float)maxHP;

    }

    //플레이어 피격
    public void DamageAction(int damage)
    {
        hp -= damage;

        if(hp > 0)
        {
            StartCoroutine(PlayHitEffect());
        }
    }

    IEnumerator PlayHitEffect()
    {
        hitEffect.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        hitEffect.SetActive(false);
    }
}

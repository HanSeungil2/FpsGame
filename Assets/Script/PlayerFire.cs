using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFire : MonoBehaviour
{
    public GameObject firePosition;

    public GameObject bombFactory;

    public float throwPower = 15f;

    public GameObject bulletEffect;

    ParticleSystem ps;

    public int weaponPower = 5;

    Animator anim;

    enum WeaponMode
    {
        Normal,
        Sniper
    }
    WeaponMode wMode;

    bool ZoomMode = false;

    public Text wModeText;

    public GameObject[] eff_flash;

    AudioSource aSource;

    public GameObject weapon01;
    public GameObject weapon02;
    public GameObject weapon01_R;
    public GameObject weapon02_R;

    public GameObject crossHair01;
    public GameObject crossHair02;
    public GameObject crossHair02_zoom;



    // Start is called before the first frame update
    void Start()
    {
        ps = bulletEffect.GetComponent<ParticleSystem>();

        anim = GetComponentInChildren<Animator>();

        wMode = WeaponMode.Normal;

        aSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gm.gState != GameManager.GameState.Run)
        {
            return;
        }

        //마우스 왼쪽
        if (Input.GetMouseButtonDown(0))
        {
            aSource.Play();

            if (anim.GetFloat("MoveMotion") == 0)
            {
                anim.SetTrigger("Attack");
            }

            //레이를 생성한후 발사될 위치와 진행방향을 설정
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            StartCoroutine(ShootEffectOn(0.05f));


            //레이가 부딪힌 대상의 정보
            RaycastHit hitInfo = new RaycastHit();

            //레이를 발사한후 부딪힌 물체가 있으면 피격 이펙트 표시
            if(Physics.Raycast(ray, out hitInfo))
            {
                // 에네미에게 피격
                if(hitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    EnemyFSM eFSM = hitInfo.transform.GetComponent<EnemyFSM>();
                    eFSM.HitEnemy(weaponPower);

                } else
                {
                    //피격 이펙트의 위치를 레이가 부딪힌 지점으로 이동
                    bulletEffect.transform.position = hitInfo.point;

                    //피격 이펙트의 방향을 레이가 부딪힌 지점의 법선 벡터와 일치
                    bulletEffect.transform.forward = hitInfo.normal;

                    ps.Play();
                }
                
            }
        }

        //마우스 오른쪽
        if (Input.GetMouseButtonDown(1))
        {
            switch (wMode)
            {
                case WeaponMode.Normal:
                    GameObject bomb = Instantiate(bombFactory);
                    bomb.transform.position = firePosition.transform.position;

                    Rigidbody rb = bomb.GetComponent<Rigidbody>();

                    //카메라 정면방향으로 수류탄에 물리적 힘을 가한다.
                    rb.AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);
                    break;

                case WeaponMode.Sniper:
                    if(!ZoomMode)
                    {
                        Camera.main.fieldOfView = 15f;
                        ZoomMode = true;

                        crossHair02_zoom.SetActive(true);
                        crossHair02.SetActive(false);

                    } else
                    {
                        Camera.main.fieldOfView = 60f;
                        ZoomMode = false;

                        crossHair02_zoom.SetActive(false);
                        crossHair02.SetActive(true);
                    }

                    break;
            }

        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            wMode = WeaponMode.Normal;

            Camera.main.fieldOfView = 60f;

            wModeText.text = "Normal Mode";

            weapon01.SetActive(true);
            weapon02.SetActive(false);
            weapon01_R.SetActive(true);
            weapon02_R.SetActive(false);
            crossHair01.SetActive(true);
            crossHair02.SetActive(false);
            crossHair02_zoom.SetActive(false);


        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            wMode = WeaponMode.Sniper;

            wModeText.text = "Sniper Mode";

            weapon01.SetActive(false);
            weapon02.SetActive(true);
            weapon01_R.SetActive(false);
            weapon02_R.SetActive(true);
            crossHair01.SetActive(false);
            crossHair02.SetActive(true);
        }
    }

    IEnumerator ShootEffectOn(float duration)
    {
        int num = Random.Range(0, eff_flash.Length - 1);

        eff_flash[num].SetActive(true);

        yield return new WaitForSeconds(duration);

        eff_flash[num].SetActive(false);
        eff_flash[num].SetActive(false);

    }
}

using System;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class PlayerMovementOn : MonoBehaviour
    {

        [Header("Movement")]
        [SerializeField] float walkSpeed = 2.0f;
        [SerializeField] private PlayerHealthOn stats;
        [Header("Camera")]
        public Camera cam;
        public Camera cameraPrefab; 

        [Header("Fire")]
        public GameObject fireballPrefab; 
        public Transform firePointPivot;
        public Transform firePoint;

        public float fireSpeed = 12f;
        public float fireCooldown = 0.2f;

        [Header("Animator Params")]
        public string animParamDir8 = "dir8";
        public string animParamAttack = "attack";
        public string animParamShift = "shiftHeld";
        public string animParamSpeed = "Speed";
        public string animParamPlayRate = "PlayRate";
        public string animParamAttackPlayRate = "AttackPlayRate";

        PhotonView view;
        Rigidbody2D rb;
        Animator animator;

        float Speed;
        bool attack;
        bool shiftHeld;

        Vector2 moveDirection;
        public int dir8;
        public Vector2 cachedAimDir = Vector2.right;

        float nextFireTime;

        void Awake()
        {
            view = GetComponent<PhotonView>();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }
        // Start is called before the first frame update
        void Start()
        {

            if (view && view.IsMine && cam == null && cameraPrefab != null)
            {
                cam = Instantiate(cameraPrefab);
                var p = cam.transform.position;
                cam.transform.position = new Vector3(transform.position.x, transform.position.y, p.z != 0 ? p.z : -10f);

                var follow = cam.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollowOn>();
                if (follow != null) follow.target = this.transform;

                EnsureSingleAudioListener(cam);
            }
            if (cam == null) cam = Camera.main;

        }

        void Update()
        {
            if (!view || !view.IsMine)
                return;

            shiftHeld = Input.GetKey(KeyCode.LeftShift);
            attack = Input.GetMouseButton(0);
            Speed = (shiftHeld ? walkSpeed * 1.5f : walkSpeed) * (1 + stats.Speed);

            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");
            moveDirection = new Vector2(moveX, moveY).normalized;

            if (rb) rb.velocity = moveDirection * Speed;

            animator.SetFloat("Horizontal", moveX);
            animator.SetFloat("Vertical", moveY);
            animator.SetFloat(animParamSpeed, moveDirection.sqrMagnitude);
            animator.SetFloat(animParamPlayRate, 1.0f + stats.Speed);
            animator.SetFloat(animParamAttackPlayRate, 1.0f + (2.0f * stats.Speed));
            animator.SetBool(animParamShift, shiftHeld);
            animator.SetBool(animParamAttack, attack);
            
            if (cam == null) return;
            Vector3 sp = Input.mousePosition;
            float planeZ = transform.position.z;
            sp.z = cam.orthographic ? 0f : (planeZ - cam.transform.position.z);
            Vector3 mouseW = cam.ScreenToWorldPoint(sp);
            Vector2 d = (Vector2)(mouseW - transform.position);
            float sq = d.sqrMagnitude;
            if (sq > 1e-6f)
            {
                cachedAimDir = d / Mathf.Sqrt(sq);
            }
            float ang = Mathf.Atan2(cachedAimDir.y, cachedAimDir.x) * Mathf.Rad2Deg;
            if (ang < 0f) ang += 360f;
            dir8 = Mathf.RoundToInt(ang / 45f) % 8;

            animator.SetFloat(animParamDir8, (float)dir8);

            firePointPivot.right = cachedAimDir;

        }



        public void ShootEvent()
        {

            Shoot(cachedAimDir);
        }

        void Shoot(Vector2 dir)
        {

            if (!view || !view.IsMine) return;

            Vector3 spawnPos = firePoint.transform.position;
            float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;


            var go = PhotonNetwork.Instantiate(
                fireballPrefab.name,
                spawnPos,
                Quaternion.Euler(0, 0, z),
                0
            );
            var prb = go.GetComponent<Rigidbody2D>();
            if (prb) prb.velocity = dir.normalized * fireSpeed;
            var myCol = GetComponent<Collider2D>();
            var projCol = go.GetComponent<Collider2D>();
            if (myCol && projCol) Physics2D.IgnoreCollision(myCol, projCol, true);
        }

        static void EnsureSingleAudioListener(Camera cam)
        {
            var all = FindObjectsOfType<AudioListener>();
            foreach (var al in all) al.enabled = false;

            var myAL = cam.GetComponent<AudioListener>();
            if (myAL == null) myAL = cam.gameObject.AddComponent<AudioListener>();
            myAL.enabled = true;
        }

    }
}
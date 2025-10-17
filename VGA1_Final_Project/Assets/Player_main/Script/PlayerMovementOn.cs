using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Photon.Pun;

namespace Player
{
    public class PlayerMovementOn : MonoBehaviour
    {

        float walkSpeed = 2.0f;
        float Speed;
        bool attack;

        private Rigidbody2D rb;

        Vector2 MoveDirction;

        Animator animator;

        public Camera cam;
        public Camera cameraPrefab;

        PhotonView view;

        public int dir8;

        bool shiftHeld;

        public GameObject fireballPrefab;
        public Transform firePoint;
        public float fireSpeed = 12f;
        public float fireCooldown = 0.2f;
        float nextFireTime;
        public Vector2 cachedAimDir = Vector2.right;

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            view = GetComponent<PhotonView>();

            if (view != null && view.IsMine && cam == null && cameraPrefab != null)
            {
                cam = Instantiate(cameraPrefab);

                var p = cam.transform.position;
                cam.transform.position = new Vector3(transform.position.x, transform.position.y, p.z != 0 ? p.z : -10f);

                var follow = cam.GetComponent<Cainos.PixelArtTopDown_Basic.CameraFollowOn>();
                if (follow != null)
                {
                    follow.target = this.transform;
                }

                EnsureSingleAudioListener(cam);
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

        // Update is called once per frame
        void Update()
        {
            if (view.IsMine)
            {
                shiftHeld = Input.GetKey(KeyCode.LeftShift);
                attack = Input.GetMouseButton(0);

                var ph = Player.PlayerHealth.Instance;


                if (shiftHeld)
                {
                    Speed = walkSpeed * 1.5f * (1 + ph.Speed);
                }
                else
                {
                    Speed = walkSpeed * (1 + ph.Speed);
                }


                float moveX = Input.GetAxis("Horizontal");
                float moveY = Input.GetAxis("Vertical");

                MoveDirction = new Vector2(moveX, moveY).normalized;

                rb.velocity = MoveDirction * Speed;

                animator.SetFloat("Horizontal", moveX);
                animator.SetFloat("Vertical", moveY);
                animator.SetFloat("Speed", MoveDirction.sqrMagnitude);
                animator.SetFloat("PlayRate", 1.0f + ph.Speed);
                animator.SetFloat("AttackPlayRate", 1.0f + (2.0f * ph.Speed));


                animator.SetBool("shiftHeld", shiftHeld);

                if (cam == null) return;
                Vector3 sp = Input.mousePosition;
                float planeZ = transform.position.z;
                sp.z = cam.orthographic ? 0f : (planeZ - cam.transform.position.z);
                Vector3 mouseW = cam.ScreenToWorldPoint(sp);

                Vector2 d = (Vector2)(mouseW - transform.position);
                if (d.sqrMagnitude < 1e-6f) return;

                float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                if (ang < 0f) ang += 360f;
                dir8 = Mathf.RoundToInt(ang / 45f) % 8;

                animator.SetFloat("dir8", dir8);

                animator.SetBool("attack", attack);

                if (d.sqrMagnitude > 1e-6f) cachedAimDir = d.normalized;

            }
            

        }

        public void ShootEvent()
        {
            Shoot(cachedAimDir);  // 直接用缓存的方向发射
        }

        void Shoot(Vector2 dir)
        {
            if (!fireballPrefab) return;

            Vector3 spawnPos = firePoint ? firePoint.position : transform.position;
            GameObject go = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);

            float z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0, 0, z);

            var prb = go.GetComponent<Rigidbody2D>();
            if (prb)
            {
                prb.gravityScale = 0f;
                prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                prb.velocity = dir * fireSpeed;
            }
            var myCol = GetComponent<Collider2D>();
            var projCol = go.GetComponent<Collider2D>();
            if (myCol && projCol) Physics2D.IgnoreCollision(myCol, projCol, true);
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;

namespace Cainos.PixelArtTopDown_Basic
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownCharacterControllerTest : MonoBehaviour
    {
        public float speed = 3f;

        public Tilemap layer1Ground;
        public Tilemap ceiling;
        public Tilemap tunnelGround;
        public Tilemap layer2Ground;
        public Tilemap layer3Ground;
        
        public GameObject[] tunnels;

        public Transform feet;

        public SpriteRenderer spriteRenderer;

        private Animator animator;
        private Rigidbody2D rb;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            tunnels = GameObject.FindGameObjectsWithTag("Tunnel");
        }

        private void Update()
        {
            // Movement
            Vector2 dir = Vector2.zero;

            if (Input.GetKey(KeyCode.A)) { dir.x = -1; animator?.SetInteger("Direction", 3); }
            else if (Input.GetKey(KeyCode.D)) { dir.x = 1; animator?.SetInteger("Direction", 2); }

            if (Input.GetKey(KeyCode.W)) { dir.y = 1; animator?.SetInteger("Direction", 1); }
            else if (Input.GetKey(KeyCode.S)) { dir.y = -1; animator?.SetInteger("Direction", 0); }

            dir = dir.normalized;
            animator?.SetBool("IsMoving", dir.sqrMagnitude > 0f);

            rb.velocity = speed * dir;

            // Update sort order
            UpdateSortingOrder();
            
            // Update Tunnel
            UpdateTunnel();
        }

        // Order Rule: Ground ~00; Wall ~25; Props ~50; Player ~75; 
        private void UpdateSortingOrder()
        {
            Vector3 p = feet ? feet.position : (transform.position + new Vector3(0f, -0.08f, 0f));

            if (IsOn(layer3Ground, p)) { SetOrder(375); return; }

            if (IsOn(layer2Ground, p) || IsOn(ceiling, p)) { SetOrder(275); return; }
            
            SetOrder(230);
        }

        private static bool IsOn(Tilemap tm, Vector3 worldPos)
        {
            if (!tm) return false;
            Vector3Int cell = tm.WorldToCell(worldPos);
            return tm.HasTile(cell);
        }

        private void SetOrder(int order)
        {
            print("SetOrder" + order);
            if (spriteRenderer) spriteRenderer.sortingOrder = order;
        }

        private void UpdateTunnel()
        {
            Vector3 p = feet ? feet.position : (transform.position + new Vector3(0f, -0.08f, 0f));
            foreach (GameObject go in tunnels)
            {
                if (IsOn(layer2Ground, p) || IsOn(ceiling, p))
                {
                    go.SetActive(false);
                    tunnelGround.enabled = false;
                }
                else
                {
                    go.SetActive(true);
                    tunnelGround.enabled = true;
                }
                
            }

        }
    }
}

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallOn : MonoBehaviour
{
    public float lifetime = 3f;
    public int damage = 1;
    PhotonView pv;

    // Start is called before the first frame update
    void Awake() {
        pv = GetComponent<PhotonView>(); 
    }
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!pv || !pv.IsMine) return;
        if (other.CompareTag("Player"))
        {
            PhotonView targetPV = other.GetComponent<PhotonView>();
            PlayerHealthOn targetHP = other.GetComponent<PlayerHealthOn>();

            if (targetPV != null && targetHP != null && targetPV.Owner != pv.Owner)
            {
                targetPV.RPC("RPC_TakeDamage", targetPV.Owner, (float)damage);
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }

}

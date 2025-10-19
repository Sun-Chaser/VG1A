using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

    public class PlayerHealthOn : MonoBehaviourPun, IPunObservable
    {
        public delegate void OnHealthChangedDelegate();
        public OnHealthChangedDelegate onHealthChangedCallback;

        [SerializeField] private float health = 3f;
        [SerializeField] private float maxHealth = 3f;
        [SerializeField] private float maxTotalHealth = 10f;
        [SerializeField] private float speed = 3.5f;
        [SerializeField] private float maxSpeed = 6f;

        public float Health => health;
        public float MaxHealth => maxHealth;
        public float MaxTotalHealth => maxTotalHealth;
        public float Speed => speed;
        public float MaxSpeed => maxSpeed;

        public void Heal(float amount)
        {
            if (!photonView.IsMine) return;
            health += amount;
            ClampHealth();
        }

        public void TakeDamage(float dmg)
        {
            if (!photonView.IsMine) return;
            health -= dmg;
            ClampHealth();
        }

        public void AddHealth()
        {
            if (!photonView.IsMine) return;
            if (maxHealth < maxTotalHealth)
            {
                maxHealth += 1f;
                health = maxHealth;
                onHealthChangedCallback?.Invoke();
            }
        }

        public void AddSpeed()
        {
            if (!photonView.IsMine) return;
            if (speed < maxSpeed)
            {
                speed = Mathf.Min(maxSpeed, speed + 0.1f);
            }
        }

        private void ClampHealth()
        {
            health = Mathf.Clamp(health, 0f, maxHealth);
            onHealthChangedCallback?.Invoke();

            if (photonView.IsMine && health <= 0f)
            {
                //var current = SceneManager.GetActiveScene();
                //SceneManager.LoadScene(current.buildIndex);
                PhotonNetwork.Destroy(gameObject);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(health);
                stream.SendNext(maxHealth);
                stream.SendNext(speed);
            }
            else
            {
                health = (float)stream.ReceiveNext();
                maxHealth = (float)stream.ReceiveNext();
                speed = (float)stream.ReceiveNext();
                onHealthChangedCallback?.Invoke();
            }
        }

        [PunRPC]
        public void RPC_TakeDamage(float dmg)
        {
            if (!photonView.IsMine) return;
            TakeDamage(dmg);
        }
    }

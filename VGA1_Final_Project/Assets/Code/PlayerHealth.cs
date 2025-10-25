/*
 *  Author: ariel oliveira [o.arielg@gmail.com]
 */

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerHealth : MonoBehaviour
    {
        public delegate void OnHealthChangedDelegate();
        public OnHealthChangedDelegate onHealthChangedCallback;

        #region Sigleton
        private static PlayerHealth instance;
        public static PlayerHealth Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<PlayerHealth>();
                return instance;
            }
        }
        #endregion

        [SerializeField]
        private float health;
        [SerializeField]
        private float maxHealth;
        [SerializeField]
        private float maxTotalHealth;
        [SerializeField]
        float speed;
        [SerializeField]
        private float maxSpeed;

        public float Health { get { return health; } }
        public float MaxHealth { get { return maxHealth; } }
        public float MaxTotalHealth { get { return maxTotalHealth; } }
        public float Speed { get { return speed; } }
        public float MaxSpeed { get { return maxSpeed; } }

        public void Heal(float health)
        {
            this.health += health;
            ClampHealth();
        }

        public void TakeDamage(float dmg)
        {
            health -= dmg;
            ClampHealth();
        }

        public void AddHealth()
        {
            if (maxHealth < maxTotalHealth)
            {
                maxHealth += 1;
                health = maxHealth;

                if (onHealthChangedCallback != null)
                    onHealthChangedCallback.Invoke();
            }   
        }

        void ClampHealth()
        {
            health = Mathf.Clamp(health, 0, maxHealth);

            if (onHealthChangedCallback != null)
                onHealthChangedCallback.Invoke();

            if (health <= 0f)
            {
                if (PlayerPrefs.GetInt("HighestScore", 0) < GameController.score)
                {
                    PlayerPrefs.SetInt("HighestScore", GameController.score);
                }
                SceneManager.LoadScene("GameResults");
            }
        }

        public void AddSpeed()
        {
            if (speed < maxSpeed)
            {
                speed += 0.1f;
            }
        }
    }
}

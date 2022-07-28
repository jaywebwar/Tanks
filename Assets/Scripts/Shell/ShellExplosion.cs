using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask;
    public ParticleSystem m_ExplosionParticles;       
    public AudioSource m_ExplosionAudio;              
    public float m_MaxDamage = 100f;                  
    public float m_ExplosionForce = 1000f;            
    public float m_MaxLifeTime = 2f;                  
    public float m_ExplosionRadius = 5f;              


    private void Start()
    {
        Destroy(gameObject, m_MaxLifeTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        // Find all the tanks in an area around the shell and damage them.
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

        for(int i = 0; i < colliders.Length; i++)
        {
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();
            if(!targetRigidbody)
                continue;

            targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
            if(!targetHealth)
                continue;
            
            float damage = CalculateDamage(targetRigidbody.position);

            targetHealth.TakeDamage(damage);

        }
        // Leave the Explosion particle system at the point of impact by unparenting from shell gameobject.
        m_ExplosionParticles.transform.parent = null;

        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();

        // Destroy the particles at the end of their duration and destroy this gameobject
        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
        Destroy(gameObject);
    }


    private float CalculateDamage(Vector3 targetPosition)
    {
        // Calculate the amount of damage a target should take based on it's position.

        Vector3 explosionCenterToTarget = targetPosition - transform.position;
        float targetDistanceFromExplosion = explosionCenterToTarget.magnitude;

        // normalize the distance to the radius of the explosion.
        float normalDistance = (m_ExplosionRadius - targetDistanceFromExplosion)/m_ExplosionRadius;

        float damage = normalDistance * m_MaxDamage;

        // Enforces that one cannot receive negative damage in the event where the center transform of
        // the targeted gameObject is outside of the explosion radius, but the collider is within.
        damage = Mathf.Max(0f, damage);
        
        return damage;
    }
}
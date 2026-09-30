using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExplodingCube : MonoBehaviour
{
    bool hasExploded;

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded || other.GetComponent<MainChar>() == null)
        {
            return;
        }

        hasExploded = true;
        PlayExplosion(transform.position);
        Destroy(gameObject);
    }

    static void PlayExplosion(Vector3 position)
    {
        GameObject prefab = null;
#if UNITY_EDITOR
        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Explosions/CFXR Explosion 1.prefab");
#endif
        if (prefab != null)
        {
            Object.Instantiate(prefab, position, Quaternion.identity);
            return;
        }

        CreateFallbackExplosion(position);
    }

    static void CreateFallbackExplosion(Vector3 position)
    {
        var effectObject = new GameObject("Cube Explosion");
        effectObject.transform.position = position;
        var particles = effectObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.duration = 1.2f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.55f);
        main.startColor = new Color(1f, 0.48f, 0.08f);
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;
        var emission = particles.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 48) });
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.45f;
        particles.Play();
    }
}

using UnityEngine;

public class Particle_System : MonoBehaviour
{
    private ParticleSystem auraParticles;
    
    void Start()
    {
        CreateAura();
    }
    
    void CreateAura()
    {
        // Create particle system as child
        GameObject auraObj = new GameObject("SSJ_Aura");
        auraObj.transform.SetParent(transform);
        auraObj.transform.localPosition = Vector3.zero;
        
        auraParticles = auraObj.AddComponent<ParticleSystem>();
        var main = auraParticles.main;
        var emission = auraParticles.emission;
        var shape = auraParticles.shape;
        var colorOverLifetime = auraParticles.colorOverLifetime;
        var sizeOverLifetime = auraParticles.sizeOverLifetime;
        var velocityOverLifetime = auraParticles.velocityOverLifetime;
        var renderer = auraParticles.GetComponent<ParticleSystemRenderer>();
        
        // Main module - core settings
        main.startLifetime = 1.5f;
        main.startSpeed = 2f;
        main.startSize = 0.3f;
        main.startColor = new Color(0.2f, 0.6f, 1f, 0.8f); // initial color
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission - particle spawn rate
        emission.rateOverTime = 80f;
        
        // Shape - emit from around the cube
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(1.2f, 1.2f, 1.2f);
        
        // Color over lifetime - fade out
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
               new GradientColorKey(new Color(0.5f, 0.8f, 1f), 0f),  // Light blue
                new GradientColorKey(new Color(0.2f, 0.5f, 1f), 0.5f),  // Medium blue
                new GradientColorKey(new Color(0f, 0.3f, 0.8f), 1f)  // Dark blue
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.6f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;
        
        // Size over lifetime - grow then shrink
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Velocity - upward rising motion
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(3f, 5f);
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
        
        // Renderer settings
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateParticleMaterial();
    }
    
    Material CreateParticleMaterial()
    {
        // Use built-in particle shader with additive blending
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetFloat("_Mode", 3); // Transparent mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.color = Color.white;
        
        return mat;
    }
}

using UnityEngine;

namespace WakeAIUp.UI
{
    /// <summary>
    /// Creates high-tech particle effects for UI events:
    /// ambient floating particles, elimination bursts, victory confetti, scan lines.
    /// </summary>
    public class ParticleEffects : MonoBehaviour
    {
        private static ParticleEffects _instance;
        public static ParticleEffects Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("ParticleEffects");
                    _instance = go.AddComponent<ParticleEffects>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public ParticleSystem CreateAmbientParticles(Transform parent)
        {
            var go = new GameObject("AmbientParticles");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 6f;
            main.startSpeed = 8f;
            main.startSize = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.maxParticles = 60;
            main.startColor = new ParticleSystem.MinMaxGradient(
                UITheme.GlowBlue,
                UITheme.GlowCyan
            );
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.02f;

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(20f, 12f, 1f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.15f, 0.3f),
                    new GradientAlphaKey(0.15f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 0.5f, 1f, 1.5f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = -1;

            return ps;
        }

        public ParticleSystem CreateEliminationBurst(Transform parent, Vector3 position, Color playerColor)
        {
            var go = new GameObject("EliminationBurst");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.2f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(30f, 80f);
            main.startSize = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.maxParticles = 40;
            main.startColor = new ParticleSystem.MinMaxGradient(
                playerColor,
                new Color(playerColor.r, playerColor.g, playerColor.b, 0.5f)
            );
            main.duration = 0.5f;
            main.loop = false;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 30)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(playerColor, 0f),
                    new GradientColorKey(UITheme.Border, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0f));

            ps.Play();
            Destroy(go, 2f);

            return ps;
        }

        public ParticleSystem CreateVictoryConfetti(Transform parent)
        {
            var go = new GameObject("VictoryConfetti");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 4f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(20f, 50f);
            main.startSize = new ParticleSystem.MinMaxCurve(4f, 10f);
            main.maxParticles = 200;
            main.startColor = new ParticleSystem.MinMaxGradient(
                UITheme.AccentBlue,
                UITheme.AccentPurple
            );
            main.gravityModifier = 0.3f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 80),
                new ParticleSystem.Burst(0.3f, 60),
                new ParticleSystem.Burst(0.6f, 40)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(16f, 1f, 1f);
            shape.position = new Vector3(0f, 6f, 0f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(UITheme.AccentCyan, 0f),
                    new GradientColorKey(UITheme.AccentBlue, 0.33f),
                    new GradientColorKey(UITheme.AccentPurple, 0.66f),
                    new GradientColorKey(UITheme.AccentGreen, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

            main.loop = false;
            main.duration = 1f;
            ps.Play();
            Destroy(go, 5f);

            return ps;
        }
    }
}

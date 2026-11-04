using System;
using UnityEngine;

namespace ProjectNyx.AudioRadio
{
    /// <summary>
    /// [ES] Motor DSP Multicapa de Audio Sci-Fi Pesado (LainOS Heavy Audio Engine).
    /// Sustituye los tonos simples por síntesis multicapa de alta pegada:
    /// Sub-grave exponencial (42Hz-195Hz) + Descarga de Plasma FM con distorsión +
    /// Transiente mecánico de cerrojo + Resonancia metálica de cañón.
    /// Incluye también generadores para la Brecha Interdimensional y Cofres de Suministros.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] Heavy Sci-Fi Multilayer DSP Audio Engine (LainOS Heavy Audio Engine).
    /// Generates high-impact procedural sound synthesis:
    /// Exponential sub-bass punch (42Hz-195Hz) + Distorted FM plasma discharge +
    /// Mechanical bolt transient + Metallic barrel resonance.
    /// Also includes synthesis generators for Interdimensional Rifts and Supply Chests.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    public static class SynthSfxGenerator
    {
        private const int SampleRate = 44100;

        /// <summary>
        /// [ES] Cañón de Plasma Pesado Multicapa (estilo Cyberpunk 2077 / Halo Plasma Rifle).
        /// [EN] Multilayer Heavy Plasma Cannon (Cyberpunk 2077 / Halo Plasma Rifle style).
        /// </summary>
        public static AudioClip CreatePlasmaBlasterClip()
        {
            float duration = 0.42f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[sampleCount];

            System.Random rng = new System.Random(42);
            float subPhase = 0f;
            float plasmaPhase = 0f;
            float prevNoise = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;

                // Capa 1: Sub-bass punch contundente (197Hz -> 42Hz)
                float subFreq = 42f + 155f * Mathf.Exp(-t * 28f);
                subPhase += (2f * Mathf.PI * subFreq) / SampleRate;
                float subPunch = Mathf.Sin(subPhase) * Mathf.Exp(-t * 9.5f) * 1.15f;

                // Capa 2: Pulso de plasma saturado con modulación FM
                float plasmaFreq = 95f + 580f * Mathf.Exp(-t * 18f);
                plasmaPhase += (2f * Mathf.PI * plasmaFreq) / SampleRate;
                float fmMod = Mathf.Sin(2f * Mathf.PI * 180f * t) * 2.5f * Mathf.Exp(-t * 12f);
                float rawPlasma = Mathf.Sin(plasmaPhase + fmMod);
                float plasmaWave = (float)Math.Tanh(2.4 * rawPlasma) * Mathf.Exp(-t * 11f) * 0.75f;

                // Capa 3: Transiente mecánico de cerrojo + chispa de arco eléctrico
                float rawNoise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float hiPassNoise = rawNoise - prevNoise;
                prevNoise = rawNoise;
                float sparkEnv = Mathf.Exp(-t * 48f) + 0.25f * Mathf.Exp(-t * 10f) * (1f + 0.5f * Mathf.Sin(2f * Mathf.PI * 90f * t));
                float mechSnap = hiPassNoise * sparkEnv * 0.65f;

                // Capa 4: Resonancia metálica del cañón
                float barrelRing = (Mathf.Sin(2f * Mathf.PI * 280f * t) + 0.6f * Mathf.Sin(2f * Mathf.PI * 418f * t)) * Mathf.Exp(-t * 16f) * 0.30f;

                float mix = (float)Math.Tanh(1.35 * (subPunch + plasmaWave + mechSnap + barrelRing));
                samples[i] = Mathf.Clamp(mix * 0.92f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Heavy_PlasmaCannon_DSP", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Tajo de Katana Térmica con corte de aire y zumbido de plasma.
        /// </summary>
        public static AudioClip CreateKatanaSlashClip()
        {
            float duration = 0.32f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[sampleCount];

            System.Random rng = new System.Random(108);
            float phase = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float normT = t / duration;
                float freq = Mathf.Lerp(140f, 520f, Mathf.Sin(normT * Mathf.PI));
                phase += (2f * Mathf.PI * freq) / SampleRate;

                float env = Mathf.Sin(normT * Mathf.PI);
                float whoosh = (float)(rng.NextDouble() * 2.0 - 1.0) * env * 0.45f;
                float bladeHum = (float)Math.Tanh(2.2 * Mathf.Sin(phase)) * env * 0.65f;
                samples[i] = Mathf.Clamp(whoosh + bladeHum, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Heavy_KatanaSlash_DSP", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Pulso de Escudo Balístico con carga de condensador y golpe de fuerza.
        /// </summary>
        public static AudioClip CreateShieldPulseClip()
        {
            float duration = 0.38f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[sampleCount];

            float phase = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float freq = 85f + 360f * Mathf.Exp(-t * 8f);
                phase += (2f * Mathf.PI * freq) / SampleRate;

                float env = Mathf.Exp(-t * 6.5f);
                float sub = Mathf.Sin(2f * Mathf.PI * 60f * t) * env * 0.7f;
                float shimmer = Mathf.Sin(phase + Mathf.Sin(2f * Mathf.PI * 45f * t) * 1.8f) * env * 0.55f;
                samples[i] = Mathf.Clamp((float)Math.Tanh(1.2 * (sub + shimmer)), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Heavy_ShieldPulse_DSP", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Impacto pesado en blindaje enemigo (golpe metálico + arco eléctrico).
        /// </summary>
        public static AudioClip CreateEnemyHitClip()
        {
            float duration = 0.28f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[sampleCount];

            System.Random rng = new System.Random(1337);
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float thud = Mathf.Sin(2f * Mathf.PI * (140f * Mathf.Exp(-t * 22f) + 55f) * t) * Mathf.Exp(-t * 14f) * 0.9f;
                float clang = (Mathf.Sin(2f * Mathf.PI * 680f * t) + 0.7f * Mathf.Sin(2f * Mathf.PI * 1140f * t)) * Mathf.Exp(-t * 25f) * 0.55f;
                float spark = (float)(rng.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 32f) * 0.5f;
                samples[i] = Mathf.Clamp((float)Math.Tanh(1.3 * (thud + clang + spark)), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Heavy_ArmorHit_DSP", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Apertura y expansión del Portal / Brecha Interdimensional (estilo Spatial Ops).
        /// </summary>
        public static AudioClip CreateDimensionalRiftClip()
        {
            float duration = 1.65f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[sampleCount];

            System.Random rng = new System.Random(777);
            float sweepPhase = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Pow(Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration)), 0.6f);

                float riftSub = Mathf.Sin(2f * Mathf.PI * (48f + 18f * Mathf.Sin(2f * Mathf.PI * 3.5f * t)) * t) * env * 0.9f;
                float sweepFreq = 90f + 420f * Mathf.Sin(Mathf.PI * t / duration);
                sweepPhase += (2f * Mathf.PI * sweepFreq) / SampleRate;
                float shimmer = (float)Math.Tanh(2.0 * Mathf.Sin(sweepPhase + 3f * Mathf.Sin(2f * Mathf.PI * 24f * t))) * env * 0.65f;
                float crackle = (Mathf.Sin(2f * Mathf.PI * 17f * t) > 0.65f) ? (float)(rng.NextDouble() * 2.0 - 1.0) * env * 0.35f : 0f;

                samples[i] = Mathf.Clamp((float)Math.Tanh(1.2 * (riftSub + shimmer + crackle)), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Heavy_DimensionalRift_DSP", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Apertura de Cofre de Suministros 3D (despresurización neumática + acorde LainOS).
        /// </summary>
        public static AudioClip CreateSupplyChestClip()
        {
            float duration = 0.75f;
            int sampleCount = Mathf.CeilToInt(SampleRate * duration);
            float[] samples = new float[sampleCount];

            System.Random rng = new System.Random(909);
            float[] noteStarts = { 0.0f, 0.11f, 0.22f, 0.34f };
            float[] noteFreqs = { 523.25f, 659.25f, 783.99f, 1046.50f };

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float hiss = (float)(rng.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 14f) * 0.42f;
                float chime = 0f;
                for (int n = 0; n < noteStarts.Length; n++)
                {
                    if (t >= noteStarts[n])
                    {
                        float dt = t - noteStarts[n];
                        float f = noteFreqs[n];
                        chime += (Mathf.Sin(2f * Mathf.PI * f * dt) + 0.35f * Mathf.Sin(2f * Mathf.PI * f * 2f * dt)) * Mathf.Exp(-dt * 7.5f) * 0.38f;
                    }
                }
                samples[i] = Mathf.Clamp((float)Math.Tanh(1.2 * (hiss + chime)), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Heavy_SupplyChest_DSP", sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

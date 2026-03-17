using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectHandler : MonoBehaviour {

    [SerializeField] private List<AudioClip> m_generators;
    [SerializeField] private Vector2 m_minMaxPitch;
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private bool m_playOnAwake;
    [SerializeField] private float m_fadeTime = 1;
    Coroutine m_fadeRoutine;

    private void Awake() {
        if (m_playOnAwake)
            Play();
    }

    public void Play() {

        AudioClip generator = GetRandomClip();
        m_audioSource.pitch = GetRandomPitch();
        m_audioSource.PlayOneShot(generator);
    }

    public void FadeIn() {
        StartLoop();

        if (m_fadeRoutine != null)
            StopCoroutine(m_fadeRoutine);

        m_fadeRoutine = StartCoroutine(Fade(1f));
    }

    public void FadeOut() {
        if (m_fadeRoutine != null)
            StopCoroutine(m_fadeRoutine);

        m_fadeRoutine = StartCoroutine(Fade(0f));
    }

    public void StopImmediate() {
        if (m_fadeRoutine != null)
            StopCoroutine(m_fadeRoutine);

        m_audioSource.Stop();
    }

    IEnumerator Fade(float target) {
        while (Mathf.Abs(m_audioSource.volume - target) > 0.01f) {
            m_audioSource.volume = Mathf.MoveTowards(
                m_audioSource.volume,
                target,
                m_fadeTime * Time.deltaTime
            );

            yield return null;
        }

        m_audioSource.volume = target;

        if (target == 0f)
            StopLoop();
    }

    private void StartLoop() {
        if (m_audioSource.isPlaying)
            return;

        m_audioSource.clip = GetRandomClip();
        m_audioSource.pitch = GetRandomPitch();
        m_audioSource.loop = true;
        m_audioSource.Play();
    }

    private void StopLoop() {
        m_audioSource.loop = false;
        m_audioSource.Stop();
    }

    private AudioClip GetRandomClip() {
        int index = Random.Range(0, m_generators.Count);
        return m_generators[index];
    }

    private float GetRandomPitch() {
        return Random.Range(m_minMaxPitch.x, m_minMaxPitch.y);
    }

}

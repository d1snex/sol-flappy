using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip clipWing;
    [SerializeField] private AudioClip clipPoint;
    [SerializeField] private AudioClip clipHit;
    [SerializeField] private AudioClip clipDie;
    [SerializeField] private AudioClip clipSwoosh;

    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayWing() => sfxSource.PlayOneShot(clipWing);
    public void PlayPoint() => sfxSource.PlayOneShot(clipPoint);
    public void PlayHit() => sfxSource.PlayOneShot(clipHit);
    public void PlayDie() => sfxSource.PlayOneShot(clipDie);
    public void PlaySwoosh() => sfxSource.PlayOneShot(clipSwoosh);
}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic; 
    public AudioClip shopMusic; 

    public AudioClip buttonClickClip;
    public AudioClip cardClickClip;
    public AudioClip loseLifeClip;
    public AudioClip cheatClip; 
    public AudioClip shopPurchaseClip; 
    public AudioClip matchStartClip;  
    private Coroutine fade;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu")
        {
            PlayMusic(mainMenuMusic);
        }
        else if (scene.name == "SampleScene")
        {
            PlayMusic(gameplayMusic);
            PlayMatchStart();
        }
        else
        {
            PlayMusic(shopMusic);
        }
    }

    public void PlayMusic(AudioClip musicClip, float duration = 2f)
    {
        if (musicClip == null) return;

        if (fade != null)
        {
            StopCoroutine(fade);
        }
        fade = StartCoroutine(FadeAndSwitchMusic(musicClip, duration));
    }

    private IEnumerator FadeAndSwitchMusic(AudioClip newClip, float duration)
    {
        if (musicSource.isPlaying && musicSource.clip != newClip)
        {
            float startVolume = musicSource.volume;
            float timer = 0f;
            while (timer < duration / 2f)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / (duration / 2f));
                yield return null;
            }
            musicSource.Stop();
        }
        else if (musicSource.clip == newClip && musicSource.isPlaying)
        {
            yield break;
        }

        musicSource.clip = newClip;
        musicSource.volume = 0f; 
        musicSource.loop = true;
        musicSource.Play();

        float fadeInTimer = 0f;
        while (fadeInTimer < duration / 2f)
        {
            fadeInTimer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, 1f, fadeInTimer / (duration / 2f));
            yield return null;
        }
        musicSource.volume = 1f;
    }

    public void ChangeSceneWithFade(string sceneName, float fadeDuration = 1.0f)
    {
        StartCoroutine(FadeOutAndLoad(sceneName, fadeDuration));
    }

    private IEnumerator FadeOutAndLoad(string sceneName, float duration)
    {
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }
            musicSource.Stop();
        }
        SceneManager.LoadScene(sceneName);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    public void PlayButtonClick() => PlaySFX(buttonClickClip);
    public void PlayCardClick() => PlaySFX(cardClickClip);
    public void PlayLoseLife() => PlaySFX(loseLifeClip);
    public void PlayCheat() => PlaySFX(cheatClip);
    public void PlayShopPurchase() => PlaySFX(shopPurchaseClip);
    public void PlayMatchStart() => PlaySFX(matchStartClip);
}
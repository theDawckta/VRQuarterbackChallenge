using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace VRStandardAssets.Utils
{
    // This class is used to fade the entire screen to black (or
    // any chosen colour).  It should be used to smooth out the
    // transition between scenes or restarting of a scene.
    public class VRCameraFade : MonoBehaviour
    {
        public event Action OnFadeComplete;                             // This is called when the fade in or out has finished.


        [SerializeField] public GameObject FadeCube;
        // Reference to the image that covers the screen.
        [SerializeField] private AudioMixerSnapshot m_DefaultSnapshot;  // Settings for the audio mixer to use normally.
        [SerializeField] private AudioMixerSnapshot m_FadedSnapshot;    // Settings for the audio mixer to use when faded out.
        [SerializeField] private Color m_FadeColor = Color.black;       // The colour the image fades out to.
        [SerializeField] private float m_FadeDuration = 0.5f;           // How long it takes to fade in seconds.
        [SerializeField] private bool m_FadeInOnSceneLoad = false;      // Whether a fade in should happen as soon as the scene is loaded.
        [SerializeField] private bool m_FadeInOnStart = false;          // Whether a fade in should happen just but Updates start.

        private List<Image> _fadePanels = new List<Image>();
        private bool m_IsFading;                                        // Whether the screen is currently fading.
        private float m_FadeStartTime;                                  // The time when fading started.
        private Color m_FadeOutColor;                                   // This is a transparent version of the fade colour, it will ensure fading looks normal.
        private float _fadeCameraTo = 0.7f;								// alpha of partway camera fade

        public bool IsFading { get { return m_IsFading; } }
        public float Alpha { get { return _fadePanels[0].color.a; } }

        private void Awake()
        {
			if (FadeCube != null)
			{
				if (!FadeCube.activeInHierarchy)
					FadeCube.SetActive (true);
			}
            m_FadeOutColor = new Color(m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, 0f);
            _fadePanels = FadeCube.GetComponentsInChildren<Image>().ToList();
            SetFadePanels(true);
        }

        private void SetFadePanels(bool status)
        {
            foreach (var panel in _fadePanels)
            {
                panel.gameObject.SetActive(status);
            }
        }


        private void Start()
        {
            // If applicable set the immediate colour to be faded out and then fade in.
            if (m_FadeInOnStart)
            {
                SetFadePanels(true);
                for (int i = 0; i < _fadePanels.Count; i++)
                    _fadePanels[i].color = m_FadeColor;
                FadeIn(true);
            }
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            // If applicable set the immediate colour to be faded out and then fade in.
            if (m_FadeInOnSceneLoad)
            {
                SetFadePanels(true);

                for (int i = 0; i < _fadePanels.Count; i++)
                    _fadePanels[i].color = m_FadeColor;
                FadeIn(true);
            }
        }


        // Since no duration is specified with this overload use the default duration.
        public void FadeOut(bool fadeAudio)
        {
            FadeOut(m_FadeDuration, fadeAudio);
        }


        public void FadeOut(float duration, bool fadeAudio)
        {
            // If not already fading start a coroutine to fade from the fade out colour to the fade colour.
            if (m_IsFading)
                return;

            StartCoroutine(BeginFade(m_FadeOutColor, m_FadeColor, duration));

            // Fade out the audio over the same duration.
            if (m_FadedSnapshot && fadeAudio)
                m_FadedSnapshot.TransitionTo(duration);
        }

        // Since no duration is specified with this overload use the default duration.
        public void FadeIn(bool fadeAudio)
        {
            FadeIn(m_FadeDuration, fadeAudio);
        }

        public void FadeCameraOut()
        {
            if (IsFading)
                return;

            SetFadePanels(true);
            StartCoroutine(BeginFade(_fadePanels[0].color, new Color(0, 0, 0, _fadeCameraTo), 0.3f));
        }

        public void FadeCameraOut(float endValue)
        {
            if (IsFading)
                return;

            SetFadePanels(true);
            StartCoroutine(BeginFade(_fadePanels[0].color, new Color(0, 0, 0, endValue), 0.3f));
        }

        public void FadeCameraIn()
        {
            if (IsFading)
                return;

            SetFadePanels(true);

            if (_fadePanels[0].color.a == _fadeCameraTo) // is faded out
            {
                StartCoroutine(BeginFade(_fadePanels[0].color, new Color(0, 0, 0, 0.0f), 0.3f));
            }
        }


        public void FadeIn(float duration, bool fadeAudio)
        {
            // If not already fading start a coroutine to fade from the fade colour to the fade out colour.
            if (m_IsFading)
                return;

            StartCoroutine(BeginFade(m_FadeColor, m_FadeOutColor, duration));

            // Fade in the audio over the same duration.
            if (m_DefaultSnapshot && fadeAudio)
                m_DefaultSnapshot.TransitionTo(duration);
        }

        public IEnumerator BeginFadeOut(bool fadeAudio)
        {
            // Fade out the audio over the default duration.
            if (m_FadedSnapshot && fadeAudio)
                m_FadedSnapshot.TransitionTo(m_FadeDuration);

            yield return StartCoroutine(BeginFade(m_FadeOutColor, m_FadeColor, m_FadeDuration));
        }


        public IEnumerator BeginFadeOut(float duration, bool fadeAudio)
        {
            // Fade out the audio over the given duration.
            if (m_FadedSnapshot && fadeAudio)
                m_FadedSnapshot.TransitionTo(duration);

            yield return StartCoroutine(BeginFade(m_FadeOutColor, m_FadeColor, duration));
        }


        public IEnumerator BeginFadeIn(bool fadeAudio, float fadeDurationAddition = 0.0f)
        {
            float fadeDuration = m_FadeDuration + fadeDurationAddition;
            // Fade in the audio over the default duration.
            if (m_DefaultSnapshot && fadeAudio)
                m_DefaultSnapshot.TransitionTo(fadeDuration);

            yield return StartCoroutine(BeginFade(m_FadeColor, m_FadeOutColor, fadeDuration));
        }


        public IEnumerator BeginFadeIn(float duration, bool fadeAudio)
        {
            // Fade in the audio over the given duration.
            if (m_DefaultSnapshot && fadeAudio)
                m_DefaultSnapshot.TransitionTo(duration);

            yield return StartCoroutine(BeginFade(m_FadeColor, m_FadeOutColor, duration));
        }


        public IEnumerator BeginFade(Color startCol, Color endCol, float duration)
        {
            // Fading is now happening.  This ensures it won't be interupted by non-coroutine calls.
            m_IsFading = true;

            SetFadePanels(true);

            // Execute this loop once per frame until the timer exceeds the duration.
            float timer = 0f;
            while (timer <= duration)
            {
                // Set the colour based on the normalised time.
                foreach(var panel in _fadePanels)
                    panel.color = Color.Lerp(startCol, endCol, timer / duration);

                // Increment the timer by the time between frames and return next frame.
                timer += Time.deltaTime;
                yield return null;
            }

            // Fading is finished so allow other fading calls again.
            m_IsFading = false;
            foreach (var panel in _fadePanels)
                panel.color = endCol;

            if (endCol.a == 0)
            {
                SetFadePanels(false);
            }

            // If anything is subscribed to OnFadeComplete call it.
            if (OnFadeComplete != null)
            {
                OnFadeComplete();
            }
        }
    }
}
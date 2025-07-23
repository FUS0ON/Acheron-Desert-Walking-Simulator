using System.Linq;
using UnityEngine;
using Photon.Pun;

public class FirstPersonAudio : MonoBehaviourPunCallbacks
{
    public FirstPersonMovement character;
    public GroundCheck groundCheck;

    [Header("Step")]
    public AudioSource stepAudio;
    public AudioSource runningAudio;
    [Tooltip("Minimum velocity for moving audio to play")]
    /// <summary> "Minimum velocity for moving audio to play" </summary>
    public float velocityThreshold = .01f;
    Vector2 lastCharacterPosition;
    Vector2 CurrentCharacterPosition => new Vector2(character.transform.position.x, character.transform.position.z);

    [Header("Landing")]
    public AudioSource landingAudio;
    public AudioClip[] landingSFX;

    [Header("Jump")]
    public Jump jump;
    public AudioSource jumpAudio;
    public AudioClip[] jumpSFX;

    [Header("Crouch")]
    public Crouch crouch;
    public AudioSource crouchStartAudio, crouchedAudio, crouchEndAudio;
    public AudioClip[] crouchStartSFX, crouchEndSFX;

    [Header("Audio Fade")]
    [Tooltip("Скорость заглушения аудио (единиц в секунду).")]
    public float fadeSpeed = 5f;

    AudioSource[] MovingAudios => new AudioSource[] { stepAudio, runningAudio, crouchedAudio };
    AudioSource lastTargetAudio = null; // Добавьте это поле в класс
    PhotonView photonView;

    void Awake()
    {
        // Setup stuff.
        character = GetComponentInParent<FirstPersonMovement>();
        groundCheck = (transform.parent ?? transform).GetComponentInChildren<GroundCheck>();
        stepAudio = GetOrCreateAudioSource("Step Audio");
        runningAudio = GetOrCreateAudioSource("Running Audio");
        landingAudio = GetOrCreateAudioSource("Landing Audio");

        // Setup jump audio.
        jump = GetComponentInParent<Jump>();
        if (jump)
        {
            jumpAudio = GetOrCreateAudioSource("Jump audio");
        }

        // Setup crouch audio.
        crouch = GetComponentInParent<Crouch>();
        if (crouch)
        {
            crouchStartAudio = GetOrCreateAudioSource("Crouch Start Audio");
            crouchedAudio = GetOrCreateAudioSource("Crouched Audio");
            crouchEndAudio = GetOrCreateAudioSource("Crouch End Audio");
        }

    photonView = GetComponent<PhotonView>();
}

    void OnEnable() { SubscribeToEvents(); }

    void OnDisable() { UnsubscribeToEvents(); }

    void FixedUpdate()
    {
    if (!photonView || !photonView.IsMine) return;

    if (character == null) Debug.LogError("character is NULL");
    if (groundCheck == null) Debug.LogError("groundCheck is NULL");
    if (crouch == null) Debug.LogWarning("crouch is NULL");
    if (jump == null) Debug.LogError("Jump is NULL");
        // Play moving audio if the character is moving and on the ground.
        float velocity = Vector3.Distance(CurrentCharacterPosition, lastCharacterPosition);
        AudioSource targetAudio = null;
        if (velocity >= velocityThreshold && groundCheck && groundCheck.isGrounded)
        {
            if (crouch && crouch.IsCrouched)
            {
                targetAudio = crouchedAudio;
                if (crouchedAudio != null)
                    crouchedAudio.pitch = 0.5f; 
            }
            else if (character.IsRunning)
            {
                targetAudio = runningAudio;
                if (runningAudio != null)
                    runningAudio.pitch = 2f;
            }
            else
            {
                targetAudio = stepAudio;
                if (stepAudio != null)
                    stepAudio.pitch = 1f;
            }
        }

        // Если targetAudio сменился, сразу выставить volume
        if (targetAudio != lastTargetAudio)
        {
            foreach (var audio in MovingAudios)
            {
                if (audio == null) continue;
                audio.volume = (audio == targetAudio) ? 1f : 0f;
                if (audio.volume > 0f && !audio.isPlaying)
                    audio.Play();
                if (audio.volume == 0f && audio.isPlaying)
                    audio.Pause();
            }
        }
        else
        {
            // Плавно регулируем громкость всех аудио
            foreach (var audio in MovingAudios)
            {
                if (audio == null) continue;
                float targetVolume = (audio == targetAudio) ? 1f : 0f;
                audio.volume = Mathf.MoveTowards(audio.volume, targetVolume, fadeSpeed * Time.fixedDeltaTime);
                if (audio.volume > 0f && !audio.isPlaying)
                    audio.Play();
                if (audio.volume == 0f && audio.isPlaying)
                    audio.Pause();
            }
        }

        lastTargetAudio = targetAudio;

        // Remember lastCharacterPosition.
        lastCharacterPosition = CurrentCharacterPosition;
    }


    /// <summary>
    /// Pause all MovingAudios and enforce play on audioToPlay.
    /// </summary>
    /// <param name="audioToPlay">Audio that should be playing.</param>
    void SetPlayingMovingAudio(AudioSource audioToPlay)
    {
        // Устарело, логика теперь в FixedUpdate через volume
    }

    #region Play instant-related audios.
    void PlayLandingAudio()
    {
        if (photonView != null && photonView.IsMine)
            photonView.RPC("RPC_PlayLandingAudio", RpcTarget.All);
    }
    void PlayJumpAudio()
    {
        if (photonView != null && photonView.IsMine)
            photonView.RPC("RPC_PlayJumpAudio", RpcTarget.All);
    }
    void PlayCrouchStartAudio()
    {
        if (photonView != null && photonView.IsMine)
            photonView.RPC("RPC_PlayCrouchStartAudio", RpcTarget.All);
    }
    void PlayCrouchEndAudio()
    {
        if (photonView != null && photonView.IsMine)
            photonView.RPC("RPC_PlayCrouchEndAudio", RpcTarget.All);
    }

    [PunRPC]
    void RPC_PlayLandingAudio() => PlayRandomClip(landingAudio, landingSFX);
    [PunRPC]
    void RPC_PlayJumpAudio() => PlayRandomClip(jumpAudio, jumpSFX);
    [PunRPC]
    void RPC_PlayCrouchStartAudio() => PlayRandomClip(crouchStartAudio, crouchStartSFX);
    [PunRPC]
    void RPC_PlayCrouchEndAudio() => PlayRandomClip(crouchEndAudio, crouchEndSFX);
    #endregion

    #region Subscribe/unsubscribe to events.
    void SubscribeToEvents()
    {
        // PlayLandingAudio when Grounded.
        groundCheck.Grounded += PlayLandingAudio;

        // PlayJumpAudio when Jumped.
        if (jump)
        {
            jump.Jumped += PlayJumpAudio;
        }

        // Play crouch audio on crouch start/end.
        if (crouch)
        {
            crouch.CrouchStart += PlayCrouchStartAudio;
            crouch.CrouchEnd += PlayCrouchEndAudio;
        }
    }

    void UnsubscribeToEvents()
    {
        // Undo PlayLandingAudio when Grounded.
        groundCheck.Grounded -= PlayLandingAudio;

        // Undo PlayJumpAudio when Jumped.
        if (jump)
        {
            jump.Jumped -= PlayJumpAudio;
        }

        // Undo play crouch audio on crouch start/end.
        if (crouch)
        {
            crouch.CrouchStart -= PlayCrouchStartAudio;
            crouch.CrouchEnd -= PlayCrouchEndAudio;
        }
    }
    #endregion

    #region Utility.
    /// <summary>
    /// Get an existing AudioSource from a name or create one if it was not found.
    /// </summary>
    /// <param name="name">Name of the AudioSource to search for.</param>
    /// <returns>The created AudioSource.</returns>
    AudioSource GetOrCreateAudioSource(string name)
    {
        // Try to get the audiosource.
        AudioSource result = System.Array.Find(GetComponentsInChildren<AudioSource>(), a => a.name == name);
        if (result)
            return result;

        // Audiosource does not exist, create it.
        result = new GameObject(name).AddComponent<AudioSource>();
        result.spatialBlend = 1;
        result.playOnAwake = false;
        result.transform.SetParent(transform, false);
        return result;
    }

    static void PlayRandomClip(AudioSource audio, AudioClip[] clips)
    {
        if (!audio || clips.Length <= 0)
            return;

        // Get a random clip. If possible, make sure that it's not the same as the clip that is already on the audiosource.
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clips.Length > 1)
            while (clip == audio.clip)
                clip = clips[Random.Range(0, clips.Length)];

        // Play the clip.
        audio.clip = clip;
        audio.Play();
    }
    #endregion 
}

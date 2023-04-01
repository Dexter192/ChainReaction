using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{

    [SerializeField] private static AudioSource jumpSound;
    public static AudioSource JumpSound { get => jumpSound; }

}

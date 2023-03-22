using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    public static int cherries = 0;
    [SerializeField] private TextMeshProUGUI cherriesText;
    [SerializeField] private AudioSource collectionSoundEffect;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cherry"))
        {
            Destroy(collision.gameObject);
            cherries++;
            cherriesText.text = "Cherries: " + cherries;
            collectionSoundEffect.Play();
        }

    }

    private void Start()
    {
        cherriesText = GameObject.Find("Cherries Text").GetComponent<TextMeshProUGUI>();
        Debug.Log(cherriesText);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivePlayerIndicator : MonoBehaviour
{
    [SerializeField] private GameObject activeIndicator;
    private Playerhandler _playerhandler;

    private void Start()
    {
        _playerhandler = Playerhandler.Instance;        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void SetActive()
    {
        activeIndicator.SetActive(true);
    }

    public void SetColor(Color color)
    {
        activeIndicator.GetComponent<SpriteRenderer>().color = color;
    }

    public Color GetColor()
    {
        return activeIndicator.GetComponent<SpriteRenderer>().color;
    }

    public void SetInactive()
    {
        activeIndicator.SetActive(false);
    }
}

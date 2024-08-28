using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayButton : MonoBehaviour
{
    [SerializeField] private GameObject _tutoGameObject;
    [SerializeField] private Button _closeButton;
    void Start()
    {
        _tutoGameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    public void OnButtonClicked()
    {
        if (_tutoGameObject.activeInHierarchy)
            _tutoGameObject.SetActive(false);
        else
            _tutoGameObject.SetActive(true);
    }

    public void OnTutoClicked()
    {
        _tutoGameObject.SetActive(false);
    }
}

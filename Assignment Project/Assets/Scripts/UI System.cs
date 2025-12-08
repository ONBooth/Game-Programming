using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UISystem : MonoBehaviour
{
    private UIDocument _document;
    private Button _button;
    private Button _quitButton;
    private List<Button> _menuButtons = new List<Button>();
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();

        Time.timeScale = 0f;

        _button = _document.rootVisualElement.Q<Button>("StartGameButton");
        _button.RegisterCallback<ClickEvent>(OnPlayGameClick);

        _quitButton = _document.rootVisualElement.Q<Button>("QuitButton");
        _quitButton.RegisterCallback<ClickEvent>(OnQuitGameClick);

        _menuButtons = _document.rootVisualElement.Query<Button>().ToList();

        for (int i = 0; i < _menuButtons.Count; i++)
        {
            _menuButtons[i].RegisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }

    private void OnDisable()
    {
        _button.UnregisterCallback<ClickEvent>(OnPlayGameClick);
        _quitButton.UnregisterCallback<ClickEvent>(OnQuitGameClick);

        for (int i = 0; i < _menuButtons.Count; i++)
        {
            _menuButtons[i].UnregisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }

    private void OnPlayGameClick(ClickEvent evt)
    {
        Debug.Log("You pressed the Start Button!");
        StartGame();
    }

    private void StartGame()
    {
        
        _document.rootVisualElement.style.display = DisplayStyle.None;
        
        
        Time.timeScale = 1f;
    }

    private void OnAllButtonsClick(ClickEvent evt)
    {
        _audioSource.Play();
    }

    private void OnQuitGameClick(ClickEvent evt)
    {
        Debug.Log("Quitting game...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
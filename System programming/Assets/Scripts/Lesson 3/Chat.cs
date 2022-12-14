using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    #region Fields

    [SerializeField] private TextMeshProUGUI _textObject;
    [SerializeField] private Scrollbar _scrollbar;
    
    private List<string> _messages = new List<string>();

    #endregion

    #region Unity events

    private void Start()
    {
        _scrollbar
            .onValueChanged
            .AddListener(_ => UpdateText());
    }

    private void OnDestroy()
    {
        _scrollbar
            .onValueChanged
            .RemoveAllListeners();
    }

    #endregion

    #region Methods

    public void ReceiveMessage(object message)
    {
        _messages.Add(message.ToString());
        
        var value = (_messages.Count - 1) * _scrollbar.value;

        _scrollbar.value = Mathf.Clamp(value, 0, 1);
        
        UpdateText();
    }
    
    private void UpdateText()
    {
        var text = "";
        
        var index = (int)(_messages.Count * _scrollbar.value);

        for (int i = index; i < _messages.Count; i++)
        {
            text += _messages[i] + "\n";
        };

        _textObject.text = text;
    }

    #endregion
}
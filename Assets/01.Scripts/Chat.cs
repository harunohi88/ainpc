using UnityEngine;
using TMPro;

public class Chat : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _chatText;

    public void DisplayMessage(string text)
    {
        _chatText.text = text;
    }
}

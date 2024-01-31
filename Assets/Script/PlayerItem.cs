using TMPro;
using UnityEngine;

[System.Serializable]
public class PlayerItem : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;

    public void SetPlayerInfo(string playerName)
    {
        this.playerName.SetText(playerName);
    }
}
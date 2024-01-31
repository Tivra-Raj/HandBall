using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MasterClient : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI CountDownText;
    public float MaxTime;
    public float CurrentTime;

    private PhotonView masterPhotonView;

    [SerializeField] private GameObject GameOverUI;
    [SerializeField] private TextMeshProUGUI teamWontext;
    [SerializeField] private TextMeshProUGUI goalText;

    void Start()
    {
        PhotonNetwork.SetMasterClient(PhotonNetwork.MasterClient);
        CurrentTime = MaxTime;
        masterPhotonView = PhotonView.Get(this);
        GameOverUI.gameObject.SetActive(false);

        SendTimer();
    }

    private void Update()
    {
        GameOver();  
    }

    private void SendTimer()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(GameplayTImer());
        }
    }


    public IEnumerator GameplayTImer()
    {
        while (CurrentTime >= 0)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(CurrentTime);
            string timeString = timeSpan.ToString(@"mm\:ss");
            masterPhotonView.RPC(nameof(SetTimeText), RpcTarget.All, timeString);

            CurrentTime -= Time.deltaTime;
            CurrentTime--;
            yield return new WaitForSeconds(1);
        }      
    }

    [PunRPC]
    private void SetTimeText(string text)
    {
        CountDownText.text = text;
    }

    private void GameOver()
    {
        if(CurrentTime <= 0)
        {
            masterPhotonView.RPC(nameof(SetGameStatus), RpcTarget.All, GameStatusEnum.GameOver);

            masterPhotonView.RPC(nameof(SetGameOverUIStatus), RpcTarget.All, true);

            if(ScoreController.Instance.GetRedTeamScore() > ScoreController.Instance.GetBlueTeamScore())
            {
                masterPhotonView.RPC(nameof(SetTeamWonValue), RpcTarget.All, "Red Team Won", color.Red);
            }
            else if(ScoreController.Instance.GetRedTeamScore() < ScoreController.Instance.GetBlueTeamScore())
            {
                masterPhotonView.RPC(nameof(SetTeamWonValue), RpcTarget.All, "Blue Team Won", color.Blue);
            }
            else
            {
                masterPhotonView.RPC(nameof(SetTeamWonValue), RpcTarget.All, "Game Draw", color.Green);
            }
        }
    }

    public PhotonView GetMasterClientPhotonView() => masterPhotonView;

    [PunRPC]
    public void SetGameStatus(GameStatusEnum gameStatus)
    {
        GameStatus.gameStatus = gameStatus;
    }

    [PunRPC]
    private void SetGameOverUIStatus(bool isActive)
    {
        GameOverUI.gameObject.SetActive(isActive);
    }

    [PunRPC]
    private void SetTeamWonValue(string value, color teamColor)
    {
        teamWontext.SetText(value);
        teamWontext.color = ColorID.GetColorFromEnum(teamColor);
    }

    [PunRPC]
    public void SetGoalText(bool isActive, color color)
    {
        goalText.gameObject.SetActive(isActive);
        goalText.color = ColorID.GetColorFromEnum(color);
    }
}

public static class ColorID
{
    public static Color GetColorFromEnum(color colorEnum)
    {
        switch (colorEnum)
        {
            case color.Red:
                return Color.red;

            case color.Blue:
                return Color.blue;

            case color.Green:
                return Color.green;

            default: return Color.white;
        }
    }
}

public enum color
{
    None,
    Red,
    Blue,
    Green,  
}
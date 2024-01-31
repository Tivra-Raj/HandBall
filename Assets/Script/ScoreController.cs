using Photon.Pun;
using TMPro;
using UnityEngine;

public class ScoreController : MonoBehaviourPun
{
    private static ScoreController instance;
    public static ScoreController Instance {  get { return instance; } } 

    [SerializeField] private TextMeshProUGUI redTeamScore;
    [SerializeField] private TextMeshProUGUI blueTeamScore;

    [SerializeField] private int redScore;
    [SerializeField] private int blueScore;

    PhotonView scorePhotonView;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);

        scorePhotonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void IncreaseRedTeamScore(int value)
    {
        redScore += value;
        RefreshScore();
    }

    [PunRPC]
    public void IncreaseBlueTeamScore(int value)
    {
        blueScore += value;
        RefreshScore();
    }

    public void RefreshScore()
    {
        redTeamScore.SetText(redScore.ToString("0"));
        blueTeamScore.SetText(blueScore.ToString("0"));
    }

    public PhotonView GetScorePhotonView() => scorePhotonView;  

    public int GetRedTeamScore() => redScore;

    public int GetBlueTeamScore() => blueScore;
}
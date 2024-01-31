using UnityEngine;
using Photon.Pun;
using System.Collections;

public class ObjectSpawner : MonoBehaviourPun
{
    public GameObject redPlayerPrefab;
    [SerializeField] private GameObject bluePlayerPrefab;
    public GameObject BallPrefab;

    [SerializeField] private Transform[] redPlayerPosition;
    [SerializeField] private Transform[] bluePlayerPosition;

    [SerializeField] private MasterClient masterClient;

    private Transform localPlayerTransform;

    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            for(int i = 0; i < LocalTeamData.redPlayerList.Count; i++)
            {
                photonView.RPC(nameof(SpawnRedTeamPlayer), LocalTeamData.redPlayerList[i], redPlayerPosition[i].position);
            }

            for (int i = 0; i < LocalTeamData.bluePlayerList.Count; i++)
            {
                photonView.RPC(nameof(SpawnBlueTeamPlayer), LocalTeamData.bluePlayerList[i], bluePlayerPosition[i].position);
            }
        }

        SpawnBall();
    }

    [PunRPC]
    private void SpawnRedTeamPlayer(Vector3 position)
    {
        PlayerController newPlayer = PhotonNetwork.Instantiate(redPlayerPrefab.name, position, Quaternion.identity).GetComponent<PlayerController>();
        newPlayer.SetObjectSpawner(this);
        localPlayerTransform = newPlayer.transform;
    }

    [PunRPC]
    private void SpawnBlueTeamPlayer(Vector3 position)
    {
        PlayerController newPlayer = PhotonNetwork.Instantiate(bluePlayerPrefab.name, position, Quaternion.identity).GetComponent<PlayerController>();
        newPlayer.SetObjectSpawner(this);
        localPlayerTransform = newPlayer.transform;
    }

    public void ResetPositionOnMasterClient()
    {
        masterClient.GetMasterClientPhotonView().RPC(nameof(masterClient.SetGameStatus), RpcTarget.All, GameStatusEnum.GamePaused);
        photonView.RPC(nameof(ResetPlayerPositionOnNetwork), RpcTarget.MasterClient);
        Invoke(nameof(SetGameStatustInProgess), 2f);
    }

    private void SetGameStatustInProgess()
    {
        masterClient.GetMasterClientPhotonView().RPC(nameof(masterClient.SetGameStatus), RpcTarget.All, GameStatusEnum.GameInprogress);
    }

    public void ActivateGoalText(color color)
    {
        masterClient.GetMasterClientPhotonView().RPC(nameof(masterClient.SetGoalText), RpcTarget.All, true, color);
        StartCoroutine(DeactivateGoalText(color));
    }

    private IEnumerator DeactivateGoalText(color color)
    {
        yield return new WaitForSeconds(2f);
        masterClient.GetMasterClientPhotonView().RPC(nameof(masterClient.SetGoalText), RpcTarget.All, false, color);
    }

    [PunRPC]
    public void ResetPlayerPositionOnNetwork()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < LocalTeamData.redPlayerList.Count; i++)
            {
                photonView.RPC(nameof(ResetPlayerPosition), LocalTeamData.redPlayerList[i], redPlayerPosition[i].position);
            }

            for (int i = 0; i < LocalTeamData.bluePlayerList.Count; i++)
            {
                photonView.RPC(nameof(ResetPlayerPosition), LocalTeamData.bluePlayerList[i], bluePlayerPosition[i].position);
            }
        }
    }

    [PunRPC]
    private void ResetPlayerPosition(Vector3 position)
    {
        localPlayerTransform.position = position;
    }

    private void SpawnBall()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(BallPrefab.name, new Vector2(0.1f, -0.5f), Quaternion.identity);
        }
    }
}
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using System.Collections.Generic;

public class LobbyService : MonoBehaviourPunCallbacks
{
    [Header("Lobby Panel Manager")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject roomPanel;

    [Header("Lobby Input")]
    [SerializeField] private TMP_InputField createRoomInput;
    [SerializeField] private TMP_InputField joinRoomInput;

    [Header("Lobby Button")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    [Header("Room Panel Manager")]
    [SerializeField] private TMP_Text roomNameText;

    [Header("Total Member List")]
     private List<PlayerItem> playerItemsList = new List<PlayerItem>();
    [SerializeField] private PlayerItem playerItemPrefab;
    [SerializeField] private Transform playerItemHolder;

    [Header("Red Team Member List")]
     private List<PlayerItem> redTeamplayerItemsList = new List<PlayerItem>();
    [SerializeField] private Transform redTeamplayerItemHolder;
    [SerializeField] private Button redTeamJoinButton;

    [Header("Blue Team Member List")]
     private List<PlayerItem> blueTeamplayerItemsList = new List<PlayerItem>();
    [SerializeField] private Transform blueTeamplayerItemHolder;
    [SerializeField] private Button blueTeamJoinButton;

    private List<Player> noTeamPlayerList = new List<Player>();

    [SerializeField] private Button StartGameButton;

    private void Start()
    {
        ManageLobbyPanelActiveStatus(true, false);

        StartGameButton.GetComponent<Button>().enabled = false;

        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);

        redTeamJoinButton.onClick.AddListener(UpdatePlayerToRedTeamOnNetwork);
        blueTeamJoinButton.onClick.AddListener(UpdatePlayerToBlueTeamOnNetwork);
        StartGameButton.onClick.AddListener(LoadGame);
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            StartGameButton.gameObject.SetActive(false);
        }
        else
        {
            StartGameButton.gameObject.SetActive(true);
        }


        if (PhotonNetwork.IsMasterClient && LocalTeamData.redPlayerList.Count >= 1 && LocalTeamData.bluePlayerList.Count >= 1)
            StartGameButton.GetComponent<Button>().enabled = true;
        else
            StartGameButton.GetComponent <Button>().enabled = false;
    }

    private void ManageLobbyPanelActiveStatus(bool loobyPanelStatus, bool roomPanelStatus)
    {
        lobbyPanel.SetActive(loobyPanelStatus);
        roomPanel.SetActive(roomPanelStatus);
    }

    private void CreateRoom()
    {
        if(createRoomInput.text.Length >= 1)
        {
            PhotonNetwork.CreateRoom(createRoomInput.text, new RoomOptions(){ MaxPlayers = 10});
        }
    }

    public void JoinRoom()
    {
        if(joinRoomInput.text.Length >= 1)
        {
            PhotonNetwork.JoinRoom(joinRoomInput.text);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        ManageLobbyPanelActiveStatus(false, true);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        photonView.RPC(nameof(AddPlayerToNoTeamList), RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public void LoadGame()
    {
        PhotonNetwork.LoadLevel(2);
    }

    private void UpdatePlayerList(List<PlayerItem> playerItemsList, List<Player> TeamPlayerList, Transform playerItemHolder)
    {
        if (playerItemsList.Count > 0)
        {
            foreach (PlayerItem playerItem in playerItemsList)
            {
                if(playerItem != null)
                    Destroy(playerItem.gameObject);
            }
        }
        playerItemsList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
            return;

        string listName = "";

        foreach (Player player in TeamPlayerList)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemHolder);
            newPlayerItem.SetPlayerInfo(player.NickName);
            listName += $"{player.NickName}, ";
            playerItemsList.Add(newPlayerItem);
        }
    }

    [PunRPC]
    private void UpdateTeamPlayerList(Player player, TeamName teamName, TeamName currentTeamName)
    {
        if(player == PhotonNetwork.LocalPlayer)
        {
            LocalTeamData.teamID = teamName;
        }

        switch (teamName)
        {
            case TeamName.RedTeam : AddPlayerToRedTeamList(player, currentTeamName); break;

            case TeamName.BlueTeam : AddPlayerToBlueTeamList(player, currentTeamName); break;
        }
    }

    private void UpdatePlayerToRedTeamOnNetwork()
    {
        if (LocalTeamData.teamID == TeamName.RedTeam)
            return;

        photonView.RPC(nameof(UpdateTeamPlayerList), RpcTarget.All, PhotonNetwork.LocalPlayer, TeamName.RedTeam, LocalTeamData.teamID);
    }

    private void UpdatePlayerToBlueTeamOnNetwork()
    {
        if (LocalTeamData.teamID == TeamName.BlueTeam)
            return;

        photonView.RPC(nameof(UpdateTeamPlayerList), RpcTarget.All, PhotonNetwork.LocalPlayer, TeamName.BlueTeam, LocalTeamData.teamID);
    }

    [PunRPC]
    private void AddPlayerToNoTeamList(Player player)
    {
        noTeamPlayerList.Add(player);
        UpdatePlayerList(playerItemsList, noTeamPlayerList, playerItemHolder);
    }
    
    [PunRPC]
    private void AddPlayerToRedTeamList(Player player, TeamName teamName)
    {
        LocalTeamData.redPlayerList.Add(player);
        UpdatePlayerList(redTeamplayerItemsList, LocalTeamData.redPlayerList, redTeamplayerItemHolder);

        switch(teamName)
        {
            case TeamName.none:
                                if(noTeamPlayerList.Count > 0)
                                    noTeamPlayerList.Remove(player);
                                UpdatePlayerList(playerItemsList, noTeamPlayerList, playerItemHolder);
                                break;

            case TeamName.RedTeam: Debug.LogWarning("Not Possible");
                                   break;

            case TeamName.BlueTeam:
                if (LocalTeamData.bluePlayerList.Count > 0)
                    LocalTeamData.bluePlayerList.Remove(player);
                                    UpdatePlayerList(blueTeamplayerItemsList, LocalTeamData.bluePlayerList, blueTeamplayerItemHolder);
                                    break;  
        }
    }

    [PunRPC]
    private void AddPlayerToBlueTeamList(Player player, TeamName teamName)
    {
        LocalTeamData.bluePlayerList.Add(player);
        UpdatePlayerList(blueTeamplayerItemsList, LocalTeamData.bluePlayerList, blueTeamplayerItemHolder);

        switch (teamName)
        {
            case TeamName.none:
                if (noTeamPlayerList.Count > 0)
                    noTeamPlayerList.Remove(player);
                UpdatePlayerList(playerItemsList, noTeamPlayerList, playerItemHolder);
                break;

            case TeamName.RedTeam:
                if (LocalTeamData.redPlayerList.Count > 0)
                    LocalTeamData.redPlayerList.Remove(player);
                UpdatePlayerList(redTeamplayerItemsList, LocalTeamData.redPlayerList, redTeamplayerItemHolder); 
                break;

            case TeamName.BlueTeam:
                Debug.LogWarning("Not Possible");
                break;
        }
    }

    [PunRPC]
    private void RemovePlayerFromNoTeamList(Player player, TeamName teamName)
    {
        noTeamPlayerList.Remove(player);
        UpdatePlayerList(playerItemsList, noTeamPlayerList, playerItemHolder);
    }

    [PunRPC]
    private void RemovePlayerFromRedTeamList(Player player, TeamName teamName)
    {
        LocalTeamData.redPlayerList.Remove(player);
        UpdatePlayerList(redTeamplayerItemsList, LocalTeamData.redPlayerList, redTeamplayerItemHolder);
    }

    [PunRPC]
    private void RemovePlayerFromBlueTeamList(Player player, TeamName teamName)
    {
        LocalTeamData.bluePlayerList.Remove(player);
        UpdatePlayerList(blueTeamplayerItemsList, LocalTeamData.bluePlayerList, blueTeamplayerItemHolder);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        
        switch(LocalTeamData.teamID)
        {
            case TeamName.none: photonView.RPC(nameof(AddPlayerToNoTeamList), newPlayer, PhotonNetwork.LocalPlayer);
                                break;

            case TeamName.RedTeam : photonView.RPC(nameof(AddPlayerToRedTeamList), newPlayer, PhotonNetwork.LocalPlayer, LocalTeamData.teamID);
                                    break;

            case TeamName.BlueTeam : photonView.RPC(nameof(AddPlayerToBlueTeamList), newPlayer, PhotonNetwork.LocalPlayer, LocalTeamData.teamID);
                                     break;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        //UpdatePlayerList();
    }
}

public enum TeamName
{
    none,
    RedTeam,
    BlueTeam,
}
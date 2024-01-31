using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private PhotonView photonView;
    [SerializeField] public Transform BallPosition;
    public GameObject Ball;
    [SerializeField] private GameObject spriteBody;
    private ObjectSpawner spawner;

    private bool facingRight;
    private bool facingUp;

    [SerializeField] private TextMeshProUGUI playerName;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
        Ball.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerName.text = photonView.IsMine?PhotonNetwork.LocalPlayer.NickName : photonView.Owner.NickName;
    }

    void Update()
    {
        if(photonView.IsMine)
        {
            if (GameStatus.gameStatus == GameStatusEnum.GameOver || GameStatus.gameStatus == GameStatusEnum.GamePaused) 
            {
                rb.velocity = Vector3.zero;
                return; 
            }

            Movement();
        }
    }

    private void Movement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");     

        Vector2 movement = new Vector2(horizontalInput, verticalInput);
        movement.Normalize();

        rb.velocity = new Vector2(movement.x * moveSpeed, movement.y * moveSpeed);

        Vector3 scale = spriteBody.transform.localScale;
        if (facingRight && horizontalInput < 0)
        {
            scale.x = -1f * Mathf.Abs(scale.x);
            facingRight = false;
        }
        else if (!facingRight && horizontalInput > 0)
        {
            scale.x = Mathf.Abs(scale.x);
            facingRight = true;
        }
        spriteBody.transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine && Ball.gameObject.activeSelf)
        {
            if (collision.gameObject.GetComponent<PlayerController>() != null)
            {
                photonView.RPC(nameof(UpdateBallOnPlayer), RpcTarget.All, false);

                PhotonView collidedPhotonView = collision.gameObject.GetComponent<PhotonView>();

                if (collidedPhotonView != null)
                {
                    collidedPhotonView.RPC(nameof(UpdateBallOnPlayer), RpcTarget.All, true);
                }
            }

            if (collision.gameObject.CompareTag("Goal"))
            {
                photonView.RPC(nameof(UpdateBallOnPlayer), RpcTarget.All, false);    

                BallController ballController = BallController.Instance;
                ballController.GetBallPhotonView().RPC(nameof(ballController.UpdateBallStatus), RpcTarget.All, true);

                ScoreController scoreController = ScoreController.Instance;
                if(LocalTeamData.teamID == TeamName.RedTeam && collision.gameObject.GetComponent<GoalController>().GoalId == GoalPost.BlueTeamGoalPost)
                {
                    spawner.ResetPositionOnMasterClient();
                    spawner.ActivateGoalText(color.Red);
                    scoreController.GetScorePhotonView().RPC(nameof(scoreController.IncreaseRedTeamScore), RpcTarget.All, 1);
                }
                else if (LocalTeamData.teamID == TeamName.BlueTeam && collision.gameObject.GetComponent<GoalController>().GoalId == GoalPost.RedTeamGoalPost)
                {
                    spawner.ResetPositionOnMasterClient();
                    spawner.ActivateGoalText(color.Blue);
                    scoreController.GetScorePhotonView().RPC(nameof(scoreController.IncreaseBlueTeamScore), RpcTarget.All, 1);
                }
            }
        }
    }

    [PunRPC]
    public void UpdateBallOnPlayer(bool isActive)
    {
        Ball.gameObject.SetActive(isActive);
    }

    public void SetObjectSpawner(ObjectSpawner spawner)
    {
        this.spawner = spawner;
    }
}
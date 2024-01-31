using Photon.Pun;
using UnityEngine;

public class BallController : MonoBehaviourPunCallbacks
{
    private PhotonView ballPhotonView;

    private static BallController instance;
    public static BallController Instance {  get { return instance; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    private void Start()
    {
        ballPhotonView = GetComponent<PhotonView>();
    }

    public PhotonView GetBallPhotonView()
    {
        return ballPhotonView;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(ballPhotonView.IsMine)
        {
            PlayerController collidedobject = collision.gameObject.GetComponent<PlayerController>();

            if (collidedobject != null)
            {
                ballPhotonView.RPC(nameof(UpdateBallStatus), RpcTarget.All, false);

                PhotonView collidedPhotonView = collision.gameObject.GetComponent<PhotonView>();

                if (collidedPhotonView != null)
                {    
                    collidedPhotonView.RPC(nameof(collidedobject.UpdateBallOnPlayer), RpcTarget.All, true);
                }
            }
        }
    }

    [PunRPC]
    public void UpdateBallStatus(bool isActive)
    {   
        this.gameObject.SetActive(isActive);
    }
}
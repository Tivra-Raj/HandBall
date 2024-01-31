using UnityEngine;

public class GoalController : MonoBehaviour
{
    public GoalPost GoalId;
}

public enum GoalPost
{
    None,
    RedTeamGoalPost,
    BlueTeamGoalPost,
}
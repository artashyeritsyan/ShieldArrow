using UnityEngine;

[CreateAssetMenu(fileName = "TeamsColors", menuName = "Scriptable Objects/TeamsColors")]
public class TeamsColors : ScriptableObject
{
    [SerializeField] public static Color enemyColor;
    //[SerializeField] Color playerTeam;
    [SerializeField] static Color[] playersColors;
    [SerializeField] static Color defaultColor = Color.white;

    public static Color GetTeamColor(Team team, PlayerClass playerClass = 0)
    {
        if (team == Team.Enemy)
        {
            return enemyColor;
        }
        else if (team == Team.Player)
        {
            return playersColors[(int)playerClass];
        }
        else
        {
            return defaultColor;
        }
    }

}

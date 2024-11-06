/// <summary>
/// All possible turn types.
/// Neverset is used as a default for function args and debugging.
/// </summary>
public enum TurnType{Player, AI, Event, NeverSet};


/// <summary>
/// System class that handles turns.
/// </summary>
public class TurnBasedSystem : MonoBehaviour
{
    static private TurnBasedSystem S;
    
    /// Type of the active turn.
    TurnType currentTurn;

    /// In-game text displaying remaining turns.
    public TextMeshProUGUI txt;

    /// Combined number of rounds until defeat.
    [Header("Number of rounds until player defeat.")]
    public int roundsUntilDefeat = 10;

    /// <summary>
    /// Tracking # of turns of each type taken, as well as total turns.
    /// </summary>
    private int totalTurnCount = 0;
    private int playerTurnCount = 0;
    private int aiTurnCount = 0;
    private int eventTurnCount = 0;

    /// <summary>
    /// Lists of skills to destroy after X turns, set in individual skills.
    /// First add to destroyable list, then handle deletion in rdyToDestroy.
    /// </summary>
    List<DestroyAfterRounds> destroyableBarriers = new List<DestroyAfterRounds>();
    List<DestroyAfterRounds> rdyToDestroyBarriers = new List<DestroyAfterRounds>(); 
    List<DestroyAfterRounds> destroyableAOEs = new List<DestroyAfterRounds>();
    List<DestroyAfterRounds> rdyToDestroyAOEs = new List<DestroyAfterRounds>(); 
    
    // Start turn type off as player and display turns remaining.
    void Start()
    {
        S = this;
        S.txt.text = "Turns Left: " + (S.roundsUntilDefeat/2 - S.aiTurnCount);
        currentTurn = TurnType.Player;
    }
    
    /// Returns true if active turn is AI turn.
    static public bool IS_AI_TURN()
    {
        return currentTurn == TurnType.AI;
    }
    
    /// Returns true if active turn is Player turn.
    static public bool IS_PLAYER_TURN()
    {
        return currentTurn == TurnType.Player;
    }


    /// <summary>
    /// Gets turn count of specified turn type.
    /// Defaults to total turn count if no arguments provided.
    /// </summary>
    /// <param name="tt">
    /// Type of turn you want the count of. Leave empty for total combined turns.
    /// </param>
    /// <returns>
    /// Int count of provided turn type.
    /// </returns>
    static public int GET_TURN_COUNT(TurnType tt = TurnType.NeverSet) 
    {
        switch(tt)
        {
            case TurnType.Player:
            {
                return S.playerTurnCount;
            }

            case TurnType.AI:
            {
                return S.aiTurnCount;
            }

            case TurnType.Event:
            {
            return S.eventTurnCount;
            {

            default:
            {
            return S.totalTurnCount;
            }
        }
    }


    /// <summary>
    /// Adds respective turn taken to data stored in this system class.
    /// </summary>
    /// <param name="tt">
    /// Type of turn taken.
    /// </param>
    static public void ADD_TURN(TurnType tt)
    {
        S.totalTurnCount += 1;

        switch(tt)
        {
            case TurnType.Player:
            {
            S.ResetAOEFear();
            currentTurn = TurnType.Player; 
            S.playerTurnCount ++;
            break;
            }

            case TurnType.AI:
            {
            S.TriggerAOEFear();
            currentTurn = TurnType.AI;
            S.aiTurnCount ++;
            break;
            }

            case TurnType.Event:
            {
            S.eventTurnCount ++;
            break;
            }

            default:
            {
            break;
            }

        }

        // Update the turns remaining in-game text.
        S.txt.text = "Turns Left: " + (S.roundsUntilDefeat/2 - S.aiTurnCount);

        /*
        * Each time a turn is taken, check if any skill has expired.
        * If it has, we destroy the skill and add the DestroyAfterRounds
        * instance to a list for removal.
        */
        foreach(DestroyAfterRounds destroyable in S.destroyableBarriers)
        {
            destroyable.CheckBarrierDestroy();
        }
        // Once added to destroyable list, destroy them to clean list. must be done this way. 
        foreach(DestroyAfterRounds destroy in S.rdyToDestroyBarriers)
        {
            S.destroyableBarriers.Remove(destroy);
        }

        foreach(DestroyAfterRounds destroyable in S.destroyableAOEs)
        {
            destroyable.CheckAOEDestroy();
        }
        
        foreach(DestroyAfterRounds destroy in S.rdyToDestroyAOEs)
        {
            S.destroyableAOEs.Remove(destroy);
        }

    }


    /// <summary>
    /// Check if player has reached the ending round.
    /// If so, initiate the game over screen.
    /// </summary>
    static public void ROUND_CHECK()
    {
        if(S.totalTurnCount == S.roundsUntilDefeat && IsPlayerTurn())
        {
            GameManager.Instance.Lose("Summon opening missed");
        }
    }


    /// <summary>
    /// Nested class that handles the deletion of skills after X number of rounds.
    /// </summary>
    public class DestroyAfterRounds
    {
        /// Player round to destroy this skill on.
        int destroyOnRound;

        /// Associated object of skill.
        GameObject obToDestroy;
        
        /// Associated type of wall for when destroying walls.
        WallType wallType;


        /// <summary>
        /// Constructor for destroying barriers after rounds.
        /// </summary>
        /// <param name="rounds">
        /// Number of PLAYER turns until this barrier is destroyed.
        /// </param>
        /// <param name="obj">
        /// Wall object to be destroyed. i.e. the visual wall.
        /// </param>
        /// <param name="wT">
        /// Type of wall to be destroyed. i.e. the coded wall the AI sees.
        /// </param>
        public DestroyAfterRounds(int rounds, GameObject obj, WallType wT)
        {
            destroyOnRound = rounds + S.playerTurnCount;
            obToDestroy = obj;
            wallType = wT;
            S.destroyableBarriers.Add(this);
        }


        /// <summary>
        /// Constructor for destroying AOEs after rounds.
        /// </summary>
        /// <param name="rounds">
        /// Number of PLAYER turns until this AOE is destroyed.
        /// </param>
        /// <param name="obj">
        ///  AOE object to be destroyed.
        /// </param>
        public DestroyAfterRounds(int rounds, GameObject obj)
        {
            destroyOnRound = rounds + S.playerTurnCount;
            obToDestroy = obj;
            S.destroyableAOEs.Add(this);
        }


        /// <summary>
        /// Check if an instance of DestroyAfterRounds is ready to
        /// destroy its associated wall object.
        /// </summary>
        public void CheckBarrierDestroy()
        {
            if(destroyOnRound == S.playerTurnCount)
            {
                MazeCellObject cell = obToDestroy.GetComponentInParent<MazeCellObject>();
                MazeGenerator.Instance.SetWallInternalState(cell.GetXCoord(), cell.GetYCoord(), wallType, false); // Deactivate this wall internally in the maze array
                obToDestroy.SetActive(false); // Set the active state so it may be reactivated later.
                S.rdyToDestroyBarriers.Add(this);
            }
        }


        /// <summary>
        /// Checks if an instance of DestroyAfterRounds is ready to
        /// destroy its associated AOE object.
        /// </summary>
        public void CheckAOEDestroy()
        {
            if(destroyOnRound == S.playerTurnCount)
            {
                S.rdyToDestroyAOEs.Add(this);
                Destroy(obToDestroy);
            }
        }
    }
}

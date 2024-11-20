/**
* NOTE: While this task was primarily worked on by Jaylon, other programmers contributed to this code!
*/

/// This refers to the required collider trigger area detecting the player.
[RequireComponent(typeof(Collider2D))]

/// <summary>
/// Class handling the behavior of grate.
/// I.e. Passing through grate when in liquid & gas forms, and becoming a 
/// barrier/platform in solid form.
/// <summary>
public class Grate : PhysInteraction 
{
    /// Grate's box collider the player will be colliding with.
    private BoxCollider2D coll;

    /// Parent object where the Grate collider & layer resides.
    private GameObject parentObj;

    /// Layer that ignores the player controller raycast.
    int ignoreRayCastLayer = 2;
    
    /// Layer that detects the player controller raycast.
    int detectRayCastLayer = 0;


    public override void OnInit()
    {
        coll = gameObject.GetComponentInParent<BoxCollider2D>();
        parentObj = gameObject.transform.parent.gameObject;
    }


    /// <summary>
    /// Update collision between the grate & player based on the player's state.
    /// </summary>
    /// <param name="collider">
    /// Collider of object entering the grate trigger area.
    /// </param>
    private void UpdateCollider(Collider2D collider)
    {
        if (!playerState) return;

        Collider2D playerCollider = playerState.gameObject.GetComponent<Collider2D>();
        if (!playerCollider) return;

        /*
        * If state is not solid, ignore the collision and player raycast.
        * Otherwise, reset the layer for platforming.
        */
        Physics2D.IgnoreCollision(playerCollider, coll, playerState.currentState != PlayerStateHandler.States.solid);

        if (playerState.currentState != PlayerStateHandler.States.solid) 
        {
            parentObj.layer = ignoreRayCastLayer; 
        }
        else
        {
            parentObj.layer = detectRayCastLayer;
        }
    }


    /// <summary>
    /// Update collision between player & grate when player enters trigger area.
    /// </summary>
    /// <param name="collider">
    /// Collider of object entering the grate trigger area.
    /// </param>
    private void OnTriggerEnter2D(Collider2D collider)
    {
        UpdateCollider(collider);
    }


    /// <summary>
    /// Update collision between player & grate while player is in trigger area.
    /// Necessary as the player may change states while in the trigger area, 
    /// requiring a collision update.
    /// </summary>
    /// <param name="collider">
    /// Collider of object in the grate trigger area.
    /// </param>
    private void OnTriggerStay2D(Collider2D collider)
    {
        UpdateCollider(collider);
    }


    /// <summary>
    /// Update collision between player & grate when player exits trigger area.
    /// Based on last known player state.
    /// </summary>
    /// <param name="collider">
    /// Collider of object exiting the grate trigger area.
    /// </param>
    private void OnTriggerExit2D(Collider2D collider)
    {
        UpdateCollider(collider);
    }
}

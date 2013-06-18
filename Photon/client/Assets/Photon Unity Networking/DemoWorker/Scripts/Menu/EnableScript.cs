using UnityEngine;
using System.Collections;

/// <summary>
/// This script makes sure that only the right gameobject with scripts is active.
/// If the scripts are disabled they still receive callbacks, this causes problems.
/// E.g. the disabled mainmenu would receive OnJoinedRoom while having just switched to the game scene.
/// Disabling the gameobjects prevents this problem.
/// </summary>
public class EnableScript : MonoBehaviour
{
    public GameObject game;
    public GameObject mainMenu;

    /// <summary>
    /// Enable one GO and remove the other
    /// </summary>
    public void Awake()
    {
        if (PhotonNetwork.room != null)
        {
            Destroy(this.mainMenu);
            this.game.active = true;
        }
        else
        {
            Destroy(this.game);
            this.mainMenu.active = true;
        }
        
        // now this script is not needed anymore. destroy it and it's gameobject
        Destroy(this.gameObject);
    }


}

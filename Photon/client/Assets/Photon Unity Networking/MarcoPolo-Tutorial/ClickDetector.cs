using System;
using UnityEngine;

public class ClickDetector : MonoBehaviour
{

	void Update()
    {
        // if this player is not "it", the player can't tag anyone, so don't do anything on collision
        if (PhotonNetwork.player.ID != GameLogic.playerWhoIsIt)
        {
            return;
        }

        if (Input.GetButton("Fire1"))
        {
            GameObject goPointedAt = RaycastObject(Input.mousePosition);

            if (goPointedAt != null && goPointedAt != this.gameObject && goPointedAt.name.Equals("monsterprefab(Clone)", StringComparison.OrdinalIgnoreCase))
            {
                PhotonView rootView = goPointedAt.transform.root.GetComponent<PhotonView>();
                GameLogic.TagPlayer(rootView.owner.ID);
            }
        }
	}

    private GameObject RaycastObject(Vector2 screenPos)
    {
        RaycastHit info;
        if (Physics.Raycast(Camera.mainCamera.ScreenPointToRay(screenPos), out info, 200))
        {
            return info.collider.gameObject;
        }

        return null;
    }
}

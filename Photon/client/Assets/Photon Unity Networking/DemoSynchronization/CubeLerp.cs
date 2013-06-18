using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class CubeLerp : Photon.MonoBehaviour
{
    Vector3 latestCorrectPos = Vector3.zero;
    Quaternion latestCorrectRot = Quaternion.identity;

    public void Awake()
    {
        if (photonView.isMine)
        {
            this.enabled = false;//Only enable inter/extrapol for remote players
        }

        latestCorrectPos = transform.position;
        latestCorrectRot = Quaternion.identity;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Always send transform (depending on reliability of the network view)
        if (stream.isWriting)
        {
            Vector3 pos = transform.localPosition;
            Quaternion rot = transform.localRotation;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
        }
        // When receiving, buffer the information
        else
        {
            // Receive latest state information
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);

            latestCorrectPos = pos;
        }
    }

    // This only runs where the component is enabled, which is only on remote peers (server/clients)
    public void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, latestCorrectPos, Time.deltaTime * 20);
        transform.rotation = latestCorrectRot;
    }
}

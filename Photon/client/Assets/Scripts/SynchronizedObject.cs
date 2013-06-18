using UnityEngine;
using System.Collections;

/// <summary>
/// Any non-static object in the game must have a component inherited from this class. This
/// class is the "root network component" of a game object.
/// 
/// The attached object can be owned by any client, and is not aware of concepts such as damage-taking
/// or walking around. Its inherited classes, however, can be.
/// 
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class SynchronizedObject : Photon.MonoBehaviour
{
	/// <summary>
	/// Cached transform.
	/// </summary>
	protected Transform myTransform;
	
	/// <summary>
	/// Cached rigid body.
	/// </summary>
	protected Rigidbody myRigidbody;
	
	/// <summary>
	/// Synchronization: The correct object position according to this object's owner.
	/// </summary>
   	protected Vector3 correctPos = Vector3.zero; // We lerp towards this
	
	/// <summary>
	/// Synchronization: The correct object rotation according to this object's owner.
	/// </summary>
    protected Quaternion correctRot = Quaternion.identity; // We lerp towards this
		
	#region MonoBehavior
	
	protected virtual void OnEnable()
	{
		photonView.observed = this;
		photonView.synchronization = ViewSynchronization.ReliableDeltaCompressed;
	}
	
	protected virtual void OnDisable()
	{
	}
	
	protected virtual void Awake()
	{
		myTransform = transform;
		myRigidbody = rigidbody;
		correctPos = myTransform.position;
		correctRot = myTransform.rotation;
		// Update the kinematic state of the rigid body. Only the master client
		// will simulate this object's physics.
		if (null != myRigidbody)
		{
			myRigidbody.isKinematic = !PhotonNetwork.isMasterClient;
		}
	}
	
	protected virtual void Start()
	{
	}
	
	protected virtual void Update()
	{
		// Synchronize remote players
        if (!photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, correctPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctRot, Time.deltaTime * 5);
        }		
	}
		
	#endregion
	
	#region PhotonNetworkingMessage
	
    protected virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this object (though we may not be the master client): send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
			// TODO: The server needs to somehow validate these values before they get from the
			// owner to all the players
			
            // Network object, receive data
            this.correctPos = (Vector3)stream.ReceiveNext();
            this.correctRot = (Quaternion)stream.ReceiveNext();
        }
    }
	
	protected virtual void OnMasterClientSwitched(PhotonPlayer player)
    {
		// Update the kinematic state of the rigid body. Only the master client
		// will simulate this object's physics.
		if (null != myRigidbody)
		{
			myRigidbody.isKinematic = !PhotonNetwork.isMasterClient;
		}		
	}
	
	#endregion	
	
}

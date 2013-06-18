var target : Transform;
var targetOffset = Vector3.zero;
var distance = 4.0;

var lineOfSightMask : LayerMask = 0;
var closerRadius : float = 0.2;
var closerSnapLag : float = 0.2;

var xSpeed = 200.0;
var ySpeed = 80.0;

var yMinLimit = -20;
var yMaxLimit = 80;

private var currentDistance = 10.0;
private var x = 0.0;
private var y = 0.0;
private var distanceVelocity = 0.0;

function Start () {
    var angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;
	currentDistance = distance;
	
	// Make the rigid body not change rotation
   	if (rigidbody)
		rigidbody.freezeRotation = true;
}

function LateUpdate () {
    if (target) {
        x += Input.GetAxis("Mouse X") * xSpeed * 0.02;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02;
 		
 		y = ClampAngle(y, yMinLimit, yMaxLimit);
 		       
        var rotation = Quaternion.Euler(y, x, 0);
        var targetPos = target.position + targetOffset;
        var direction = rotation * -Vector3.forward;
		
        var targetDistance = AdjustLineOfSight(targetPos, direction);
		currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, distanceVelocity, closerSnapLag * .3);
        
        transform.rotation = rotation;
        transform.position = targetPos + direction * currentDistance;
    }
}

function AdjustLineOfSight (target : Vector3, direction : Vector3) : float
{
	var hit : RaycastHit;
	if (Physics.Raycast (target, direction, hit, distance, lineOfSightMask.value))
		return hit.distance - closerRadius;
	else
		return distance;
}

static function ClampAngle (angle : float, min : float, max : float) {
	if (angle < -360)
		angle += 360;
	if (angle > 360)
		angle -= 360;
	return Mathf.Clamp (angle, min, max);
}

@script AddComponentMenu("Third Person Camera/Mouse Orbit")
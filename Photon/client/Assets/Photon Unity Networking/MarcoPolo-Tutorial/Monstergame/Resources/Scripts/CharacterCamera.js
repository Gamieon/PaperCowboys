//Make sure that we have CharacterControl Included in this gameobject
@script RequireComponent(CharacterControl)

//Angular smooth
public var smoothTime : float = 0.1;
public var maxSpeed : float = 150.0;

public var heightSmoothTime : float = 0.1;

public var distance : float = 2.5;
public var height : float = 0.75;

private var f_heightVelocity : float = 0.0;
private var f_angleVelocity : float = 0.0;

private var v3_velocity : Vector3;
//Transform
private var target : Transform;
private var cameraTransform : Transform;

private var f_maxRotation : float;
//Character Control
private var c_characterControl : CharacterControl;

//Target
private var f_targetHeight : float = Mathf.Infinity;
private var v3_centerOffset = Vector3.zero;

public function Awake () : void {
	//Get Our Main Camera from the scene
	cameraTransform = Camera.main.transform;
	target = transform;
	c_characterControl = GetComponent(CharacterControl);
	
	//Get target center offset
	var characterController : CharacterController = target.collider;
	v3_centerOffset = characterController.bounds.center - target.position;
} 

//Get the angle distance between two angle
//This function took from the built-in Third Person Camera Script
public function AngleDistance (a : float, b : float) : float {
	//Loop the value a and b not higher than 360 and not lower than 0
	a = Mathf.Repeat(a, 360);
	b = Mathf.Repeat(b, 360);
	
	return Mathf.Abs(b - a);
}

//We use LateUpdate here because we need to wait for the user input before we update our camera. 
public function LateUpdate () : void {
	var v3_targetCenter : Vector3 = target.position + v3_centerOffset;
	
	//Calculate the current & target rotation angles
	var f_originalTargetAngle : float = target.eulerAngles.y;
	var f_currentAngle : float = cameraTransform.eulerAngles.y;
	var f_targetAngle : float = f_originalTargetAngle;
	
	// Lock the camera when moving backwards!
	// * It is really confusing to do 180 degree spins when turning around. So We fixed the camera rotation
	if (AngleDistance (f_currentAngle, f_targetAngle) > 160 && c_characterControl.IsMoveBackward ()) {
		f_targetAngle += 180;
	}
	//Apply rotation to the camera
	f_currentAngle = Mathf.SmoothDampAngle(f_currentAngle, f_targetAngle, f_angleVelocity, smoothTime, maxSpeed);
	
	//Update camera height position
	f_targetHeight = v3_targetCenter.y + height;
	
	// Damp the height
	var f_currentHeight : float = cameraTransform.position.y;
	f_currentHeight = Mathf.SmoothDamp (f_currentHeight, f_targetHeight, f_heightVelocity, heightSmoothTime);

	// Convert the angle into a rotation, by which we then reposition the camera
	var q_currentRotation : Quaternion = Quaternion.Euler (0, f_currentAngle, 0);
	
	// Set the position of the camera on the x-z plane to:
	// distance meters behind the target
	cameraTransform.position = v3_targetCenter;
	cameraTransform.position += q_currentRotation * Vector3.back * distance;
	
	// Set the height of the camera
	cameraTransform.position.y = f_currentHeight;
	
	// Always look at the target	
	SetUpRotation(v3_targetCenter);
}

private function SetUpRotation (v3_centerPos : Vector3) {
	var v3_cameraPos = cameraTransform.position; //Camera position
	var v3_offsetToCenter : Vector3 = v3_centerPos - v3_cameraPos; //Get the camera center offset
	
	//Generate base rotation only around y-axis
	var q_yRotation : Quaternion = Quaternion.LookRotation(Vector3(v3_offsetToCenter.x, v3_offsetToCenter.y + height, v3_offsetToCenter.z));
	//Apply the rotation to the camera
	var v3_relativeOffset = Vector3.forward * distance + Vector3.down * height;
	cameraTransform.rotation = q_yRotation * Quaternion.LookRotation(v3_relativeOffset);
}
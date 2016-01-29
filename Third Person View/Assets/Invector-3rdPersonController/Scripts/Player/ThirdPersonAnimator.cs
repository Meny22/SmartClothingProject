using UnityEngine;
using System.Collections;

public abstract class ThirdPersonAnimator : ThirdPersonMotor
{
    #region Variables
    // generate a random idle animation
    private float randomIdleCount, randomIdle;
	// used to lerp the head track
	private Vector3 lookPosition;
	// apply new rotation based on the character and camera
	private Quaternion _rotation;
	// match target to help animation to reach their target
	[HideInInspector] public Transform matchTarget;
	// head track control, if you want to turn off at some point, make it 0
	[HideInInspector] public float lookAtWeight;
	// access the animator states (layers)
	[HideInInspector] public AnimatorStateInfo stateInfo;
    private bool shiftGun = true;
    private bool akSoundStarted = false;
    public AudioClip akClip;
    public AudioSource akSource;
    public LayerMask mask;
    public int shotDamage;
    #endregion

    //**********************************************************************************//
    // ANIMATOR			      															//
    // update animations at the animator controller (Mecanim)							//
    //**********************************************************************************//
    public void UpdateAnimator()
    {
        if (ragdolled)        
            DisableActions();        
        else
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            actions = jumpOver || stepUp || climbUp || rolling;
            RandomIdle();
            RollForwardAnimation();
            QuickTurn180Animation();
            QuickStopAnimation();
            LandHighAnimation();
            JumpOverAnimation();
            ClimbUpAnimation();
            StepUpAnimation();
            ShootingAnimation();
            AimingAnimation();
            ControlLocomotion();
            animator.SetBool("Aiming", aiming);
            animator.SetBool("Crouch", crouch);
            animator.SetBool("OnGround", onGround);
            animator.SetFloat("GroundDistance", groundDistance);
            animator.SetFloat("VerticalVelocity", verticalVelocity);
            animator.SetBool("Walking", walking);
        }           
    }

	//**********************************************************************************//
	// DISABLE ACTIONS	      															//
	// turn false every action bool if ragdoll is enabled 								//
	//**********************************************************************************//    
    void DisableActions()
    {
		quickTurn180 = false;
		quickStop = false;
		canSprint = false;
		landHigh = false;
		jumpOver = false;
		rolling = false;
		stepUp = false;
		crouch = false;
		aiming = false;
    }

	//**********************************************************************************//
	// RANDOM IDLE		      															//
	// assign the animations into the layer IdleVariations on the Animator				//
	//**********************************************************************************//    
    void RandomIdle()
    {
        if(randomIdleTime > 0)
        {
            if (input.sqrMagnitude == 0 && !aiming && !crouch && capsuleCollider.enabled && onGround)
            {
                randomIdleCount += Time.deltaTime;
                if (randomIdleCount > 6)
                {
                    randomIdleCount = 0;
                    animator.SetTrigger("IdleRandomTrigger");
                    animator.SetInteger("IdleRandom", Random.Range(1, 4));
                }
            }
            else
            {
                randomIdleCount = 0;
                animator.SetInteger("IdleRandom", 0);
            }
        }        
    }



	//**********************************************************************************//
	// CONTROL LOCOMOTION      															//
	//**********************************************************************************//
    void ControlLocomotion()
    {
        bool freeLocomotionConditions = !aiming && !actions;

        // free directional movement

        speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
        speed = Mathf.Clamp(speed, 0, 1);
        // add 0.5f on sprint to change the animation on animator
        if (canSprint) speed = speed + 0.5f;
        if (freeLocomotionConditions)
        {
            // set speed to both vertical and horizontal inputs
			

            // set the correct angle when input are apply 
			if (input != Vector2.zero && !quickTurn180 && !lockPlayer && onGround)
            {
                _rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                var newAngle = _rotation.eulerAngles - transform.eulerAngles;
                direction = newAngle.NormalizeAngle().y;

                // activate quickTurn180 based on the directional angle
				if (!crouch && direction >= 165 || !crouch && direction <= -165)
                    quickTurn180 = true;

                // apply free directional rotation while not turning180 animations
                if (!quickTurn180)
                {
                    Vector3 lookDirection = targetDirection.normalized;
                    _rotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, _rotation, rotationSpeed * Time.deltaTime);
                }
                // send the angle direction to the animator
				animator.SetFloat("Direction", lockPlayer ? 0f : direction, 0f, Time.deltaTime);
            }
        }
		// move forward, backwards, strafe left and right
        else
        {
            //speed = input.y;
            direction = input.x;
            animator.SetFloat("Direction", lockPlayer ? 0f : direction, 0.1f, Time.deltaTime);
        }

        animator.SetFloat("Speed", !stopMove || lockPlayer ? speed : 0f, 0.2f, Time.deltaTime);
        if(speed == 0)
        {
            walking = false;
        }
        else if(speed > 0)
        {
            walking = true;
        }
    }

    //**********************************************************************************//
    // QUICK TURN 180	      															//
    //**********************************************************************************//
    void QuickTurn180Animation()
    {
        animator.SetBool("QuickTurn180", quickTurn180);

        // complete the 180 with matchTarget and disable quickTurn180 after completed
        if (stateInfo.IsName("Grounded.QuickTurn180"))
        {
            if (!animator.IsInTransition(0) && !ragdolled)
                MatchTarget(Vector3.one, _rotation, AvatarTarget.Root,
                             new MatchTargetWeightMask(Vector3.zero, 1f),
                             animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 0.9f);

            if (stateInfo.normalizedTime > 0.9f)
                quickTurn180 = false;
        }
    }

    //**********************************************************************************//
    // QUICK STOP		      															//
    //**********************************************************************************//
    void QuickStopAnimation()
    {
        animator.SetBool("QuickStop", quickStop);

		bool quickStopConditions = !actions && onGround;
        if (inputType == InputType.MouseKeyboard)
        {
            // make a quickStop when release the key while running
			if (input.sqrMagnitude < 0.2f && animator.GetFloat("Speed") >= 0.9f && quickStopConditions)
                quickStop = true;
        }
        else if (inputType == InputType.Controler)
        {
            // make a quickStop when release the analogue while running
			if (speed == 0 && animator.GetFloat("Speed") >= 0.9f && quickStopConditions)
                quickStop = true;            
        }

        // disable quickStop
        if (quickStop && input.sqrMagnitude > 0.9f && !stateInfo.IsName("Grounded.QuickStop") || actions)
            quickStop = false;
        else if (stateInfo.IsName("Grounded.QuickStop"))
        {
            if (stateInfo.normalizedTime > 0.9f || input.sqrMagnitude >= 0.1f || stopMove)
                quickStop = false;
        }
    }

    void AimingAnimation()
    {
        GameObject ak = GameObject.Find("Ak-47Player");
        GameObject parentAk = null;
        GameObject crosshair = GameObject.Find("3rdPersonCamera/aimReference/crosshair");
        bool puttingAway = false;
        if (stateInfo.IsName("Grounded.Pullout"))
        {
            parentAk = GameObject.FindGameObjectWithTag("RightHand");
            crosshair.SetActive(true);
        }
        else if(stateInfo.IsName("Grounded.Aiming_Locomotive"))
        {
            parentAk = GameObject.FindGameObjectWithTag("LeftHand");
        }
        else if(stateInfo.IsName("Grounded.Putaway"))
        {
            parentAk = GameObject.FindGameObjectWithTag("Shoulder");
            puttingAway = true;
            shiftGun = true;
            crosshair.SetActive(false);
        }
        if (parentAk != null)
        {
            ak.transform.parent = parentAk.transform;
            if (shiftGun && !puttingAway)
            {
                ak.transform.localPosition = new Vector3(-0.041f, 0.199f, 0.052f);
                ak.transform.localEulerAngles = new Vector3(275.3f, 237.9f, 222.4f);
                shiftGun = false;
            }
            else if(shiftGun && puttingAway)
            {
                ak.transform.localPosition = new Vector3(0.11f, 0.066f, 0.303f);
                ak.transform.localEulerAngles = new Vector3(312.658f, 0.4f, 338.9f);
            }
        }
    }

    void ShootingAnimation()
    {
        Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        animator.SetBool("Shooting", shooting);
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        if (stateInfo.IsName("Action.Shoot"))
        {
            akSource.clip = akClip;
            if (!akSource.isPlaying && animator.GetBool("Shooting"))
                akSource.Play();
            else if (!animator.GetBool("Shooting"))
                akSource.Stop();
            if(stateInfo.normalizedTime > 0.0f)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit,10000, mask)) {
                    //Debug.Log(hit.collider.gameObject.transform.root.gameObject);
                    if (hit.collider.gameObject.transform.root.gameObject.CompareTag("Enemy") && fired)
                    {
                        //if (hit.collider.gameObject != hit.collider.gameObject.transform.root.gameObject)
                        //{
                            //Debug.Log(hit.collider.gameObject);
                            hit.collider.gameObject.transform.root.gameObject.GetComponent<EnemyAI>().TakeDamage(100);
                            fired = false;
                        //}
                    }
                }
            }
        }
    }

    //**********************************************************************************//
    // ROLLING			      															//
    //**********************************************************************************//
    void RollForwardAnimation()
    {
        animator.SetBool("RollForward", rolling);

        // rollFwd
        if (stateInfo.IsName("Action.RollForward"))
        {
            lockPlayer = true;
            _rigidbody.useGravity = false;

			// prevent the character to rolling up 
            if (verticalVelocity >= 1)
                _rigidbody.velocity = Vector3.ProjectOnPlane(_rigidbody.velocity, groundHit.normal);

            // reset the rigidbody a little ealier to the character fall while on air
            if (stateInfo.normalizedTime > 0.3f)
                _rigidbody.useGravity = true;

            if (!crouch && stateInfo.normalizedTime > 0.9f)
            {
                lockPlayer = false;
                rolling = false;
            }
			else if (crouch && stateInfo.normalizedTime > 0.75f)
			{
				lockPlayer = false;
				rolling = false;
			}
        }
    }

	// control the direction of rolling when strafing
	public void Rolling()
	{
		bool conditions = (aiming || speed >= 0.25f) && !stopMove && onGround && !actions && !landHigh;
		
		if (conditions)
		{			
			if (aiming)
			{
				// check the right direction for rolling if you are aiming
				_rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
				var newAngle = _rotation.eulerAngles - transform.eulerAngles;
				direction = newAngle.NormalizeAngle().y;		
				transform.Rotate(0, direction, 0, Space.Self);
			}
			rolling = true;
			quickTurn180 = false;
		}
	}

    //**********************************************************************************//
    // HARD LANDING		      															//
    //**********************************************************************************//
    void LandHighAnimation()
    {
        animator.SetBool("LandHigh", landHigh);

		// if the character fall from a great height, landhigh animation
		if (!rolling && !onGround && verticalVelocity <= landHighVel)
            landHigh = true;

        if (landHigh && stateInfo.IsName("Airborne.LandHigh"))
        {
            lockPlayer = true;
            if (stateInfo.normalizedTime >= 0.1f && stateInfo.normalizedTime <= 0.2f)
			{
				// vibrate the controller 
				#if !UNITY_WEBPLAYER && !UNITY_ANDROID
				StartCoroutine(GamepadVibration(0.15f));
				#endif	
			}
			
			if (stateInfo.normalizedTime > 0.9f)
            {
                landHigh = false;
                lockPlayer = false;
            }
        }
    }

    //**********************************************************************************//
    // STEP UP			      															//
    //**********************************************************************************//
    void StepUpAnimation()
    {
        animator.SetBool("StepUp", stepUp);

        if (stateInfo.IsName("Action.StepUp"))
        {
            if (stateInfo.normalizedTime > 0.1f && stateInfo.normalizedTime < 0.3f)
            {
                gameObject.GetComponent<Collider>().isTrigger = true;
                _rigidbody.useGravity = false;
            }

			// we are using matchtarget to find the correct height of the object
            if (!animator.IsInTransition(0))
                MatchTarget(matchTarget.position, matchTarget.rotation,
                            AvatarTarget.LeftHand, new MatchTargetWeightMask
                            (new Vector3(0, 1, 1), 0), animator.GetFloat("MatchStart"),
                            animator.GetFloat("MatchEnd"));

            if (stateInfo.normalizedTime > 0.9f)
            {
                gameObject.GetComponent<Collider>().isTrigger = false;
                _rigidbody.useGravity = true;
                stepUp = false;
            }
        }
    }

    //**********************************************************************************//
    // JUMP OVER		      															//
    //**********************************************************************************//
    void JumpOverAnimation()
    {
        animator.SetBool("JumpOver", jumpOver);

		if (stateInfo.IsName("Action.JumpOver"))
        {
			if (stateInfo.normalizedTime > 0.1f && stateInfo.normalizedTime < 0.3f)
				_rigidbody.useGravity = false;

			// we are using matchtarget to find the correct height of the object
            if (!animator.IsInTransition(0))
                MatchTarget(matchTarget.position, matchTarget.rotation,
                            AvatarTarget.LeftHand, new MatchTargetWeightMask
                            (new Vector3(0, 1, 1), 0), animator.GetFloat("MatchStart"),
                            animator.GetFloat("MatchEnd"));

            if (stateInfo.normalizedTime > 0.8f)
			{
				_rigidbody.useGravity = true;
				jumpOver = false;
			}                
        }
    }

    //**********************************************************************************//
    // CLIMB			      															//
    //**********************************************************************************//
    void ClimbUpAnimation()
    {
        animator.SetBool("ClimbUp", climbUp);

        if (stateInfo.IsName("Action.ClimbUp"))
        {
            if (stateInfo.normalizedTime > 0.1f && stateInfo.normalizedTime < 0.3f)
            {
				_rigidbody.useGravity = false;
                gameObject.GetComponent<Collider>().isTrigger = true;               
            }

			// we are using matchtarget to find the correct height of the object
            if (!animator.IsInTransition(0))
                MatchTarget(matchTarget.position, matchTarget.rotation,
                            AvatarTarget.LeftHand, new MatchTargetWeightMask
                            (new Vector3(0, 1, 1), 0), animator.GetFloat("MatchStart"),
                            animator.GetFloat("MatchEnd"));

            if (stateInfo.normalizedTime > 0.9f)
            {
                gameObject.GetComponent<Collider>().isTrigger = false;
				_rigidbody.useGravity = true;
                climbUp = false;
            }
        }
    }    
  
   //**********************************************************************************//
   // HEAD TRACK		      															//
   // head follow where you point at													//
   //**********************************************************************************//

#if !MOBILE_INPUT
   Vector3 lookPoint(float distance)
   {
       Ray ray = new Ray(tpCamera.transform.position, tpCamera.transform.forward);
       return ray.GetPoint(distance);
   }

   void OnAnimatorIK()
   {
        if(headTracking)
        {
            // get the random idle layer to blend animations
            stateInfo = animator.GetCurrentAnimatorStateInfo(1);

            var rot = Quaternion.LookRotation(lookPoint(1000f) - transform.position, Vector3.up);
            var newAngle = rot.eulerAngles - transform.eulerAngles;
            var ang = newAngle.NormalizeAngle().y;

            if (!aiming)
            {
                if (ang <= 70 && ang >= 0 && !lockPlayer && stateInfo.IsName("Null") || ang > -70 && ang <= 0 && !lockPlayer && stateInfo.IsName("Null"))
                    lookAtWeight += 1f * Time.deltaTime;
                else
                    lookAtWeight -= 1f * Time.deltaTime;

                lookAtWeight = Mathf.Clamp(lookAtWeight, 0f, 1f);
                animator.SetLookAtWeight(lookAtWeight, 0.2f, 1f);
            }
            else
                animator.SetLookAtWeight(0.5f);

            lookPosition = Vector3.Lerp(lookPosition, lookPoint(1000f), 10f * Time.deltaTime);
            animator.SetLookAtPosition(lookPosition);
        }       
   }
#endif

	// helps find the right direction
	Vector3 targetDirection
	{
		get
		{
			Vector3 cameraForward = tpCamera.transform.TransformDirection(Vector3.forward);
			cameraForward.y = 0;	//set to 0 because of camera rotation on the X axis
			
			//get the right-facing direction of the camera
			Vector3 cameraRight = tpCamera.transform.TransformDirection(Vector3.right);
			
			// determine the direction the player will face based on input and the camera's right and forward directions
			Vector3 refDir = input.x * cameraRight + input.y * cameraForward;
			return refDir;
		}
	}
	
	//**********************************************************************************//
	// MATCH TARGET																		//
	// call this method to help animations find the correct target						//
	// don't forget to add the curve MatchStart and MatchEnd on the animation clip		//
	//**********************************************************************************//
	void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, AvatarTarget target, 
	                 MatchTargetWeightMask weightMask, float normalisedStartTime, float normalisedEndTime)
	{
		if (animator.isMatchingTarget)
			return;
		
		float normalizeTime = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
		if (normalizeTime > normalisedEndTime)
			return;

		if(!ragdolled)
		animator.MatchTarget(matchPosition, matchRotation, target, weightMask, normalisedStartTime, normalisedEndTime);
	}   
	
}

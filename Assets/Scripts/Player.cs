using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public event System.Action OnReachedEndOfLevel;

    public float speed = 7;
    public float smoothMoveTime = .1f;
    public float turnSpeed = 8f;

    float currentAngle;
    float smoothInputMagnitude;
    float smoothMoveVelocity;
    Vector3 velocity;

    Rigidbody rigidbody;
    bool disabled;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Guard.OnGuardHasSpottedPlayer += Disable;
    }

    void Update()
    {
        Vector3 inputDirection = Vector3.zero;
        if(!disabled)
        {
            inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }
        float inputMagnitude = inputDirection.magnitude;
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        float inputAngle = ToUnityCircleAngles(Mathf.Atan2(inputDirection.z, inputDirection.x) * Mathf.Rad2Deg);

        // stop interpolation by multiplying by inputMagnitude so that the angle doesn't change when you stop pressing a direction
        currentAngle = Mathf.LerpAngle(currentAngle, inputAngle, Time.deltaTime * turnSpeed * inputMagnitude);

        velocity = transform.forward * speed * smoothInputMagnitude;
        // handle rotation with rigidbody
        //transform.eulerAngles = Vector3.up * currentAngle;
        //transform.Translate(inputDirection * Time.deltaTime * speed * smoothInputMagnitude, Space.World);
    }
    
    // RigidBodyies need to be updated in the fixed update method
    private void FixedUpdate()
    {
        rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * currentAngle));
        rigidbody.MovePosition(rigidbody.position + velocity * Time.deltaTime );
    }

    private void Disable()
    {
        disabled = true;
    }

    float ToUnityCircleAngles (float angle)
    {
        return 90 - angle;
    }

    private void OnTriggerEnter(Collider hitCollider)
    {
        if(hitCollider.tag == "Finish")
        {
            Disable();
            if(OnReachedEndOfLevel != null)
            {
                OnReachedEndOfLevel();
            }
        }
    }

    private void OnDestroy()
    {
        Guard.OnGuardHasSpottedPlayer -= Disable;
    }
}


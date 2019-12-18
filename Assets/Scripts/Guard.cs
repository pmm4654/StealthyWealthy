using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public static event System.Action OnGuardHasSpottedPlayer;

    public float speed = 5;
    public float waitTime = .3f;
    public float turnSpeed = 90; // 90 deg / second

    public float timeToSpotPlayer = .5f;
    float playerVisibleTimer;
    public Transform pathHolder;
    
    public Light spotlight;
    public float viewDistance;
    float viewAngle;

    public LayerMask viewMask;
    Transform player;
    Color originalSpotlightColor;

    private void Start()
    {
        originalSpotlightColor = spotlight.color;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotlight.spotAngle;
        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for(int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
        StartCoroutine(FollowPath(waypoints));
    }

    private void Update()
    {
        if(CanSeePlayer())
        {
            playerVisibleTimer += Time.deltaTime;
            spotlight.color = Color.red;
        } else
        {
            playerVisibleTimer -= Time.deltaTime;
            spotlight.color = originalSpotlightColor;
        }
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        spotlight.color = Color.Lerp(originalSpotlightColor, Color.red, playerVisibleTimer / timeToSpotPlayer);

        if(playerVisibleTimer >= timeToSpotPlayer)
        {
            if(OnGuardHasSpottedPlayer != null)
            {
                OnGuardHasSpottedPlayer();
            }
        }
    }

    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            //(angle returns smallest angle (so instead of 210, it would return 30 so you can just check is less than 2f)
            if (angleBetweenGuardAndPlayer < viewAngle / 2f) // are you inside of the flashlight range 
            {
                if (!Physics.Linecast(transform.position, player.position, viewMask)) // if you haven't hit any other obstacles, then you can see the player
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;
        foreach(Transform waypoint in pathHolder)
        {
            float gizmoRadius = .3f;
            Gizmos.DrawSphere(waypoint.position, gizmoRadius);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

    IEnumerator FollowPath(Vector3[] waypoints) 
    {
        transform.position = waypoints[0];

        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetWaypoint); // make so that the target initially faces the first waypoint

        while(true)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            if(transform.position == targetWaypoint) // you have reached your target waypoint
            {
                // when your current index is equal to the number of waypoints, it goes back to 0 and loops through the path again
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];

                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint)); // wait for this coroutine to stop before continuing
            }
            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = ToUnityCircleAngles(Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg);
        while(Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f) { // don't use 0 because it might no actually get all the way to 0
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }
    

    float ToUnityCircleAngles(float angle)
    {
        return 90 - angle;
    }
}

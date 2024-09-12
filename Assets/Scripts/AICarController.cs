using UnityEngine;
using System.Collections.Generic; // For List<T>

public class AICarController : MonoBehaviour
{
      public Transform[] waypoints; // Array of waypoints
      public Transform[] brakingPoints; // Array of braking points (cubes)
      public float maxSpeed = 400f; // Maximum speed of the car
      public float turnSpeed = 1f; // Turning speed
      public float waypointThreshold = 20f; // Distance to waypoint before moving to the next one
      public float brakeSpeed = 100f; // Speed when braking at tight turns
      public float brakeThreshold = 10f; // Distance to the braking point
      public float accelerationTime = 7f; // Time taken to accelerate to max speed
      public float startDelay = 3f; // Delay before the car starts moving
      public float skidDuration = 0.2f; // Duration of skid marks
      public float downforce = 40f; // Additional force to keep the car grounded
      public float groundRayLength = 1.5f; // Length of the ray to detect the ground
      public float maxTiltAngle = 30f; // Maximum tilt angle before car auto-corrects
      public LayerMask groundLayer; // Layer for ground detection

      public Transform body; // Car body for applying visual effects
      public ParticleSystem leftTireSmoke; // Tire smoke particle system
      public ParticleSystem rightTireSmoke; // Tire smoke particle system
      public TrailRenderer leftTireSkid; // Skid mark trail
      public TrailRenderer rightTireSkid; // Skid mark trail
      public bool drawDebugLines = true; // Enable or disable debug lines (optional)

      public Transform finishLine; // Finish line position
      public static List<AICarController> raceResults = new List<AICarController>(); // Static list for race results

      private Rigidbody rb;
      private int currentWaypointIndex = 0;
      private bool isSkidding = false; // Tracks skidding state
      private bool isBraking = false; // Tracks whether the car is braking
      private float currentSpeed = 0f; // Current speed of the car
      private float accelerationRate; // How fast the car accelerates per second
      private float raceStartTime; // When the race started
      private bool isGrounded = true;
      private int lapCount = 0; // To track laps
      private bool raceFinished = false; // Check if the car has finished the race

      void Start()
      {
            rb = GetComponent<Rigidbody>();

            // Set the acceleration rate based on time to max speed
            accelerationRate = maxSpeed / accelerationTime;

            // Set the race start time (current time + delay)
            raceStartTime = Time.time + startDelay;

            // Set skid mark duration
            leftTireSkid.time = skidDuration;
            rightTireSkid.time = skidDuration;

            // Start with effects off
            leftTireSkid.emitting = false;
            rightTireSkid.emitting = false;
            leftTireSmoke.Stop();
            rightTireSmoke.Stop();
      }

      void Update()
      {
            if (raceFinished) return; // If race finished, don't update any further

            // Wait for the delay before starting the race
            if (Time.time < raceStartTime)
            {
                  return; // Don't do anything until the race starts
            }

            // Apply downforce to help keep the car grounded
            ApplyDownforce();

            // Check if the car is grounded and correct if necessary
            CheckGroundStatus();

            // Accelerate and control the car
            Accelerate();
            DriveTowardsWaypoint();
            HandleEffects();
            CheckForBrakingPoints();
      }

      private void OnTriggerEnter(Collider other)
      {
            if (other.transform == finishLine)
            {
                  lapCount++;

                  if (lapCount >= 2)
                  {
                        float raceFinishTime = Time.time - raceStartTime;
                        raceResults.Add(this); // Add car to the race results
                        Debug.Log($"{gameObject.name} finished in {raceFinishTime} seconds.");

                        if (raceResults.Count == 1)
                        {
                              DisplayRaceResults();
                        }
                  }
            }
      }


      private void Decelerate()
      {
            currentSpeed = 1000f; // Set the speed to zero to stop the car
      }

      void ApplyDownforce()
      {
            // Add downforce proportional to speed
            rb.AddForce(-transform.up * downforce * currentSpeed);
      }

      void CheckGroundStatus()
      {
            // Cast a ray downward to check if the car is grounded
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, groundRayLength, groundLayer))
            {
                  isGrounded = true;
            }
            else
            {
                  isGrounded = false;
                  // Apply downward force if car is not grounded
                  rb.AddForce(Vector3.down * downforce * 2f);
            }

            // If the car tilts too much, correct its rotation
            float tiltAngle = Vector3.Angle(transform.up, Vector3.up);
            if (tiltAngle > maxTiltAngle)
            {
                  Quaternion targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
                  rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f));
            }

            // Visualize the ray (optional)
            if (drawDebugLines)
            {
                  Debug.DrawRay(transform.position, -transform.up * groundRayLength, isGrounded ? Color.green : Color.red);
            }
      }

      void Accelerate()
      {
            // Gradually increase speed until reaching max speed
            if (currentSpeed < maxSpeed && !isBraking)
            {
                  currentSpeed += accelerationRate * Time.deltaTime;
            }
            else if (isBraking && currentSpeed > brakeSpeed)
            {
                  // Decelerate if braking
                  currentSpeed = Mathf.Lerp(currentSpeed, brakeSpeed, Time.deltaTime * 2f);
            }

            currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed); // Ensure the speed stays within limits
      }

      void DriveTowardsWaypoint()
      {
            // Get the current waypoint
            Vector3 target = waypoints[currentWaypointIndex].position;
            // Direction to the waypoint
            Vector3 direction = (target - transform.position).normalized;

            // Visualize the ray to the target waypoint (Debugging)
            if (drawDebugLines)
            {
                  Debug.DrawLine(transform.position, target, Color.green); // Draw a green line to the next waypoint
            }

            // Rotate the car smoothly towards the target waypoint
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);

            // Move the car forward based on the current speed
            rb.MovePosition(transform.position + transform.forward * currentSpeed * Time.deltaTime);

            // Check if we've reached the current waypoint
            float distanceToWaypoint = Vector3.Distance(transform.position, target);
            if (distanceToWaypoint < waypointThreshold)
            {
                  currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // Loop through waypoints
            }
      }

      void CheckForBrakingPoints()
      {
            // Check if we are near any braking points
            isBraking = false;
            foreach (var brakingPoint in brakingPoints)
            {
                  float distanceToBrakePoint = Vector3.Distance(transform.position, brakingPoint.position);
                  if (distanceToBrakePoint < brakeThreshold)
                  {
                        isBraking = true;
                        break; // No need to check other braking points
                  }
            }
      }

      void HandleEffects()
      {
            // Check if the car is skidding (e.g., when turning sharply or drifting)
            float turningAmount = Vector3.Angle(transform.forward, rb.velocity.normalized);

            if (turningAmount > 15f) // Arbitrary threshold for skidding
            {
                  if (!isSkidding)
                  {
                        // Start effects
                        leftTireSkid.emitting = true;
                        rightTireSkid.emitting = true;
                        leftTireSmoke.Play();
                        rightTireSmoke.Play();
                        isSkidding = true;
                  }
            }
            else
            {
                  if (isSkidding)
                  {
                        // Stop effects
                        leftTireSkid.emitting = false;
                        rightTireSkid.emitting = false;
                        leftTireSmoke.Stop();
                        rightTireSmoke.Stop();
                        isSkidding = false;
                  }
            }
      }

      void DisplayRaceResults()
      {
            raceResults.Sort((car1, car2) => car1.lapCount.CompareTo(car2.lapCount)); // Sort by lapCount or time
            // Debug.Log("Race Results:");
            foreach (var car in raceResults)
            {
                  Debug.Log(car.name);
            }
      }

      // Optional: Debugging to visualize waypoints and braking points
      void OnDrawGizmos()
      {
            if (waypoints.Length > 0)
            {
                  for (int i = 0; i < waypoints.Length; i++)
                  {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(waypoints[i].position, 1f);
                  }
            }

            if (brakingPoints.Length > 0)
            {
                  for (int i = 0; i < brakingPoints.Length; i++)
                  {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube(brakingPoints[i].position, Vector3.one);
                  }
            }
      }
}

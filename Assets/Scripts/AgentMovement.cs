using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// for target NavMeshAgent
using UnityEngine.AI;

// ML-Agents imports
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class AgentMovement : Agent
{
    Rigidbody rBody;
    NavMeshAgent nma;
    public GameObject Target;

    // reward function tuning parameters
    float A = 1f;
    float d = 3f; 
    float c = 36.1247837364f;    
    float lam = 0.00555556f; 
 
    // public vars 
    // all relate to how cubes move and can be altered from inspector window mid-play
    public float forceMultiplier = 500f;
    public float torqueMultiplier = 250f;        
    public float targetSpeed = 2f;

    // static vars
    // NB half width and height (from centre) and according to cube transform not ground transform  
    float groundWidth = 12f;                                                             
    float groundLength = 12f;                                                           

    // variable vars
    float episodeCounter;	
    Vector3 heading;
    float a;

    void Start ()
    {
	// saving to variable so only need to access rigidbody and navmeshagent once
        rBody = GetComponent<Rigidbody>();
	nma = Target.GetComponent<NavMeshAgent>();

	// initialise episode counter			
        episodeCounter = 0;
    }

    public override void OnEpisodeBegin()
    {
	// add one to episode counter	
	episodeCounter += 1f;
	    
	// zero agent's velocity
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;

	// set target's speed (maximum speed will travel, will not necessarily maintain e.g. when turning)
	nma.speed = targetSpeed;

	// place target in random starting position with random facing direction
	float randx = Random.Range(-groundWidth, groundWidth);                      
	float randz = Random.Range(-groundLength, groundLength);
	float randa = Random.Range(-180f,180f);
        Target.transform.localPosition = new Vector3(randx, 0.5f, randz);
        Target.transform.Rotate(0, randa, 0, Space.Self);

	// place agent in random starting position with random facing direction
	randx = Random.Range(-groundWidth, groundWidth);                            
	randz = Random.Range(-groundLength, groundLength);
	randa = Random.Range(-180f,180f);
	this.transform.localPosition = new Vector3( randx, 0.5f, randz);
        this.transform.Rotate(0, randa, 0, Space.Self);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent's position
        sensor.AddObservation(this.transform.localPosition.x);
        sensor.AddObservation(this.transform.localPosition.z);

        // Agent's speed
        sensor.AddObservation(rBody.velocity.magnitude);                       

	// Agent's facing direction (NB this is a vector, so classes as three obs)
	sensor.AddObservation(this.transform.forward);

	// Target's position
        sensor.AddObservation(Target.transform.localPosition.x);
        sensor.AddObservation(Target.transform.localPosition.z);

	// Target's facing direction (NB this is a vector, so classes as three obs)
	sensor.AddObservation(Target.transform.forward);

	// Target's speed (actual speed, not navmeshagent settings as above)                                 
	sensor.AddObservation(nma.velocity.magnitude);
    }

    public override void OnActionReceived(float[] act)
    {
	// assign incoming discrete action decision to actual force / torque application 
	// NB no code for if act equals 0 because this is our do nothing action decision
	if (act[0].Equals(1)){rBody.AddForce(transform.forward * forceMultiplier);}
	if (act[0].Equals(2)){rBody.AddForce(transform.forward * -forceMultiplier);}
	if (act[0].Equals(3)){rBody.AddTorque(transform.up * -torqueMultiplier);}
	if (act[0].Equals(4)){rBody.AddTorque(transform.up * torqueMultiplier);}

	// calculate heading, vector pointing from agent's position to target's position
	heading = Target.transform.localPosition - this.transform.localPosition;

	// calculate angle between heading vector and vector pointing down agent's local positive z axis i.e facing direction
	a = Vector3.Angle(heading, this.transform.forward);

	// the reward function is trying to reward the agent being behind the target 
	// however heading.z changes sign depending on which direction the target is travelling with respect to the world z axis
	// this bit of code ensures that, whatever the direction of travel, heading.z is positive if the agent is 'behind'
	if (Vector3.Dot(heading.normalized, Target.transform.forward) > 0)
	{
		heading.z = Mathf.Abs(heading.z);
	} else {
		heading.z = -Mathf.Abs(heading.z);
	}

	// implementation of reward function presented in Luo et al 2018
        var rDist = Mathf.Sqrt(Mathf.Pow(heading.x, 2f) + Mathf.Pow((heading.z - d), 2f));        
	var r = A - ((rDist/c) + (a*lam));
	AddReward(r);
    }

    void FixedUpdate() 
    {
	// episode termination criteria
	if (this.StepCount > 3000 | this.GetCumulativeReward() < -450) 
	{
	    TargetMovement t = Target.GetComponent(typeof(TargetMovement)) as TargetMovement;	
	    t.Respawn();
	    EndEpisode();
	}
    }

    // controlling agent with keyboard - useful for testing - Behaviour Type must be set to Heuristic in Behaviour Parameters
    public override void Heuristic(float[] actionsOut)
    {
	if (Input.GetKey(KeyCode.UpArrow)) {
		rBody.AddForce(transform.forward * forceMultiplier);
	}
	if (Input.GetKey(KeyCode.DownArrow)) {
		rBody.AddForce(transform.forward * -forceMultiplier);
	}
	if (Input.GetKey(KeyCode.RightArrow)) {
		rBody.AddTorque(transform.up * torqueMultiplier);
	}
	if (Input.GetKey(KeyCode.LeftArrow)) {
		rBody.AddTorque(transform.up * -torqueMultiplier);
	}
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for NavMeshAgent
using UnityEngine.AI; 

public class TargetMovement : MonoBehaviour
{
    private NavMeshAgent nma;
    private GameObject[] RandomPoint;
    private List<GameObject> SameCourtWaypoints = new List<GameObject>();
    private int CurrentRandom;

    private void Start()
    {	
	// save NavMeshAgent to variable so only need to access once
	nma = this.GetComponent<NavMeshAgent>();

	// finds cylinder waypoint objects, but across all Area prefabs
	RandomPoint = GameObject.FindGameObjectsWithTag("RandomPoint");

	// finds those within the same training court as the target
	foreach(GameObject wp in RandomPoint)
	{
   	    if (wp.transform.parent == this.transform.parent)
	    {
		SameCourtWaypoints.Add(wp);
	    }
	}
	RandomPoint = SameCourtWaypoints.ToArray();
    }

    // function that can be called from AgentMovement script whenever episode restarts
    public void Respawn()	
    {
	// select random waypoint belonging to court
	CurrentRandom = Random.Range(0, RandomPoint.Length - 1);

	// set the position of that selected waypoint as the Target's destination
	nma.SetDestination(RandomPoint[CurrentRandom].transform.position);
    }

    private void Update()
    {
	// if the waypoint has been reached, choose the next
	if (nma.hasPath == false)
	{
	    CurrentRandom = Random.Range(0, RandomPoint.Length - 1);
	    nma.SetDestination(RandomPoint[CurrentRandom].transform.position);
	}
    }
}

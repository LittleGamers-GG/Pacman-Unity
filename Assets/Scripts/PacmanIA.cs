using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.SocialPlatforms.Impl;

public class PacmanAI : MonoBehaviour
{
    public Transform target;
    //public float moveSpeed = 5f;
    private Vector2 currentDirection;
    private Vector2 nextDirection;

    private List<Vector2> availableDirections = new List<Vector2>();

    private Pacman pacman;

    private void Awake()
    {
        pacman = GetComponent<Pacman>();
        print("I am using this scipt");
    }
    void Start()
    {
        // Initialize availableDirections using your existing system
        // For example, if you have a script called DirectionManager that provides available directions:
        // availableDirections = DirectionManager.GetAvailableDirections();
        currentDirection = Vector2.right; // Initial direction
        nextDirection = currentDirection;
    }

    void Update()
    {
        nextDirection = UpdateDirection();
 
        Move(nextDirection);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();
        if (node != null)
        {
            availableDirections = node.availableDirections;
        }
    }
        Vector2 UpdateDirection()
    {
        if (availableDirections.Count > 0)
        {
            print("I am in");
            // Combine all heuristics
            float distanceToTargetWeight = 2.0f;
            float randomDirectionWeight = 0.5f;
            float avoidGhostsWeight = 0.8f;

            (Vector2 directionDistanceToTarget, float score1) = GetDirectionWithMinDistanceToTarget();
            Vector2 directionRandom = availableDirections[Random.Range(0, availableDirections.Count)];
            (Vector2 directionAvoidGhosts, float score2) = GetDirectionAwayFromGhosts();
            
            if( score1 * distanceToTargetWeight > score2 * avoidGhostsWeight )
                nextDirection = directionDistanceToTarget;
            
            if (score1 * distanceToTargetWeight > score2 * avoidGhostsWeight)
                nextDirection = directionAvoidGhosts;
        }
        print("Next:" + nextDirection);
        return nextDirection;
    }
    (Vector2,float) GetDirectionWithMinDistanceToTarget()
    {
        float minDistance = float.MaxValue;
        Vector2 bestDirection = Vector2.zero;
        float score = 0;
        foreach (Vector2 availableDirection in availableDirections)
        {
            Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
            float distanceToTarget = (target.transform.position - newPosition).sqrMagnitude;
            if (distanceToTarget < minDistance)
            {
                minDistance = distanceToTarget;
                bestDirection = availableDirection;
                score = 1;
            }
        }
        print(bestDirection);
        return (bestDirection,score);
    }

   (Vector2,float) GetDirectionAwayFromGhosts()
    {
        Vector2 bestDirection = Vector3.zero;
        float score = 0;
        foreach (Vector2 direction in availableDirections)
        {
            Vector3 newPosition = transform.position + new Vector3(direction.x, direction.y);
            GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            bool isSafeDirection = true;
            float distanceToGhost = 0;
            foreach (var ghost in ghosts)
            {
                if (!ghost.GetComponent<Ghost>().frightened) 
                {
                    distanceToGhost = Vector3.Distance(newPosition, ghost.transform.position);
                    if (distanceToGhost < 1.5f) // Adjust this threshold according to your game
                    {
                        isSafeDirection = false;

                        break;
                    }
                }
            }

            if (!isSafeDirection)
            {
                bestDirection = new Vector3(-direction.x, -direction.y); ;
                score = 1.5f - distanceToGhost;
            }
        }
        

        return (bestDirection, score) ;
    }

    bool IsValidPosition(Vector3 position)
    {
        // Check if the position is valid (not colliding with walls or other obstacles)
        // Implement your own logic here based on your game setup
        return true; // Placeholder, replace with actual logic
    }

    void Move(Vector2 direction)
    {
        // Move Pacman in the calculated direction
        pacman.movement.SetDirection(direction);
    }
}

using UnityEngine;
using System.Collections.Generic;

public class PacmanAI : MonoBehaviour
{
    public Transform target;
    private Vector2 currentDirection;
    private Vector2 nextDirection;

    private List<Vector2> availableDirections = new List<Vector2>();
    private float maxDistanceToGhosts; // Nouvelle variable globale pour stocker la distance maximale aux fantômes

    private Pacman pacman;

    private void Awake()
    {
        pacman = GetComponent<Pacman>();
    }

    void Start()
    {
        currentDirection = Vector2.right; // Initial direction
        nextDirection = currentDirection;
        availableDirections.Add(new Vector2(-1, 0));
        maxDistanceToGhosts = CalculateMaxDistanceToGhosts();

    }

    void Update()
    {
        nextDirection = UpdateDirection();
        Debug.Log("Next Direction: " + nextDirection.x + ", " + nextDirection.y);
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

    private void Move(Vector2 direction)
    {
        // Move Pacman in the calculated direction
        pacman.movement.SetDirection(direction);
    }

    private Vector2 UpdateDirection()
    {
        float distanceToTargetWeight = 2.0f;
        float avoidGhostsWeight = 2.0f;

        (Vector2 directionDistanceToTarget, float score1) = GetDirectionWithMinDistanceToTarget();
        (Vector2 directionAvoidGhosts, float score2) = GetDirectionAwayFromGhosts();
        Debug.Log("Direction 1 :" + directionDistanceToTarget + "Score 1 : " + score1);
        Debug.Log("Direction 2 :" + directionAvoidGhosts + "Score 2 : " + score2);

        Vector2 nextDirection = Vector2.zero;

        // Combine all heuristics
        float weightedScore1 = score1 * distanceToTargetWeight;
        float weightedScore2 = score2 * avoidGhostsWeight;

        if (weightedScore1 >= weightedScore2)
        {
            nextDirection = directionDistanceToTarget;
        }
        else
        {
            nextDirection = directionAvoidGhosts;
        }
        Debug.Log("NextDirection :" + nextDirection);

        return nextDirection;
    }

    private (Vector2, float) GetDirectionWithMinDistanceToTarget()
    {
        float maxDistance = Vector3.Distance(Vector3.zero, new Vector3(20, 20, 0)); // Max possible distance
        float minDistance = float.MaxValue;
        Vector2 bestDirection = Vector2.zero;
        float bestScore = 0;

        foreach (Vector2 availableDirection in availableDirections)
        {
            Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
            float distanceToTarget = Vector3.Distance(target.position, newPosition);

            // Calcul du score basé sur la distance à la cible (plus la distance est courte, plus le score est élevé)
            float score = 1 - (distanceToTarget / maxDistance); // Score normalisé entre 0 et 1

            // Mettre à jour la meilleure direction et le meilleur score si nécessaire
            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = availableDirection;
            }
        }

        return (bestDirection, bestScore);
    }


    private (Vector2, float) GetDirectionAwayFromGhosts()
    {
        Vector2 bestDirection = Vector2.zero;
        float bestScore = 0;

        foreach (Vector2 direction in availableDirections)
        {
            Vector3 newPosition = transform.position + new Vector3(direction.x, direction.y);
            float distanceToGhosts = CalculateDistanceToGhosts(newPosition);

            // Calcul du score de sécurité (plus la distance aux fantômes est grande, plus le score est élevé)
            float score = 1 - (distanceToGhosts / maxDistanceToGhosts); // Score normalisé entre 0 et 1

            // Mettre à jour la meilleure direction et le meilleur score si nécessaire
            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = direction;
            }
        }

        return (bestDirection, bestScore);
    }

    private float CalculateMaxDistanceToGhosts()
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        float maxDistance = 0;

        foreach (var ghost in ghosts)
        {
            float distanceToGhost = Vector3.Distance(transform.position, ghost.transform.position);
            if (distanceToGhost > maxDistance)
            {
                maxDistance = distanceToGhost;
            }
        }

        return maxDistance;
    }

    private float CalculateDistanceToGhosts(Vector3 position)
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        float minDistanceToGhosts = float.MaxValue;

        foreach (var ghost in ghosts)
        {
            float distanceToGhost = Vector3.Distance(position, ghost.transform.position);
            minDistanceToGhosts = Mathf.Min(minDistanceToGhosts, distanceToGhost);
        }

        return minDistanceToGhosts;
    }


}

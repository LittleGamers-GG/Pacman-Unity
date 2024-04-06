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
        print("Next Direction :" + nextDirection.x + ", " + nextDirection.y);
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

    private Vector2 UpdateDirection()
    {
        float distanceToTargetWeight = 2.0f;
        float randomDirectionWeight = 0.5f;
        float avoidGhostsWeight = 0.8f;

        (Vector2 directionDistanceToTarget, float score1) = GetDirectionWithMinDistanceToTarget();
        Vector2 directionRandom = availableDirections[Random.Range(0, availableDirections.Count)];
        (Vector2 directionAvoidGhosts, float score2) = GetDirectionAwayFromGhosts();

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

        print("Next: " + nextDirection + " Score1 : " + score1 + " Score2 : " + score2);
        return nextDirection;
    }



    private (Vector2, float) GetDirectionWithMinDistanceToTarget()
    {
        float minDistance = float.MaxValue;
        Vector2 bestDirection = Vector2.zero;
        float bestScore = 0;

        foreach (Vector2 availableDirection in availableDirections)
        {
            Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
            float distanceToTarget = (target.transform.position - newPosition).sqrMagnitude;

            // Calculer un score basé sur la distance à la cible
            // Plus la distance est courte, plus le score est élevé
            float score = distanceToTarget%1; // Utilisation de 1 / (distance + 1) pour éviter une division par zéro

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
        float bestDistanceToGhosts = float.MaxValue;

        foreach (Vector2 direction in availableDirections)
        {
            Vector3 newPosition = transform.position + new Vector3(direction.x, direction.y);
            float distanceToGhosts = CalculateDistanceToGhosts(newPosition);
            print("Distance to Ghost : " + distanceToGhosts);
            // Si la position hypothétique est sûre et plus éloignée des fantômes que la meilleure direction actuelle
            if (distanceToGhosts > 1.5f && distanceToGhosts < bestDistanceToGhosts)
            {
                bestDistanceToGhosts = distanceToGhosts;
                bestDirection = direction;
            }
        }

        float score = bestDistanceToGhosts%1; // Calcul du score en fonction de la distance aux fantômes

        print("Ghost Direction: " + bestDirection + " Score: " + score);
        return (bestDirection, score);
    }

    // Fonction pour calculer la distance à tous les fantômes
    private float CalculateDistanceToGhosts(Vector3 position)
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        float minDistanceToGhosts = float.MaxValue;

        foreach (var ghost in ghosts)
        {
            Debug.Log("Ghosts : " + ghost.GetComponent<Ghost>().frightened.enabled);

            if (!ghost.GetComponent<Ghost>().frightened.enabled)
            {
                float distanceToGhost = Vector3.Distance(position, ghost.transform.position);
                minDistanceToGhosts = Mathf.Min(minDistanceToGhosts, distanceToGhost);
            }
            Debug.Log("Ghosts : " + ghosts + "Min distance /" + minDistanceToGhosts);

        }

        return minDistanceToGhosts;
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

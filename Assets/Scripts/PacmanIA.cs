using UnityEngine;
using System.Collections.Generic;

public class PacmanAI : MonoBehaviour
{
    private Vector2 currentDirection;
    private Vector2 nextDirection;

    private List<Vector2> availableDirections = new List<Vector2>();
    private float maxDistanceToGhosts; // Nouvelle variable globale pour stocker la distance maximale aux fantômes

    private Pacman pacman;
    private Pellet pellet;
    private PowerPellet powerPellet;

    private void Awake()
    {
        pacman = GetComponent<Pacman>();
        pellet = GetComponent<Pellet>();
        powerPellet = GetComponent<PowerPellet>();
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
        float avoidGhostsWeight = 1.0f;
        foreach (var ghost in GameObject.FindGameObjectsWithTag("Ghost"))
        {
            Ghost ghostTemp = ghost.GetComponent<Ghost>();
            if (ghostTemp.frightened)
            {
                avoidGhostsWeight = 0.0f;
            }
        }
        float distanceToTargetWeight = 1.2f;
        float powerPelletWeight = 0.8f;

        (Vector2 directionDistanceToTarget, float score1) = GetDirectionWithMinDistanceToTarget();
        (Vector2 directionAvoidGhosts, float score2) = GetDirectionAwayFromGhosts();
        (Vector2 directionPowerPellet, float score3) = GetDirectionWithMinDistanceToPowerPellet();
        Debug.Log("Direction 1 :" + directionDistanceToTarget + "Score 1 : " + score1);
        Debug.Log("Direction 2 :" + directionAvoidGhosts + "Score 2 : " + score2);
        Debug.Log("Direction 3 :" + directionPowerPellet + "Score 3 : " + score3);
        Vector2 nextDirection = Vector2.zero;

        // Combine all heuristics
        float weightedScore1 = score1 * distanceToTargetWeight;
        float weightedScore2 = score2 * avoidGhostsWeight;
        float weightedScore3 = score3 * powerPelletWeight;

        if (weightedScore1 > weightedScore2)
        {
            if (weightedScore1 > weightedScore3)
            {
                nextDirection = directionDistanceToTarget;
            }
        }
        if (weightedScore2 >= weightedScore1)
        {
            if (weightedScore2 >= weightedScore3)
            {
                nextDirection = directionAvoidGhosts;
            }
        }
        if (weightedScore3 > weightedScore1)
        {
            if (weightedScore3 > weightedScore2)
            {
                nextDirection = directionPowerPellet;
            }
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
            GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");
            foreach (var pellet in pellets)
            {

                float distanceToTarget = Vector3.Distance(pellet.transform.position, newPosition);

                // Calcul du score basé sur la distance à la cible (plus la distance est courte, plus le score est élevé)
                float score = 1 - (distanceToTarget / maxDistance); // Score normalisé entre 0 et 1

                // Mettre à jour la meilleure direction et le meilleur score si nécessaire
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = availableDirection;
                }
            }
        }

        return (bestDirection, bestScore);
    }

    private (Vector2, float) GetDirectionWithMinDistanceToPowerPellet()
    {
        float maxDistance = Vector3.Distance(Vector3.zero, new Vector3(20, 20, 0)); // Max possible distance
        //float minDistance = float.MaxValue;
        Vector2 bestDirection = Vector2.zero;
        float bestScore = 0;

        foreach (Vector2 availableDirection in availableDirections)
        {
            Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
            GameObject[] powerPellets = GameObject.FindGameObjectsWithTag("PowerPellet");
            foreach (var powerPellet in powerPellets)
            {

                float distanceToTarget = Vector3.Distance(powerPellet.transform.position, newPosition);

                // Calcul du score basé sur la distance à la cible (plus la distance est courte, plus le score est élevé)
                float score = 1 - (distanceToTarget / maxDistance); // Score normalisé entre 0 et 1

                // Mettre à jour la meilleure direction et le meilleur score si nécessaire
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = availableDirection;
                }
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
                bestDirection = -direction;
            }
            if (distanceToGhosts < 0.2f)
            {
                bestScore = 3;
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

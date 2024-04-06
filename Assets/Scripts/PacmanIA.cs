using UnityEngine;
using System.Collections.Generic;

public class PacmanAI : MonoBehaviour
{
    private Vector2 currentDirection;
    private Vector2 nextDirection;

    private List<Vector2> availableDirections = new List<Vector2>();
    private float maxDistanceToGhosts; // Nouvelle variable globale pour stocker la distance maximale aux fant�mes

    private Pacman pacman;
    private Pellet pellet;
    private PowerPellet powerPellet;
    private GameObject[] ghosts;


    private void Awake()
    {
        pacman = GetComponent<Pacman>();
        pellet = GetComponent<Pellet>();
        powerPellet = GetComponent<PowerPellet>();
        ghosts = GameObject.FindGameObjectsWithTag("Ghost");

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
            availableDirections.Clear(); // Effacer les anciennes directions disponibles
            availableDirections.AddRange(node.availableDirections); // Mettre � jour avec les nouvelles directions disponibles du n�ud
        }
    }

    private void Move(Vector2 direction)
    {
        // Move Pacman in the calculated direction
        pacman.movement.SetDirection(direction);
    }

    private Vector2 UpdateDirection()
    {
        float distanceToTargetWeight = 1.0f;
        float avoidGhostsWeight = 1.0f;
        float powerPelletWeight = 1.0f;

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

        if (weightedScore1 >= weightedScore2)
        {
            if (weightedScore1 >= weightedScore3)
            {
                nextDirection = directionDistanceToTarget;
            }else
            {
                nextDirection = directionPowerPellet;
            }
        }
        else if (weightedScore2 >= weightedScore1)
        {
            if (weightedScore2 >= weightedScore3)
            {
                nextDirection = directionAvoidGhosts;
            }
            else
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
            GameObject[] powerPellets = GameObject.FindGameObjectsWithTag("Pellet");
            foreach (var pellet in powerPellets)
            {

                float distanceToTarget = Vector3.Distance(pellet.transform.position, newPosition);

                // Calcul du score bas� sur la distance � la cible (plus la distance est courte, plus le score est �lev�)
                float score = 1 - (distanceToTarget / maxDistance); // Score normalis� entre 0 et 1

                // Mettre � jour la meilleure direction et le meilleur score si n�cessaire
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

                // Calcul du score bas� sur la distance � la cible (plus la distance est courte, plus le score est �lev�)
                float score = 1 - (distanceToTarget / maxDistance); // Score normalis� entre 0 et 1

                // Mettre � jour la meilleure direction et le meilleur score si n�cessaire
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
            print("IM INNN");
            print(direction);
            Vector3 newPosition = transform.position + new Vector3(direction.x, direction.y);
            float distanceToGhosts = CalculateDistanceToGhosts(newPosition);

            // Calcul du score de s�curit� (plus la distance aux fant�mes est grande, plus le score est �lev�)
            float score = 1 - (distanceToGhosts / maxDistanceToGhosts); // Score normalis� entre 0 et 1

            // Mettre � jour la meilleure direction et le meilleur score si n�cessaire
            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = direction;
            }
            else
            {
                bestDirection = -direction;
            }
        }

        return (bestDirection, 1-bestScore);
    }

    private float CalculateMaxDistanceToGhosts()
    {
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
        float minDistanceToGhosts = float.MaxValue;

        foreach (var ghost in ghosts)
        {
            float distanceToGhost = Vector3.Distance(position, ghost.transform.position);
            minDistanceToGhosts = Mathf.Min(minDistanceToGhosts, distanceToGhost);
        }

        return minDistanceToGhosts;
    }


}

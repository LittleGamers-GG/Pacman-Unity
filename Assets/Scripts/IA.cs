using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class IA : MonoBehaviour
{

    public GameObject target;
    private Pacman pacman;

    private void Awake()
    {
        pacman = GetComponent<Pacman>();
  
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Node node = other.GetComponent<Node>();
        print("I am in");
        // Do nothing while the ghost is frightened
        if (node != null)
        {
            Vector2 direction = Vector2.zero;
            float minDistance = float.MaxValue;
            print("I am in if");
            // Find the available direction that moves closet to pacman
            foreach (Vector2 availableDirection in node.availableDirections)
            {
                // If the distance in this direction is less than the current
                // min distance then this direction becomes the new closest
                Vector3 newPosition = transform.position + new Vector3(availableDirection.x, availableDirection.y);
                float distance = (target.transform.position - newPosition).sqrMagnitude;

                if (distance < minDistance)
                {
                    direction = availableDirection;
                    minDistance = distance;
                }
            }
            print("x:" + direction.x + "y:" + direction.y);
            pacman.movement.SetDirection(direction);
        }
    }
}

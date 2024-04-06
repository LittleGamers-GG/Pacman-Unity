using UnityEngine;

public class NodeCollisionHandler : MonoBehaviour
{
    public bool isNodeCollidingWithPacman = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        print("HEY IM IN");
        if (other.CompareTag("PacmanCollider"))
        {
            print("COLLIDER OKKK");
            isNodeCollidingWithPacman = true;
            // Mettre ici le code à exécuter lorsque le node entre en collision avec Pacman
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PacmanCollider"))
        {
            isNodeCollidingWithPacman = false;
            // Mettre ici le code à exécuter lorsque le node cesse de colliser avec Pacman
        }
    }

    // Méthode facultative pour vérifier si le node est en contact avec Pacman
    public bool IsNodeCollidingWithPacman()
    {
        return isNodeCollidingWithPacman;
    }
}

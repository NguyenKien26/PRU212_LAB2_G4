using UnityEngine;

public class Barrier : MonoBehaviour
{
    [SerializeField] private float jumpHeightThreshold = 0.5f; // Minimum height to consider the skier "jumping"
    [SerializeField] private string playerTag = "Player"; // Tag of the player object

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag(playerTag))
        {
            // Get the player's position and the barrier's position
            float playerHeight = other.transform.position.y;
            float barrierHeight = transform.position.y;

            // Check if the player is high enough to be considered jumping
            bool isJumping = playerHeight > barrierHeight + jumpHeightThreshold;

            if (!isJumping)
            {
                // Player failed to jump over the barrier, trigger game over
                GameManager.Instance.GameOver(GameManager.Instance.currentScore, GameManager.Instance.currentDistance);
            }
            // If the player is jumping, they clear the barrier and no action is needed
        }
    }
}
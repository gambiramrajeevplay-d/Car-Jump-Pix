using UnityEngine;

public class JumpTrigger : MonoBehaviour
{
    public enum JumpDirection
    {
        Straight,
        Left,
        Right
    }
    
    public JumpDirection jumpDirection;

    public float jumpheight = 10f;

    public float jumpAngle = 35f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        BoostTrigger boost =
            other.GetComponent<BoostTrigger>();

        if (boost == null)
            return;

        Vector3 dir = other.transform.forward;

        if (jumpDirection == JumpDirection.Left)
        {
            dir = Quaternion.Euler(
                0f,
                -jumpAngle,
                0f
            ) * other.transform.forward;
        }
        else if (jumpDirection == JumpDirection.Right)
        {
            dir = Quaternion.Euler(
                0f,
                jumpAngle,
                0f
            ) * other.transform.forward;
        }
        boost.Jump(dir.normalized, jumpheight);
    }
}
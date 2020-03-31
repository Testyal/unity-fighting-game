using UnityEngine;

[ExecuteInEditMode]
public class MaintainGroundDistance : MonoBehaviour
{
    [SerializeField] private float groundDistance;
    
    private void Update()
    {
        RaycastHit raycast;
        if (Physics.Raycast(transform.position, Vector3.down, out raycast))
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.blue);

            float distance = Vector3.Magnitude(raycast.point - transform.position);
            
            transform.Translate((groundDistance - distance) * Vector3.up);
        }
    }
}

using UnityEngine;

public abstract class BaseState : MonoBehaviour
{
    // Implement these in every state
    public virtual void EnableState()
    {
        this.gameObject.SetActive(true);
    }
    public virtual void DisableState()
    {
        this.gameObject.SetActive(false);
    }
}

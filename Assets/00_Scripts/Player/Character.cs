using Unity.Netcode;
using UnityEngine;

public class Character : NetworkBehaviour
{
    protected Animator animator;
    protected new SpriteRenderer renderer;

    public virtual void Awake()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        renderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    public void OrderChange(int value)
    {
        renderer.sortingOrder = value;
    }

    public void GetInitCharacter(string controller, string rarity)
    {
        animator.runtimeAnimatorController = Resources.Load<Hero_Scriptable>("Character_Scriptable/"+ rarity +"/"+ controller).m_animator;
    }

    protected void AnimatorChanage(string temp, bool Trigger)
    {

        if (Trigger)
        {
            animator.SetTrigger(temp);
            return;
        }
        animator.SetBool("IDLE", false);
        animator.SetBool("MOVE", false);        
        animator.SetBool(temp, true);       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

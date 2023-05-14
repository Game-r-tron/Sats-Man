using UnityEngine;

public class PowerPellet : Pellet
{
    public float duration = 8f;

    protected override void Eat()
    {
        FindAnyObjectByType<GameManager>().PowerPelletEaten(this);
    }

}

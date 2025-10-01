using UnityEngine;
public interface EnemyInterface
{
    int hitCounter { get; set; }
    
    float health { get; set; }
    public float maxHealth { get; set; }

    public Vector3 spawn { get; set; }

}


using UnityEngine;

public class FistWeapon : MeleeWeapon
{
    [Header("Alternance des poings")]
    public bool useLeftFist = true;

    protected override void PerformAttack()
    {
        base.PerformAttack();

        useLeftFist = !useLeftFist;
    }

    public bool IsLeftFist()
    {
        return useLeftFist;
    }

    public bool IsRightFist()
    {
        return !useLeftFist;
    }
}


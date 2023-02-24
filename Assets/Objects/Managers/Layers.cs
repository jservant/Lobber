using UnityEngine;

public enum Layers : int {
    Default = 0,
    TransparentFX,
    IgnoreRaycast,
    Unused1,
    Water,
    UI,
    EnemyHitbox,
    EnemyHurtbox,
    PlayerHitbox,
    PlayerHurtbox,
}

public class Mask {
    public static int Get(Layers[] array) {
        int Result = 0;
        foreach (Layers layer in array) {
            Result |= 1<<((int)layer);
        }
        return Result; 
    }
    public static int Get(Layers layer) {
        return Get(new Layers[]{layer});
    }
}

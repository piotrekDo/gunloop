using System;
using UnityEngine;

public class GameEvents {
    private static GameEvents m_instance;

    public static GameEvents Instance {
        get {
            m_instance ??= new GameEvents();
            return m_instance;
        }
    }

    public event Action<GunController> onGunpickup;
    public event Action<Vector2, float> onNoise;
    public event Action onZombieDies;

    public void ZombieDies() {
       onZombieDies?.Invoke();
    }

    public void PickupGun(GunController gun) {
        onGunpickup?.Invoke(gun);
    }

    public void MakeNoise(Vector2 position, float radius) {
        onNoise?.Invoke(position, radius);
    }
}

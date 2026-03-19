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

    public void PickupGun(GunController gun) {
        onGunpickup?.Invoke(gun);
    }
}

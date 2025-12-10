using UnityEngine;
using UnityEngine.InputSystem;

public class AttachWeapon : MonoBehaviour
{
    public GameObject weaponSprite;
    private CharacterControls controls;
    private bool equipped = false;

    private void Awake()
    {
        // Set up input controls and hook the equip action
        controls = new CharacterControls();

        controls.Player.EquipWeapon.performed += ctx => ToggleWeapon();
    }

    private void OnEnable()
    {
        // Enable input listening when the object becomes active
        controls.Enable();
    }

    private void OnDisable()
    {
        // Disable input to avoid leaks when the object is inactive
        controls.Disable();
    }

    void ToggleWeapon()
    {
        // Toggle equipped flag and show/hide the weapon sprite
        equipped = !equipped;
        weaponSprite.SetActive(equipped);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class AttachWeapon : MonoBehaviour
{
    public GameObject weaponSprite;
    private CharacterControls controls;
    private bool equipped = false;

    private void Awake()
    {
        controls = new CharacterControls();

        controls.Player.EquipWeapon.performed += ctx => ToggleWeapon();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void ToggleWeapon()
    {
        equipped = !equipped;
        weaponSprite.SetActive(equipped);
    }
}

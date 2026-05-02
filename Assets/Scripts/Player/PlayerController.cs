using Sirenix.OdinInspector;

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [LabelText("虚拟摇杆")]
    [SerializeField] private Joystick _joystick;

    private CharacterController _characterController;
    private float _verticalVelocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float horizontal = _joystick.Horizontal;
        float vertical = _joystick.Vertical;

        Vector3 moveDir = new Vector3(horizontal, 0f, vertical);

        // 重力累积
        if (_characterController.isGrounded)
        {
            _verticalVelocity = -1f; // 保持贴地，避免 isGrounded 抖动
        }
        else
        {
            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        Vector3 motion = moveDir * CommonConfigSO.Instance.PlayerMoveSpeed * Time.deltaTime;
        motion.y = _verticalVelocity * Time.deltaTime;
        _characterController.Move(motion);

        // 有输入时平滑朝向移动方向
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                CommonConfigSO.Instance.PlayerRotateSpeed * Time.deltaTime
            );
        }
    }
}

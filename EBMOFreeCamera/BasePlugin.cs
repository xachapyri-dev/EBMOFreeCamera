using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using EBMOModCoreAPI;

namespace EBMOFreeCamera
{
    [BepInPlugin("com.xachapyridev.ebmofreecamera", "EBMO Free Camera", "1.0.0")]
    [BepInDependency("com.xachapuridev.ebmomodcoreapi")]
    public class FreeCameraPlugin : BaseUnityPlugin
    {
        [Header("Настройки управления")]
        public KeyCode ToggleKey = KeyCode.F5;
        public float MoveSpeed = 8.0f;
        public float FastMultiplier = 2.5f;
        public float LookSensitivity = 2.5f;
        private bool _isActive = false;
        private float _yaw = 0f;
        private float _pitch = 0f;
        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Logger.LogInfo("Free Camera Addon успешно инициализирован! Нажмите F5 для переключения.");
            NotificationUI.Initialize();
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DisableFreeCam();
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_isActive)
            {
                DisableFreeCam();
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey))
            {
                ToggleFreeCam();
            }
            if (!_isActive) return;
            if (!PlayerCameraAPI.IsAvailable)
            {
                DisableFreeCam();
                return;
            }
            HandleRotation();
            HandleMovement();
        }
        private void ToggleFreeCam()
        {
            if (_isActive)
            {
                DisableFreeCam();
                NotificationUI.Show("Свободная камера: ВЫКЛ", NotificationType.Info);
            }
            else
            {
                if (!PlayerCameraAPI.IsAvailable)
                {
                    NotificationUI.Show("Камера недоступна на этой сцене", NotificationType.Warning);
                    return;
                }
                EnableFreeCam();
                NotificationUI.Show("Свободная камера: ВКЛ (F5)", NotificationType.Info);
            }
        }
        private void EnableFreeCam()
        {
            _isActive = true;
            PlayerCameraAPI.IsFollowEnabled = false;
            PlayerAPI.BlockControl();
            Vector3 currentEuler = PlayerCameraAPI.Transform.eulerAngles;
            _yaw = currentEuler.y;
            _pitch = currentEuler.x;
            if (_pitch > 180f) _pitch -= 360f;
        }
        private void DisableFreeCam()
        {
            _isActive = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (PlayerCameraAPI.IsAvailable)
            {
                PlayerCameraAPI.FollowActiveCharacter();
                PlayerCameraAPI.IsFollowEnabled = true;
            }
            PlayerAPI.UnblockControl();
        }
        private void HandleRotation()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * LookSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * LookSensitivity;
                _yaw += mouseX;
                _pitch -= mouseY;
                _pitch = Mathf.Clamp(_pitch, -89f, 89f);
                PlayerCameraAPI.Transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }
            if (Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        private void HandleMovement()
        {
            Transform camTransform = PlayerCameraAPI.Transform;
            Vector3 direction = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) direction += camTransform.forward;
            if (Input.GetKey(KeyCode.S)) direction -= camTransform.forward;
            if (Input.GetKey(KeyCode.D)) direction += camTransform.right;
            if (Input.GetKey(KeyCode.A)) direction -= camTransform.right;
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E)) direction += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q)) direction -= Vector3.up;
            float speed = MoveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed *= FastMultiplier;
            }
            if (direction != Vector3.zero)
            {
                camTransform.position += direction.normalized * speed * Time.unscaledDeltaTime;
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {
    [SerializeField] protected Camera target;
    Vector3 moveTarget = Vector3.zero;
    Vector3 rotateTarget = new Vector3(0, 0, 1);

    public enum MouseButton { Left = 0, Right = 1, Middle = 2 }
    public enum MouseMove { X = 0, Y = 1, ScrollWheel = 2 }

    static readonly string[] mouseKeywords = new string[] {
    "Mouse X",
    "Mouse Y",
    "Mouse ScrollWheel"
    };

    [SerializeField] protected bool controllable;

    public MouseMove zoomTrigger = MouseMove.ScrollWheel;
    public bool enableZoom = true;
    public bool invertZoomDirection = false;
    public float zoomSpeed = 6f;
    public bool limitZoomX = false;
    public bool limitZoomY = false;
    public bool limitZoomZ = false;
    public bool smoothZoom = true;
    public float smoothZoomSpeed = 10f;

    public MouseButton rotateTrigger = MouseButton.Right;
    public bool enableRotate = true;
    public bool invertRotateDirection = false;
    public float rotateSpeed = 3f;
    public bool limitRotateX = false;
    public bool limitRotateY = false;
    public bool smoothRotate = true;
    public float smoothRotateSpeed = 10f;

    public MouseButton dragTrigger = MouseButton.Middle;
    public bool enablePan = true;
    public bool invertPanDirection = false;
    public float dragSpeed = 3f;
    public bool limitPanX = false;
    public bool limitPanY = false;
    public bool smoothPan = true;
    public float smoothPanSpeed = 10f;

    public void Activate() {
        target.enabled = true;
        controllable = true;
    }

    public void Deactivate() {
        target.enabled = false;
        controllable = false;
    }

    protected void OnEnable() {
        if (target == null) {
            target = Camera.main;
        }
    }

    protected void Start() {
        Reset();
    }

    public void Reset() {
        this.moveTarget = target.transform.position;
        this.rotateTarget = target.transform.forward;
    }

    protected void Update() {
        if ( !controllable || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) ) return; 
        Zoom();
        Rotate();
        Pan();
    }

    protected void Zoom() {
        if (!this.enableZoom) return; 
        float moveAmount = Input.GetAxis(mouseKeywords[(int)this.zoomTrigger]);

        if (moveAmount != 0) {
            float direction = this.invertZoomDirection ? -1 : 1;
            this.moveTarget = target.transform.forward;
            this.moveTarget *= this.zoomSpeed * moveAmount * direction;
            this.moveTarget += target.transform.position;

            if (this.limitZoomX) this.moveTarget.x = target.transform.position.x;
            if (this.limitZoomY) this.moveTarget.y = target.transform.position.y;
            if (this.limitZoomZ) this.moveTarget.z = target.transform.position.z;
        }

        if (this.smoothZoom) {
            if (this.moveTarget == target.transform.position) this.moveTarget = target.transform.position;
            target.transform.position = Vector3.Lerp(target.transform.position, this.moveTarget, Time.deltaTime * this.smoothZoomSpeed);
        } else {
            target.transform.position = moveTarget;
        }
    }

    protected void Rotate() {
        if (!this.enableRotate) return;

        float direction = this.invertRotateDirection ? -1 : 1;
        float mouseX = Input.GetAxis(mouseKeywords[(int)MouseMove.X]) * direction;
        float mouseY = Input.GetAxis(mouseKeywords[(int)MouseMove.Y]) * direction;

        if (Input.GetMouseButton((int)this.rotateTrigger)) {
            if (!this.limitRotateX) this.rotateTarget = Quaternion.Euler(0, mouseX * this.rotateSpeed, 0) * this.rotateTarget;
            if (!this.limitRotateY) this.rotateTarget = Quaternion.AngleAxis(mouseY * this.rotateSpeed, Vector3.Cross(target.transform.forward, Vector3.up)) * this.rotateTarget;
        }

        if (this.smoothRotate) {
            target.transform.rotation = Quaternion.Slerp(target.transform.rotation, Quaternion.LookRotation(this.rotateTarget), Time.deltaTime * this.smoothRotateSpeed);
        } else {
            target.transform.rotation = Quaternion.LookRotation(this.rotateTarget);
        }
    }

    protected void Pan() {
        if (!this.enablePan) return;

        float direction = this.invertPanDirection ? 1 : -1;
        float mouseX = Input.GetAxis(mouseKeywords[(int)MouseMove.X]) * direction;
        float mouseY = Input.GetAxis(mouseKeywords[(int)MouseMove.Y]) * direction;

        if (Input.GetMouseButton((int)this.dragTrigger)) {
            this.moveTarget = target.transform.position;
            if (!this.limitPanX) this.moveTarget += target.transform.right * mouseX * dragSpeed;
            if (!this.limitPanY) this.moveTarget += target.transform.up * mouseY * dragSpeed;
        }

        if (this.smoothPan) {
            target.transform.position = Vector3.Lerp(target.transform.position, this.moveTarget, Time.deltaTime * this.smoothPanSpeed);
        } else {
            target.transform.position = this.moveTarget;
        }
    }
}
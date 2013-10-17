using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	
	public int EdgeThreshold = 25,
				CameraMoveSpeed = 50,
				MinimumY = 5,
				MaximumY = 50;
	public float CameraScrollMultiplier = 2f,
				CameraRotateMultiplier = 3f;
	
	
	private int screenWidth, screenHeight;
	private Vector3 homePosition = Vector3.zero;
	private GameObject playerObject;

	// Use this for initialization
	void Start () {
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		playerObject = this.transform.parent.gameObject;
		homePosition = playerObject.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(KeyCode.Home)) {
			playerObject.transform.position = homePosition;			
			return;
		}
		
		Vector3 mousePos = Input.mousePosition;
		float deltaTime = Time.deltaTime;
		Vector3 cameraVector = Vector3.zero;
		
		if (Input.GetMouseButton(1)) {
			float rotateVelocity = Input.GetAxis("Mouse X") * CameraMoveSpeed * CameraRotateMultiplier * deltaTime;
			playerObject.transform.Rotate(0, rotateVelocity, 0, Space.World);
		}
		
		Vector3 edgeMove = edgeCameraMove(mousePos, deltaTime);
		if (edgeMove != Vector3.zero) {
			cameraVector += edgeMove;
		}
		
		Vector3 scrollMove = scrollCameraMove(deltaTime);
		if (scrollMove != Vector3.zero) {
			cameraVector += scrollMove;
		}
		
		playerObject.transform.Translate(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"), Space.Self);
		playerObject.transform.Translate(cameraVector, Space.World);
	}
	
	private Vector3 scrollCameraMove(float deltaTime) {
		Vector3 cameraVelocity = Vector3.zero;
		
		if (Input.GetAxis("Mouse ScrollWheel") < 0) { // back 
			if (this.transform.position.y < MaximumY) {
				cameraVelocity = -this.transform.forward * CameraScrollMultiplier * CameraMoveSpeed * deltaTime;
			}
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0) { // forward
			if (this.transform.position.y > MinimumY) {
				cameraVelocity = this.transform.forward * CameraScrollMultiplier * CameraMoveSpeed * deltaTime;	 
			}
		}		
		
		return cameraVelocity;
	}
	
	private Vector3 edgeCameraMove(Vector3 mousePos, float deltaTime) {
		Vector3 cameraVelocity = Vector3.zero;

		if (mousePos.x > screenWidth - EdgeThreshold) {
			cameraVelocity = playerObject.transform.right * CameraMoveSpeed * deltaTime;	
		}
		else if (mousePos.x < EdgeThreshold) {
			cameraVelocity = -playerObject.transform.right * CameraMoveSpeed * deltaTime;
		}
		
		if (mousePos.y > screenHeight - EdgeThreshold) {
			cameraVelocity = playerObject.transform.forward * CameraMoveSpeed * deltaTime;
		}
		else if (mousePos.y < EdgeThreshold) {
			cameraVelocity = -playerObject.transform.forward * CameraMoveSpeed * deltaTime;
		}
		
		return cameraVelocity;		
	}
}

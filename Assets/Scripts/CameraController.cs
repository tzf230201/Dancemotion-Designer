using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;          // Objek pusat untuk orbit kamera
    public float orbitSpeed = 5f;     // Kecepatan rotasi kamera
    public float panSpeed = 0.2f;     // Kecepatan geser kamera
    public float zoomSpeed = 5f;     // Kecepatan zoom kamera
    public float minDistance = 2f;    // Jarak minimal kamera ke target
    public float maxDistance = 50f;   // Jarak maksimal kamera ke target

    private float distance;           // Jarak kamera dari target
    private Vector3 currentRotation;  // Rotasi kamera saat ini
    private Vector3 targetPosition;   // Posisi target yang bisa digeser

    public TextMeshProUGUI debug_text;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject player;
    private Vector3 offset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        debug_text.text = "hello world";
        

        if (target == null)
        {
            Debug.LogError("Target belum diset. Pastikan kamu menetapkan objek untuk diorbit.");
            return;
        }

        // Hitung jarak awal kamera dari target
        distance = Vector3.Distance(transform.position, target.position);

        // Simpan rotasi awal kamera
        currentRotation = transform.eulerAngles;

        // Simpan posisi awal target
        targetPosition = target.position;
        offset = target.position - player.transform.position;
    }


    // Update is called once per frame
    void LateUpdate()
{

        debug_text.text = "Time : " + Time.time.ToString();

        if (target == null) return;

        // Orbit kamera dengan klik kanan
        if (Input.GetMouseButton(1)) // Klik kanan
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentRotation.x -= mouseY * orbitSpeed;
            currentRotation.y += mouseX * orbitSpeed;
        }

        // Geser area pandang (pan) dengan tombol tengah mouse
        if (Input.GetMouseButton(2)) // Klik tengah
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Hitung offset pan berdasarkan arah kamera
            Vector3 panOffset = (transform.right * -mouseX * panSpeed) + (transform.up * -mouseY * panSpeed);

            // Geser posisi target (kamera tetap mengikuti target)
            targetPosition += panOffset;
        }

        // Zoom kamera dengan scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Update posisi kamera berdasarkan target, rotasi, dan jarak
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        Vector3 position = rotation * new Vector3(0, 0, -distance) + targetPosition;

        transform.rotation = rotation;
        transform.position = position;

        offset = target.position - player.transform.position;

        Vector3 cur_pose = transform.position;
        

        position.y = cur_pose.y - offset.y;
        
        transform.position = position;
    }
}

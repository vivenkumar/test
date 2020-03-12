using UnityEngine;

public class DragAndDropPC : MonoBehaviour
{
    public bool GlowWhileInteracting = true;
    public bool TranslucentWhileDragging = true;

    Camera mainCamera;
    public float scrollScale = 0.3f;
    public float glowSpeed = 0.3f;
    public float glowThreshold = 0.2f;
    float distanceFromCamera;

    bool isDragging = false;
    bool isHovering = false;
    bool lookAtCameraInit = false;
    bool lookAtCameraLoop = false;
    Material oldMaterial, translucentMaterial, glowMaterial;
    MeshRenderer renderer;
    Rigidbody rb;
    Color color;
    private float HSV_Hue;
    private float HSV_Saturation;
    private float HSV_Value;
    private float HSV_ValueInitial;

    private int HSV_Counter;

    Quaternion oldRotCam = Quaternion.identity;
    Quaternion deltaRotCam = Quaternion.identity;
    Quaternion oldRotObj = Quaternion.identity;
    Quaternion deltaRotObj = Quaternion.identity;

    GameObject EmptyParentGameObject;
    float TranslateSelectedObjectUsingScroll;
    
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mainCamera.transform.hasChanged = false;
        renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            oldMaterial = renderer.material;
            translucentMaterial = new Material(oldMaterial);
            translucentMaterial.color = new Color(oldMaterial.color.r, oldMaterial.color.g, oldMaterial.color.b, 0.5f);
            glowMaterial = new Material(translucentMaterial);

            color = oldMaterial.color;
            Color.RGBToHSV(color, out HSV_Hue, out HSV_Saturation, out HSV_Value);
            HSV_ValueInitial = HSV_Value;
        }
        rb = GetComponent<Rigidbody>();
        //TranslateSelectedObjectUsingScroll = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        deltaRotCam = mainCamera.transform.rotation * Quaternion.Inverse(oldRotCam);
        oldRotCam = mainCamera.transform.rotation;
        if (Input.GetMouseButton(1))
        {
            SmoothenedMouseLook.stopMouseLook = false;
            if (!lookAtCameraLoop)
            {
                if(EmptyParentGameObject != null)
                {
                    oldRotObj = EmptyParentGameObject.gameObject.transform.rotation;
                }
                lookAtCameraInit = true;
            }
        }
        else
        {
            SmoothenedMouseLook.stopMouseLook = true;
            lookAtCameraInit = false;
            lookAtCameraLoop = false;
        }
        
        if (isDragging || isHovering) HSV_Value += glowSpeed * Time.deltaTime;
        else HSV_Value -= glowSpeed * Time.deltaTime;        
        
        HSV_Value = Mathf.Clamp(HSV_Value, HSV_ValueInitial, HSV_ValueInitial + glowThreshold);
        color = Color.HSVToRGB(HSV_Hue, HSV_Saturation, HSV_Value);
        glowMaterial.color = color;
        
        // To use scroll to move selected game object back and forth
        //if(Input.GetAxis("Mouse ScrollWheel") != 0)
        //{
        //    //TranslateSelectedObjectUsingScroll = Input.GetAxis("Mouse ScrollWheel");
        //    //TranslateSelectedObjectUsingScroll *= Time.deltaTime;
        //}
    }

    //private void FixedUpdate()
    //{
    //    //if (lookAtCameraInit)
    //    //{
    //    //    lookAtCameraInit = false;
    //    //    transform.LookAt(mainCamera.transform.position);
    //    //    deltaRotObj = transform.rotation * Quaternion.Inverse(oldRotObj);
    //    //    lookAtCameraLoop = true;
    //    //}

    //    //if (lookAtCameraLoop)
    //    //{
    //    //    transform.LookAt(mainCamera.transform.position);
    //    //    deltaRotObj.eulerAngles = new Vector3(0, deltaRotObj.eulerAngles.y, 0);
    //    //    transform.rotation *= deltaRotObj;
    //    //    transform.rotation = Quaternion.Euler(new Vector3(oldRotObj.eulerAngles.x, transform.rotation.eulerAngles.y, oldRotObj.eulerAngles.z));
    //    //}
    //}

    private void OnMouseDown()
    {
        EmptyParentGameObject = new GameObject(this.gameObject.name + "Parent");
        EmptyParentGameObject.gameObject.transform.position = this.gameObject.transform.position;
        EmptyParentGameObject.gameObject.transform.rotation = this.gameObject.transform.rotation;

        Vector3 targetPostition = new Vector3(mainCamera.transform.position.x, EmptyParentGameObject.transform.position.y, mainCamera.transform.position.z);
        EmptyParentGameObject.gameObject.transform.LookAt(targetPostition);
        this.gameObject.transform.parent = EmptyParentGameObject.gameObject.transform;
        
        isDragging = true;
        distanceFromCamera = (EmptyParentGameObject.gameObject.transform.position - mainCamera.transform.position).magnitude;
        if (TranslucentWhileDragging && this.renderer != null)
        {
            if (!GlowWhileInteracting) this.renderer.material = translucentMaterial;
        }
    }

    
   private void OnMouseDrag()
    {
        rb.useGravity = false;
        if (rb != null) rb.Sleep();
        distanceFromCamera += Input.mouseScrollDelta.y * scrollScale;
        //distanceFromCamera += TranslateSelectedObjectUsingScroll; 
        Vector3 zFromCamera = (EmptyParentGameObject.gameObject.transform.position - mainCamera.transform.position).normalized;
        EmptyParentGameObject.gameObject.transform.position = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane)) + Mathf.Clamp(distanceFromCamera, 1, 200f) * zFromCamera;
        Vector3 targetPostition = new Vector3(mainCamera.transform.position.x, EmptyParentGameObject.transform.position.y, mainCamera.transform.position.z);
        EmptyParentGameObject.gameObject.transform.LookAt(targetPostition);
    }

    private void OnMouseUp()
    {
        rb.useGravity = true;
        isDragging = false;
        if (TranslucentWhileDragging && renderer != null)
        {
            if (!GlowWhileInteracting) renderer.material = oldMaterial;
        }
        EmptyParentGameObject.gameObject.transform.DetachChildren();
        Destroy(EmptyParentGameObject);
    }

    private void OnMouseOver()
    {
        isHovering = true;
        if (GlowWhileInteracting && renderer != null) renderer.material = glowMaterial;        
    }

    private void OnMouseExit()
    {
        isHovering = false;
        if (GlowWhileInteracting && renderer != null) renderer.material = glowMaterial;
    }
}

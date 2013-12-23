


/*
OculusStereoGui code by OculusVR
Rearranged by Daniel "Dakor" Korgel

Renders Gui Elements in Stereoscopic 3D if placed below the marked line.
If you want other Elements than the StereoBox, write a new class which 
extends the OVRGui Class and place your own stereoscopic elements there
*/

using UnityEngine;
using System.Collections;
public class HUDScript: MonoBehaviour {
   
	private static float RADIANS_TO_DEGREES = 180.0f / Mathf.PI;
	private static float BAR_LENGTH_FACTOR = 0.01f;
   	private Dataread dataObj;
   	private OVRGUI        	GuiHelper        = new OVRGUI();
   	private GameObject      GUIRenderObject  = null;
   	private RenderTexture   GUIRenderTexture = null;
   
	private static string lookAheadString = "Obstacle Ahead!";
	private static string lookRightString = "Take a left";
	private static string lookLeftString = "Take a right";
	private static string lookBehindString = "Obstacle Behind";
	
   	// Handle to OVRCameraController
   	private 		OVRCameraController CameraController = null;
   	public Font    	FontReplace = null;
   	
	// Helath, stamina, and score strings to be displayed on the HUD
	private string healthString;
	private string staminaString;
	private string scoreString;
	private float healthBarLength;
	private float staminaBarLength;
	private float healthBarDefault = 100.0f; // 100.0f
	
	// PowerUps
	private bool healthPUStored;
	private bool staminaPUstored;
	
	// Bubbles
	private int bubbleNum;
	
	// Timer
	private float timer;
	private string timeString;
	private float startTime;
	
	// Icons
	public Texture healthIcon, staminaIcon, bubblesIcon, scoreIcon;
	public Texture bubbleIconColor, bubbleIconGrey;
	public Texture healthPUIconColor, healthPUIconGrey;
	public Texture staminaPUIconColor, staminaPUIconGrey;
	public Texture timeIcon;
	public Texture healthBar, staminaBar;
	
	public Texture rightArrow, leftArrow;
		
	
    // Awake
	void Awake() {
		
		CameraController = GetComponentInChildren(typeof(OVRCameraController)) as OVRCameraController;
		
		if(CameraController==null){
			Debug.LogWarning("Single Method Failed!");
		
		// Find camera controller
		OVRCameraController[] CameraControllers;
		CameraControllers = gameObject.GetComponentsInChildren<OVRCameraController>();
		
		
		if(CameraControllers.Length == 0)
			Debug.LogWarning("OVRMainMenu: No OVRCameraController attached.");
		else if (CameraControllers.Length > 1)
			Debug.LogWarning("OVRMainMenu: More then 1 OVRCameraController attached.");
		else
			CameraController = CameraControllers[0];
		}
	}
   
   // Use this for initialization
   void Start () {
		
		GameObject gl = GameObject.Find("Global");
		dataObj  = gl.GetComponent<Dataread>();
	
		healthBarLength = healthBarDefault ;//* (float)((float)globalObj.currentHealth/(float)globalObj.maxHealth);
	 	staminaBarLength = healthBarDefault ;//* (float)((float)globalObj.currentStamina/(float)globalObj.maxStamina);
		
		startTime = Time.time;
		timer = startTime;
		
		int minutes = (int)(timer / 60);
   		int seconds = (int)(timer % 60);
 
   		timeString = string.Format("{0:00}:{1:00}", minutes, seconds);
	
        // Ensure that camera controller variables have been properly
      	// initialized before we start reading them
      	if(CameraController != null)
      	{
         	CameraController.InitCameraControllerVariables();
         	GuiHelper.SetCameraController(ref CameraController);
      	}
      
      	// Set the GUI target 
      	GUIRenderObject = GameObject.Instantiate(Resources.Load("OVRGUIObjectMain")) as GameObject;
      
      	if(GUIRenderObject != null)
      	{
         	if(GUIRenderTexture == null)
         	{
            	int w = Screen.width;
            	int h = Screen.height;

            	if(CameraController.PortraitMode == true)
            	{
               		int t = h;
               		h = w;
               		w = t;
            	}
            
            	// We don't need a depth buffer on this texture
            	GUIRenderTexture = new RenderTexture(w, h, 0);   
            	GuiHelper.SetPixelResolution(w, h);
            	GuiHelper.SetDisplayResolution(OVRDevice.HResolution, OVRDevice.VResolution);
         	}
      	}
      
      	// Attach GUI texture to GUI object and GUI object to Camera
      	if(GUIRenderTexture != null && GUIRenderObject != null)
      	{
         	GUIRenderObject.renderer.material.mainTexture = GUIRenderTexture;
         
         	if(CameraController != null)
         	{
            	// Grab transform of GUI object
            	Transform t = GUIRenderObject.transform;
            	// Attach the GUI object to the camera
            	CameraController.AttachGameObjectToCamera(ref GUIRenderObject);
            	// Reset the transform values (we will be maintaining state of the GUI object
            	// in local state)
            	OVRUtils.SetLocalTransform(ref GUIRenderObject, ref t);
            	// Deactivate object until we have completed the fade-in
            	// Also, we may want to deactive the render object if there is nothing being rendered
            	// into the UI
            	// we will move the position of everything over to the left, so get
            	// IPD / 2 and position camera towards negative X
            	Vector3 lp = GUIRenderObject.transform.localPosition;
            	float ipd = 0.0f;
            	CameraController.GetIPD(ref ipd);
            	lp.x -= ipd * 0.5f;
            	GUIRenderObject.transform.localPosition = lp;
            
            	GUIRenderObject.SetActive(false);
         	}
      	}
   	}
   
	float calculateBarLength() {
		
		float minimum = 200000.0f;
		float angleInDegrees = 0;
		for (int i = 0; i < dataObj.numberOfEntries; i++) {
			angleInDegrees = dataObj.angles[i] * RADIANS_TO_DEGREES;

			if (angleInDegrees > 45 && angleInDegrees <= 145) {
				if(dataObj.distances[i] < minimum)
					minimum = dataObj.distances[i];
			}
		}
		Debug.Log ("Minimum Distance: " + minimum);
		return (150.0f - BAR_LENGTH_FACTOR * minimum);
	}

   // Update is called once per frame
   void Update () {
		
		timer = Time.time - startTime;
		
		healthBarLength = healthBarDefault;
	 	staminaBarLength = healthBarDefault ;//* (float)((float)globalObj.currentStamina/(float)globalObj.maxStamina);
				
		int minutes = (int)(timer / 60);
   		int seconds = (int)(timer % 60);
 
   		timeString = string.Format("{0:00}:{1:00}", minutes, seconds);
   }
   
   void OnGUI () {
		
    	// Important to keep from skipping render events
      	if (Event.current.type != EventType.Repaint)
         	return;

      	// We can turn on the render object so we can render the on-screen menu
      	if(GUIRenderObject != null)
      	{
         	GUIRenderObject.SetActive(true);
      	}
      	// Set the GUI matrix to deal with portrait mode
      	Vector3 scale = Vector3.one;
      	if(CameraController.PortraitMode == true)
      	{
         	float h = OVRDevice.HResolution;
         	float v = OVRDevice.VResolution;
         	scale.x = v / h;                // calculate hor scale
         	scale.y = h / v;                // calculate vert scale
      	}
   		Matrix4x4 svMat = GUI.matrix; // save current matrix
       	// substitute matrix - only scale is altered from standard
       	GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
      
      	// Cache current active render texture
      	RenderTexture previousActive = RenderTexture.active;
      
      	// if set, we will render to this texture
      	if(GUIRenderTexture != null)
      	{
         	RenderTexture.active = GUIRenderTexture;
         	GL.Clear (false, true, new Color (0.0f, 0.0f, 0.0f, 0.0f));
      	}
      
      	// Update OVRGUI functions (will be deprecated eventually when 2D renderingc
      	// is removed from GUI)
      	GuiHelper.SetFontReplace(FontReplace);
      
      	if(CameraController != null) {
			
			Color uiColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
			Color uiColor2 = new Color(0.5f, 0.5f, 0.5f, 0.5f);
			Color uiColor3 = new Color(0.5f, 0.5f, 0.5f, 0.5f);
					
			GuiHelper.StereoBox(600, 430, 100, 40, ref timeString, Color.white);
			GuiHelper.StereoBox(600, 220, 100, 40, ref dataObj.errorString, Color.white);
						
			if (dataObj.tooClose.Length > 0 && dataObj.angles.Length > 0)
				displayTooCloseMessages(dataObj.tooClose, dataObj.angles);
        }

        // Restore active render texture
      	RenderTexture.active = previousActive;
      
      	// ***
      	// Restore previous GUI matrix
      	GUI.matrix = svMat; 
   	}
	
	
	void displayTooCloseMessages(bool[] tooCloseArray, float[] angles) {
		//Debug.Log("TOO CLOSE ARRAY: ");
		for (int i = 0; i < dataObj.numberOfEntries; i++) {
			
			float angleInDegrees = angles[i] * RADIANS_TO_DEGREES;
			
			Debug.Log("[" + i + "]" + tooCloseArray[i] + "angle: " + angleInDegrees);
			
			if( (angleInDegrees <= 135.0f && angleInDegrees > 45.0f) && tooCloseArray[i])  {
				
				GuiHelper.StereoBox(600, 330, 100, 40, ref lookAheadString, Color.white);
				return;
			}
			
			else if((angleInDegrees > 135.0f && angleInDegrees <= 225.0f) && tooCloseArray[i]) {
				GuiHelper.StereoDrawTexture(400, 100, 150, 50, ref rightArrow, new Color(0.5f, 0.5f, 0.5f, 0.7f));
				GuiHelper.StereoBox(575, 100, 100, 50, ref lookLeftString, Color.white);
				return;
			}
			
			else if( (angleInDegrees > 225.0f && angleInDegrees <= 315.0f) && tooCloseArray[i]) {
				//GuiHelper.StereoBox(600, 330, 100, 40, ref lookBehindString, Color.white);
				return;
			}
			
			else if ( ((angleInDegrees > 315.0f && angleInDegrees <= 360.0f) || 
					  (angleInDegrees > 0.0f && angleInDegrees <= 45.0f))  && 
					  tooCloseArray[i]) {
				
				GuiHelper.StereoDrawTexture(700, 100, 150, 50, ref leftArrow, new Color(0.5f, 0.5f, 0.5f, 0.7f));
				GuiHelper.StereoBox(575, 100, 100, 50, ref lookRightString, Color.white);
				return;
			}
		}
	}
	
	
	/*
	void displayPowerUpIcons()
	{
		if(healthPUStored)
			GuiHelper.StereoDrawTexture(640, 145, 30, 30, ref healthPUIconColor, Color.grey);
		else
			GuiHelper.StereoDrawTexture(640, 145, 30, 30, ref healthPUIconGrey, Color.gray);
			
		if(staminaPUstored)
			GuiHelper.StereoDrawTexture(675, 147, 40, 25, ref staminaPUIconColor, Color.grey);
		else
			GuiHelper.StereoDrawTexture(675, 147, 40, 25, ref staminaPUIconGrey, Color.gray);	
	} */
	
	/*
	void displayBubbles()
	{
		switch(bubbleNum)
		{
			case 0:
			default:
					GuiHelper.StereoDrawTexture(717, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(737, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(757, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(777, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(797, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 1:
					GuiHelper.StereoDrawTexture(717, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(737, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(757, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(777, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(797, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 2:
					GuiHelper.StereoDrawTexture(717, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(737, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(757, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(777, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(797, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 3:
					GuiHelper.StereoDrawTexture(717, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(737, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(757, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(777, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(797, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 4:
					GuiHelper.StereoDrawTexture(717, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(737, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(757, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(777, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(797, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 5:
					GuiHelper.StereoDrawTexture(717, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(737, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(757, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(777, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(797, 173, 20, 17, ref bubbleIconColor, Color.gray);
					break;
		}
	}
	
	void displayPowerUpIcons2()
	{
		if(healthPUStored)
			GuiHelper.StereoDrawTexture(485, 145, 30, 30, ref healthPUIconColor, Color.grey);
		else
			GuiHelper.StereoDrawTexture(485, 145, 30, 30, ref healthPUIconGrey, Color.gray);
			
		if(staminaPUstored)
			GuiHelper.StereoDrawTexture(690, 147, 40, 25, ref staminaPUIconColor, Color.grey);
		else
			GuiHelper.StereoDrawTexture(690, 147, 40, 25, ref staminaPUIconGrey, Color.gray);	
	}
	
	void displayBubbles2()
	{
		switch(bubbleNum)
		{
			case 0:
			default:
					GuiHelper.StereoDrawTexture(510, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(530, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(550, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(570, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(590, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 1:
					GuiHelper.StereoDrawTexture(510, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(530, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(550, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(570, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(590, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 2:
					GuiHelper.StereoDrawTexture(510, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(530, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(550, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(570, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(590, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 3:
					GuiHelper.StereoDrawTexture(510, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(530, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(550, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(570, 173, 20, 17, ref bubbleIconColor, Color.black);
					GuiHelper.StereoDrawTexture(590, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 4:
					GuiHelper.StereoDrawTexture(510, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(530, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(550, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(570, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(590, 173, 20, 17, ref bubbleIconColor, Color.black);
					break;
			
			case 5:
					GuiHelper.StereoDrawTexture(560, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(580, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(600, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(620, 173, 20, 17, ref bubbleIconColor, Color.gray);
					GuiHelper.StereoDrawTexture(640, 173, 20, 17, ref bubbleIconColor, Color.gray);
					break;
		}
	}
	
	*/
 }
     

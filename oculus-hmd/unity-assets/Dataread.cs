using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;

public class Dataread : MonoBehaviour {
	
	private static float DISTANCE_FACTOR = 20.0f;
	private static float LIDAR_TO_BOT_HEAD_OFFSET = 0.0f;
	private static float DEGREES_TO_RADIANS = Mathf.PI / 180.0f;
	private static string localFileToRead = "lidar_data.txt";		
	private static string file = "data";
	
	private float timer;
	private float speedOfBot = 0;
	public int numberOfEntries;

	public float[] distances;
	public float[] angles; 
	public GameObject walle;
	public GameObject[] wallArray;
	public GameObject ground;
	
	public Vector3[] obstacleCoordinates;
	public Vector3 groundPosition;
		
	public string errorString = "";
	
	public bool[] tooClose;
	private bool robotHalted = false;
	


	/* 
	 * The Lidar data read from the file needs to be converted to match Unity Coordinate System
	 * LIDAR DATA: 0 DEGREE = STRAIGHT AHEAD
	 * 			   90 DEGREE = LEFT, 
	 * 			   180 DEGREE = BEHIND 
	 * 			   and so on..
	 * */
  	private float convertToUnityAngleFinal (float angle) {
		return ((angle + 90.0f) % 360.0f) * DEGREES_TO_RADIANS;
	}
	
	/* Read from the text file
	 * Error Codes
	 * 360,1  -> Lidar not sending data. Display "Getting sensor input..."
	 * 400,1  -> Wiimote has sent a brake signal. The bot is halted. Dont move the ground
	 * 
	 */ 
	private bool Load(string fileName)
	{		
	    string line;
		int i = 0;
		
		if (!File.Exists(fileName)) {
			return false;
		}
		
	    StreamReader theReader = new StreamReader(fileName, Encoding.Default);
		
		using (theReader) {
	        do {
	            line = theReader.ReadLine();
	
	            if (line != null && line != "") {
						
	                string[] entries = line.Split(',');
	                if (entries.Length > 0) {
						
						// Debug.Log("Entries[0]: " + float.Parse(entries[0]));
						
						// Sometimes the Lidar doesnt give readings
						if (entries[0] == "360") {
							errorString = (entries[1] == "1") ?  "Getting\nsensor input..." : "";
						}
						else if(entries[0] == "400") {
							Debug.Log ("Entry: 400th = " + entries[0] + ": " + entries[1]);
							robotHalted = (entries[1] == "1") ? true : false;
						}
						
						else {
							angles[i] = convertToUnityAngleFinal (float.Parse (entries[0]));
							float validDistance = float.Parse (entries[1]) / DISTANCE_FACTOR;
							// Debug.Log("Valid Distance " + validDistance);
							
							if (validDistance > 20.0f) {		
								distances[i] = validDistance;
								tooClose[i] = false;
							}
							else {
								distances[i] = 20.0f;
								//errorString = "Too close!";
								tooClose[i] = true;
								Debug.Log("TOO CLOSE!! ");	
							}
							errorString = "";
							i++;
						}
						
						/* Debug.Log ("the distance is  " + distances[i]);
					       Debug.Log ("the angle  is   " + angles[i]); */
					}	
					
	            }
	        } while (line != null);
				
			numberOfEntries = i;
			// Debug.Log("Number of Entries: " + numberOfEntries);
	        theReader.Close();
	        return true;
        } 
    }

	
	/* Called when the application starts */
	void Start () {

		ground = GameObject.FindGameObjectWithTag("ground");
		groundPosition = ground.transform.position;
		
		distances = new float[360];
		angles = new float[360];
		tooClose = new bool[360];		
		obstacleCoordinates = new Vector3[360];
			
		bool success = Load(file);
		if (!success) {
			Debug.Log("File: " + file + " doesn't exit. ");
			return;
		}
		
		for (int i = 0; i < numberOfEntries; i++) {
			obstacleCoordinates[i] = new Vector3 (distances[i] * Mathf.Cos(angles[i]), 
												  5.0f, 
												  distances[i] * Mathf.Sin(angles[i]) - LIDAR_TO_BOT_HEAD_OFFSET);
			
			/* If you want to make the obstacles face towards you at any angle */
			// float rotationRadians = ((Mathf.PI / 2) - angles[i]) * RADIANS_TO_DEGREES;
			// Instantiate(walle, obstacleCoordinates, Quaternion.Euler(0, rotationRadians, 0));
			Instantiate(walle, obstacleCoordinates[i], Quaternion.identity);
		}
	
	}

	
	/* Update is called once per frame */
	void Update () {
			
		if (Time.frameCount % 3 == 0) {
			
			if (!robotHalted)
				groundPosition.z -= 0.1f;
			ground.transform.position = groundPosition;

			wallArray = GameObject.FindGameObjectsWithTag("mywall");
			
			for (int i = 0; i < wallArray.Length; i++) {
				GameObject.Destroy(wallArray[i]);
			}
			
			bool success = Load(file);
			
			if (!success) {
				Debug.Log("File: " + file + " doesn't exit. ");
				return;
			}
				
			for (int i = 0; i < numberOfEntries; i++) {
				
				obstacleCoordinates[i] = new Vector3(distances[i]*Mathf.Cos(angles[i]), 
												3.0f ,distances[i]*Mathf.Sin(angles[i]));
				
				/* If you want to make the obstacles face towards you at any angle */
				// float rotationRadians = ((Mathf.PI / 2) - angles[i]) * RADIANS_TO_DEGREES;
				// Instantiate(walle, obstacleCoordinates, Quaternion.Euler(0, rotationRadians, 0));
				Instantiate(walle, obstacleCoordinates[i], Quaternion.identity);
			}
		}
	}
}



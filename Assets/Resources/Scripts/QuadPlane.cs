using UnityEngine;
using System;
using System.Collections;
using LibNoise.Unity;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class QuadPlane : MonoBehaviour
{
	// Use this for initialization
	bool beginTesting = false;
	
	public float second;
	public GameObject m_sun;
	public GameObject planeObject;
	
	public float rad = 5f;
	public float scaleFactor = 1f;
	Mesh qMesh;

	private GameObject[] mQuads;
	private QuadPlane[] mPlanes;
	public Vector3[] mQuadPositions;
	private GameObject[] mSiblings;
	
	public bool keepx = false;
	public bool keepy = false;
	public bool keepz = false;
	public float mod;
	public float maxdist;
	public Quaternion rotation;
	
	public bool z1;
	public bool z2;
	public bool x1;
	public bool x2;
	public bool y1;
	public bool y2;
	
	public ModuleBase noise;
	
	public bool hasSplit = false;
	public bool cannotRevert = false;
	
	public GameObject parent;
	public float noisemod;

	public Vector3 planetPos;
	public Vector3 relativePos;
	
	public bool hasOcean = false;
	public Color lowhold;
	
	GameObject cameras;
	
	public bool render = true;
	
	public double left;
	public double right;
	public double bottom;
	public double top;
	
	public bool generateMap = false;
	
	public Material groundfromspace;
	
	Vector3 oldCamPos;
	
	public int splitCount;
	public int timesSplit;
	
	public bool initSplit = false;
	public bool splitAgain = false;
	
	public GameObject planetMaker;
	
	Plane[] planes;
	
	public float camTimeWithin = 0;
	
	public bool isLoaded = false;
	
	public float latitude = 0;
	public float longitude = 0;
	
	public Material groundFromGround;
	
	public bool fromground;
	public bool testmat;

	public InitializePlanet planetInit;

	void Awake()
	{
		cameras = GameObject.FindGameObjectWithTag ("MainCamera");
		rotation = transform.rotation;

		mQuads = new GameObject[4];
		mPlanes = new QuadPlane[4];
		mQuadPositions = new Vector3[4];
		mSiblings = new GameObject[4];
	}

	public void CalculatePositions()
	{		
		relativePos = gameObject.transform.position - planetPos;
		maxdist = (6f * mod);

		if (keepx == true) 
		{ 
			//Quad is on the x plane				
			mQuadPositions[0] = new Vector3 (transform.position.x, transform.position.y + mod, transform.position.z + mod);
			mQuadPositions[1] = new Vector3 (transform.position.x, transform.position.y - mod, transform.position.z + mod);
			mQuadPositions[2] = new Vector3 (transform.position.x, transform.position.y - mod, transform.position.z - mod);
			mQuadPositions[3] = new Vector3 (transform.position.x, transform.position.y + mod, transform.position.z - mod);
		}
		
		if (keepy == true) 
		{ 
			//Quad is on the x plane				
			mQuadPositions[0] = new Vector3 (transform.position.x + mod, transform.position.y, transform.position.z + mod);
			mQuadPositions[1] = new Vector3 (transform.position.x - mod, transform.position.y, transform.position.z + mod);
			mQuadPositions[2] = new Vector3 (transform.position.x - mod, transform.position.y, transform.position.z - mod);
			mQuadPositions[3] = new Vector3 (transform.position.x + mod, transform.position.y, transform.position.z - mod);
		}
		
		if (keepz == true) 
		{ 
			//Quad is on the x plane
				
			mQuadPositions[0] = new Vector3 (transform.position.x + mod, transform.position.y + mod, transform.position.z);
			mQuadPositions[1] = new Vector3 (transform.position.x - mod, transform.position.y + mod, transform.position.z);
			mQuadPositions[2] = new Vector3 (transform.position.x - mod, transform.position.y - mod, transform.position.z);
			mQuadPositions[3] = new Vector3 (transform.position.x + mod, transform.position.y - mod, transform.position.z);
		}		
		
		if (keepx == false && keepy == false && keepz == false) 
		{ 
			//script hasn't inherited these values and is the first quad
			if (relativePos.x != 0) 
			{ 
				//Quad is on the x plane
				keepx = true;

				mQuadPositions[0] = new Vector3 (transform.position.x, transform.position.y + mod, transform.position.z + mod);
				mQuadPositions[1] = new Vector3 (transform.position.x, transform.position.y - mod, transform.position.z + mod);
				mQuadPositions[2] = new Vector3 (transform.position.x, transform.position.y - mod, transform.position.z - mod);
				mQuadPositions[3] = new Vector3 (transform.position.x, transform.position.y + mod, transform.position.z - mod);		
			}
			
			if (relativePos.y != 0) 
			{ 
				//Quad is on the x plane
				keepy = true;

				mQuadPositions[0] = new Vector3 (transform.position.x + mod, transform.position.y, transform.position.z + mod);
				mQuadPositions[1] = new Vector3 (transform.position.x - mod, transform.position.y, transform.position.z + mod);
				mQuadPositions[2] = new Vector3 (transform.position.x - mod, transform.position.y, transform.position.z - mod);
				mQuadPositions[3] = new Vector3 (transform.position.x + mod, transform.position.y, transform.position.z - mod);
			}
			
			if (relativePos.z != 0) 
			{ 
				//Quad is on the x plane
				keepz = true;

				mQuadPositions[0] = new Vector3 (transform.position.x + mod, transform.position.y + mod, transform.position.z);
				mQuadPositions[1] = new Vector3 (transform.position.x - mod, transform.position.y + mod, transform.position.z);
				mQuadPositions[2] = new Vector3 (transform.position.x - mod, transform.position.y - mod, transform.position.z);
				mQuadPositions[3] = new Vector3 (transform.position.x + mod, transform.position.y - mod, transform.position.z);
			}
		}

		if (relativePos.z == rad) 
		{
			rotation = Quaternion.Euler (90, 0, 0);
		}

		if (relativePos.z == -rad) 
		{
			rotation = Quaternion.Euler (270, 0, 0);
		}

		if (relativePos.y == rad) 
		{
			rotation = Quaternion.Euler (0, 0, 0);
		}

		if (relativePos.y == -rad) 
		{
			rotation = Quaternion.Euler (0, 0, 180);
		}

		if (relativePos.x == rad) 
		{
			rotation = Quaternion.Euler (0, 0, 270);
		}

		if (relativePos.x == -rad) 
		{
			rotation = Quaternion.Euler (0, 0, 90);
		}
	}
	
	void Scale (float scalefactor)
	{
		qMesh = GetComponent<MeshFilter> ().mesh;

		Vector3[] vertices = qMesh.vertices;
		Vector3[] verticesN = qMesh.vertices;
		Vector3[] normals = qMesh.normals;
		Vector3[] truevpos = qMesh.vertices;
		
		for (int i = 0; i < vertices.Length; i++) 
		{
			truevpos [i] = transform.TransformPoint (vertices [i]);
			verticesN [i] = vertices [i] / scalefactor;
		}

		qMesh.vertices = verticesN;
		qMesh.RecalculateNormals ();
		qMesh.RecalculateBounds ();
	}

	void Spherify (float radius, ModuleBase noi)
	{
		Vector3[] vertices = qMesh.vertices;
		Vector3[] verticesN = qMesh.vertices;
		Vector3[] normals = qMesh.normals;
		Vector3[] truevpos = qMesh.vertices;
		
		for (int i = 0; i < vertices.Length; i++) 
		{
			truevpos [i] = (transform.TransformPoint (vertices [i])) - planetPos;			
			verticesN [i] = (((truevpos [i].normalized) * (radius + (((float)noi.GetValue ((truevpos [i].normalized * radius) + planetPos)) * noisemod)))) - (relativePos);			
		}

		transform.rotation = Quaternion.Euler (0, 0, 0);
		qMesh.vertices = verticesN;
		qMesh.RecalculateNormals ();
		qMesh.RecalculateBounds ();
	}

	Texture2D AltGenTex (Vector3[] verts, ModuleBase module)
	{
		Texture2D tex = new Texture2D ((int)Mathf.Sqrt (verts.Length), (int)Mathf.Sqrt (verts.Length));
		Vector3[] interpolatedPoints = new Vector3[verts.Length];
		
		int reso = (int)Mathf.Sqrt (verts.Length);
		int pixelx = 0;
		int pixely = 0;
		
		
		for (int i = 0; i < verts.Length; i++) 
		{
			if (i < verts.Length - 1) 
			{
				interpolatedPoints [i] = ((verts [i] + verts [i + 1]) / 2);
			}
			
			verts [i] = transform.TransformPoint (verts [i]) - planetPos;

			if (pixelx == reso) 
			{
				pixelx = 0;
				pixely += 1;
			}

			float noiseval = (float)module.GetValue ((verts [i].normalized * rad) + planetPos);
			noiseval = Mathf.Clamp ((noiseval + 0.5f) / 2f, 0f, 1f);

			Color pixelColor = new Color (noiseval, noiseval, noiseval, 0);

			tex.SetPixel (pixelx, pixely, pixelColor);
			tex.Apply ();

			pixelx += 1;
		}
		
		tex.wrapMode = TextureWrapMode.Clamp;
		tex.Apply ();
		return tex;
	}

	void SplitQuad ()
	{		
		if (hasSplit == false) 
		{
			CreateQuads();
		} 
		else 
		{
			for (int i = 0; i < mQuads.Length; i++)
			{
				mQuads[i].SetActive(true);
			}
		}

		hasSplit = true;		
		
		beginTesting = true;
		gameObject.SetActive (false);	
	}

	public void InitialSplit ()
	{
		renderer.enabled = false;
		
		CreateQuads();

		beginTesting = true;
		gameObject.SetActive (false);
	}

	private void CreateQuads()
	{
		for (int i = 0; i < 4; i++)
		{
			mQuads[i] = (GameObject)Instantiate(planeObject);
			mQuads[i].transform.position = mQuadPositions[i];
			mQuads[i].transform.rotation = rotation;

			mPlanes[i] = (QuadPlane)mQuads[i].GetComponent(typeof(QuadPlane));
			mPlanes[i].rad = rad;
			mPlanes[i].scaleFactor = scaleFactor * 2;
			mPlanes[i].keepx = keepx;
			mPlanes[i].keepy = keepy;
			mPlanes[i].keepz = keepz;
			mPlanes[i].mod = (mod / 2);
			mPlanes[i].noise = noise;
			mPlanes[i].parent = gameObject;
			mPlanes[i].cannotRevert = false;
			mPlanes[i].noisemod = noisemod;
			mPlanes[i].planetPos = planetPos;
			mPlanes[i].lowhold = lowhold;
			mPlanes[i].m_sun = m_sun;
			mPlanes[i].groundfromspace = groundfromspace;
			mPlanes[i].splitCount = splitCount;
			mPlanes[i].timesSplit = timesSplit + 1;
			mPlanes[i].planetMaker = planetMaker;

			mQuads[i].transform.parent = planetMaker.transform;

			mPlanes[i].planeObject = planeObject;
			mPlanes[i].groundFromGround = groundFromGround;
			mPlanes[i].planetInit = planetInit;
		}

		for (int i = 0; i < mPlanes.Length; i++)
		{
			for (int j = 0; j < mPlanes[i].mSiblings.Length; j++)
			{
				for (int k = 0; k < mQuads.Length; k++)
				{
					if (i != k)
					{
						mPlanes[i].mSiblings[j] = mQuads[k];
					}
				}
			}
		}
	}

	void Start ()
	{	
		if (((cameras.transform.position - planetPos).magnitude < 1.025f * rad)) 
		{
			fromground = true;
			renderer.material = groundFromGround;
			renderer.material.SetTexture ("_DetailTex", ((Texture2D)Resources.Load ("Models/Bump")));
			renderer.material.SetTexture ("_DetailTex2", ((Texture2D)Resources.Load ("Models/Gbumb")));
			renderer.material.SetFloat ("_MultTest", (4096) / (Mathf.Pow (2, timesSplit)));
		}

		if (((cameras.transform.position - planetPos).magnitude > 1.025f * rad)) 
		{
			fromground = false;
			renderer.material = groundfromspace;
			renderer.material.SetTexture ("_DetailTex", ((Texture2D)Resources.Load ("Models/Bump")));
			renderer.material.SetTexture ("_DetailTex2", ((Texture2D)Resources.Load ("Models/Gbumb")));
			renderer.material.SetFloat ("_MultTest", (4096) / (Mathf.Pow (2, timesSplit)));
		}

		testmat = true;

		m_sun = GameObject.Find ("Sunlight");

		CalculatePositions ();
		Scale (scaleFactor);
		renderer.material.SetFloat ("_MultTest", (4096) / (Mathf.Pow (2, timesSplit)));

		renderer.material.SetTexture ("_DetailTex", ((Texture2D)Resources.Load ("Models/Bump")));
		renderer.material.SetTexture ("_DetailTex2", ((Texture2D)Resources.Load ("Models/Gbumb")));
		Spherify (rad, noise);
		
		Vector3 grelativePos = transform.TransformPoint (qMesh.vertices [60]) - transform.parent.position;
		latitude = (Mathf.Asin (grelativePos.y / rad) * 180) / Mathf.PI;
		float LAT = latitude * Mathf.PI / 180;

		float longitude1 = (180 * (Mathf.Asin ((grelativePos.z) / (rad * Mathf.Cos (LAT))))) / Mathf.PI;	//There are two possible solutions for longitude, compare these by re-entering them to the XYZ formula
		float longitude2 = (180 * (Mathf.Acos ((grelativePos.x) / (-rad * Mathf.Cos (LAT))))) / Mathf.PI;
		
		float LON1 = longitude1 * Mathf.PI / 180;
		float LON2 = longitude2 * Mathf.PI / 180;
		
		Vector3 testVector1 = new Vector3 ();
		testVector1.x = -rad * Mathf.Cos (LAT) * Mathf.Cos (LON1);
		testVector1.y = rad * Mathf.Sin (LAT);
		testVector1.z = rad * Mathf.Cos (LAT) * Mathf.Sin (LON1);
		testVector1 = testVector1 + planetPos;
		
		Vector3 testVector2 = new Vector3 ();
		testVector2.x = -rad * Mathf.Cos (LAT) * Mathf.Cos (LON2);
		testVector2.y = rad * Mathf.Sin (LAT);
		testVector2.z = rad * Mathf.Cos (LAT) * Mathf.Sin (LON2);
		testVector2 = testVector2 + planetPos;
		
		testVector1.x = Mathf.Round (testVector1.x);
		testVector1.y = Mathf.Round (testVector1.y);
		testVector1.z = Mathf.Round (testVector1.z);
		
		testVector2.x = Mathf.Round (testVector2.x);
		testVector2.y = Mathf.Round (testVector2.y);
		testVector2.z = Mathf.Round (testVector2.z);
		
		Vector3 testpos = new Vector3 ();
		testpos.x = Mathf.Round (transform.TransformPoint (qMesh.vertices [60]).x);
		testpos.y = Mathf.Round (transform.TransformPoint (qMesh.vertices [60]).y);
		testpos.z = Mathf.Round (transform.TransformPoint (qMesh.vertices [60]).z);

		if (testpos.x > testVector1.x - 10f && testpos.x < testVector1.x + 10f) 
		{
			if (testpos.y > testVector1.y - 10f && testpos.y < testVector1.y + 10f) 
			{
				if (testpos.z > testVector1.z - 10f && testpos.z < testVector1.z + 10f) 
				{
					longitude = longitude1;
				}
			}
		} 
		else 
		{
			longitude = longitude2;
		}
		
		qMesh.RecalculateBounds ();

		beginTesting = true;

		if (planeObject == null) 
		{
			Debug.Log ("Quad at" + transform.position.ToString () + "null planeObject");
		}
		
		if (timesSplit < splitCount) 
		{	
			timesSplit += 1;
			InitialSplit ();
		}

		isLoaded = true;
		gameObject.name = ("Quad " + Mathf.Round (latitude).ToString () + " " + Mathf.Round (longitude).ToString ());
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.T)) 
		{
			SplitQuad ();
		}
	 	
		renderer.material.SetVector ("v3LightPos", m_sun.transform.forward * -1.0f);
		
		planes = GeometryUtility.CalculateFrustumPlanes (Camera.main);
		if (GeometryUtility.TestPlanesAABB (planes, qMesh.bounds)) 
		{
			//Debug.Log("plane at" + transform.position.ToString() + "within bounds");
		}		
	
		if ((transform.TransformPoint (qMesh.vertices [60]) - cameras.transform.position).magnitude < maxdist) 
		{
			camTimeWithin += Time.deltaTime;
			
			if (camTimeWithin > 1f) 
			{
				if (beginTesting == true) 
				{
					if (GeometryUtility.TestPlanesAABB (planes, renderer.bounds)) 
					{
						if (scaleFactor < ((5 / rad) * (512))) 
						{
							SplitQuad ();
						}
					}
				}
			}
		}

		if ((transform.TransformPoint (qMesh.vertices [60]) - cameras.transform.position).magnitude > maxdist) 
		{
			camTimeWithin = 0f;
		}

		if ((transform.TransformPoint (qMesh.vertices [60]) - cameras.transform.position).magnitude > 4f * maxdist) 
		{			
			if (beginTesting == true) 
			{
				if (cannotRevert == false) 
				{
					//deactivates sibling quads to remove "scanning" effect
					foreach (GameObject sibling in mSiblings)
					{
						sibling.SetActive(false);
					}
					
					if (((cameras.transform.position - planetPos).magnitude > 1.025f * rad)) 
					{
						parent.renderer.material = groundfromspace;
						parent.renderer.material.SetTexture ("_DetailTex", ((Texture2D)Resources.Load ("Models/Bump")));
						parent.renderer.material.SetTexture ("_DetailTex2", ((Texture2D)Resources.Load ("Models/Gbumb")));
						parent.renderer.material.SetFloat ("_MultTest", (4096) / (Mathf.Pow (2, timesSplit - 1)));
					} 
					else 
					{
						parent.renderer.material = groundFromGround;
						parent.renderer.material.SetTexture ("_DetailTex", ((Texture2D)Resources.Load ("Models/Bump")));
						parent.renderer.material.SetTexture ("_DetailTex2", ((Texture2D)Resources.Load ("Models/Gbumb")));
						parent.renderer.material.SetFloat ("_MultTest", (4096) / (Mathf.Pow (2, timesSplit - 1)));
					}
				
					parent.SetActive (true);
					gameObject.SetActive (false);
				}
			}
		}

		if (testmat == true) 
		{
			if (((cameras.transform.position - planetPos).magnitude < 1.025f * rad) && fromground == false) 
			{
				fromground = true;

				renderer.material = groundFromGround;
				renderer.material.SetTexture ("_DetailTex", ((Texture2D)Resources.Load ("Models/Bump")));
				renderer.material.SetTexture ("_DetailTex2", ((Texture2D)Resources.Load ("Models/Gbumb")));
				renderer.material.SetFloat ("_MultTest", (4096) / (Mathf.Pow (2, timesSplit)));
			}

			if (((cameras.transform.position - planetPos).magnitude > 1.025f * rad) && fromground == true) 
			{
				fromground = false;

				renderer.material = groundfromspace;
				renderer.material.SetTexture ("_DetailTex", ((Texture2D)Resources.Load ("Models/Bump")));
				renderer.material.SetTexture ("_DetailTex2", ((Texture2D)Resources.Load ("Models/Gbumb")));
				renderer.material.SetFloat ("_MultTest", (4096) / (Mathf.Pow (2, timesSplit)));
			}
		}

		oldCamPos = cameras.transform.position;
	}
}

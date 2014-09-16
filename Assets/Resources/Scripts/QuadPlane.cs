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
	private Mesh mMesh;

	private GameObject[] mQuads;
	private QuadPlane[] mPlanes;
	private Vector3[] mQuadPositions;

	private QuadPlane mParent;
	private GameObject[] mSiblings;

	private float mMaxDistance;
	private float mScaleFactor;

	private bool mHasSplit = false;


	// Use this for initialization
	bool beginTesting = false;
	
	public bool keepx = false;
	public bool keepy = false;
	public bool keepz = false;
	public float mod;

	public Quaternion rotation;

	public bool cannotRevert = false;

	public Vector3 relativePos;
	public Color lowhold;
		
	public int splitCount;
	public int timesSplit;
	
	public bool initSplit = false;
	public bool splitAgain = false;
	
	public float camTimeWithin = 0;
	
	public bool isLoaded = false;
	
	public float latitude = 0;
	public float longitude = 0;
	
	public bool fromground;
	public bool testmat;

	public InitializePlanet mPlanet;

	public QuadPlane Parent
	{
		set { mParent = value; }
	}

	public float ScaleFactor
	{
		set { mScaleFactor = value; }
	}

	void Awake()
	{
		rotation = transform.rotation;

		mQuads = new GameObject[4];
		mPlanes = new QuadPlane[4];
		mQuadPositions = new Vector3[4];
		mSiblings = new GameObject[4];
	}

	public void CalculatePositions()
	{		
		relativePos = gameObject.transform.position - mPlanet.Position;
		mMaxDistance = (6f * mod);

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

		if (relativePos.z == mPlanet.radius) 
		{
			rotation = Quaternion.Euler (90, 0, 0);
		}

		if (relativePos.z == -mPlanet.radius) 
		{
			rotation = Quaternion.Euler (270, 0, 0);
		}

		if (relativePos.y == mPlanet.radius) 
		{
			rotation = Quaternion.Euler (0, 0, 0);
		}

		if (relativePos.y == -mPlanet.radius) 
		{
			rotation = Quaternion.Euler (0, 0, 180);
		}

		if (relativePos.x == mPlanet.radius) 
		{
			rotation = Quaternion.Euler (0, 0, 270);
		}

		if (relativePos.x == -mPlanet.radius) 
		{
			rotation = Quaternion.Euler (0, 0, 90);
		}
	}
	
	void Scale (float scalefactor)
	{
		mMesh = GetComponent<MeshFilter> ().mesh;

		Vector3[] vertices = mMesh.vertices;
		Vector3[] verticesN = mMesh.vertices;
		Vector3[] truevpos = mMesh.vertices;
		
		for (int i = 0; i < vertices.Length; i++) 
		{
			truevpos [i] = transform.TransformPoint (vertices [i]);
			verticesN [i] = vertices [i] / scalefactor;
		}

		mMesh.vertices = verticesN;
		mMesh.RecalculateNormals ();
		mMesh.RecalculateBounds ();
	}

	void Spherify (float radius, ModuleBase noi)
	{
		Vector3[] vertices = mMesh.vertices;
		Vector3[] verticesN = mMesh.vertices;
		Vector3[] truevpos = mMesh.vertices;
		
		for (int i = 0; i < vertices.Length; i++) 
		{
			truevpos [i] = (transform.TransformPoint (vertices [i])) - mPlanet.Position;			
			verticesN [i] = (((truevpos [i].normalized) * (radius + (((float)noi.GetValue ((truevpos [i].normalized * radius) + mPlanet.Position)) * mPlanet.NoiseComponent.NoiseMod)))) - (relativePos);			
		}

		transform.rotation = Quaternion.Euler (0, 0, 0);
		mMesh.vertices = verticesN;
		mMesh.RecalculateNormals ();
		mMesh.RecalculateBounds ();
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
			
			verts [i] = transform.TransformPoint (verts [i]) - mPlanet.Position;

			if (pixelx == reso) 
			{
				pixelx = 0;
				pixely += 1;
			}

			float noiseval = (float)module.GetValue ((verts [i].normalized * mPlanet.radius) + mPlanet.Position);
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
		if (mHasSplit == false) 
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

		mHasSplit = true;		
		
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
			mQuads[i] = (GameObject)Instantiate(PlanetUtilityComponent.Instance.PlanePrefab);
			mQuads[i].transform.position = mQuadPositions[i];
			mQuads[i].transform.rotation = rotation;

			mPlanes[i] = (QuadPlane)mQuads[i].GetComponent(typeof(QuadPlane));
			mPlanes[i].ScaleFactor = mScaleFactor * 2;
			mPlanes[i].keepx = keepx;
			mPlanes[i].keepy = keepy;
			mPlanes[i].keepz = keepz;
			mPlanes[i].mod = (mod / 2);
			mPlanes[i].Parent = this;
			mPlanes[i].cannotRevert = false;
			mPlanes[i].lowhold = lowhold;
			mPlanes[i].splitCount = splitCount;
			mPlanes[i].timesSplit = timesSplit + 1;

			mQuads[i].transform.parent = mPlanet.transform;

			mPlanes[i].mPlanet = mPlanet;
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
		if ((mPlanet.DistanceFromCamera.magnitude < 1.025f * mPlanet.radius)) 
		{
			fromground = true;
			SetRenderMaterial(mPlanet.GroundFromGroundMaterial, false);
		}

		if ((mPlanet.DistanceFromCamera.magnitude > 1.025f * mPlanet.radius)) 
		{
			fromground = false;
			SetRenderMaterial(mPlanet.GroundFromSpaceMaterial, false);
		}

		testmat = true;

		CalculatePositions();
		Scale(mScaleFactor);

		renderer.material.SetFloat ("_MultTest", (4096) / (Mathf.Pow (2, timesSplit)));
		renderer.material.SetTexture ("_DetailTex", PlanetUtilityComponent.Instance.BumpTexture);
		renderer.material.SetTexture ("_DetailTex2", PlanetUtilityComponent.Instance.GBumpTexture);

		Spherify(mPlanet.radius, mPlanet.NoiseComponent.Noise);
		
		Vector3 grelativePos = transform.TransformPoint (mMesh.vertices [60]) - transform.parent.position;
		latitude = (Mathf.Asin (grelativePos.y / mPlanet.radius) * 180) / Mathf.PI;
		float LAT = latitude * Mathf.PI / 180;

		float longitude1 = (180 * (Mathf.Asin ((grelativePos.z) / (mPlanet.radius * Mathf.Cos (LAT))))) / Mathf.PI;	//There are two possible solutions for longitude, compare these by re-entering them to the XYZ formula
		float longitude2 = (180 * (Mathf.Acos ((grelativePos.x) / (-mPlanet.radius * Mathf.Cos (LAT))))) / Mathf.PI;
		
		float LON1 = longitude1 * Mathf.PI / 180;
		float LON2 = longitude2 * Mathf.PI / 180;
		
		Vector3 testVector1 = new Vector3();
		testVector1.x = -mPlanet.radius * Mathf.Cos(LAT) * Mathf.Cos(LON1);
		testVector1.y = mPlanet.radius * Mathf.Sin(LAT);
		testVector1.z = mPlanet.radius * Mathf.Cos(LAT) * Mathf.Sin(LON1);
		testVector1 = testVector1 + mPlanet.Position;
		
		Vector3 testVector2 = new Vector3 ();
		testVector2.x = -mPlanet.radius * Mathf.Cos (LAT) * Mathf.Cos (LON2);
		testVector2.y = mPlanet.radius * Mathf.Sin (LAT);
		testVector2.z = mPlanet.radius * Mathf.Cos (LAT) * Mathf.Sin (LON2);
		testVector2 = testVector2 + mPlanet.Position;
		
		testVector1.x = Mathf.Round(testVector1.x);
		testVector1.y = Mathf.Round(testVector1.y);
		testVector1.z = Mathf.Round(testVector1.z);
		
		testVector2.x = Mathf.Round(testVector2.x);
		testVector2.y = Mathf.Round(testVector2.y);
		testVector2.z = Mathf.Round(testVector2.z);
		
		Vector3 testpos = new Vector3 ();
		testpos.x = Mathf.Round(transform.TransformPoint(mMesh.vertices[60]).x);
		testpos.y = Mathf.Round(transform.TransformPoint(mMesh.vertices[60]).y);
		testpos.z = Mathf.Round(transform.TransformPoint(mMesh.vertices[60]).z);

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
		
		mMesh.RecalculateBounds();

		beginTesting = true;
		
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
	 	
		renderer.material.SetVector("v3LightPos", PlanetUtilityComponent.Instance.Sun.transform.forward * -1.0f);

		if (GeometryUtility.TestPlanesAABB(PlanetUtilityComponent.Instance.FrustumPlanes, mMesh.bounds)) 
		{
			//Debug.Log("plane at" + transform.position.ToString() + "within bounds");
		}		
	
		if ((transform.TransformPoint (mMesh.vertices [60]) - PlanetUtilityComponent.Instance.CameraPosition).magnitude < mMaxDistance) 
		{
			camTimeWithin += Time.deltaTime;
			
			if (camTimeWithin > 1f) 
			{
				if (beginTesting == true) 
				{
					if (GeometryUtility.TestPlanesAABB(PlanetUtilityComponent.Instance.FrustumPlanes, renderer.bounds)) 
					{
						if (mScaleFactor < ((5 / mPlanet.radius) * (512))) 
						{
							SplitQuad ();
						}
					}
				}
			}
		}

		if ((transform.TransformPoint(mMesh.vertices [60]) - PlanetUtilityComponent.Instance.CameraPosition).magnitude > mMaxDistance) 
		{
			camTimeWithin = 0f;
		}

		if ((transform.TransformPoint(mMesh.vertices [60]) - PlanetUtilityComponent.Instance.CameraPosition).magnitude > 4f * mMaxDistance) 
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
					
					if ((mPlanet.DistanceFromCamera.magnitude > 1.025f * mPlanet.radius)) 
					{
						SetRenderMaterial(mPlanet.GroundFromSpaceMaterial, true);
					} 
					else 
					{
						SetRenderMaterial(mPlanet.GroundFromGroundMaterial, true);
					}
				
					mParent.gameObject.SetActive(true);
					gameObject.SetActive (false);
				}
			}
		}

		if (testmat == true) 
		{
			if ((mPlanet.DistanceFromCamera.magnitude < 1.025f * mPlanet.radius) && fromground == false) 
			{
				fromground = true;

				SetRenderMaterial(mPlanet.GroundFromGroundMaterial, false);
			}

			if ((mPlanet.DistanceFromCamera.magnitude > 1.025f * mPlanet.radius) && fromground == true) 
			{
				fromground = false;

				SetRenderMaterial(mPlanet.GroundFromSpaceMaterial, false);
			}
		}
	}

	private void SetRenderMaterial(Material material, bool parent)
	{
		renderer.material = material;

		QuadPlane plane = parent ? mParent : this;

		plane.renderer.material.SetTexture("_DetailTex", PlanetUtilityComponent.Instance.BumpTexture);
		plane.renderer.material.SetTexture("_DetailTex2", PlanetUtilityComponent.Instance.GBumpTexture);
		plane.renderer.material.SetFloat("_MultTest", (4096) / (Mathf.Pow (2, timesSplit - 1)));
	}
}

using UnityEngine;
using System.Collections;
using LibNoise.Unity;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;

public class InitializePlanet : MonoBehaviour
{
	//Planetary Control variables
	public Color mainColor;
	public float radius = 10;
	public int splitCount = 1;
	public bool hasOcean;
	public bool hasAtmo;
	
	Transform atmoFromGroundTransform;
	Transform atmoFromSpaceTransform;
	
	//Camera Stuff
	public Transform camTransform;
	float cameraDistance;

	float scalar;

	private PlanetNoiseComponent mNoiseComponent;

	private GameObject[] mQuads;
	private QuadPlane[] mPlanes;
	
	//Atmospheric Scattering Variables
	public Material m_groundMaterial;
	public Material m_skyGroundMaterial;
	public Material m_skySpaceMaterial;
	public Vector3 planetPosition;
	public Material m_groundfromgroundMaterial;
	
	public float m_hdrExposure = 0.8f;
	public Vector3 m_waveLength = new Vector3 (0.65f, 0.57f, 0.475f);
	// Wave length of sun light

	public float m_ESun = 20.0f;
	// Sun brightness constant

	public float m_kr = 0.0025f;
	// Rayleigh scattering constant

	public float m_km = 0.0010f;
	// Mie scattering constant

	public float m_g = -0.990f;
	// The Mie phase asymmetry factor, must be between 0.999 to -0.999
	
	//Dont change these
	private const float m_outerScaleFactor = 1.025f;
	// Difference between inner and ounter radius. Must be 2.5%
	private float m_innerRadius;
	// Radius of the ground sphere
	private float m_outerRadius;
	// Radius of the sky sphere
	private float m_scaleDepth = 0.25f;
	// The scale depth (i.e. the altitude at which the atmosphere's average density is found)

	public PlanetNoiseComponent NoiseComponent
	{
		get { return mNoiseComponent; }
	}

	public Vector3 Position
	{
		get { return transform.position; }
	}

	public Vector3 DistanceFromCamera
	{
		get { return PlanetUtilityComponent.Instance.CameraPosition - Position; }
	}

	public float ScaleFactor
	{
		get { return 5 / radius; }
	}

	public Material GroundFromGroundMaterial
	{
		get { return m_groundfromgroundMaterial; }
	}

	public Material GroundFromSpaceMaterial
	{
		get { return m_groundMaterial; }
	}
	
	void Awake ()
	{
		planetPosition = gameObject.transform.position;
		camTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
		mNoiseComponent = gameObject.AddComponent<PlanetNoiseComponent>();
	}

	public Vector3 SurfacePoint(float latitude, float longitude, float raddif, ModuleBase noi, GameObject planetObject)	//Allows objects to be placed with ease by useing lat, long(-180, 180)
	{
		Mathf.Clamp (latitude, -90, 90);
		Mathf.Clamp (longitude, -180, 180);
		Vector3 spoint;
		float lat = latitude * Mathf.PI / 180;
		float lon = longitude * Mathf.PI / 180;
		float rad = radius;
		spoint.x = (-rad * Mathf.Cos (lat) * Mathf.Cos (lon));
		spoint.y = (rad * Mathf.Sin (lat));
		spoint.z = (rad * Mathf.Cos (lat) * Mathf.Sin (lon));
		spoint = spoint + planetObject.transform.position;
		 
		raddif = (float)noi.GetValue (spoint);

		int noisemod = 1;
		rad = radius + (raddif * noisemod);
		spoint.x = (-rad * Mathf.Cos (lat) * Mathf.Cos (lon));
		spoint.y = (rad * Mathf.Sin (lat));
		spoint.z = (rad * Mathf.Cos (lat) * Mathf.Sin (lon));

		return (spoint + planetObject.transform.position);
	}

	public Quaternion SurfaceRotation(Vector3 position)
	{
		return Quaternion.LookRotation (position - transform.position);		
	}	
	
	void Start()
	{
		m_innerRadius = radius;
		m_outerRadius = m_innerRadius * m_outerScaleFactor;	
		
		if (hasAtmo == true) 
		{
			CreateAtmosphere ();
		} 
		else 
		{
			m_groundfromgroundMaterial = new Material (Shader.Find ("DiffusePlanet"));
			m_groundMaterial = new Material (Shader.Find ("DiffusePlanet"));
		}

		CreateQuads ();	
	}

	private void CreateQuads ()
	{
		float x = transform.position.x;
		float y = transform.position.y;
		float z = transform.position.z;
		scalar = 5 / radius;

		mQuads = new GameObject[6];
		mPlanes = new QuadPlane[6];
		Vector3[] positions = { new Vector3 (x + radius, y, z), new Vector3 (x, y, z + radius), new Vector3 (x, y, z - radius), new Vector3 (x, y + radius, z),
			new Vector3 (x, y - radius, z), new Vector3 (x - radius, y, z)};
		Quaternion[] rotations = { Quaternion.Euler (0, 0, 270), Quaternion.Euler (90, 0, 0), Quaternion.Euler (270, 0, 0), Quaternion.Euler (0, 0, 0), 
			Quaternion.Euler (0, 0, 180), Quaternion.Euler (0, 0, 90)};

		for (int i = 0; i < mQuads.Length; i++)
		{
			GameObject quad = mQuads[i];
			QuadPlane plane = mPlanes[i];

			quad = (GameObject)Instantiate(PlanetUtilityComponent.Instance.PlanePrefab);
			quad.transform.position = positions[i];
			quad.transform.rotation = rotations[i];

			plane = (QuadPlane)quad.GetComponent (typeof(QuadPlane));
			plane.ScaleFactor = scalar;
			plane.mod = (radius / 2);
			plane.cannotRevert = true;
			plane.name = "q" + i.ToString();
			plane.mPlanet = this;

			plane.CalculatePositions();

			plane.initSplit = true;
			plane.splitAgain = true;
			plane.splitCount = splitCount;
			plane.timesSplit = 0;

			quad.transform.parent = gameObject.transform;
		}
	}

	public float[] XYZ2LATON (Vector3 XYZ)
	{
		Vector3 grelativePos = XYZ - planetPosition;
		float latitude = (Mathf.Asin (planetPosition.y / radius) * 180) / Mathf.PI;
		float LAT = latitude * Mathf.PI / 180;
		float longitude = 0;
		
		float longitude1 = (180 * (Mathf.Asin ((grelativePos.z) / (radius * Mathf.Cos (LAT))))) / Mathf.PI;	//There are two possible solutions for longitude, compare these by re-entering them to the XYZ formula
		float longitude2 = (180 * (Mathf.Acos ((grelativePos.x) / (-radius * Mathf.Cos (LAT))))) / Mathf.PI;
		
		float LON1 = longitude1 * Mathf.PI / 180;
		float LON2 = longitude2 * Mathf.PI / 180;
		
		Vector3 testVector1 = new Vector3 ();
		testVector1.x = -radius * Mathf.Cos (LAT) * Mathf.Cos (LON1);
		testVector1.y = radius * Mathf.Sin (LAT);
		testVector1.z = radius * Mathf.Cos (LAT) * Mathf.Sin (LON1);
		testVector1 = testVector1 + planetPosition;
		
		Vector3 testVector2 = new Vector3 ();
		testVector2.x = -radius * Mathf.Cos (LAT) * Mathf.Cos (LON2);
		testVector2.y = radius * Mathf.Sin (LAT);
		testVector2.z = radius * Mathf.Cos (LAT) * Mathf.Sin (LON2);
		testVector2 = testVector2 + planetPosition;
		
		testVector1.x = Mathf.Round (testVector1.x);
		testVector1.y = Mathf.Round (testVector1.y);
		testVector1.z = Mathf.Round (testVector1.z);
		
		testVector2.x = Mathf.Round (testVector2.x);
		testVector2.y = Mathf.Round (testVector2.y);
		testVector2.z = Mathf.Round (testVector2.z);
		
		Vector3 testpos = new Vector3 ();
		testpos.x = Mathf.Round (XYZ.x);
		testpos.y = Mathf.Round (XYZ.y);
		testpos.z = Mathf.Round (XYZ.z);		
		
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

		float[] latlon = new float[2];
		latlon [0] = latitude;
		latlon [1] = longitude;
		
		return latlon;
	}

	private void CreateAtmosphere()
	{
		m_skyGroundMaterial = new Material(Shader.Find("Atmosphere/SkyFromAtmosphere"));
		m_skySpaceMaterial = new Material(Shader.Find("Atmosphere/SkyFromSpace"));
		m_groundMaterial = new Material(Shader.Find("Atmosphere/GroundFromSpace"));
		m_groundfromgroundMaterial = new Material(Shader.Find("DiffusePlanet"));
		
		InitializeMaterial(m_groundMaterial, Vector3.one);
		InitializeMaterial(m_skyGroundMaterial, new Vector3 (3f, 3f, 3f));
		InitializeMaterial(m_skySpaceMaterial, new Vector3 (1.8f, 1.8f, 1.8f));
		InitializeMaterial(m_groundfromgroundMaterial, Vector3.one);
		
		GameObject atmoFromGround = (GameObject)Instantiate (Resources.Load ("Prefabs/Atmosphere"));
		atmoFromGround.transform.position = planetPosition;
		atmoFromGround.transform.localScale = new Vector3 (m_outerRadius, m_outerRadius, m_outerRadius);
		atmoFromGroundTransform = atmoFromGround.transform.Find ("SphereObject");
		GameObject atmoFromSpace = (GameObject)Instantiate (Resources.Load ("Prefabs/Atmosphere"));
		atmoFromSpace.transform.position = planetPosition;
		atmoFromSpace.transform.localScale = new Vector3 (m_outerRadius, m_outerRadius, m_outerRadius);
		atmoFromSpaceTransform = atmoFromSpace.transform.Find ("SphereObject");
		
		atmoFromGroundTransform.renderer.material = m_skyGroundMaterial;
		atmoFromSpaceTransform.renderer.material = m_skySpaceMaterial;	
		atmoFromGround.transform.parent = gameObject.transform;
		atmoFromSpace.transform.parent = gameObject.transform;		
	}
	
	private void InitializeMaterial (Material mat, Vector3 cshift)
	{
		Vector3 invWaveLength4 = new Vector3 (1.0f / Mathf.Pow (m_waveLength.x, 4.0f), 1.0f / Mathf.Pow (m_waveLength.y, 4.0f), 1.0f / Mathf.Pow (m_waveLength.z, 4.0f));
		float scale = 1.0f / (m_outerRadius - m_innerRadius);
		
		mat.SetVector("_ColorShift", cshift);
		mat.SetVector("_PlanetPos", planetPosition);
		mat.SetVector("v3LightPos", PlanetUtilityComponent.Instance.Sun.transform.forward * -1.0f);
		mat.SetVector("v3InvWavelength", invWaveLength4);
		mat.SetFloat("fOuterRadius", m_outerRadius);
		mat.SetFloat("fOuterRadius2", m_outerRadius * m_outerRadius);
		mat.SetFloat("fInnerRadius", m_innerRadius);
		mat.SetFloat("fInnerRadius2", m_innerRadius * m_innerRadius);
		mat.SetFloat("fKrESun", m_kr * m_ESun);
		mat.SetFloat("fKmESun", m_km * m_ESun);
		mat.SetFloat("fKr4PI", m_kr * 4.0f * Mathf.PI);
		mat.SetFloat("fKm4PI", m_km * 4.0f * Mathf.PI);
		mat.SetFloat("fScale", scale);
		mat.SetFloat("fScaleDepth", m_scaleDepth);
		mat.SetFloat("fScaleOverScaleDepth", scale / m_scaleDepth);
		mat.SetFloat("fHdrExposure", m_hdrExposure);
		mat.SetFloat("g", m_g);
		mat.SetFloat("g2", m_g * m_g);
	}
	
	// Update is called once per frame
	void Update()
	{		
		if (hasAtmo == true) 
		{
			cameraDistance = ((camTransform.position) - planetPosition).magnitude;
			
			if ((cameraDistance > radius * 1.5f) && atmoFromGroundTransform.renderer.enabled == true) 
			{
				atmoFromGroundTransform.renderer.enabled = false;
				atmoFromSpaceTransform.renderer.enabled = true;
			}

			if ((cameraDistance < radius * 1.5f) && atmoFromSpaceTransform.renderer.enabled == true) 
			{
				atmoFromGroundTransform.renderer.enabled = true;
				atmoFromSpaceTransform.renderer.enabled = false;	
			}
		}	
	}
}

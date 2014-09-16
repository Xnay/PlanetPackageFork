using UnityEngine;

public class PlanetUtilityComponent : SingletonBehaviour<PlanetUtilityComponent>
{
	private Plane[] mFrustumPlanes;
	private GameObject mSun;

	private Camera mCamera;

	private GameObject mPlanePrefab;

	private Texture2D mBumpTexture;
	private Texture2D mGBumpTexture;

	public Plane[] FrustumPlanes
	{
		get { return mFrustumPlanes; }
	}

	public Vector3 CameraPosition
	{
		get { return mCamera.transform.position; }
	}

	public GameObject Sun
	{
		get { return mSun; }
	}

	public GameObject PlanePrefab
	{
		get { return mPlanePrefab; }
	}

	public Texture2D BumpTexture
	{
		get { return mBumpTexture; }
	}

	public Texture2D GBumpTexture
	{
		get { return mGBumpTexture; }
	}

	protected override void OnSingletonAwake()
	{
		mSun =  GameObject.Find("Sunlight");
		mCamera = Camera.main;
		mFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

		mPlanePrefab = (GameObject)Resources.Load("Prefabs/QuadPlane");

		mBumpTexture = (Texture2D)Resources.Load("Models/Bump");
		mGBumpTexture = (Texture2D)Resources.Load("Models/Gbumb");
	}

	void Update()
	{
		mFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
	}
}
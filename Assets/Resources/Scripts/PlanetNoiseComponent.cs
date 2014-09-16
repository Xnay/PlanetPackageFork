using LibNoise.Unity;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;
using UnityEngine;

public class PlanetNoiseComponent : MonoBehaviour
{
	public int Octaves = 8;
	public float ScalarV;
	public float NoiseMod = 1;

	private ModuleBase mNoise;

	private RidgedMultifractal mMountainTerrain = new RidgedMultifractal();
	private Billow mBaseFlatTerrain = new Billow();
	private Perlin mTerrainType = new Perlin();

	public ModuleBase Noise
	{
		get { return mNoise; }
	}

	void Awake()
	{
		mMountainTerrain.OctaveCount = Octaves;
		mMountainTerrain.Frequency = 2.0f;

		mBaseFlatTerrain.OctaveCount = Octaves;
		mBaseFlatTerrain.Frequency = 2.0f;

		mTerrainType.OctaveCount = Octaves;
		mTerrainType.Frequency = 0.5f;
		mTerrainType.Persistence = 0.25;

		Voronoi voronoiNoise = new Voronoi();
		voronoiNoise.Frequency = 5.0f;

		Perlin perlinNoise = new Perlin();
		perlinNoise.Frequency = 2.0f;

		ScaleBias flatTerrain = new ScaleBias(0.125, -0.75, mBaseFlatTerrain);

		Select terrainSelector = new Select(flatTerrain, mMountainTerrain, mTerrainType);
		terrainSelector.SetBounds(0.0, 1000.0);
		terrainSelector.FallOff = 0.125;

		Turbulence finalTerrain = new Turbulence(0.25, terrainSelector);
		finalTerrain.Frequency = 4.0f;		

		mNoise = new Scale(ScalarV, ScalarV, ScalarV, finalTerrain);
	}
}
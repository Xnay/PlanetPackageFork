using UnityEngine;

public class QuadTree
{
	private GameObject[] mQuads;
	private QuadPlane[] mPlanes;

	public QuadTree()
	{
		mQuads = new GameObject[6];
		mPlanes = new QuadPlane[6];
	}
}
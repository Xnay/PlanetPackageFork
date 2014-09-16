﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton behaviour.
/// 
/// Helper class for implementing singleton functionality on a MonoBehaviour
/// </summary>
public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
	private static T _instance = null;
	private static bool _isAwake = false;

	private bool _destructing = false;

	public bool isSelfDestructing {
		get { return _destructing; }	
	}

	// primary accessor
	public static T instance {
		get {
			if (_instance == null) {
				_instance = (T)FindObjectOfType (typeof(T));
				if (_instance == null) {
					string instanceName = typeof(T).ToString ();
					GameObject go = GameObject.Find (instanceName);
					if (go == null) {
						go = new GameObject ();
						go.name = instanceName;
					}
					_instance = go.AddComponent<T> ();
				}
			}
			return _instance;
		}
	}

	// alternate accessor
	public static T Instance {
		get { return instance; }
	}

	// alternate accessor
	public static T GetInstance() {
		return instance;
	}

	protected virtual void Awake() {
		if (instance == this) {
			_instance.OnSingletonAwake();
			_isAwake = true;
		}
		else {
			_destructing = true;

			Debug.LogWarning("Multiple instances of '" + typeof(T).ToString() + "' were found, Self destructing.");  
			Destroy(gameObject);
		}
	}

	protected virtual void OnDestroy() {
		if (_instance == this) {
			_instance.OnSingletonDestroy();
			_instance = null;
			_isAwake = false;
		}
	}

	/// <summary>
	/// This method should be used instead of MonoBehaviour.Awake()
	/// </summary>
	protected virtual void OnSingletonAwake()
	{
	}

	/// <summary>
	/// This method should be used instead of MonoBehaviour.OnDestroy()
	/// </summary>
	protected virtual void OnSingletonDestroy()
	{
	}

	public static bool Exist()
	{
		return _instance != null;
	}

	public static bool IsAwake()
	{
		if(!Exist()){ return false; }
		return _isAwake;
	}

	public static void DestroyExplicit()
	{
		if(_instance != null)
		{
			Destroy(_instance.gameObject);
			_instance = null;
		}
	}
}
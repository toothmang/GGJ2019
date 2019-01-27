using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static string LogName = string.Format("[{0}] ", typeof(T).Name);

    protected static T _instance;

    private static object _lock = new object();

    public static bool executeInEditMode = false;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock(_lock)
                {
                    if (_instance == null)
                    {
                        var instances = FindObjectsOfType<T>();

                        if (instances.Length > 1)
                        {
                            Debug.LogError(LogName + "Something went really wrong " +
                                " - there should never be more than 1 singleton!" +
                                " Reopenning the scene might fix it.");
                            _instance = instances[0];
                        }
                        else if (instances.Length == 1)
                        {
                            _instance = instances[0];
                        }
                        else
                        {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = "(singleton) " + typeof(T).ToString();

                            Debug.Log("[Singleton] An instance of " + typeof(T) +
                                " is needed in the scene, so '" + singleton +
                                "' was created.");
                        }
                    }
                }
            }

            return _instance;
        }
    }

    public static bool applicationIsQuitting = false;

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public virtual void OnDestroy()
    {
        applicationIsQuitting = true;
    }
}
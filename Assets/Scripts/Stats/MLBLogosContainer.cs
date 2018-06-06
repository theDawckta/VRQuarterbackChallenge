using UnityEngine;

public class MLBLogosContainer : LogosContainer {

    #region SINGLETON
    private static MLBLogosContainer _instance;
    public static MLBLogosContainer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MLBLogosContainer>();

                if (_instance == null)
                {
                    GameObject container = new GameObject("MLBLogosHolder");
                    _instance = container.AddComponent<MLBLogosContainer>();
                }
            }

            return _instance;
        }
    }
    #endregion
}

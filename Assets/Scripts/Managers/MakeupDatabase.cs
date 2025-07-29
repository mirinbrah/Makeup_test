using UnityEngine;
using System.Collections.Generic;

public class MakeupDatabase : MonoBehaviour
{
    [System.Serializable]
    public class MakeupMapping
    {
        public ColorSource keyColorSource;
        public GameObject faceObject;
    }

    [System.Serializable]
    public class LipstickMapping
    {
        public ClickableItem keyLipstick; 
        public GameObject faceObject;
    }

    public List<MakeupMapping> blushMappings;
    public List<MakeupMapping> eyeshadowMappings;
    public List<LipstickMapping> lipstickMappings;

    public GameObject GetFaceObjectFor(ColorSource source, GamePhase phase)
    {
        List<MakeupMapping> listToSearch = null;

        switch (phase)
        {
            case GamePhase.Blush:
                listToSearch = blushMappings;
                break;
            case GamePhase.Eyeshadow:
                listToSearch = eyeshadowMappings;
                break;
        }

        if (listToSearch != null)
        {
            MakeupMapping foundMapping = listToSearch.Find(m => m.keyColorSource == source);
            if (foundMapping != null)
            {
                return foundMapping.faceObject;
            }
        }

        return null;
    }

    public GameObject GetFaceObjectFor(ClickableItem lipstick)
    {
        LipstickMapping foundMapping = lipstickMappings.Find(m => m.keyLipstick == lipstick);
        if (foundMapping != null)
        {
            return foundMapping.faceObject;
        }
        return null;
    }
}
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

    public List<MakeupMapping> blushMappings;
    public List<MakeupMapping> eyeshadowMappings;

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
}
// MakeupDatabase.cs
using UnityEngine;
using System.Collections.Generic;

// Это обычный MonoBehaviour, который можно повесить на любой объект в сцене
public class MakeupDatabase : MonoBehaviour
{
    // Внутренний класс для удобной настройки в инспекторе
    [System.Serializable]
    public class MakeupMapping
    {
        [Tooltip("Объект-источник цвета в палетке (перетащить из сцены)")]
        public ColorSource keyColorSource;
        [Tooltip("Объект на лице, который соответствует этому источнику (перетащить из сцены)")]
        public GameObject faceObject;
    }

    // Список всех наших соответствий
    public List<MakeupMapping> blushMappings;

    // Вспомогательный метод для поиска нужного объекта на лице
    public GameObject GetFaceObjectFor(ColorSource source)
    {
        // Ищем в списке соответствие, где ключ совпадает с переданным источником
        MakeupMapping foundMapping = blushMappings.Find(m => m.keyColorSource == source);

        if (foundMapping != null)
        {
            return foundMapping.faceObject;
        }

        // Если ничего не найдено, возвращаем null
        return null;
    }
}
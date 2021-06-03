using UnityEngine;
public enum ResourceTypeEnum
{
    wood,
    stone,
    food
}
public interface IResource
{
    GameObject gameObject { get; }
    ResourceSettings m_settings { get; }
    Coroutine GatherResource(GameObject gather);
    void StopGathering(Coroutine coroutine);
    bool AbleToBeHarvested();
}
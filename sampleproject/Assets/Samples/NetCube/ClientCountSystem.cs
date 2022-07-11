using Unity.Entities;
using Unity.NetCode;
using UnityEditor.Experimental;

[UpdateInGroup(typeof(ClientPresentationSystemGroup))]
public class ClientCountSystem : ComponentSystem
{
    private EntityQuery _players;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        _players = Entities.WithAllReadOnly<MovableCubeComponent>().ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Counter.Count = _players.CalculateEntityCount();
    }
}

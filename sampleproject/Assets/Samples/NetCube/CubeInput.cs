using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct CubeInputCommand : ICommandData
{
    public uint Tick {get; set;}
    public int horizontal;
    public int vertical;
}

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[AlwaysSynchronizeSystem]
public partial class CubeInput : SystemBase
{
    private Random _random;
    private double _timeOutToUpdate = 10;
    private double _startTime = 0;

    ClientSimulationSystemGroup m_ClientSimulationSystemGroup;
    protected override void OnCreate()
    {
        _random = new Random((uint) System.DateTime.Now.Ticks);
        RequireSingletonForUpdate<NetworkIdComponent>();
        // RequireSingletonForUpdate<EnableNetCubeGame>();
        m_ClientSimulationSystemGroup = World.GetExistingSystem<ClientSimulationSystemGroup>();
    }

    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            var verticalSpeed = _random.NextFloat(0.01f, 1f);
            var horizontalSpeed = _random.NextFloat(0.01f, 1f);
            _startTime = World.Time.ElapsedTime;

            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            var commandTargetEntity = GetSingletonEntity<CommandTargetComponent>();
            Entities.WithAll<MovableCubeComponent>().WithNone<CubeInputCommand>().ForEach((Entity ent, ref GhostOwnerComponent ghostOwner) =>
            {
                if (ghostOwner.NetworkId == localPlayerId)
                {
                    commandBuffer.AddBuffer<CubeInputCommand>(ent);
                    commandBuffer.SetComponent(commandTargetEntity, new CommandTargetComponent {targetEntity = ent});

                    commandBuffer.AddComponent<MoveParams>(ent);
                    commandBuffer.SetComponent(ent, new MoveParams()
                    {
                        VerticalSpeed = verticalSpeed,
                        HorizontalSpeed = horizontalSpeed
                    });
                }
            }).Run();
            commandBuffer.Playback(EntityManager);
            return;
        }
        // var input = default(CubeInput);
        // input.Tick = m_ClientSimulationSystemGroup.ServerTick;
        // if (Input.GetKey("a"))
        //     input.horizontal -= 1;
        // if (Input.GetKey("d"))
        //     input.horizontal += 1;
        // if (Input.GetKey("s"))
        //     input.vertical -= 1;
        // if (Input.GetKey("w"))
        //     input.vertical += 1;


        var moveParams = EntityManager.GetComponentData<MoveParams>(localInput);
        var input = default(CubeInputCommand);
        input.Tick = m_ClientSimulationSystemGroup.ServerTick;
        input.horizontal = (int) math.sign(math.sin(UnityEngine.Time.time * moveParams.HorizontalSpeed * 1f));
        input.vertical = (int) math.sign(math.sin(UnityEngine.Time.time * moveParams.VerticalSpeed * 1f));



        var inputBuffer = EntityManager.GetBuffer<CubeInputCommand>(localInput);
        inputBuffer.AddCommandData(input);

        if (World.Time.ElapsedTime - _startTime > _timeOutToUpdate)
        {
            Debug.Log("Update params");
            _startTime = World.Time.ElapsedTime;
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            commandBuffer.SetComponent(localInput, new MoveParams()
            {
                VerticalSpeed = _random.NextFloat(0.01f, 1f),
                HorizontalSpeed = _random.NextFloat(0.01f, 1f)
            });
            commandBuffer.Playback(EntityManager);
        }
    }
}

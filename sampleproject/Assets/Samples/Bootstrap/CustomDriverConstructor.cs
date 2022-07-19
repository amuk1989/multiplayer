using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;

public class CustomDriverConstructor : INetworkStreamDriverConstructor
{
    public void CreateClientDriver(World world, out NetworkDriver driver, out NetworkPipeline unreliablePipeline, out NetworkPipeline reliablePipeline, out NetworkPipeline unreliableFragmentedPipeline)
    {
        var reliabilityParams = new ReliableUtility.Parameters {WindowSize = 1};
        var fragmentationParams = new FragmentationUtility.Parameters {PayloadCapacity = 1 * 2000};

        driver = NetworkDriver.Create(reliabilityParams, fragmentationParams);
        unreliablePipeline = driver.CreatePipeline(typeof(NullPipelineStage));
        reliablePipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        unreliableFragmentedPipeline = driver.CreatePipeline(typeof(FragmentationPipelineStage));
    }

    public void CreateServerDriver(World world, out NetworkDriver driver, out NetworkPipeline unreliablePipeline, out NetworkPipeline reliablePipeline, out NetworkPipeline unreliableFragmentedPipeline)
    {
        var reliabilityParams = new ReliableUtility.Parameters {WindowSize = 32};
        var fragmentationParams = new FragmentationUtility.Parameters {PayloadCapacity = 4 * 1024};

        var baselibParams = BaselibNetworkInterface.DefaultParameters;
        baselibParams.receiveQueueCapacity *= 1;
        baselibParams.sendQueueCapacity *= 1;
        driver = NetworkDriver.Create(reliabilityParams, fragmentationParams, baselibParams);

        unreliablePipeline = driver.CreatePipeline(typeof(NullPipelineStage));
        reliablePipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
        unreliableFragmentedPipeline = driver.CreatePipeline(typeof(FragmentationPipelineStage));
    }
}

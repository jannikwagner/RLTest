
public interface IDistanceRewarder
{
    float Reward();
}

public class DistanceRewarder : IDistanceRewarder
{
    private float startDistance;
    private float lastDistance;
    private System.Func<float> getDistance;
    public DistanceRewarder(System.Func<float> getDistance)
    {
        this.getDistance = getDistance;
        startDistance = getDistance();
        lastDistance = startDistance;
    }
    public float Reward()
    {
        float distance = getDistance();
        float reward = (lastDistance - distance) / startDistance;
        lastDistance = distance;
        return reward;
    }
}

public class OnlyImprovingDistanceRewarder : IDistanceRewarder
{
    private float startDistance;
    private float bestDistance;
    private System.Func<float> getDistance;
    public OnlyImprovingDistanceRewarder(System.Func<float> getDistance)
    {
        this.getDistance = getDistance;
        startDistance = getDistance();
        bestDistance = startDistance;
    }
    public float Reward()
    {
        float distance = getDistance();
        if (distance < bestDistance)
        {
            float reward = (bestDistance - distance) / startDistance;
            bestDistance = distance;
            return reward;
        }
        return 0f;
    }
}

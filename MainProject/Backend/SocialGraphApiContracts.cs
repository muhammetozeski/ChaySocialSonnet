namespace ChaySocialSonnet.MainProject.Backend
{
    public sealed record FollowRequest(string FollowerPublicId);

    public sealed record FollowStatusResponse(bool IsFollowing, int FollowerCount, int FollowingCount);
}

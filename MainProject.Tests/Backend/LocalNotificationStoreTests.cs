using ChaySocialSonnet.MainProject.Backend;
using ChaySocialSonnet.Web.Backend;

namespace ChaySocialSonnet.MainProject.Tests.Backend
{
    public class LocalNotificationStoreTests
    {
        [Fact]
        public async Task AddAsync_ThenGetForUserAsync_ReturnsIt()
        {
            var store = new LocalNotificationStore();

            await store.AddAsync("alice", "bob", NotificationKind.Follow, subjectPostId: null);
            IReadOnlyList<AppNotification> notifications = await store.GetForUserAsync("alice", 10);

            AppNotification notification = Assert.Single(notifications);
            Assert.Equal("bob", notification.ActorPublicId);
            Assert.Equal(NotificationKind.Follow, notification.Kind);
            Assert.False(notification.IsRead);
        }

        [Fact]
        public async Task GetForUserAsync_ReturnsNewestFirst()
        {
            var store = new LocalNotificationStore();
            await store.AddAsync("alice", "bob", NotificationKind.Follow, subjectPostId: null);
            await store.AddAsync("alice", "carol", NotificationKind.Like, subjectPostId: "post1");

            IReadOnlyList<AppNotification> notifications = await store.GetForUserAsync("alice", 10);

            Assert.Equal("carol", notifications[0].ActorPublicId);
            Assert.Equal("bob", notifications[1].ActorPublicId);
        }

        [Fact]
        public async Task GetForUserAsync_DoesNotReturnAnotherUsersNotifications()
        {
            var store = new LocalNotificationStore();
            await store.AddAsync("alice", "bob", NotificationKind.Follow, subjectPostId: null);

            IReadOnlyList<AppNotification> carolsNotifications = await store.GetForUserAsync("carol", 10);

            Assert.Empty(carolsNotifications);
        }

        [Fact]
        public async Task GetUnreadCountAsync_CountsOnlyUnread()
        {
            var store = new LocalNotificationStore();
            await store.AddAsync("alice", "bob", NotificationKind.Follow, subjectPostId: null);
            await store.AddAsync("alice", "carol", NotificationKind.Like, subjectPostId: "post1");

            Assert.Equal(2, await store.GetUnreadCountAsync("alice"));
        }

        [Fact]
        public async Task MarkAllReadAsync_ZerosOutTheUnreadCount()
        {
            var store = new LocalNotificationStore();
            await store.AddAsync("alice", "bob", NotificationKind.Follow, subjectPostId: null);
            await store.AddAsync("alice", "carol", NotificationKind.Like, subjectPostId: "post1");

            await store.MarkAllReadAsync("alice");

            Assert.Equal(0, await store.GetUnreadCountAsync("alice"));
            Assert.All(await store.GetForUserAsync("alice", 10), n => Assert.True(n.IsRead));
        }

        [Fact]
        public async Task MarkAllReadAsync_DoesNotAffectAnotherUsersNotifications()
        {
            var store = new LocalNotificationStore();
            await store.AddAsync("alice", "bob", NotificationKind.Follow, subjectPostId: null);
            await store.AddAsync("carol", "dave", NotificationKind.Follow, subjectPostId: null);

            await store.MarkAllReadAsync("alice");

            Assert.Equal(1, await store.GetUnreadCountAsync("carol"));
        }
    }
}
